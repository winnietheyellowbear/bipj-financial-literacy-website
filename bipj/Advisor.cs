using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace bipj
{
    public class Advisor
    {
        private static string ConnStr => ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        public int AdvisorId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; }

        [MaxLength(50, ErrorMessage = "Specialty cannot exceed 50 characters")]
        public string Specialty1 { get; set; }

        [MaxLength(50, ErrorMessage = "Specialty cannot exceed 50 characters")]
        public string Specialty2 { get; set; }

        [MaxLength(50, ErrorMessage = "Specialty cannot exceed 50 characters")]
        public string Specialty3 { get; set; }

        [Required(ErrorMessage = "Bio is required")]
        [MaxLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        public string Bio { get; set; }

        public string PhotoPath { get; set; }

        [NotMapped]
        public HttpPostedFileBase PhotoFile { get; set; }

        public byte Status { get; set; } = 0; // 0=Pending, 1=Approved, 2=Rejected
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public Advisor() { }

        public Advisor(int advisorId, string name, string email, string category,
                      string spec1, string spec2, string spec3,
                      string bio, string photoPath, byte status,
                      DateTime createdAt, DateTime updatedAt)
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
            UpdatedAt = updatedAt;
        }

        /// <summary>
        /// Inserts this advisor as a new record and returns its new ID.
        /// </summary>
        public int Insert()
        {
            try
            {
                if (PhotoFile != null && PhotoFile.ContentLength > 0)
                {
                    PhotoPath = SavePhoto(PhotoFile);
                }

                const string sql = @"
                INSERT INTO Advisor
                 (Name, Email, Category,
                  Specialty1, Specialty2, Specialty3,
                  Bio, PhotoPath, Status, CreatedAt, UpdatedAt)
                 VALUES
                 (@Name, @Email, @Category,
                  @S1, @S2, @S3,
                  @Bio, @Photo, @Status, @Created, @Updated);
                SELECT SCOPE_IDENTITY();";

                using (var conn = new SqlConnection(ConnStr))
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
                    cmd.Parameters.AddWithValue("@Updated", UpdatedAt);

                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                // Log error here
                throw new Exception("Error inserting advisor: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing advisor record.
        /// </summary>
        public int Update()
        {
            try
            {
                if (PhotoFile != null && PhotoFile.ContentLength > 0)
                {
                    // Delete old photo if exists
                    if (!string.IsNullOrEmpty(PhotoPath))
                    {
                        DeletePhoto(PhotoPath);
                    }
                    PhotoPath = SavePhoto(PhotoFile);
                }

                const string sql = @"
                UPDATE Advisor
                   SET Name = @Name,
                       Email = @Email,
                       Category = @Category,
                       Specialty1 = @S1,
                       Specialty2 = @S2,
                       Specialty3 = @S3,
                       Bio = @Bio,
                       PhotoPath = @Photo,
                       Status = @Status,
                       UpdatedAt = @Updated
                 WHERE AdvisorId = @Id";

                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", AdvisorId);
                    cmd.Parameters.AddWithValue("@Name", Name);
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@Category", Category);
                    cmd.Parameters.AddWithValue("@S1", (object)Specialty1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@S2", (object)Specialty2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@S3", (object)Specialty3 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Bio", Bio);
                    cmd.Parameters.AddWithValue("@Photo", (object)PhotoPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", Status);
                    cmd.Parameters.AddWithValue("@Updated", DateTime.Now);

                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log error here
                throw new Exception("Error updating advisor: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes an advisor record.
        /// </summary>
        public int Delete()
        {
            try
            {
                // Delete photo if exists
                if (!string.IsNullOrEmpty(PhotoPath))
                {
                    DeletePhoto(PhotoPath);
                }

                const string sql = "DELETE FROM Advisor WHERE AdvisorId = @Id";

                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", AdvisorId);

                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log error here
                throw new Exception("Error deleting advisor: " + ex.Message);
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
                   Bio, PhotoPath, Status, CreatedAt, UpdatedAt
              FROM Advisor
             WHERE Status = @Status
            ORDER BY CreatedAt DESC;";

            using (var conn = new SqlConnection(ConnStr))
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
                            dr.GetDateTime(10),
                            dr.GetDateTime(11)
                        ));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Retrieves all advisors.
        /// </summary>
        public static List<Advisor> GetAll()
        {
            var list = new List<Advisor>();
            const string sql = @"
            SELECT AdvisorId, Name, Email, Category,
                   Specialty1, Specialty2, Specialty3,
                   Bio, PhotoPath, Status, CreatedAt, UpdatedAt
              FROM Advisor
            ORDER BY CreatedAt DESC;";

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
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
                            dr.GetDateTime(10),
                            dr.GetDateTime(11)
                        ));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Updates the status (0=Pending, 1=Approved, 2=Rejected) of an advisor by ID.
        /// </summary>
        public static bool UpdateStatus(int advisorId, byte newStatus)
        {
            const string sql = @"
            UPDATE Advisor
               SET Status = @Status,
                   UpdatedAt = @Updated
             WHERE AdvisorId = @Id;";

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Status", newStatus);
                cmd.Parameters.AddWithValue("@Id", advisorId);
                cmd.Parameters.AddWithValue("@Updated", DateTime.Now);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
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
                   Bio, PhotoPath, Status, CreatedAt, UpdatedAt
              FROM Advisor
             WHERE AdvisorId = @Id;";

            using (var conn = new SqlConnection(ConnStr))
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
                        dr.GetDateTime(10),
                        dr.GetDateTime(11)
                    );
                }
            }
        }

        /// <summary>
        /// Saves the uploaded photo to the server and returns the path.
        /// </summary>
        private string SavePhoto(HttpPostedFileBase photoFile)
        {
            try
            {
                var uploadsFolder = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Uploads/Advisors"));
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                photoFile.SaveAs(filePath);

                return $"/Content/Uploads/Advisors/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving photo: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes the photo file from the server.
        /// </summary>
        private void DeletePhoto(string photoPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(photoPath))
                {
                    var filePath = HttpContext.Current.Server.MapPath(photoPath);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting photo: " + ex.Message);
            }
        }
    }
}