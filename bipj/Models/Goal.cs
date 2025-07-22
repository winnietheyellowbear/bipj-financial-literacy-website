using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using bipj.Data;

namespace bipj.Models
{
    public class Goal
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private int _GoalId;
        private int _UserId;
        private int? _JarId;
        private string _GoalName;
        private decimal _TargetAmount;
        private decimal _SavedAmount;
        private DateTime _Deadline;
        private DateTime _CreatedAt;

        public Goal() { }

        public Goal(int goalId, int userId, int? jarId, string goalName, decimal targetAmount, decimal savedAmount, DateTime deadline, DateTime createdAt)
        {
            _GoalId = goalId;
            _UserId = userId;
            _JarId = jarId;
            _GoalName = goalName;
            _TargetAmount = targetAmount;
            _SavedAmount = savedAmount;
            _Deadline = deadline;
            _CreatedAt = createdAt;
        }

        public Goal(int userId, int? jarId, string goalName, decimal targetAmount, DateTime deadline)
        {
            _UserId = userId;
            _JarId = jarId;
            _GoalName = goalName;
            _TargetAmount = targetAmount;
            _SavedAmount = 0;
            _Deadline = deadline;
            _CreatedAt = DateTime.Now;
        }

        public class GoalDashboard
        {
            public string GoalName { get; set; }
            public decimal TotalSaved { get; set; }
            public decimal TargetAmount { get; set; }
            public DateTime Deadline { get; set; }
            public decimal ProgressPercentage { get; set; }
        }

        public int GoalId { get { return _GoalId; } set { _GoalId = value; } }
        public int UserId { get { return _UserId; } set { _UserId = value; } }
        public int? JarId { get { return _JarId; } set { _JarId = value; } }
        public string GoalName { get { return _GoalName; } set { _GoalName = value; } }
        public decimal TargetAmount { get { return _TargetAmount; } set { _TargetAmount = value; } }
        public decimal SavedAmount { get { return _SavedAmount; } set { _SavedAmount = value; } }
        public DateTime Deadline { get { return _Deadline; } set { _Deadline = value; } }
        public DateTime CreatedAt { get { return _CreatedAt; } set { _CreatedAt = value; } }
        public string JarName { get; set; }

