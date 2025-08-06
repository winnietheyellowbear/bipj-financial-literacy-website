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

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var defaultJar = GetDefaultJar(UserId);
                        if (defaultJar == null)
                        {
                            CreateDefaultJars(UserId);
                            defaultJar = GetDefaultJar(UserId) ?? throw new InvalidOperationException("Failed to create default jars.");
                        }

                        ReassignGoalTransactions(conn, tran, JarId, defaultJar.JarId);

                        const string markSql = "UPDATE Jars SET IsDeleted=1 WHERE JarId=@JarId AND UserId=@UserId";
                        using (var markCmd = new SqlCommand(markSql, conn, tran))
                        {
                            markCmd.Parameters.AddWithValue("@JarId", JarId);
                            markCmd.Parameters.AddWithValue("@UserId", UserId);
                            int result = markCmd.ExecuteNonQuery();
                            tran.Commit();
                            return result;
                        }
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
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

        public List<(int JarId, decimal Percentage)> GetJarsWithPercentages(int userId)
        {
            const string sql = "SELECT JarId, Percentage FROM Jars WHERE UserId=@UserId ORDER BY Position";
            return Db.Query(sql,
                p => p.AddWithValue("@UserId", userId),
                r => (Convert.ToInt32(r["JarId"]), Convert.ToDecimal(r["Percentage"])));
        }

        public bool UserHasJars(int userId)
        {
            const string sql = "SELECT COUNT(*) FROM Jars WHERE UserID=@UserID";
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

        // new internal helper that can use an existing connection+transaction
        public void CreateDefaultJars(int userId, SqlConnection conn, SqlTransaction tx)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (tx == null) throw new ArgumentNullException(nameof(tx));
            CreateDefaultJarsInternal(userId, conn, tx);
        }

        private void CreateDefaultJarsInternal(int userId, SqlConnection conn, SqlTransaction tx)
        {
            // Example: create six default jars. Adjust names/logic to match your original.
            var defaultJarNames = new List<string> { "Jar 1", "Jar 2", "Jar 3", "Jar 4", "Jar 5", "Jar 6" };
            foreach (var name in defaultJarNames)
            {
                using (var cmd = new SqlCommand(
                    @"INSERT INTO Jars (UserId, JarName, Balance, CreatedAt)
                      VALUES (@UserId, @Name, 0, GETDATE())", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void ResetAllJarsForUserSimple_HardDelete(int userId)
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
                        // --- before counts (for debugging) ---
                        int beforeJars = Count("Jars", userId, conn, tx);
                        int beforeTxns = CountJoinedTransactions(userId, conn, tx);
                        int beforeSnapshots = Count("JarSnapshots", userId, conn, tx);
                        System.Diagnostics.Trace.TraceInformation($"[Reset] before: jars={beforeJars}, txns={beforeTxns}, snaps={beforeSnapshots}");

                        // 1. Delete transactions
                        using (var cmd = new SqlCommand(
                            @"DELETE T
                      FROM JarTransactions T
                      INNER JOIN Jars J ON T.JarId = J.JarId
                      WHERE J.UserId = @UserId", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            int deletedTx = cmd.ExecuteNonQuery();
                            System.Diagnostics.Trace.TraceInformation($"[Reset] deleted transactions rows: {deletedTx}");
                        }

                        // 2. Delete snapshots
                        using (var cmd = new SqlCommand(
                            "DELETE FROM JarSnapshots WHERE UserId = @UserId", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            int deletedSnap = cmd.ExecuteNonQuery();
                            System.Diagnostics.Trace.TraceInformation($"[Reset] deleted snapshots rows: {deletedSnap}");
                        }

                        // 3. Delete jars
                        using (var cmd = new SqlCommand(
                            "DELETE FROM Jars WHERE UserId = @UserId", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            int deletedJars = cmd.ExecuteNonQuery();
                            System.Diagnostics.Trace.TraceInformation($"[Reset] deleted jars rows: {deletedJars}");
                        }

                        // --- after deletion counts ---
                        int midJars = Count("Jars", userId, conn, tx);
                        System.Diagnostics.Trace.TraceInformation($"[Reset] after delete: jars={midJars}");

                        // 4. Recreate default jars
                        var jarManager = new Jar();
                        jarManager.CreateDefaultJars(userId, conn, tx); // same tx

                        tx.Commit();
                        System.Diagnostics.Trace.TraceInformation($"[Reset] committed transaction");
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        System.Diagnostics.Trace.TraceError($"Reset failed for user {userId}: {ex}");
                        throw;
                    }
                }
            }

            // regen snapshots (non-critical)
            try
            {
                var snapshot = new JarSnapshot();
                snapshot.GenerateSnapshots(userId, "daily");
                snapshot.GenerateSnapshots(userId, "monthly");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"Snapshot regen failed: {ex.Message}");
            }
        }

        // helpers for debug counts
        private static int Count(string table, int userId, SqlConnection conn, SqlTransaction tx)
        {
            using (var cmd = new SqlCommand($"SELECT COUNT(1) FROM {table} WHERE UserId = @UserId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static int CountJoinedTransactions(int userId, SqlConnection conn, SqlTransaction tx)
        {
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(1)
          FROM JarTransactions T
          INNER JOIN Jars J ON T.JarId = J.JarId
          WHERE J.UserId = @UserId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
