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
                LoadUserLessons();

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

        #region Chat Handler with OpenAI Integration

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static string ChatHandler(string message)
        {
            try
            {
                // Log the question to database (optional)
                try
                {
                    LogChatQuestion(message);
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Database logging failed: {dbEx.Message}");
                    // Continue even if logging fails
                }

                // Get response from OpenAI
                string response = GetOpenAIResponse(message);
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ChatHandler: {ex.ToString()}");
                return "Oops! I'm having a little tech hiccup right now 🤖💔 Please try again in a moment!";
            }
        }

        private static void LogChatQuestion(string question)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if question exists
                    string checkQuery = "SELECT Id FROM ChatQuestionTemplates WHERE Question = @Question";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Question", question);
                        object existingId = checkCmd.ExecuteScalar();

                        if (existingId != null)
                        {
                            // Update existing
                            string updateQuery = @"UPDATE ChatQuestionTemplates 
                                                 SET UsageCount = UsageCount + 1, 
                                                     LastUsed = GETDATE() 
                                                 WHERE Id = @Id";
                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@Id", existingId);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insert new
                            string insertQuery = @"INSERT INTO ChatQuestionTemplates (Question, UsageCount, LastUsed) 
                                                 VALUES (@Question, 1, GETDATE())";
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@Question", question);
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging chat question: {ex.Message}");
            }
        }

        private static string GetOpenAIResponse(string userMessage)
        {
            try
            {
                string apiKey = ConfigurationManager.AppSettings["OpenAI_API_Key"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    return "Hmm, seems like my brain isn't plugged in properly! 🧠⚡ Please contact our support team to get this sorted out!";
                }

                // Enhanced system prompt with FinClarity context
                var systemPrompt = @"You are Finny, the friendly and quirky AI assistant for FinClarity - a comprehensive financial literacy platform! 🚀, you can speak, write and read English, Chinese, Malay and tamil

Here's what FinClarity offers:

🏛️ **FORUM**: A community space where users share financial experiences, post blogs, comment, like posts, and use smart search filters to find information.

📚 **EDUCATION**: Interactive learning modules created by our team, plus a fun Unity-based gamified learning experience on the landing page.

🎯 **WORKSHOPS**: Book individual or group sessions with financial advisors. Users provide email and discussion details, then get email confirmations and Google Calendar reminders.

💰 **FINANCIAL TOOLS** (3 main features):
- **Insurance**: Create personalized insurance recommendations based on user forms
- **Investment**: Build portfolios with real-time stocks/crypto prices, heat maps, graphs, and predictive values  
- **Budgeting**: Two main features:
  - **Jars**: 6 preset auto-created ""wallets"" that split income into percentages, track balances & transactions
  - **Goals**: Set savings targets, add funds from jars or manually, track progress with dashboard charts

📊 **DASHBOARD**: Shows balances, progress, charts, and transaction filters by date, plus tools for bulk transactions and spending analysis.

Your personality: Be helpful, friendly, and slightly quirky! Keep responses concise (2-3 sentences max) so users don't get overwhelmed. Use emojis sparingly but effectively. Think of yourself as a knowledgeable friend who makes finance less scary and more approachable!

Help users navigate FinClarity, answer questions about features, and guide them to the right sections. If they ask about specific financial advice, remind them about the workshop feature to book sessions with real advisors.";

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = systemPrompt
                        },
                        new
                        {
                            role = "user",
                            content = userMessage
                        }
                    },
                    temperature = 0.8,
                    max_tokens = 200
                };

                using (var client = new HttpClient())
                {
                    // Set timeout
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    string json = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Synchronous calls
                    var response = client.PostAsync("https://api.openai.com/v1/chat/completions", content).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic result = JsonConvert.DeserializeObject(responseString);
                        return result.choices[0].message.content.ToString();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"OpenAI Error: {response.StatusCode} - {responseString}");

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            return "Looks like my API credentials need a refresh! 🔑 Please contact our tech team.";
                        }
                        else if ((int)response.StatusCode == 429)
                        {
                            return "Whoa there! I'm getting too many questions at once! 😅 Give me a quick breather and try again.";
                        }
                        else
                        {
                            return "Something went a bit wonky on my end! 🤖💫 Please try asking again in a moment.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetOpenAIResponse: {ex.ToString()}");
                return "I seem to be having a little brain freeze! 🧠❄️ Please try again in a moment.";
            }
        }

        #endregion

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
    }
}