        public int InsertGoal()
        {
            const string sql = @"INSERT INTO Goals
(UserId, JarId, GoalName, TargetAmount, SavedAmount, Deadline, CreatedAt)
VALUES (@UserId, @JarId, @GoalName, @TargetAmount, @SavedAmount, @Deadline, @CreatedAt)";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@UserId", UserId);
                p.AddWithValue("@JarId", (object)JarId ?? DBNull.Value);
                p.AddWithValue("@GoalName", GoalName);
                p.AddWithValue("@TargetAmount", TargetAmount);
                p.AddWithValue("@SavedAmount", SavedAmount);
                p.AddWithValue("@Deadline", Deadline);
                p.AddWithValue("@CreatedAt", CreatedAt);
            });
        }

        public List<Goal> GetGoalsByUser(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            DateTime sqlMin = SqlDateTime.MinValue.Value;
            DateTime sqlMax = SqlDateTime.MaxValue.Value;
            if (fromDate.HasValue && fromDate.Value < sqlMin) fromDate = sqlMin;
            if (toDate.HasValue && toDate.Value > sqlMax) toDate = sqlMax;

            const string sql = @"
SELECT g.*, j.JarName AS JarName
FROM Goals g
LEFT JOIN Jars j ON g.JarId = j.JarId
WHERE g.UserId = @UserId
  AND g.CreatedAt >= @FromDate
  AND g.CreatedAt <  @ToDate
ORDER BY g.Deadline";

            return Db.Query(sql,
                p =>
                {
                    p.AddWithValue("@UserId", userId);
                    p.AddWithValue("@FromDate", (object)fromDate ?? DBNull.Value);
                    p.AddWithValue("@ToDate", (object)toDate ?? DBNull.Value);
                },
                r =>
                {
                    var goal = new Goal(
                        Convert.ToInt32(r["GoalId"]),
                        Convert.ToInt32(r["UserId"]),
                        r["JarId"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["JarId"]),
                        r["GoalName"].ToString(),
                        Convert.ToDecimal(r["TargetAmount"]),
                        Convert.ToDecimal(r["SavedAmount"]),
                        Convert.ToDateTime(r["Deadline"]),
                        Convert.ToDateTime(r["CreatedAt"])
                    );
                    goal.JarName = r["JarName"] == DBNull.Value ? null : r["JarName"].ToString();
                    return goal;
                });
        }

        public int UpdateGoal()
        {
            const string sql = @"UPDATE Goals
SET JarId=@JarId, GoalName=@GoalName, TargetAmount=@TargetAmount, Deadline=@Deadline
WHERE GoalId=@GoalId";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@JarId", (object)JarId ?? DBNull.Value);
                p.AddWithValue("@GoalName", GoalName);
                p.AddWithValue("@TargetAmount", TargetAmount);
                p.AddWithValue("@Deadline", Deadline);
                p.AddWithValue("@GoalId", GoalId);
            });
        }

        public int DeleteGoal()
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        decimal savedAmount = new GoalTransaction().GetTotalSavedAmount(GoalId, UserId);

                        int? defaultJarId = null;
                        const string getJarSql = "SELECT JarId FROM Jars WHERE UserId=@UserId AND IsDefault=1";
                        using (var getJarCmd = new SqlCommand(getJarSql, conn, trans))
                        {
                            getJarCmd.Parameters.AddWithValue("@UserId", UserId);
                            var jarResult = getJarCmd.ExecuteScalar();
                            if (jarResult != null && jarResult != DBNull.Value) defaultJarId = Convert.ToInt32(jarResult);
                        }

                        if (savedAmount > 0 && savedAmount < TargetAmount && JarId.HasValue)
                        {
                            const string insertTxn = @"INSERT INTO Transactions
(UserId, JarId, Name, Amount, Date, TransactionType, Category)
VALUES (@UserId, @JarId, @Name, @Amount, @Date, 'Income', 'Transfer In')";
                            using (var txnCmd = new SqlCommand(insertTxn, conn, trans))
                            {
                                txnCmd.Parameters.AddWithValue("@UserId", UserId);
                                txnCmd.Parameters.AddWithValue("@JarId", defaultJarId);
                                txnCmd.Parameters.AddWithValue("@Name", "Transferred from Deleted Goal: " + GoalName);
                                txnCmd.Parameters.AddWithValue("@Amount", savedAmount);
                                txnCmd.Parameters.AddWithValue("@Date", DateTime.Now);
                                txnCmd.ExecuteNonQuery();
                            }

                            const string updateJar = "UPDATE Jars SET Amount = Amount + @Amount WHERE JarId=@JarId";
                            using (var updateJarCmd = new SqlCommand(updateJar, conn, trans))
                            {
                                updateJarCmd.Parameters.AddWithValue("@Amount", savedAmount);
                                updateJarCmd.Parameters.AddWithValue("@JarId", defaultJarId);
                                updateJarCmd.ExecuteNonQuery();
                            }
                        }

                        const string deleteTxnsSql = "DELETE FROM GoalTransactions WHERE GoalId=@GoalId";
                        using (var deleteTxnsCmd = new SqlCommand(deleteTxnsSql, conn, trans))
                        {
                            deleteTxnsCmd.Parameters.AddWithValue("@GoalId", GoalId);
                            deleteTxnsCmd.ExecuteNonQuery();
                        }

                        const string deleteGoalSql = "DELETE FROM Goals WHERE GoalId=@GoalId";
                        using (var deleteGoalCmd = new SqlCommand(deleteGoalSql, conn, trans))
                        {
                            deleteGoalCmd.Parameters.AddWithValue("@GoalId", GoalId);
                            int result = deleteGoalCmd.ExecuteNonQuery();
                            trans.Commit();
                            return result;
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public void ReassignGoalsToJar(int oldJarId, int newJarId, int userId)
        {
            const string sql = @"UPDATE Goals SET JarId=@NewJarId WHERE JarId=@OldJarId AND UserId=@UserId";
            Db.Exec(sql, p =>
            {
                p.AddWithValue("@NewJarId", newJarId);
                p.AddWithValue("@OldJarId", oldJarId);
                p.AddWithValue("@UserId", userId);
            });
        }

        public int UpdateSavedAmount(decimal newAmount)
        {
            const string sql = "UPDATE Goals SET SavedAmount=@Amount WHERE GoalId=@GoalId";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@Amount", newAmount);
                p.AddWithValue("@GoalId", GoalId);
            });
        }

        public Goal GetGoalById(int goalId, int userId)
        {
            const string sql = "SELECT TOP 1 * FROM Goals WHERE GoalId=@GoalId AND UserId=@UserId";
            var list = Db.Query(sql,
                p =>
                {
                    p.AddWithValue("@GoalId", goalId);
                    p.AddWithValue("@UserId", userId);
                },
                r => new Goal
                {
                    GoalId = Convert.ToInt32(r["GoalId"]),
                    UserId = Convert.ToInt32(r["UserId"]),
                    JarId = r["JarId"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["JarId"]),
                    GoalName = r["GoalName"].ToString(),
                    TargetAmount = Convert.ToDecimal(r["TargetAmount"]),
                    Deadline = Convert.ToDateTime(r["Deadline"]),
                    SavedAmount = Convert.ToDecimal(r["SavedAmount"]),
                    CreatedAt = r["CreatedAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["CreatedAt"])
                });
            return list.Count == 0 ? null : list[0];
        }

        public int GetTotalGoalsCount(int userId)
        {
            const string sql = "SELECT COUNT(*) FROM Goals WHERE UserId=@UserId";
            return Db.Scalar(sql, p => p.AddWithValue("@UserId", userId), 0);
        }

        public decimal GetTotalSavedAmountByUser(int userId)
        {
            const string sql = "SELECT SUM(SavedAmount) FROM Goals WHERE UserId=@UserId";
            return Db.Scalar(sql, p => p.AddWithValue("@UserId", userId), 0m);
        }

        public decimal GetTotalTargetAmountByUser(int userId)
        {
            const string sql = "SELECT SUM(TargetAmount) FROM Goals WHERE UserId=@UserId";
            return Db.Scalar(sql, p => p.AddWithValue("@UserId", userId), 0m);
        }

        public GoalDashboard GetGoalDashboard(int goalId, int userId)
        {
            var goal = GetGoalById(goalId, userId);
            if (goal == null) return null;

            var totalSaved = new GoalTransaction().GetTotalSavedAmount(goal.GoalId, userId);
            return new GoalDashboard
            {
                GoalName = goal.GoalName,
                TotalSaved = totalSaved,
                TargetAmount = goal.TargetAmount,
                Deadline = goal.Deadline,
                ProgressPercentage = goal.TargetAmount == 0 ? 0 : Math.Min(100m, (totalSaved / goal.TargetAmount) * 100m)
            };
        }
    }
}
