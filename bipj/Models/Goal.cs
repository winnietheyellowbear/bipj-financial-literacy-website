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
        private bool _IsArchived;


        public Goal() { }

        public Goal(int goalId, int userId, int? jarId, string goalName, decimal targetAmount, decimal savedAmount, DateTime deadline, DateTime createdAt, bool isArchived)
        {
            _GoalId = goalId;
            _UserId = userId;
            _JarId = jarId;
            _GoalName = goalName;
            _TargetAmount = targetAmount;
            _SavedAmount = savedAmount;
            _Deadline = deadline;
            _CreatedAt = createdAt;
            _IsArchived = isArchived;
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
        public bool IsArchived { get { return _IsArchived; } set { _IsArchived = value; } }
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

        public List<Goal> GetGoalsByUser(int userId, DateTime? fromDate = null, DateTime? toDate = null, bool includeArchived = false)
        {
            var from = fromDate ?? SqlDateTime.MinValue.Value;
            var to = toDate ?? SqlDateTime.MaxValue.Value;
            if (from < SqlDateTime.MinValue.Value) from = SqlDateTime.MinValue.Value;
            if (to > SqlDateTime.MaxValue.Value) to = SqlDateTime.MaxValue.Value;

            const string sql = @"
            SELECT g.*, j.JarName
            FROM Goals g
            LEFT JOIN Jars j ON g.JarId = j.JarId
            WHERE g.UserId = @UserId
              AND g.CreatedAt >= @FromDate
              AND g.CreatedAt <  @ToDate
              AND (@IncludeArchived = 1 OR g.IsArchived = 0)
            ORDER BY g.Deadline;";

            return Db.Query(sql,
                p =>
                {
                    p.AddWithValue("@UserId", userId);
                    p.AddWithValue("@FromDate", from);
                    p.AddWithValue("@ToDate", to);
                    p.AddWithValue("@IncludeArchived", includeArchived ? 1 : 0);
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
                        Convert.ToDateTime(r["CreatedAt"]),
                        r["IsArchived"] != DBNull.Value && Convert.ToBoolean(r["IsArchived"])
                    );
                    goal.IsArchived = r["IsArchived"] != DBNull.Value && Convert.ToBoolean(r["IsArchived"]);
                    goal.JarName = r["JarName"] == DBNull.Value ? null : r["JarName"].ToString();
                    return goal;
                });
        }

        public int ArchiveGoal(int userId, int goalId)
        {
            const string sql = @"UPDATE Goals SET IsArchived = 1 WHERE GoalId=@G AND UserId=@U";
            return Db.Exec(sql, p => { p.AddWithValue("@G", goalId); p.AddWithValue("@U", userId); });
        }

        public int UnarchiveGoal(int userId, int goalId)
        {
            const string sql = @"UPDATE Goals SET IsArchived = 0 WHERE GoalId=@G AND UserId=@U";
            return Db.Exec(sql, p => { p.AddWithValue("@G", goalId); p.AddWithValue("@U", userId); });
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
                        var goal = GetGoalById(GoalId, UserId);
                        if (goal == null) return 0;

                        bool isCompleted = goal.SavedAmount >= goal.TargetAmount;
                        decimal savedAmount = goal.SavedAmount;

                        if (isCompleted)
                        {
                            // Expense for completed goals
                            int reportingJarId = new Jar().GetOrCreateReportingJarId(UserId, conn, trans);
                            decimal expenseAmount = goal.TargetAmount;

                            using (var expenseCmd = new SqlCommand(@"
                        INSERT INTO JarTransactions
                            (UserId, JarId, Name, Amount, [Date], TransactionType, Category)
                        VALUES
                            (@U, @J, @N, @A, @D, 'Expense', 'Goal Purchase')", conn, trans))
                            {
                                expenseCmd.Parameters.AddWithValue("@U", UserId);
                                expenseCmd.Parameters.AddWithValue("@J", reportingJarId);
                                expenseCmd.Parameters.AddWithValue("@N", "Purchase from completed goal: " + goal.GoalName);
                                expenseCmd.Parameters.AddWithValue("@A", -expenseAmount);
                                expenseCmd.Parameters.AddWithValue("@D", DateTime.Now);
                                expenseCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Refund for uncompleted goals
                            if (savedAmount > 0m)
                            {
                                int refundJarId;
                                using (var cmd = new SqlCommand(@"
                            SELECT TOP 1 JarId
                            FROM Jars
                            WHERE UserId=@U
                            ORDER BY IsDefault DESC, JarId ASC;", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@U", UserId);
                                    refundJarId = Convert.ToInt32(cmd.ExecuteScalar());
                                }

                                using (var txnCmd = new SqlCommand(@"
                            INSERT INTO JarTransactions
                              (UserId, JarId, Name, Amount, Date, TransactionType, Category)
                            VALUES
                              (@U, @J, @N, @A, @D, 'Income', 'Uncompleted goals')", conn, trans))
                                {
                                    txnCmd.Parameters.AddWithValue("@U", UserId);
                                    txnCmd.Parameters.AddWithValue("@J", refundJarId);
                                    txnCmd.Parameters.AddWithValue("@N", "Refund from deleted goal: " + goal.GoalName);
                                    txnCmd.Parameters.AddWithValue("@A", savedAmount);
                                    txnCmd.Parameters.AddWithValue("@D", DateTime.Now);
                                    txnCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // Archive all goals (both completed and uncompleted)
                        using (var archiveGoal = new SqlCommand(
                            "UPDATE Goals SET IsArchived = 1 WHERE GoalId=@G AND UserId=@U", conn, trans))
                        {
                            archiveGoal.Parameters.AddWithValue("@G", GoalId);
                            archiveGoal.Parameters.AddWithValue("@U", UserId);
                            archiveGoal.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return 1;
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
                    CreatedAt = r["CreatedAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["CreatedAt"]),
                    IsArchived = r["IsArchived"] != DBNull.Value && Convert.ToBoolean(r["IsArchived"])
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
