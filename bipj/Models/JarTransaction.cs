using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using bipj.Data;

namespace bipj.Models
{
    public enum TxnType { Income, Expense, Transfer }

    public class JarTransaction
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private int _transactionId;
        private int _userId;
        private int _jarId;
        private string _name;
        private decimal _amount;
        private DateTime _date;
        private TxnType _transactionType;
        private string _category;

        public JarTransaction() { }

        public JarTransaction(int transactionId, int userId, int jarId, string name, decimal amount, DateTime date, TxnType transactionType, string category = "")
        {
            _transactionId = transactionId;
            _userId = userId;
            _jarId = jarId;
            _name = name;
            _amount = amount;
            _date = date;
            _transactionType = transactionType;
            _category = category ?? "";
        }

        public int TransactionId { get => _transactionId; set => _transactionId = value; }
        public int UserId { get => _userId; set => _userId = value; }
        public int JarId { get => _jarId; set => _jarId = value; }
        public string Name { get => _name; set => _name = value; }
        public decimal Amount { get => _amount; set => _amount = value; }
        public DateTime Date { get => _date; set => _date = value; }
        public TxnType TransactionType { get => _transactionType; set => _transactionType = value; }
        public string Category { get => _category; set => _category = value ?? ""; }

        public int InsertTransaction()
        {
            const string sql = @"
INSERT INTO JarTransactions
(UserID, JarID, Name, Amount, Date, TransactionType, Category)
VALUES (@UserID, @JarID, @Name, @Amount, @Date, @TransactionType, @Category)";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@UserID", UserId);
                p.AddWithValue("@JarID", JarId);
                p.AddWithValue("@Name", Name ?? "");
                p.AddWithValue("@Amount", Amount);
                p.AddWithValue("@Date", Date);
                p.AddWithValue("@TransactionType", TransactionType.ToString());
                p.AddWithValue("@Category", Category ?? "");
            });
        }

        public void InsertTransfer(int userId, int fromJarId, int toJarId, decimal amount, string desc)
        {
            new JarTransaction(0, userId, fromJarId, desc, -amount, DateTime.Now, TxnType.Transfer, "Transfer").InsertTransaction();
            new JarTransaction(0, userId, toJarId, desc, amount, DateTime.Now, TxnType.Transfer, "Transfer").InsertTransaction();
        }

        public int UpdateTransaction()
        {
            const string sql = @"
UPDATE JarTransactions
SET Name=@Name, Amount=@Amount, Date=@Date
WHERE TransactionID=@TransactionID AND UserID=@UserID";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@Name", Name);
                p.AddWithValue("@Amount", Amount);
                p.AddWithValue("@Date", Date);
                p.AddWithValue("@TransactionID", TransactionId);
                p.AddWithValue("@UserID", UserId);
            });
        }

        public int DeleteTransaction()
        {
            const string sql = "DELETE FROM JarTransactions WHERE TransactionID=@TransactionID AND UserID=@UserID";
            return Db.Exec(sql, p =>
            {
                p.AddWithValue("@TransactionID", TransactionId);
                p.AddWithValue("@UserID", UserId);
            });
        }

        public List<JarTransaction> GetTransactionsByJar(int userID, int jarID)
        {
            const string sql = @"
SELECT * FROM JarTransactions
WHERE UserID=@UserID AND JarID=@JarID
ORDER BY Date DESC, TransactionID DESC";
            return Db.Query(sql,
                p =>
                {
                    p.AddWithValue("@UserID", userID);
                    p.AddWithValue("@JarID", jarID);
                },
                Map);
        }

        public JarTransaction GetTransactionById(int txnId, int userId)
        {
            const string sql = @"
SELECT TOP 1 * FROM JarTransactions
WHERE TransactionID=@TxnID AND UserID=@UserID";
            var list = Db.Query(sql,
                p =>
                {
                    p.AddWithValue("@TxnID", txnId);
                    p.AddWithValue("@UserID", userId);
                },
                Map);
            return list.Count == 0 ? null : list[0];
        }

        private static JarTransaction Map(SqlDataReader rdr)
        {
            return new JarTransaction(
                Convert.ToInt32(rdr["TransactionID"]),
                Convert.ToInt32(rdr["UserID"]),
                Convert.ToInt32(rdr["JarID"]),
                rdr["Name"]?.ToString(),
                Convert.ToDecimal(rdr["Amount"]),
                Convert.ToDateTime(rdr["Date"]),
                (TxnType)Enum.Parse(typeof(TxnType), rdr["TransactionType"].ToString(), true),
                rdr["Category"]?.ToString() ?? ""
            );
        }

        public decimal GetTransactionSum(int userId, int jarId, DateTime? fromDate = null, DateTime? toDate = null, bool includeTransfers = true)
        {
            SqlDate.Clamp(ref fromDate, ref toDate);

            var sql = new StringBuilder(@"
SELECT SUM(
    CASE TransactionType
        WHEN 'Income'  THEN Amount
        WHEN 'Expense' THEN -Amount
        WHEN 'Transfer' THEN Amount
    END)
FROM JarTransactions
WHERE UserID=@UserID AND JarID=@JarID
  AND (@incl = 1 OR TransactionType <> 'Transfer')");

            if (fromDate.HasValue) sql.Append(" AND Date >= @From");
            if (toDate.HasValue) sql.Append(" AND Date <  @To");

            return Db.Scalar(sql.ToString(),
                p =>
                {
                    p.AddWithValue("@UserID", userId);
                    p.AddWithValue("@JarID", jarId);
                    p.AddWithValue("@incl", includeTransfers ? 1 : 0);
                    if (fromDate.HasValue) p.AddWithValue("@From", fromDate.Value);
                    if (toDate.HasValue) p.AddWithValue("@To", toDate.Value);
                },
                0m);
        }

        public decimal GetTransactionSumByType(int userId, int jarId, string txnType, DateTime? fromDate = null, DateTime? toDate = null, bool includeTransfers = true)
        {
            SqlDate.Clamp(ref fromDate, ref toDate);

            var sql = new StringBuilder(@"
SELECT SUM(
    CASE @TxnType
        WHEN 'Income'  THEN Amount
        WHEN 'Expense' THEN -Amount
    END)
FROM JarTransactions
WHERE UserID=@UserID AND JarID=@JarID
  AND TransactionType=@TxnType
  AND (@incl = 1 OR TransactionType <> 'Transfer')");

            if (fromDate.HasValue) sql.Append(" AND Date >= @From");
            if (toDate.HasValue) sql.Append(" AND Date <  @To");

            return Db.Scalar(sql.ToString(),
                p =>
                {
                    p.AddWithValue("@UserID", userId);
                    p.AddWithValue("@JarID", jarId);
                    p.AddWithValue("@TxnType", txnType);
                    p.AddWithValue("@incl", includeTransfers ? 1 : 0);
                    if (fromDate.HasValue) p.AddWithValue("@From", fromDate.Value);
                    if (toDate.HasValue) p.AddWithValue("@To", toDate.Value);
                },
                0m);
        }

        private static void ClampToSqlRange(ref DateTime? from, ref DateTime? to)
        {
            DateTime sqlMin = SqlDateTime.MinValue.Value;
            DateTime sqlMax = SqlDateTime.MaxValue.Value;
            if (from.HasValue && from.Value < sqlMin) from = sqlMin;
            if (to.HasValue && to.Value > sqlMax) to = sqlMax;
        }

        public class DashboardDetail
        {
            public string JarName { get; set; }
            public decimal TotalIncome { get; set; }
            public decimal TotalExpenses { get; set; }
            public decimal Balance { get; set; }
        }
    }
}
