using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace bipj
{
    public class Advisor
    {
        private static string ConnStr =>
            ConfigurationManager
                .ConnectionStrings["FinLitDB"]
                .ConnectionString;

        public int AdvisorId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Category { get; set; }
        public string Specialty1 { get; set; }
        public string Specialty2 { get; set; }
        public string Specialty3 { get; set; }
        public string Bio { get; set; }
        public string PhotoPath { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public Advisor() { }

        public Advisor(int advisorId, string name, string email, string category,
                       string spec1, string spec2, string spec3,
                       string bio, string photoPath, byte status, DateTime createdAt)
        {
            AdvisorId = advisorId;
            Name = name;
            Email = email;
            Category = category;
            Specialty1 = spec1;
            Specialty2 = spec2;
            Specialty3 = spec3;
            Bio = bio;
            PhotoPath = photoPath;
            Status = status;
            CreatedAt = createdAt;
        }

        /// <summary>
        /// Inserts this advisor as a new "pending" record and returns its new ID.
        /// </summary>
        public int InsertAdvisor()
        {
            const string sql = @"
INSERT INTO Advisor
 (Name,Email,Category,
  Specialty1,Specialty2,Specialty3,
  Bio,PhotoPath,Status,CreatedAt)
 VALUES
 (@Name,@Email,@Category,
  @S1,@S2,@S3,
  @Bio,@Photo,@Status,@Created);
SELECT SCOPE_IDENTITY();";

            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", Name);
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@Category", Category);
                    cmd.Parameters.AddWithValue("@S1", (object)Specialty1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@S2", (object)Specialty2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@S3", (object)Specialty3 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Bio", Bio);
                    cmd.Parameters.AddWithValue("@Photo", (object)PhotoPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", Status);
                    cmd.Parameters.AddWithValue("@Created", CreatedAt);

                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Retrieves all advisors with the given status.
        /// </summary>
        public static List<Advisor> GetByStatus(byte status)
        {
            var list = new List<Advisor>();
            const string sql = @"
SELECT AdvisorId, Name, Email, Category,
       Specialty1, Specialty2, Specialty3,
       Bio, PhotoPath, Status, CreatedAt
  FROM Advisor
 WHERE Status = @Status
ORDER BY CreatedAt DESC;";

            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    conn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new Advisor(
                                dr.GetInt32(0),
                                dr.GetString(1),
                                dr.GetString(2),
                                dr.GetString(3),
                                dr.IsDBNull(4) ? null : dr.GetString(4),
                                dr.IsDBNull(5) ? null : dr.GetString(5),
                                dr.IsDBNull(6) ? null : dr.GetString(6),
                                dr.GetString(7),
                                dr.IsDBNull(8) ? null : dr.GetString(8),
                                dr.GetByte(9),
                                dr.GetDateTime(10)
                            ));
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Updates the status (1=approved,2=rejected) of an advisor by ID.
        /// </summary>
        public static bool UpdateStatus(int advisorId, byte newStatus)
        {
            const string sql = @"
UPDATE Advisor
   SET Status = @Status
 WHERE AdvisorId = @Id;";

            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@Id", advisorId);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Fetches a single advisor by its ID.
        /// </summary>
        public static Advisor GetById(int advisorId)
        {
            const string sql = @"
SELECT AdvisorId, Name, Email, Category,
       Specialty1, Specialty2, Specialty3,
       Bio, PhotoPath, Status, CreatedAt
  FROM Advisor
 WHERE AdvisorId = @Id;";

            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", advisorId);
                    conn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read()) return null;
                        return new Advisor(
                            dr.GetInt32(0),
                            dr.GetString(1),
                            dr.GetString(2),
                            dr.GetString(3),
                            dr.IsDBNull(4) ? null : dr.GetString(4),
                            dr.IsDBNull(5) ? null : dr.GetString(5),
                            dr.IsDBNull(6) ? null : dr.GetString(6),
                            dr.GetString(7),
                            dr.IsDBNull(8) ? null : dr.GetString(8),
                            dr.GetByte(9),
                            dr.GetDateTime(10)
                        );
                    }
                }
            }
        }
    }
}
