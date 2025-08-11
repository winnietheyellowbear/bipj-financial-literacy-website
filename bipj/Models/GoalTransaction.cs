using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using bipj.Data;

namespace bipj.Models
{
    public class GoalTransaction
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private int _TransactionId;
        private int _UserId;
        private int _GoalId;
        private int? _SourceJarId;
        private decimal _Amount;
        private string _SourceType;
        private string _Name;
        private DateTime _Date;

        public GoalTransaction() { }

        public GoalTransaction(int transactionId, int userId, int goalId, int? sourceJarId,
                               decimal amount, string sourceType, string name, DateTime date)
        {
            _TransactionId = transactionId;
            _UserId = userId;
            _GoalId = goalId;
            _SourceJarId = sourceJarId;
            _Amount = amount;
            _SourceType = sourceType;
            _Name = name;
            _Date = date;
        }

        public GoalTransaction(int userId, int goalId, int? sourceJarId,
                               decimal amount, string sourceType, string name, DateTime date)
            : this(0, userId, goalId, sourceJarId, amount, sourceType, name, date) { }

        public int TransactionId { get { return _TransactionId; } set { _TransactionId = value; } }
        public int UserId { get { return _UserId; } set { _UserId = value; } }
        public int GoalId { get { return _GoalId; } set { _GoalId = value; } }
        public int? SourceJarId { get { return _SourceJarId; } set { _SourceJarId = value; } }
        public decimal Amount { get { return _Amount; } set { _Amount = value; } }
        public string SourceType { get { return _SourceType; } set { _SourceType = value; } }
        public string Name { get { return _Name; } set { _Name = value; } }
        public DateTime Date { get { return _Date; } set { _Date = value; } }

