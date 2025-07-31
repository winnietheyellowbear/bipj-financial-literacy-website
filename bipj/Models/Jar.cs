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
        private decimal _initialAmount;
        private decimal _amount;
        private int _position;
        private string _colorHex = "#cccccc";
        private bool _isDeleted;
        private DateTime _createdAt;

        public Jar() { }

        public Jar(int jarId, int userId, string jarName, string description,
                   decimal percentage, bool isDefault, decimal initialAmount,
                   decimal amount, int position, string colorHex, bool isDeleted = false)
        {
            _jarId = jarId;
            _userId = userId;
            _jarName = jarName;
            _description = description;
            _percentage = percentage;
            _isDefault = isDefault;
            _initialAmount = initialAmount;
            _amount = amount;
            _position = position;
            _colorHex = colorHex ?? "#cccccc";
            _isDeleted = isDeleted;
            _createdAt = DateTime.Now;
        }

        public Jar(int userId, string jarName, string description,
                   decimal percentage, bool isDefault, decimal initialAmount,
                   decimal amount = 0, int position = 0)
            : this(0, userId, jarName, description, percentage, isDefault, initialAmount, amount, position, null) { }

        public int JarId { get { return _jarId; } set { _jarId = value; } }
        public int UserId { get { return _userId; } set { _userId = value; } }
        public string JarName { get { return _jarName; } set { _jarName = value; } }
        public string Description { get { return _description; } set { _description = value; } }
        public decimal Percentage { get { return _percentage; } set { _percentage = value; } }
        public bool IsDefault { get { return _isDefault; } set { _isDefault = value; } }
        public decimal InitialAmount { get { return _initialAmount; } set { _initialAmount = value; } }
        public decimal Amount { get { return _amount; } set { _amount = value; } }
        public int Position { get { return _position; } set { _position = value; } }
        public string ColorHex { get { return _colorHex; } set { _colorHex = value ?? "#cccccc"; } }
        public bool IsDeleted { get { return _isDeleted; } set { _isDeleted = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }

        public void CreateDefaultJars(int userId)
        {
            var jars = new List<Jar>
            {
                new Jar(userId, "NEC", "Necessities", 55, true, 0, 0, 0) { ColorHex = "#c8b6ff" },
                new Jar(userId, "FFA", "Financial Freedom", 10, false, 0, 0, 1) { ColorHex = "#a5d8ff" },
                new Jar(userId, "PLAY", "Play", 10, false, 0, 0, 2) { ColorHex = "#ff94c2" },
                new Jar(userId, "LTSS", "Long-Term Savings", 10, false, 0, 0, 3) { ColorHex = "#66a6ff" },
                new Jar(userId, "EDU", "Education", 10, false, 0, 0, 4) { ColorHex = "#7f00ff" },
                new Jar(userId, "GIVE", "Give", 5, false, 0, 0, 5) { ColorHex = "#ff00b8" }
            };

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                foreach (var jar in jars)
                {
                    const string sql = @"INSERT INTO Jars
                    (UserId, JarName, Description, Percentage, IsDefault, InitialAmount, Amount, Position, ColorHex)
                    VALUES (@UserId,@JarName,@Description,@Percentage,@IsDefault,@InitialAmount,@Amount,@Position,@ColorHex)";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", jar.UserId);
                        cmd.Parameters.AddWithValue("@JarName", jar.JarName);
                        cmd.Parameters.AddWithValue("@Description", jar.Description ?? "");
                        cmd.Parameters.AddWithValue("@Percentage", jar.Percentage);
                        cmd.Parameters.AddWithValue("@IsDefault", jar.IsDefault);
                        cmd.Parameters.AddWithValue("@InitialAmount", jar.InitialAmount);
                        cmd.Parameters.AddWithValue("@Amount", jar.Amount);
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
            (UserId, JarName, Description, Percentage, IsDefault, InitialAmount, Amount, Position, ColorHex, IsDeleted, CreatedAt)
            VALUES (@UserId,@JarName,@Description,@Percentage,@IsDefault,@InitialAmount,@Amount,@Position,@ColorHex,@IsDeleted,@CreatedAt)";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@UserId", UserId);
                p.AddWithValue("@JarName", JarName);
                p.AddWithValue("@Description", Description ?? "");
                p.AddWithValue("@Percentage", Percentage);
                p.AddWithValue("@IsDefault", false);
                p.AddWithValue("@InitialAmount", InitialAmount);
                p.AddWithValue("@Amount", Amount);
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
            InitialAmount=@InitialAmount, Amount=@Amount, Position=@Position, ColorHex=@ColorHex
            WHERE JarId=@JarId AND UserId=@UserId";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@JarName", JarName ?? "");
                p.AddWithValue("@Description", Description ?? "");
                p.AddWithValue("@InitialAmount", InitialAmount);
                p.AddWithValue("@Amount", Amount);
                p.AddWithValue("@Position", Position);
                p.AddWithValue("@ColorHex", ColorHex ?? "#cccccc");
                p.AddWithValue("@JarId", JarId);
                p.AddWithValue("@UserId", UserId);
            });
        }

        public void UpdateJarAmount()
        {
            const string sql = "UPDATE Jars SET Amount=@Amount WHERE JarId=@JarId AND UserId=@UserId";
            Db.Exec(sql, p =>
            {
                p.AddWithValue("@Amount", Amount);
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

                        decimal currentAmount = GetCurrentAmount(conn, tran);
                        if (currentAmount > 0 && JarId != defaultJar.JarId)
                        {
                            new JarTransaction().InsertTransfer(UserId, JarId, defaultJar.JarId, currentAmount, $"Transfer from Deleted Jar: {JarName}");
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

        private decimal GetCurrentAmount(SqlConnection conn, SqlTransaction tran)
        {
            const string sql = @"
            SELECT J.InitialAmount
                   + ISNULL(SUM(CASE WHEN T.TransactionType='Income'  THEN T.Amount ELSE 0 END), 0)
                   - ISNULL(SUM(CASE WHEN T.TransactionType='Expense' THEN T.Amount ELSE 0 END), 0)
            FROM Jars AS J
            LEFT JOIN JarTransactions AS T ON J.JarId = T.JarId AND J.UserId = T.UserId
            WHERE J.JarId = @JarId AND J.UserId = @UserId
            GROUP BY J.InitialAmount";
            using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@JarId", JarId);
                cmd.Parameters.AddWithValue("@UserId", UserId);
                var res = cmd.ExecuteScalar();
                return (res != null && res != DBNull.Value) ? Convert.ToDecimal(res) : 0m;
            }
        }

        public decimal GetCurrentBalance(int userId, int jarId)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                this.UserId = userId;
                this.JarId = jarId;
                return GetCurrentAmount(conn, null);
            }
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

        public void DeductAmount(int jarId, decimal amount)
        {
            const string sql = "UPDATE Jars SET Amount = Amount - @Amount WHERE JarID=@JarID";
            Db.Exec(sql, p =>
            {
                p.AddWithValue("@Amount", amount);
                p.AddWithValue("@JarID", jarId);
            });
        }

        public Jar GetDefaultJar(int userId)
        {
            const string sql = "SELECT TOP 1 * FROM Jars WHERE UserID=@UserID AND IsDefault=1";
            var list = Db.Query(sql,
                p => p.AddWithValue("@UserID", userId),
                MapJar);
            return list.Count == 0 ? null : list[0];
        }

        public List<Jar> GetJarsByUser(int userId, bool includeDeleted = false)
        {
            string sql = includeDeleted
                ? "SELECT * FROM Jars WHERE UserID=@UserID ORDER BY Position"
                : "SELECT * FROM Jars WHERE UserID=@UserID AND IsDeleted=0 ORDER BY Position";

            return Db.Query(sql,
                p => p.AddWithValue("@UserID", userId),
                MapJar);
        }

        public decimal GetTotalMoneyByUser(int userId)
        {
            var jars = GetJarsByUser(userId);
            decimal totalSaved = 0m;
            var txnManager = new JarTransaction();
            foreach (var jar in jars)
            {
                decimal netChange = txnManager.GetTransactionSum(userId, jar.JarId);
                totalSaved += jar.InitialAmount + netChange;
            }
            return totalSaved;
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
                InitialAmount = Convert.ToDecimal(reader["InitialAmount"]),
                Amount = Convert.ToDecimal(reader["Amount"]),
                Position = Convert.ToInt32(reader["Position"]),
                ColorHex = reader["ColorHex"]?.ToString() ?? "#cccccc",
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}
