using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Services;
using System.Web.Script.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;

namespace bipj
{
    public partial class UserHomepage : System.Web.UI.Page
    {
        // Protected properties to hold user data (exposed to ASPX page)
        protected string UserName = "User";
        protected int UserPoints = 0;
        protected int CompletedLessons = 0;
        protected int AdvisorSessions = 0;
        protected int DaysActive = 0;

        // New property to hold lesson progress data
        protected List<LessonProgress> UserLessons = new List<LessonProgress>();

        // Helper class for lesson progress
        public class LessonProgress
        {
            public int ModuleId { get; set; }
            public string ModuleName { get; set; }
            public decimal CompletionPercentage { get; set; }
            public string ImageUrl { get; set; }
            public DateTime LastAccessed { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is logged in
            if (Session["UserId"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Load user data and populate the page
                LoadUserData();
                LoadUserLessons(); // Add this new method

                // Set the hidden field for JavaScript access if needed
                hdnUserId.Value = Session["UserId"].ToString();
            }
        }

        private void LoadUserData()
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                string connectionString = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get user basic info and points
                    string userQuery = "SELECT Name, Point FROM [User] WHERE Id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(userQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["Name"]?.ToString() ?? "User";
                                UserPoints = reader["Point"] != DBNull.Value ? Convert.ToInt32(reader["Point"]) : 0;
                            }
                        }
                    }

                    // Get completed lessons count
                    string lessonsQuery = @"
                        SELECT COUNT(*) as CompletedCount 
                        FROM UserEducationProgress 
                        WHERE UserId = @UserId AND CompletionPercentage >= 100";
                    using (SqlCommand cmd = new SqlCommand(lessonsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        object result = cmd.ExecuteScalar();
                        CompletedLessons = result != null ? Convert.ToInt32(result) : 0;
                    }

                    // Get advisor sessions count
                    string sessionsQuery = "SELECT COUNT(*) FROM Booking WHERE UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(sessionsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        object result = cmd.ExecuteScalar();
                        AdvisorSessions = result != null ? Convert.ToInt32(result) : 0;
                    }

