using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using bipj.Data;

namespace bipj.Models
{
    public class Jar
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private int _jarId;
        private int _userId;
        private string _jarName = "";
        private string _description = "";
        private decimal _percentage;
        private bool _isDefault;
        private int _position;
        private string _colorHex = "#cccccc";
        private bool _isDeleted;
        private DateTime _createdAt;

        // Cache for balances
        private static Dictionary<int, decimal> _cachedBalances;
        private static int _cachedUserId = -1;

        public Jar() { }

        public Jar(int jarId, int userId, string jarName, string description,
                   decimal percentage, bool isDefault, int position, string colorHex, bool isDeleted = false)
        {
            _jarId = jarId;
            _userId = userId;
            _jarName = jarName;
            _description = description;
            _percentage = percentage;
            _isDefault = isDefault;
            _position = position;
            _colorHex = colorHex ?? "#cccccc";
            _isDeleted = isDeleted;
            _createdAt = DateTime.Now;
        }

        public int JarId { get { return _jarId; } set { _jarId = value; } }
        public int UserId { get { return _userId; } set { _userId = value; } }
        public string JarName { get { return _jarName; } set { _jarName = value; } }
        public string Description { get { return _description; } set { _description = value; } }
        public decimal Percentage { get { return _percentage; } set { _percentage = value; } }
        public bool IsDefault { get { return _isDefault; } set { _isDefault = value; } }
        public int Position { get { return _position; } set { _position = value; } }
        public string ColorHex { get { return _colorHex; } set { _colorHex = value ?? "#cccccc"; } }
        public bool IsDeleted { get { return _isDeleted; } set { _isDeleted = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }

        public decimal Balance { get; set; }

        public static void InvalidateCache()
        {
            _cachedBalances = null;
            _cachedUserId = -1;
        }

        public void CreateDefaultJars(int userId)
        {
            var jars = new List<Jar>
            {
                new Jar(0, userId, "NEC", "Necessities", 55, true, 0, "#c8b6ff"),
                new Jar(0, userId, "FFA", "Financial Freedom", 10, false, 1, "#a5d8ff"),
                new Jar(0, userId, "PLAY", "Play", 10, false, 2, "#ff94c2"),
                new Jar(0, userId, "LTSS", "Long-Term Savings", 10, false, 3, "#66a6ff"),
                new Jar(0, userId, "EDU", "Education", 10, false, 4, "#7f00ff"),
                new Jar(0, userId, "GIVE", "Give", 5, false, 5, "#ff00b8")
            };

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                foreach (var jar in jars)
                {
                    const string sql = @"INSERT INTO Jars
                    (UserId, JarName, Description, Percentage, IsDefault, Position, ColorHex)
                    VALUES (@UserId,@JarName,@Description,@Percentage,@IsDefault,@Position,@ColorHex)";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", jar.UserId);
                        cmd.Parameters.AddWithValue("@JarName", jar.JarName);
                        cmd.Parameters.AddWithValue("@Description", jar.Description ?? "");
                        cmd.Parameters.AddWithValue("@Percentage", jar.Percentage);
                        cmd.Parameters.AddWithValue("@IsDefault", jar.IsDefault);
                        cmd.Parameters.AddWithValue("@Position", jar.Position);
                        cmd.Parameters.AddWithValue("@ColorHex", jar.ColorHex ?? "#cccccc");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public int InsertJar()
        {
            if (IsDefault) throw new InvalidOperationException("Cannot insert a default jar via InsertJar method.");

            const string sql = @"
            INSERT INTO Jars
            (UserId, JarName, Description, Percentage, IsDefault, Position, ColorHex, IsDeleted, CreatedAt)
            VALUES (@UserId,@JarName,@Description,@Percentage,@IsDefault,@Position,@ColorHex,@IsDeleted,@CreatedAt)";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@UserId", UserId);
                p.AddWithValue("@JarName", JarName);
                p.AddWithValue("@Description", Description ?? "");
                p.AddWithValue("@Percentage", Percentage);
                p.AddWithValue("@IsDefault", false);
                p.AddWithValue("@Position", Position);
                p.AddWithValue("@ColorHex", ColorHex ?? "#cccccc");
                p.AddWithValue("@IsDeleted", false);
                p.AddWithValue("@CreatedAt", CreatedAt);
            });
        }

        public int UpdateJar(bool allowDefaultUpdate = false)
        {
            const string sql = @"
            UPDATE Jars SET JarName=@JarName, Description=@Description,
            Position=@Position, ColorHex=@ColorHex
            WHERE JarId=@JarId AND UserId=@UserId";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@JarName", JarName ?? "");
                p.AddWithValue("@Description", Description ?? "");
                p.AddWithValue("@Position", Position);
                p.AddWithValue("@ColorHex", ColorHex ?? "#cccccc");
                p.AddWithValue("@JarId", JarId);
                p.AddWithValue("@UserId", UserId);
            });
        }

        public int DeleteJar()
        {
            if (IsDefault) throw new InvalidOperationException("Cannot delete the default jar.");

            using (var conn = bipj.Data.Db.OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    // 0) Load the jar being deleted
                    var jar = bipj.Data.Db.QuerySingle(conn, tx,
                        @"SELECT JarId, UserId, COALESCE(Percentage,0) AS Pct, IsDeleted
                        FROM Jars
                        WHERE JarId=@JarId AND UserId=@UserId",
                        p => { p.AddWithValue("@JarId", JarId); p.AddWithValue("@UserId", UserId); },
                        r => new
                        {
                            JarId = Convert.ToInt32(r["JarId"]),
                            UserId = Convert.ToInt32(r["UserId"]),
                            Pct = Convert.ToDecimal(r["Pct"]),
                            IsDeleted = Convert.ToBoolean(r["IsDeleted"])
                        });

                    if (jar == null) throw new InvalidOperationException("Jar not found.");
                    if (jar.IsDeleted) return 0;

                    // 1) Find (or create) default jar
                    int defaultJarId = bipj.Data.Db.Scalar<int>(conn, tx,
                        @"SELECT TOP 1 JarId FROM Jars 
                        WHERE UserId=@U AND IsDefault=1 AND IsDeleted=0",
                        p => p.AddWithValue("@U", UserId), 0);

                    if (defaultJarId == 0)
                    {
                        CreateDefaultJars(UserId);
                        defaultJarId = bipj.Data.Db.Scalar<int>(conn, tx,
                            @"SELECT TOP 1 JarId FROM Jars 
                            WHERE UserId=@U AND IsDefault=1 AND IsDeleted=0",
                            p => p.AddWithValue("@U", UserId), 0);
                        if (defaultJarId == 0)
                            throw new InvalidOperationException("No default jar available.");
                    }

                    if (defaultJarId == JarId)
                        throw new InvalidOperationException("Default jar cannot be deleted.");

                    // 2) Move percentage to default; zero this jar
                    bipj.Data.Db.Exec(conn, tx,
                        @"UPDATE Jars 
                        SET Percentage = COALESCE(Percentage,0) + @Pct
                        WHERE JarId=@Def AND UserId=@U;
                        UPDATE Jars 
                        SET Percentage = 0
                        WHERE JarId=@Jar AND UserId=@U;",
                        p =>
                        {
                            p.AddWithValue("@Pct", jar.Pct);
                            p.AddWithValue("@Def", defaultJarId);
                            p.AddWithValue("@Jar", JarId);
                            p.AddWithValue("@U", UserId);
                        });

                    // 3) Transfer live balance to default (two rows, signed amounts)
                    decimal bal = GetCurrentBalance(UserId, JarId);
                    if (bal > 0m)
                    {
                        var now = DateTime.UtcNow;

                        // Outflow (negative) from the deleted jar
                        bipj.Data.Db.Exec(conn, tx,
                            @"INSERT INTO JarTransactions
                            (UserId, JarId, Name, Amount, Date, TransactionType, Category)
                            VALUES
                            (@U, @FromJar, @Note, @Amt, @Dt, 'Transfer', 'Transfer');",
                            p =>
                            {
                                p.AddWithValue("@U", UserId);
                                p.AddWithValue("@FromJar", JarId);
                                p.AddWithValue("@Note", "Transfer to default (auto on delete)");
                                p.AddWithValue("@Amt", -bal);
                                p.AddWithValue("@Dt", now);
                            });

                        // Inflow (positive) to the default jar
                        bipj.Data.Db.Exec(conn, tx,
                            @"INSERT INTO JarTransactions
                            (UserId, JarId, Name, Amount, Date, TransactionType, Category)
                            VALUES
                            (@U, @ToJar, @Note, @Amt, @Dt, 'Transfer', 'Transfer');",
                            p =>
                            {
                                p.AddWithValue("@U", UserId);
                                p.AddWithValue("@ToJar", defaultJarId);
                                p.AddWithValue("@Note", "Transfer from Deleted Jar (auto)");
                                p.AddWithValue("@Amt", bal);
                                p.AddWithValue("@Dt", now);
                            });
                    }

                    // 4) Reassign any goal links to default
                    ReassignGoalTransactions(conn, tx, JarId, defaultJarId);

                    // 5) Soft-delete (no DeletedAt column in your table)
                    int updated = bipj.Data.Db.Exec(conn, tx,
                        @"UPDATE Jars 
                        SET IsDeleted=1
                        WHERE JarId=@Jar AND UserId=@U;",
                        p => { p.AddWithValue("@Jar", JarId); p.AddWithValue("@U", UserId); });

                    // 6) Normalize remaining active jar percentages to 100.00%
                    NormalizeActiveJarPercentages(conn, tx, UserId);

                    tx.Commit();
                    return updated;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }


        private void NormalizeActiveJarPercentages(SqlConnection conn, SqlTransaction tx, int userId)
        {
            var rows = bipj.Data.Db.Query(conn, tx,
                @"SELECT JarId, COALESCE(Percentage,0) AS Pct
                FROM Jars
                WHERE UserId=@U AND IsDeleted=0
                ORDER BY Position",
                p => p.AddWithValue("@U", userId),
                r => new { JarId = Convert.ToInt32(r["JarId"]), Pct = Convert.ToDecimal(r["Pct"]) });

            if (rows.Count == 0) return;

            decimal sum = 0m;
            for (int i = 0; i < rows.Count; i++) sum += rows[i].Pct;

            if (sum <= 0m)
            {
                // Make default jar 100% if everything is zeroed out
                int defId = bipj.Data.Db.Scalar<int>(conn, tx,
                    @"SELECT TOP 1 JarId FROM Jars 
                    WHERE UserId=@U AND IsDefault=1 AND IsDeleted=0",
                    p => p.AddWithValue("@U", userId), 0);
                if (defId == 0) return;

                bipj.Data.Db.Exec(conn, tx,
                    @"UPDATE Jars 
                    SET Percentage = CASE WHEN JarId=@Def THEN 100 ELSE 0 END
                    WHERE UserId=@U AND IsDeleted=0",
                    p => { p.AddWithValue("@Def", defId); p.AddWithValue("@U", userId); });
                return;
            }

            // Scale to 100, round down to 2dp, then distribute the leftover 0.01% to the largest fractions
            var temp = new List<Tuple<int, decimal, decimal>>(); // (JarId, down, frac)
            for (int i = 0; i < rows.Count; i++)
            {
                var exact = (rows[i].Pct / sum) * 100m;
                var down = Math.Floor(exact * 100m) / 100m;
                var frac = exact - down;
                temp.Add(Tuple.Create(rows[i].JarId, down, frac));
            }

            decimal allocated = 0m;
            for (int i = 0; i < temp.Count; i++) allocated += temp[i].Item2;

            int cents = (int)Math.Round((100m - allocated) * 100m, MidpointRounding.AwayFromZero);

            // sort by frac desc
            temp.Sort((a, b) => b.Item3.CompareTo(a.Item3));
            int take = Math.Abs(cents);
            int sign = Math.Sign(cents);
            for (int i = 0; i < take && i < temp.Count; i++)
            {
                temp[i] = Tuple.Create(temp[i].Item1, temp[i].Item2 + (sign * 0.01m), temp[i].Item3);
            }

            // persist
            for (int i = 0; i < temp.Count; i++)
            {
                bipj.Data.Db.Exec(conn, tx,
                    @"UPDATE Jars SET Percentage=@P WHERE JarId=@J",
                    p => { p.AddWithValue("@P", temp[i].Item2); p.AddWithValue("@J", temp[i].Item1); });
            }
        }


        public Jar GetDefaultJar(int userId)
        {
            const string sql = "SELECT TOP 1 * FROM Jars WHERE UserID=@UserID AND IsDefault=1";
            var list = Db.Query(sql,
                p => p.AddWithValue("@UserID", userId),
                MapJar);
            return list.Count == 0 ? null : list[0];
        }

        public Dictionary<int, decimal> GetJarBalances(int userId)
        {
            var balances = new Dictionary<int, decimal>();
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"
            SELECT JarId,
            ISNULL(SUM(Amount),0) AS Balance
            FROM JarTransactions
            WHERE UserID=@UserId
            GROUP BY JarId", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        balances[rdr.GetInt32(0)] = rdr.GetDecimal(1);
            }
            return balances;
        }


        // ✅ Get current balance for a single jar
        public decimal GetCurrentBalance(int userId, int jarId)
        {
            var balances = GetJarBalances(userId);
            return balances.TryGetValue(jarId, out var bal)
                ? bal
                : 0m;
        }


        public List<Jar> GetJarsByUser(int userId, bool includeDeleted = false)
        {
            // pull balances from transactions
            var balances = GetJarBalances(userId);
            var jars = new List<Jar>();

            // if includeDeleted==true, ignore the IsDeleted filter
            string sql = includeDeleted
            ? @"
            SELECT JarId, JarName, Percentage, ColorHex, IsDefault
            FROM Jars
            WHERE UserId = @UserId
            ORDER BY Position"
            : @"
            SELECT JarId, JarName, Percentage, ColorHex, IsDefault
            FROM Jars
            WHERE UserId = @UserId
            AND IsDeleted = 0
            ORDER BY Position";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        int id = rdr.GetInt32(0);
                        jars.Add(new Jar
                        {
                            JarId = id,
                            JarName = rdr.GetString(1),
                            Percentage = rdr.GetDecimal(2),
                            ColorHex = rdr.GetString(3),
                            IsDefault = rdr.GetBoolean(4),
                            Balance = balances.TryGetValue(id, out var b) ? b : 0m
                        });
                    }
                }
            }

            return jars;
        }

        public Jar GetJarById(int jarId, int userId)
        {
            const string sql = "SELECT * FROM Jars WHERE JarId=@JarId AND UserId=@UserId AND IsDeleted=0";
            var list = Db.Query(sql,
                p =>
                {
                    p.AddWithValue("@JarId", jarId);
                    p.AddWithValue("@UserId", userId);
                },
                MapJar);
            return list.Count == 0 ? null : list[0];
        }

        public List<(int JarId, decimal Percentage)> GetJarsWithPercentages(int userId, bool includeDeleted = false)
        {
            var sql = @"
            SELECT JarId, COALESCE(Percentage, 0) AS Percentage
            FROM Jars
            WHERE UserId = @UserId
              " + (includeDeleted ? "" : "AND (IsDeleted = 0 OR IsDeleted IS NULL)") + @"
            ORDER BY Position";

            return Db.Query(sql,
                p => p.AddWithValue("@UserId", userId),
                r => (Convert.ToInt32(r["JarId"]), Convert.ToDecimal(r["Percentage"])));
        }


        public bool UserHasJars(int userId)
        {
            const string sql = @"
            SELECT COUNT(*) 
            FROM Jars 
            WHERE UserID = @UserID
              AND (IsDeleted = 0 OR IsDeleted IS NULL)";

            return Db.Scalar(sql, p => p.AddWithValue("@UserID", userId), 0) > 0;
        }


        public string GetDefaultJarName(int userId)
        {
            const string sql = "SELECT JarName FROM Jars WHERE UserID=@UserID AND IsDefault=1";
            return Db.Scalar(sql, p => p.AddWithValue("@UserID", userId), "");
        }

        public void UpdatePercentageAndDefault(List<Jar> updatedJars)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"UPDATE Jars SET Percentage=@Percentage, IsDefault=@IsDefault WHERE JarId=@JarId AND UserId=@UserId";
                        foreach (var jar in updatedJars)
                        {
                            using (var cmd = new SqlCommand(sql, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@Percentage", jar.Percentage);
                                cmd.Parameters.AddWithValue("@IsDefault", jar.IsDefault);
                                cmd.Parameters.AddWithValue("@JarId", jar.JarId);
                                cmd.Parameters.AddWithValue("@UserId", jar.UserId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public string GetNextAvailableColor(int userId)
        {
            var usedColors = Db.Query(
                "SELECT ColorHex FROM Jars WHERE UserId=@UserId",
                p => p.AddWithValue("@UserId", userId),
                r => r["ColorHex"].ToString().ToLower());

            string[] defaultColors =
            {
                "#c8b6ff", "#a5d8ff", "#ff94c2", "#66a6ff", "#7f00ff", "#ff00b8",
                "#ffc75f", "#f9f871", "#00c9a7", "#ff6f61", "#ff9671", "#c34a36"
            };

            foreach (var color in defaultColors)
            {
                if (!usedColors.Contains(color.ToLower())) return color;
            }
            return "#cccccc";
        }

        private static Jar MapJar(SqlDataReader reader)
        {
            return new Jar
            {
                JarId = Convert.ToInt32(reader["JarID"]),
                UserId = Convert.ToInt32(reader["UserID"]),
                JarName = reader["JarName"]?.ToString(),
                Description = reader["Description"]?.ToString(),
                Percentage = Convert.ToDecimal(reader["Percentage"]),
                IsDefault = Convert.ToBoolean(reader["IsDefault"]),
                Position = Convert.ToInt32(reader["Position"]),
                ColorHex = reader["ColorHex"]?.ToString() ?? "#cccccc",
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }

        private void ReassignGoalTransactions(SqlConnection conn, SqlTransaction tran, int oldJarId, int defaultJarId)
        {
            const string sql = "UPDATE GoalTransactions SET SourceJarID=@DefaultJarID WHERE SourceJarID=@OldJarID";
            using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@DefaultJarID", defaultJarId);
                cmd.Parameters.AddWithValue("@OldJarID", oldJarId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void ResetAllJarsForUser_Lite(int userId)
        {
            if (userId <= 0) throw new ArgumentException(nameof(userId));
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) Clear transactions for this user's jars
                        using (var cmd = new SqlCommand(@"
DELETE T
FROM JarTransactions T
JOIN Jars J ON T.JarId = J.JarId
WHERE J.UserId = @UserId;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.CommandTimeout = 120;
                            cmd.ExecuteNonQuery();
                        }

                        // 2) Clear snapshots
                        using (var cmd = new SqlCommand(
                            "DELETE FROM JarSnapshots WHERE UserId = @UserId;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.CommandTimeout = 120;
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }

            // Re-gen snapshots (non-critical)
            try
            {
                Jar.InvalidateCache();
                var snap = new JarSnapshot();
                snap.GenerateSnapshots(userId, "daily");
                snap.GenerateSnapshots(userId, "monthly");
            }
            catch { /* optional: log */ }
        }


    }
}
