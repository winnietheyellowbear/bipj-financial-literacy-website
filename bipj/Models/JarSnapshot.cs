using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace bipj.Models
{
    public class JarSnapshot
    {
        private string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        public int SnapshotId { get; set; }
        public int JarId { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public DateTime SnapshotDate { get; set; }
        public string PeriodType { get; set; } // "daily" or "monthly"

        /// Check if a snapshot already exists for the same Jar, date, and period.
        public bool CheckSnapshotExists()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM JarSnapshots 
                WHERE JarId = @JarId 
                  AND SnapshotDate = @SnapshotDate 
                  AND PeriodType = @PeriodType", conn))
            {
                cmd.Parameters.Add("@JarId", SqlDbType.Int).Value = JarId;
                cmd.Parameters.Add("@SnapshotDate", SqlDbType.Date).Value = SnapshotDate;
                cmd.Parameters.Add("@PeriodType", SqlDbType.VarChar, 10).Value = PeriodType;

                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        /// Insert or update a snapshot (upsert to prevent duplicates).
        public void UpsertSnapshot()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                MERGE JarSnapshots AS target
                USING (SELECT @JarId AS JarId, @SnapshotDate AS SnapshotDate, @PeriodType AS PeriodType) AS source
                ON target.JarId = source.JarId 
                   AND target.SnapshotDate = source.SnapshotDate
                   AND target.PeriodType = source.PeriodType
                WHEN MATCHED THEN
                    UPDATE SET Balance = @Balance
                WHEN NOT MATCHED THEN
                    INSERT (JarId, UserId, Balance, SnapshotDate, PeriodType)
                    VALUES (@JarId, @UserId, @Balance, @SnapshotDate, @PeriodType);", conn))
            {
                cmd.Parameters.Add("@JarId", SqlDbType.Int).Value = JarId;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                cmd.Parameters.Add("@Balance", SqlDbType.Decimal).Value = Balance;
                cmd.Parameters.Add("@SnapshotDate", SqlDbType.Date).Value = SnapshotDate;
                cmd.Parameters.Add("@PeriodType", SqlDbType.VarChar, 10).Value = PeriodType;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// Generate snapshots for all jars of a user (daily or monthly).
        public void GenerateSnapshots(int userId, string periodType)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT JarId, Amount 
                FROM Jars 
                WHERE UserId = @UserId", conn))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int jarId = Convert.ToInt32(reader["JarId"]);
                    decimal balance = Convert.ToDecimal(reader["Amount"]);

                    JarSnapshot snapshot = new JarSnapshot
                    {
                        JarId = jarId,
                        UserId = userId,
                        Balance = balance,
                        SnapshotDate = periodType == "daily" ? DateTime.Today : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                        PeriodType = periodType
                    };

                    snapshot.UpsertSnapshot();
                }
            }
        }

        /// Get all snapshots for a user and period (daily or monthly).
        public List<JarSnapshot> GetSnapshots(int userId, string periodType, DateTime fromDate, DateTime toDate)
        {
            List<JarSnapshot> snapshots = new List<JarSnapshot>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(@"
            SELECT SnapshotId, JarId, UserId, Balance, SnapshotDate, PeriodType
            FROM JarSnapshots
            WHERE UserId = @UserId AND PeriodType = @PeriodType
              AND SnapshotDate >= @FromDate AND SnapshotDate < @ToDate
            ORDER BY SnapshotDate ASC", conn))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@PeriodType", SqlDbType.VarChar, 10).Value = periodType;
                cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate;
                cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    snapshots.Add(new JarSnapshot
                    {
                        SnapshotId = Convert.ToInt32(reader["SnapshotId"]),
                        JarId = Convert.ToInt32(reader["JarId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Balance = Convert.ToDecimal(reader["Balance"]),
                        SnapshotDate = Convert.ToDateTime(reader["SnapshotDate"]),
                        PeriodType = reader["PeriodType"].ToString()
                    });
                }
            }

            return snapshots;
        }


        /// Get snapshot for a specific date (useful for graphing).
        public JarSnapshot GetSnapshotByDate(int jarId, string periodType, DateTime snapshotDate)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT TOP 1 SnapshotId, JarId, UserId, Balance, SnapshotDate, PeriodType
                FROM JarSnapshots
                WHERE JarId = @JarId AND PeriodType = @PeriodType AND SnapshotDate = @SnapshotDate", conn))
            {
                cmd.Parameters.Add("@JarId", SqlDbType.Int).Value = jarId;
                cmd.Parameters.Add("@PeriodType", SqlDbType.VarChar, 10).Value = periodType;
                cmd.Parameters.Add("@SnapshotDate", SqlDbType.Date).Value = snapshotDate;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new JarSnapshot
                    {
                        SnapshotId = Convert.ToInt32(reader["SnapshotId"]),
                        JarId = Convert.ToInt32(reader["JarId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Balance = Convert.ToDecimal(reader["Balance"]),
                        SnapshotDate = Convert.ToDateTime(reader["SnapshotDate"]),
                        PeriodType = reader["PeriodType"].ToString()
                    };
                }
            }
            return null;
        }

        /// Delete old snapshots (optional maintenance to save space).
        
        public void DeleteOldSnapshots(int monthsToKeep)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                DELETE FROM JarSnapshots
                WHERE SnapshotDate < DATEADD(MONTH, -@MonthsToKeep, GETDATE())", conn))
            {
                cmd.Parameters.Add("@MonthsToKeep", SqlDbType.Int).Value = monthsToKeep;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void BackfillSnapshots(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                // 1. Get earliest transaction date
                DateTime minDate;
                using (SqlCommand cmd = new SqlCommand("SELECT MIN(Date) FROM JarTransactions WHERE UserId = @UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    object result = cmd.ExecuteScalar();
                    minDate = result != DBNull.Value ? Convert.ToDateTime(result) : DateTime.Today;
                }

                DateTime? fromDate = minDate;
                DateTime? toDate = DateTime.Today;
                bipj.Data.SqlDate.Clamp(ref fromDate, ref toDate);

                // 2. Get all jars for this user
                List<int> jarIds = new List<int>();
                using (SqlCommand cmd = new SqlCommand("SELECT JarId FROM Jars WHERE UserId = @UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            jarIds.Add(reader.GetInt32(0));
                    }
                }

                foreach (int jarId in jarIds)
                {
                    // 3. Get all transactions for this jar once
                    Dictionary<DateTime, decimal> dailyChanges = new Dictionary<DateTime, decimal>();

                    using (SqlCommand cmd = new SqlCommand(@"
                SELECT Date, 
                       SUM(CASE WHEN TransactionType = 'Income' THEN Amount 
                                WHEN TransactionType = 'Expense' THEN -Amount ELSE 0 END) AS NetChange
                FROM JarTransactions
                WHERE UserId = @UserId AND JarId = @JarId
                GROUP BY Date", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@JarId", jarId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime date = Convert.ToDateTime(reader["Date"]);
                                decimal change = Convert.ToDecimal(reader["NetChange"]);
                                dailyChanges[date] = change;
                            }
                        }
                    }

                    decimal runningBalance = 0;
                    DateTime currentDate = fromDate.Value;
                    DateTime endDate = toDate.Value;

                    while (currentDate <= endDate)
                    {
                        if (dailyChanges.ContainsKey(currentDate))
                            runningBalance += dailyChanges[currentDate];

                        // Upsert daily snapshot
                        JarSnapshot snapshot = new JarSnapshot
                        {
                            JarId = jarId,
                            UserId = userId,
                            Balance = runningBalance,
                            SnapshotDate = currentDate,
                            PeriodType = "daily"
                        };
                        snapshot.UpsertSnapshot();

                        // Upsert monthly snapshot on last day of month
                        if (currentDate.Day == DateTime.DaysInMonth(currentDate.Year, currentDate.Month))
                        {
                            snapshot.SnapshotDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                            snapshot.PeriodType = "monthly";
                            snapshot.UpsertSnapshot();
                        }

                        currentDate = currentDate.AddDays(1);
                    }
                }
            }
        }
    }
}
