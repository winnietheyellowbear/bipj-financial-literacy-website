using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace bipj.Models
{
    public class Booking
    {
        private static readonly string ConnStr =
            ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int AdvisorId { get; set; }
        public string SessionType { get; set; }
        public DateTime BookingDateTime { get; set; }
        public string Focus { get; set; }
        public byte Status { get; set; } = 0;    // 0=Pending,1=Confirmed…
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Inserts a new booking and returns its new BookingId.
        /// </summary>
        public int Insert()
        {
            const string sql = @"
                INSERT INTO dbo.Booking
                  (UserId, AdvisorId, SessionType, BookingDateTime, Focus, Status, CreatedAt, UpdatedAt)
                VALUES
                  (@User, @Advisor, @Type, @DT, @Focus, @Status, @Created, @Updated);
                SELECT SCOPE_IDENTITY();";

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@User", UserId);
                cmd.Parameters.AddWithValue("@Advisor", AdvisorId);
                cmd.Parameters.AddWithValue("@Type", SessionType);
                cmd.Parameters.AddWithValue("@DT", BookingDateTime);
                cmd.Parameters.AddWithValue("@Focus", (object)Focus ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", Status);
                cmd.Parameters.AddWithValue("@Created", CreatedAt);
                cmd.Parameters.AddWithValue("@Updated", UpdatedAt);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Retrieves all bookings for a given advisor on a specific date.
        /// </summary>
        public static List<DateTime> GetBookedSlots(int advisorId, DateTime date)
        {
            var slots = new List<DateTime>();
            const string sql = @"
                SELECT BookingDateTime
                FROM dbo.Booking
                WHERE AdvisorId = @A
                  AND CAST(BookingDateTime AS DATE) = @D;";

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@A", advisorId);
                cmd.Parameters.AddWithValue("@D", date.Date);
                conn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        slots.Add(dr.GetDateTime(0));
                }
            }
            return slots;
        }

        // You can add Update(), Delete(), GetById(), etc. as needed
    }
}
