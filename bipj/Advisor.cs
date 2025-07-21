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

        // These will now be calculated dynamically from AdvisorReview table
        public decimal Rating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;

        public byte Status { get; set; } = 0; // 0=Pending, 1=Approved, 2=Rejected
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Removed RatingSum as it's now calculated dynamically

        public Advisor() { }

        public Advisor(int advisorId, string name, string email, string category,
                      string spec1, string spec2, string spec3,
                      string bio, string photoPath, byte status,
                      DateTime createdAt, DateTime updatedAt,
                      decimal rating, int ratingCount)
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
            Rating = rating;
            RatingCount = ratingCount;
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
        /// DEPRECATED: Use AdvisorReview.cs to add reviews instead
        /// This method is kept for backward compatibility but should not be used
        /// </summary>
        [Obsolete("Use AdvisorReview.AddReview() instead")]
        public static bool AddRating(int advisorId, int stars)
        {
            // This method should no longer be used - redirect to AdvisorReview
            throw new NotSupportedException("Use AdvisorReview.AddReview() method instead");
        }

        /// <summary>
        /// Deletes an advisor record and all associated reviews.
        /// </summary>
        public int Delete()
        {
            try
            {
                if (!string.IsNullOrEmpty(PhotoPath))
                {
                    DeletePhoto(PhotoPath);
                }

                // Delete associated reviews first (if you want to cascade delete)
                const string deleteReviewsSql = @"
                    DELETE FROM AdvisorReview 
                    WHERE BookingId IN (
                        SELECT BookingId FROM Booking WHERE AdvisorId = @Id
                    )";

                const string deleteAdvisorSql = "DELETE FROM Advisor WHERE AdvisorId = @Id";

                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Delete reviews first
                            using (var cmd1 = new SqlCommand(deleteReviewsSql, conn, transaction))
                            {
                                cmd1.Parameters.AddWithValue("@Id", AdvisorId);
                                cmd1.ExecuteNonQuery();
                            }

                            // Delete advisor
                            using (var cmd2 = new SqlCommand(deleteAdvisorSql, conn, transaction))
                            {
                                cmd2.Parameters.AddWithValue("@Id", AdvisorId);
                                var result = cmd2.ExecuteNonQuery();
                                transaction.Commit();
                                return result;
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error here
                throw new Exception("Error deleting advisor: " + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all advisors with the given status, including calculated ratings from AdvisorReview.
        /// </summary>
        public static List<Advisor> GetByStatus(byte status)
        {
            var list = new List<Advisor>();
            const string sql = @"
            SELECT 
                a.AdvisorId, a.Name, a.Email, a.Category,
                a.Specialty1, a.Specialty2, a.Specialty3,
                a.Bio, a.PhotoPath, a.Status, a.CreatedAt, a.UpdatedAt,
                COALESCE(AVG(CAST(r.Rating AS DECIMAL(5,2))), 0) AS AvgRating,
                COUNT(r.Rating) AS ReviewCount
            FROM Advisor a
            LEFT JOIN Booking b ON a.AdvisorId = b.AdvisorId
            LEFT JOIN AdvisorReview r ON b.BookingId = r.BookingId
            WHERE a.Status = @Status
            GROUP BY a.AdvisorId, a.Name, a.Email, a.Category,
                     a.Specialty1, a.Specialty2, a.Specialty3,
                     a.Bio, a.PhotoPath, a.Status, a.CreatedAt, a.UpdatedAt
            ORDER BY a.CreatedAt DESC;";

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
                            dr.GetInt32(0),  // AdvisorId
                            dr.GetString(1), // Name
                            dr.GetString(2), // Email
                            dr.GetString(3), // Category
                            dr.IsDBNull(4) ? null : dr.GetString(4), // Specialty1
                            dr.IsDBNull(5) ? null : dr.GetString(5), // Specialty2
                            dr.IsDBNull(6) ? null : dr.GetString(6), // Specialty3
                            dr.GetString(7), // Bio
                            dr.IsDBNull(8) ? null : dr.GetString(8), // PhotoPath
                            dr.GetByte(9),   // Status
                            dr.GetDateTime(10), // CreatedAt
                            dr.GetDateTime(11), // UpdatedAt
                            dr.GetDecimal(12),  // AvgRating (calculated)
                            dr.GetInt32(13)     // ReviewCount (calculated)
                        ));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Retrieves all advisors with calculated ratings from AdvisorReview.
        /// </summary>
        public static List<Advisor> GetAll()
        {
            var list = new List<Advisor>();
            const string sql = @"
            SELECT 
                a.AdvisorId, a.Name, a.Email, a.Category,
                a.Specialty1, a.Specialty2, a.Specialty3,
                a.Bio, a.PhotoPath, a.Status, a.CreatedAt, a.UpdatedAt,
                COALESCE(AVG(CAST(r.Rating AS DECIMAL(5,2))), 0) AS AvgRating,
                COUNT(r.Rating) AS ReviewCount
            FROM Advisor a
            LEFT JOIN Booking b ON a.AdvisorId = b.AdvisorId
            LEFT JOIN AdvisorReview r ON b.BookingId = r.BookingId
            GROUP BY a.AdvisorId, a.Name, a.Email, a.Category,
                     a.Specialty1, a.Specialty2, a.Specialty3,
                     a.Bio, a.PhotoPath, a.Status, a.CreatedAt, a.UpdatedAt
            ORDER BY a.CreatedAt DESC;";

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new Advisor(
                            dr.GetInt32(0),  // AdvisorId
                            dr.GetString(1), // Name
                            dr.GetString(2), // Email
                            dr.GetString(3), // Category
                            dr.IsDBNull(4) ? null : dr.GetString(4), // Specialty1
                            dr.IsDBNull(5) ? null : dr.GetString(5), // Specialty2
                            dr.IsDBNull(6) ? null : dr.GetString(6), // Specialty3
                            dr.GetString(7), // Bio
                            dr.IsDBNull(8) ? null : dr.GetString(8), // PhotoPath
                            dr.GetByte(9),   // Status
                            dr.GetDateTime(10), // CreatedAt
                            dr.GetDateTime(11), // UpdatedAt
                            dr.GetDecimal(12),  // AvgRating (calculated)
                            dr.GetInt32(13)     // ReviewCount (calculated)
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
        /// Fetches a single advisor by its ID with calculated rating from AdvisorReview.
        /// </summary>
        public static Advisor GetById(int advisorId)
        {
            const string sql = @"
            SELECT 
                a.AdvisorId, a.Name, a.Email, a.Category,
                a.Specialty1, a.Specialty2, a.Specialty3,
                a.Bio, a.PhotoPath, a.Status, a.CreatedAt, a.UpdatedAt,
                COALESCE(AVG(CAST(r.Rating AS DECIMAL(5,2))), 0) AS AvgRating,
                COUNT(r.Rating) AS ReviewCount
            FROM Advisor a
            LEFT JOIN Booking b ON a.AdvisorId = b.AdvisorId
            LEFT JOIN AdvisorReview r ON b.BookingId = r.BookingId
            WHERE a.AdvisorId = @Id
            GROUP BY a.AdvisorId, a.Name, a.Email, a.Category,
                     a.Specialty1, a.Specialty2, a.Specialty3,
                     a.Bio, a.PhotoPath, a.Status, a.CreatedAt, a.UpdatedAt;";

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", advisorId);
                conn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;
                    return new Advisor(
                        dr.GetInt32(0),  // AdvisorId
                        dr.GetString(1), // Name
                        dr.GetString(2), // Email
                        dr.GetString(3), // Category
                        dr.IsDBNull(4) ? null : dr.GetString(4), // Specialty1
                        dr.IsDBNull(5) ? null : dr.GetString(5), // Specialty2
                        dr.IsDBNull(6) ? null : dr.GetString(6), // Specialty3
                        dr.GetString(7), // Bio
                        dr.IsDBNull(8) ? null : dr.GetString(8), // PhotoPath
                        dr.GetByte(9),   // Status
                        dr.GetDateTime(10), // CreatedAt
                        dr.GetDateTime(11), // UpdatedAt
                        dr.GetDecimal(12),  // AvgRating (calculated)
                        dr.GetInt32(13)     // ReviewCount (calculated)
                    );
                }
            }
        }

        /// <summary>
        /// Get advisors with highest ratings (for featured/top advisors)
        /// </summary>
        public static List<Advisor> GetTopRated(int count = 5)
        {
            var list = new List<Advisor>();
            const string sql = @"
            SELECT TOP (@Count)
                a.AdvisorId, a.Name, a.Email, a.Category,
                a.Specialty1, a.Specialty2, a.Specialty3,
                a.Bio, a.PhotoPath, a.Status, a.CreatedAt, a.UpdatedAt,
                COALESCE(AVG(CAST(r.Rating AS DECIMAL(5,2))), 0) AS AvgRating,
                COUNT(r.Rating) AS ReviewCount
            FROM Advisor a
            LEFT JOIN Booking b ON a.AdvisorId = b.AdvisorId
            LEFT JOIN AdvisorReview r ON b.BookingId = r.BookingId
            WHERE a.Status = 1 -- Only approved advisors
            GROUP BY a.AdvisorId, a.Name, a.Email, a.Category,
                     a.Specialty1, a.Specialty2, a.Specialty3,
                     a.Bio, a.PhotoPath, a.Status, a.CreatedAt, a.UpdatedAt
            HAVING COUNT(r.Rating) > 0 -- Only advisors with reviews
            ORDER BY AVG(CAST(r.Rating AS DECIMAL(5,2))) DESC, COUNT(r.Rating) DESC;";

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Count", count);
                conn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new Advisor(
                            dr.GetInt32(0),  // AdvisorId
                            dr.GetString(1), // Name
                            dr.GetString(2), // Email
                            dr.GetString(3), // Category
                            dr.IsDBNull(4) ? null : dr.GetString(4), // Specialty1
                            dr.IsDBNull(5) ? null : dr.GetString(5), // Specialty2
                            dr.IsDBNull(6) ? null : dr.GetString(6), // Specialty3
                            dr.GetString(7), // Bio
                            dr.IsDBNull(8) ? null : dr.GetString(8), // PhotoPath
                            dr.GetByte(9),   // Status
                            dr.GetDateTime(10), // CreatedAt
                            dr.GetDateTime(11), // UpdatedAt
                            dr.GetDecimal(12),  // AvgRating (calculated)
                            dr.GetInt32(13)     // ReviewCount (calculated)
                        ));
                    }
                }
            }

            return list;
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