        public bool InsertGoalTransaction(int goalId, int userId, string name, decimal amount, DateTime date, string sourceType, int? sourceJarId, string goalName)
        {
            sourceType = (sourceType ?? "").ToLowerInvariant();
            if (sourceType != "jar" && sourceType != "topup")
                throw new ArgumentException("sourceType must be 'jar' or 'topup'.");

            // enforce bound jar if goal has one
            int? enforcedJarId = null;
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(
                "SELECT JarId FROM Goals WHERE GoalId=@GoalId AND UserId=@UserId", conn))
            {
                cmd.Parameters.AddWithValue("@GoalId", goalId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();
                var res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value) enforcedJarId = Convert.ToInt32(res);
            }
            if (enforcedJarId.HasValue && sourceType == "jar" &&
                (!sourceJarId.HasValue || sourceJarId.Value != enforcedJarId.Value))
                throw new InvalidOperationException("Source jar must match the jar assigned to the goal.");

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) record the goal-side transaction
                        const string insertGoalTxn = @"
                        INSERT INTO GoalTransactions
                            (UserId, GoalId, SourceJarId, Amount, SourceType, Name, [Date], CreatedAt)
                        VALUES
                            (@UserId, @GoalId, @SourceJarId, @Amount, @SourceType, @Name, @Date, GETDATE());";
                        using (var cmd = new SqlCommand(insertGoalTxn, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@GoalId", goalId);
                            cmd.Parameters.AddWithValue("@SourceJarId", (object)sourceJarId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Amount", amount);
                            cmd.Parameters.AddWithValue("@SourceType", sourceType);
                            cmd.Parameters.AddWithValue("@Name", name);
                            cmd.Parameters.AddWithValue("@Date", date);
                            cmd.ExecuteNonQuery();
                        }

                        // 2) update aggregate on Goals (column is SavedAmount, not Amount)
                        const string updateGoal = @"
                        UPDATE Goals
                        SET SavedAmount = ISNULL(SavedAmount, 0) + @Amount
                        WHERE GoalId = @GoalId AND UserId = @UserId;";
                        using (var cmd = new SqlCommand(updateGoal, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Amount", amount);
                            cmd.Parameters.AddWithValue("@GoalId", goalId);
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        // 3) if money comes from a jar, write a negative transfer in JarTransactions.
                        if (sourceType == "jar" && sourceJarId.HasValue)
                        {
                            const string insertJarTxn = @"
                            INSERT INTO JarTransactions
                                (UserId, JarId, Name, Amount, [Date], TransactionType, Category)
                            VALUES
                                (@UserId, @JarId, @Name, @NegAmount, @Date, 'Transfer', 'Goal Funding');";
                            using (var cmd = new SqlCommand(insertJarTxn, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.Parameters.AddWithValue("@JarId", sourceJarId.Value);
                                cmd.Parameters.AddWithValue("@Name", "Transferred to Goal: " + goalName);
                                cmd.Parameters.AddWithValue("@NegAmount", -amount); // outflow
                                cmd.Parameters.AddWithValue("@Date", date);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public decimal GetSumBySourceType(int userId, DateTime from, DateTime to, string sourceType)
        {
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"
            SELECT COALESCE(SUM(Amount), 0)
            FROM GoalTransactions
            WHERE UserId = @UserId
              AND Date  >= @From AND Date < @To
              AND LOWER(ISNULL(SourceType,'')) = @Src;", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@From", from);
                cmd.Parameters.AddWithValue("@To", to);
                cmd.Parameters.AddWithValue("@Src", sourceType.ToLowerInvariant());
                conn.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }


        public List<GoalTransaction> GetTransactionsByUser(int userID)
        {
            const string sql = "SELECT * FROM GoalTransactions WHERE UserID=@UserID ORDER BY Date DESC";
            return Db.Query(sql,
                p => p.AddWithValue("@UserID", userID),
                Map);
        }

        public List<GoalTransaction> GetTransactionsByGoal(int goalID, int userID)
        {
            const string sql = @"SELECT * FROM GoalTransactions WHERE GoalID=@GoalID AND UserID=@UserID ORDER BY Date DESC, TransactionID DESC";
            return Db.Query(sql,
                p =>
                {
                    p.AddWithValue("@GoalID", goalID);
                    p.AddWithValue("@UserID", userID);
                },
                Map);
        }

        public GoalTransaction GetTransactionById(int txnId, int userId)
        {
            const string sql = @"SELECT TOP 1 * FROM GoalTransactions WHERE TransactionID=@TxnID AND UserID=@UserID";
            var list = Db.Query(sql,
                p =>
                {
                    p.AddWithValue("@TxnID", txnId);
                    p.AddWithValue("@UserID", userId);
                },
                Map);
            return list.Count == 0 ? null : list[0];
        }

        public decimal GetTotalSavedAmount(int goalID, int userID)
        {
            const string sql = "SELECT ISNULL(SUM(Amount),0) FROM GoalTransactions WHERE GoalID=@GoalID AND UserID=@UserID";
            return Db.Scalar(sql,
                p =>
                {
                    p.AddWithValue("@GoalID", goalID);
                    p.AddWithValue("@UserID", userID);
                },
                0m);
        }

        public bool UpdateTransaction(int txnID, int goalID, int userID, string name, decimal amount, DateTime date)
        {
            const string sql = @"
UPDATE GoalTransactions
SET Name=@Name, Amount=@Amount, Date=@Date
WHERE TransactionID=@TxnID AND GoalID=@GoalID AND UserID=@UserID";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@Name", name);
                p.AddWithValue("@Amount", amount);
                p.AddWithValue("@Date", date);
                p.AddWithValue("@TxnID", txnID);
                p.AddWithValue("@GoalID", goalID);
                p.AddWithValue("@UserID", userID);
            }) > 0;
        }

        public int DeleteTransaction()
        {
            int result;
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        const string reverseSql = "UPDATE Goals SET SavedAmount = SavedAmount - @Amt WHERE GoalID=@GoalID";
                        using (var reverseCmd = new SqlCommand(reverseSql, conn, trans))
                        {
                            reverseCmd.Parameters.AddWithValue("@Amt", Amount);
                            reverseCmd.Parameters.AddWithValue("@GoalID", GoalId);
                            reverseCmd.ExecuteNonQuery();
                        }

                        if (SourceType == "jar" && SourceJarId.HasValue)
                        {
                            const string refundSql = "UPDATE Jars SET Amount = Amount + @Amt WHERE JarID=@JarID";
                            using (var refundCmd = new SqlCommand(refundSql, conn, trans))
                            {
                                refundCmd.Parameters.AddWithValue("@Amt", Amount);
                                refundCmd.Parameters.AddWithValue("@JarID", SourceJarId.Value);
                                refundCmd.ExecuteNonQuery();
                            }
                        }

                        const string deleteSql = "DELETE FROM GoalTransactions WHERE TransactionID=@TxnID";
                        using (var deleteCmd = new SqlCommand(deleteSql, conn, trans))
                        {
                            deleteCmd.Parameters.AddWithValue("@TxnID", TransactionId);
                            result = deleteCmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            return result;
        }

        public decimal GetTotalTopupsByUser(DateTime fromDate, DateTime toDate)
        {
            DateTime? f = fromDate;
            DateTime? t = toDate;
            SqlDate.Clamp(ref f, ref t);

            const string sql = @"
SELECT SUM(Amount)
FROM GoalTransactions
WHERE UserID=@UserID
  AND SourceType='topup'
  AND Date>=@FromDate AND Date<@ToDate";

            return Db.Scalar(sql,
                p =>
                {
                    p.AddWithValue("@UserID", UserId);
                    p.AddWithValue("@FromDate", f);
                    p.AddWithValue("@ToDate", t);
                },
                0m);
        }

        public decimal GetTotalGoalContributionsByUser(DateTime fromDate, DateTime toDate)
        {
            DateTime? f = fromDate;
            DateTime? t = toDate;
            SqlDate.Clamp(ref f, ref t);

            const string sql = @"
SELECT SUM(Amount)
FROM GoalTransactions
WHERE UserID=@UserID
  AND SourceType='jar'
  AND Date>=@FromDate AND Date<@ToDate";

            return Db.Scalar(sql,
                p =>
                {
                    p.AddWithValue("@UserID", UserId);
                    p.AddWithValue("@FromDate", f);
                    p.AddWithValue("@ToDate", t);
                },
                0m);
        }

        private static GoalTransaction Map(SqlDataReader r)
        {
            return new GoalTransaction(
                Convert.ToInt32(r["TransactionID"]),
                Convert.ToInt32(r["UserID"]),
                Convert.ToInt32(r["GoalID"]),
                r["SourceJarID"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["SourceJarID"]),
                Convert.ToDecimal(r["Amount"]),
                r["SourceType"].ToString(),
                r["Name"].ToString(),
                Convert.ToDateTime(r["Date"])
            );
        }
    }
}