                    // Get days active (days since join date)
                    string daysQuery = @"
                        SELECT DATEDIFF(day, CreatedDate, GETDATE()) + 1 as DaysActive 
                        FROM [User] 
                        WHERE Id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(daysQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        object result = cmd.ExecuteScalar();
                        DaysActive = result != null ? Convert.ToInt32(result) : 1;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user data: {ex.Message}");
                // Set default values if there's an error
                UserName = Session["UserName"]?.ToString() ?? "User";
                UserPoints = 0;
                CompletedLessons = 0;
                AdvisorSessions = 0;
                DaysActive = 1;
            }
        }

        // New method to load user's lesson progress
        private void LoadUserLessons()
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                string connectionString = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get user's education progress with module information
                    string lessonQuery = @"
                        SELECT TOP 3
                            em.Id as ModuleId,
                            em.Name as ModuleName,
                            COALESCE(uep.CompletionPercentage, 0) as CompletionPercentage,
                            em.ImageUrl,
                            COALESCE(uep.LastAccessed, GETDATE()) as LastAccessed
                        FROM [dbo].[EducationModules] em
                        LEFT JOIN [dbo].[UserEducationProgress] uep ON em.Id = uep.ModuleId AND uep.UserId = @UserId
                        WHERE em.Id IS NOT NULL
                        ORDER BY 
                            CASE 
                                WHEN uep.CompletionPercentage IS NULL THEN 0
                                WHEN uep.CompletionPercentage >= 100 THEN 2
                                ELSE 1
                            END ASC,
                            COALESCE(uep.LastAccessed, '1900-01-01') DESC,
                            em.Id ASC";

                    using (SqlCommand cmd = new SqlCommand(lessonQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UserLessons.Add(new LessonProgress
                                {
                                    ModuleId = Convert.ToInt32(reader["ModuleId"]),
                                    ModuleName = reader["ModuleName"].ToString(),
                                    CompletionPercentage = Convert.ToDecimal(reader["CompletionPercentage"]),
                                    ImageUrl = reader["ImageUrl"]?.ToString() ?? "",
                                    LastAccessed = Convert.ToDateTime(reader["LastAccessed"])
                                });
                            }
                        }
                    }

                    // If no lessons found, add some default modules
                    if (UserLessons.Count == 0)
                    {
                        string defaultQuery = @"
                            SELECT TOP 3 Id, Name, ImageUrl 
                            FROM [dbo].[EducationModules] 
                            ORDER BY Id";

                        using (SqlCommand cmd = new SqlCommand(defaultQuery, conn))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    UserLessons.Add(new LessonProgress
                                    {
                                        ModuleId = Convert.ToInt32(reader["Id"]),
                                        ModuleName = reader["Name"].ToString(),
                                        CompletionPercentage = 0,
                                        ImageUrl = reader["ImageUrl"]?.ToString() ?? "",
                                        LastAccessed = DateTime.Now
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user lessons: {ex.Message}");
                // Add fallback lessons if database fails
                UserLessons.Add(new LessonProgress { ModuleName = "Getting Started", CompletionPercentage = 0 });
                UserLessons.Add(new LessonProgress { ModuleName = "Basic Budgeting", CompletionPercentage = 0 });
                UserLessons.Add(new LessonProgress { ModuleName = "Saving Strategies", CompletionPercentage = 0 });
            }
        }

        // Event handler for Explore Discussions button
        protected void btnExploreDiscussions_Click(object sender, EventArgs e)
        {
            Response.Redirect("Discussion.aspx");
        }

        #region WebMethods for AJAX calls

        // WebMethod to get current user stats (for dynamic updates)
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetUserStats()
        {
            try
            {
                // Get the current session (you may need to enable session state for web methods)
                var context = System.Web.HttpContext.Current;
                if (context?.Session?["UserId"] == null)
                {
                    return new { success = false, message = "User not logged in" };
                }

                int userId = Convert.ToInt32(context.Session["UserId"]);
                string connectionString = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var stats = new
                    {
                        Points = GetUserPoints(conn, userId),
                        CompletedLessons = GetCompletedLessonsCount(conn, userId),
                        AdvisorSessions = GetAdvisorSessionsCount(conn, userId),
                        DaysActive = GetDaysActive(conn, userId)
                    };

                    return new { success = true, data = stats };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetUserStats: {ex.Message}");
                return new { success = false, message = "Error retrieving stats" };
            }
        }

        // WebMethod to get user lesson progress (for dynamic updates)
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetUserLessonsProgress()
        {
            try
            {
                var context = System.Web.HttpContext.Current;
                if (context?.Session?["UserId"] == null)
                {
                    return new { success = false, message = "User not logged in" };
                }

                int userId = Convert.ToInt32(context.Session["UserId"]);
                string connectionString = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

                List<object> lessons = new List<object>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT TOP 3
                            em.Id as ModuleId,
                            em.Name as ModuleName,
                            COALESCE(uep.CompletionPercentage, 0) as CompletionPercentage,
                            em.ImageUrl,
                            COALESCE(uep.LastAccessed, GETDATE()) as LastAccessed
                        FROM [dbo].[EducationModules] em
                        LEFT JOIN [dbo].[UserEducationProgress] uep ON em.Id = uep.ModuleId AND uep.UserId = @UserId
                        ORDER BY 
                            CASE 
                                WHEN uep.CompletionPercentage IS NULL THEN 0
                                WHEN uep.CompletionPercentage >= 100 THEN 2
                                ELSE 1
                            END ASC,
                            COALESCE(uep.LastAccessed, '1900-01-01') DESC,
                            em.Id ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lessons.Add(new
                                {
                                    ModuleId = reader["ModuleId"],
                                    Name = reader["ModuleName"].ToString(),
                                    Progress = Convert.ToInt32(reader["CompletionPercentage"]),
                                    ImageUrl = reader["ImageUrl"]?.ToString(),
                                    LastAccessed = reader["LastAccessed"].ToString()
                                });
                            }
                        }
                    }
                }

                return new { success = true, data = lessons };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetUserLessonsProgress: {ex.Message}");
                return new { success = false, message = "Error retrieving lesson progress" };
            }
        }

        // WebMethod to get recommended topics
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetRecommendedTopics()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

                List<object> recommendedTopics = new List<object>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query to get a few recommended education modules
                    string query = @"
                SELECT TOP 4 Id, Name, BriefDescription, ImageUrl
                FROM [dbo].[EducationModules]
                ORDER BY NEWID()";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                recommendedTopics.Add(new
                                {
                                    Id = reader["Id"],
                                    Name = reader["Name"].ToString(),
                                    Description = reader["BriefDescription"]?.ToString(),
                                    ImageUrl = reader["ImageUrl"]?.ToString()
                                });
                            }
                        }
                    }
                }
                return new { success = true, data = recommendedTopics };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetRecommendedTopics: {ex.Message}");
                return new { success = false, message = "Error retrieving recommended topics." };
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetRecentActivities()
        {
            try
            {
                var context = System.Web.HttpContext.Current;
                if (context?.Session?["UserId"] == null)
                {
                    return new { success = false, message = "User not logged in" };
                }

                int userId = Convert.ToInt32(context.Session["UserId"]);
                string connectionString = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

                List<object> recentActivities = new List<object>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query to get a user's recently viewed pages
                    string query = @"
                SELECT TOP 2
                    uvp.ViewedDate,
                    em.Name AS ModuleName,
                    ep.Title AS PageTitle,
                    em.ImageUrl
                FROM [dbo].[UserViewedPages] uvp
                JOIN [dbo].[EducationPages] ep ON uvp.PageId = ep.Id
                JOIN [dbo].[EducationSubTopics] est ON ep.SubTopicId = est.Id
                JOIN [dbo].[EducationModules] em ON est.ModuleId = em.Id
                WHERE uvp.UserId = @UserId
                ORDER BY uvp.ViewedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                recentActivities.Add(new
                                {
                                    ModuleName = reader["ModuleName"].ToString(),
                                    PageTitle = reader["PageTitle"].ToString(),
                                    ImageUrl = reader["ImageUrl"]?.ToString(),
                                    ViewedDate = reader["ViewedDate"].ToString()
                                });
                            }
                        }
                    }
                }

                return new { success = true, data = recentActivities };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetRecentActivities: {ex.Message}");
                return new { success = false, message = "Error retrieving recent activities." };
            }
        }

        #endregion

        #region Helper Methods for Database Operations

        private static int GetUserPoints(SqlConnection conn, int userId)
        {
            string query = "SELECT COALESCE(Point, 0) FROM [User] WHERE Id = @UserId";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private static int GetCompletedLessonsCount(SqlConnection conn, int userId)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM UserEducationProgress 
                WHERE UserId = @UserId AND CompletionPercentage >= 100";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private static int GetAdvisorSessionsCount(SqlConnection conn, int userId)
        {
            string query = "SELECT COUNT(*) FROM Booking WHERE UserId = @UserId";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private static int GetDaysActive(SqlConnection conn, int userId)
        {
            string query = @"
                SELECT DATEDIFF(day, CreatedDate, GETDATE()) + 1 
                FROM [User] 
                WHERE Id = @UserId";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 1;
            }
        }

        #endregion

        #region Chat Handler (Enhanced Version)
        // ... Keep all your existing chat handler code here ...
        #endregion
    }
}