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
                FROM [dbo].[UserViewed Pages] uvp
                JOIN [dbo].[EducationPages] ep ON uvp.Pageld = ep.Id
                JOIN [dbo].[EducationSubTopics] est ON ep.SubTopicId = est.Id
                JOIN [dbo].[EducationModules] em ON est.ModuleId = em.Id
                WHERE uvp.Userld = @UserId
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

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static string ChatHandler(string message)
        {
            try
            {
                // Log the question to database with enhanced tracking
                try
                {
                    LogChatQuestion(message);
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Database logging failed: {dbEx.Message}");
                    // Continue even if logging fails
                }

                // Get response from OpenAI with enhanced fun personality
                string response = GetEnhancedOpenAIResponse(message);
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ChatHandler: {ex.ToString()}");
                return "Oops! 😅 I'm having a little technical hiccup right now. Please try again in a moment, or feel free to explore our Discussion forum for community support! 💬✨";
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

                    // Enhanced logging with question categorization
                    string checkQuery = "SELECT Id FROM ChatQuestionTemplates WHERE Question = @Question";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Question", question);
                        object existingId = checkCmd.ExecuteScalar();

                        if (existingId != null)
                        {
                            // Update existing question with enhanced tracking
                            string updateQuery = @"UPDATE ChatQuestionTemplates 
                                                 SET UsageCount = UsageCount + 1, 
                                                     LastUsed = GETDATE(),
                                                     LastUpdated = GETDATE()
                                                 WHERE Id = @Id";
                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@Id", existingId);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insert new question with category and metadata
                            string insertQuery = @"INSERT INTO ChatQuestionTemplates 
                                                 (Question, UsageCount, LastUsed, CreatedDate, Category, Language) 
                                                 VALUES (@Question, 1, GETDATE(), GETDATE(), @Category, @Language)";
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@Question", question);
                                insertCmd.Parameters.AddWithValue("@Category", DetermineQuestionCategory(question));
                                insertCmd.Parameters.AddWithValue("@Language", DetectLanguage(question));
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

        // Enhanced OpenAI response with fun, interactive personality
        private static string GetEnhancedOpenAIResponse(string userMessage)
        {
            try
            {
                string apiKey = ConfigurationManager.AppSettings["OpenAI_API_Key"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    return "Hmm, looks like I need to check my connection! 🔧 Please contact our support team and they'll get me back up and running! 💪";
                }

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = @"You are FinBot 🤖✨, the super friendly and enthusiastic assistant for FinClarity - a comprehensive financial literacy platform that makes learning about money FUN and engaging! 🎉💰

🎯 YOUR PERSONALITY:
- Be SUPER friendly, warm, and encouraging! 😊
- Use emojis generously to make conversations lively and fun! 🌟
- Be enthusiastic about helping users on their financial journey! 🚀
- Celebrate small wins and progress! 🎊
- Make complex financial topics feel approachable and exciting! 💡
- Use casual, conversational language that feels like talking to a helpful friend! 👥

📱 PLATFORM FEATURES & NAVIGATION (Guide users with excitement!):

🏠 MAIN SECTIONS:
- 🏠 Homepage (UserHomepage.aspx): Your starting point for financial success!
- 📊 Dashboard (Dashboard.aspx): Your personal financial command center! Track budgets, set goals, and see your progress! 
- 👤 Profile (AllProfile.aspx): Customize your experience and manage your account settings!
- 📚 Educational Content: Gamified learning with quizzes, points, and character rewards! Level up your financial knowledge! 🎮
- 💬 Forum (Discussion.aspx): Connect with our amazing community! Share tips, ask questions, and learn together! 
- 🎯 Workshop (BookingForum.aspx): Book one-on-one sessions with financial experts! Calendar sync included! 📅
- 🎁 Vouchers: Earn and exchange rewards (VoucherExchange.aspx) + manage your active vouchers (VoucherActive.aspx)!

💡 AMAZING CAPABILITIES TO GET EXCITED ABOUT:
- 🎯 AI-powered personalized recommendations just for YOU!
- 🎮 Gamified learning with points, quizzes, and character customization!
- 💰 Smart budget tracking with helpful notifications and goal setting!
- 📈 Investment portfolio builder with risk assessment tools!
- 🛡️ Insurance guidance to protect what matters most!
- 🌟 Community forum with personalized topic suggestions!
- 👩‍💼 Professional advisor consultations with easy calendar sync!
- 🌍 Multi-language support for everyone around the world!

🎮 GAMIFICATION FEATURES (Get users excited about these!):
- 🧠 Interactive quizzes on financial topics!
- ⭐ Points system for completing learning modules!
- 🎨 Character customization with earned rewards!
- 🏆 Achievement badges and progress tracking!
- 🏅 Leaderboards and friendly competition with other users!

💰 FINANCIAL TOOLS (Make these sound accessible and helpful!):
- 📋 Budget tracker with smart expense categorization!
- 🎯 Savings goal calculator to reach your dreams!
- 📊 Investment portfolio analysis made simple!
- 🛡️ Insurance needs assessment - we've got you covered!
- 🏖️ Retirement planning tools for your future self!
- 💳 Debt management strategies to break free!

👥 WHO WE HELP (Show enthusiasm for all users!):
- 🎓 Young adults (16-24) starting their financial independence journey!
- 💼 Working professionals wanting smarter money management!
- 🤔 Anyone feeling uncertain about financial planning (we've all been there!)
- 📈 People eager to learn about investments and insurance!
- 💡 Individuals ready to start budgeting like a pro!

🌍 MULTI-LANGUAGE MAGIC:
Support users in their preferred language! Ask cheerfully if you're not sure:
- 🇸🇬 Singapore languages: English, Mandarin, Malay, Tamil
- 🌏 International: Spanish, French, German, Japanese, Korean, and more!
- Always ask: 'What language would you prefer? 어떤 언어를 선호하시나요? ¿Qué idioma prefieres?' etc.

🤖 INTERACTION STYLE (This is KEY!):
- Start with energetic greetings! 'Hey there! 👋 How can I help make your financial journey awesome today? 🌟'
- Use LOTS of encouraging language: 'Amazing!', 'You've got this!', 'That's a great question!', 'Fantastic choice!'
- End responses with exciting next steps: 'Ready to dive in? 🏊‍♀️', 'Let's make it happen! 💪', 'Your financial future is looking bright! ✨'
- Celebrate everything: 'Awesome that you're taking control of your finances! 🎉'
- Use casual, friendly language: 'Let's check that out!', 'That sounds perfect for you!', 'I'd love to help with that!'

📚 EDUCATIONAL APPROACH (Make learning FUN!):
- 'Let me break this down in a super simple way! 😊'
- 'Here's a fun way to think about it... 💭'
- 'Did you know? 🤔 [Include interesting financial tips]'
- 'Want to test your knowledge with a quick quiz? 🧠✨'
- 'The community is discussing this exact topic! Want to join? 💬'

🎯 RESPONSE STRUCTURE (Every response should be engaging!):
✅ Warm, enthusiastic greeting with emojis
✅ Answer their question with excitement and clarity  
✅ Specific page recommendations: 'Head over to Dashboard.aspx to set up your dream budget! 📊✨'
✅ Encouraging language about their financial journey
✅ Fun next steps or questions to keep them engaged
✅ Community/learning opportunities when relevant

EXAMPLE RESPONSES:
❌ 'You can check the dashboard for budgeting tools.'
✅ 'That's fantastic that you want to start budgeting! 🎉 Head over to your Dashboard (Dashboard.aspx) where you can set up your personalized budget tracker! It's like having a financial GPS for your money! 🗺️💰 Ready to take control? 💪'

❌ 'We have investment tools available.'
✅ 'Ooh, getting into investing? I LOVE that energy! 🚀📈 Our investment portfolio builder is going to be perfect for you! Head to Dashboard.aspx and let's build you a portfolio that matches your goals and risk style! Want to take a quick risk assessment quiz first? It's fun, I promise! 🎯✨'

Remember: Every interaction should leave users feeling more confident, excited, and empowered about their financial journey! You're not just answering questions - you're their enthusiastic financial cheerleader! 📣💪✨"
                        },
                        new
                        {
                            role = "user",
                            content = userMessage
                        }
                    },
                    temperature = 0.8, // Slightly higher for more creative/fun responses
                    max_tokens = 350 // Increased for more detailed, engaging responses
                };

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    string json = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

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
                            return "Oops! 🔐 I'm having some authentication troubles. Our tech team will fix this super quick! In the meantime, try exploring our Discussion forum! 💬✨";
                        }
                        else if ((int)response.StatusCode == 429)
                        {
                            return "Wow, I'm popular today! 😅 Lots of people asking great financial questions! Give me just a moment to catch up, then try again! ⏰💫";
                        }
                        else
                        {
                            return "I'm having a tiny technical moment! 🤖💭 Please try again in a sec, or check out our awesome Discussion forum where our community is always ready to help! 🌟";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetEnhancedOpenAIResponse: {ex.ToString()}");
                return "Whoops! 😅 I'm experiencing some technical turbulence right now! ✈️ Try again in a moment, or head to our Discussion forum where our amazing community can help you out! 💪🌟";
            }
        }

        // Helper method to categorize questions for analytics
        private static string DetermineQuestionCategory(string question)
        {
            string lowerQuestion = question.ToLower();

            if (lowerQuestion.Contains("budget") || lowerQuestion.Contains("expense") || lowerQuestion.Contains("saving") ||
                lowerQuestion.Contains("spend") || lowerQuestion.Contains("money management"))
                return "Budgeting & Savings";
            else if (lowerQuestion.Contains("invest") || lowerQuestion.Contains("stock") || lowerQuestion.Contains("portfolio") ||
                     lowerQuestion.Contains("shares") || lowerQuestion.Contains("trading"))
                return "Investment";
            else if (lowerQuestion.Contains("insurance") || lowerQuestion.Contains("coverage") || lowerQuestion.Contains("policy") ||
                     lowerQuestion.Contains("protection"))
                return "Insurance";
            else if (lowerQuestion.Contains("advisor") || lowerQuestion.Contains("consultation") || lowerQuestion.Contains("booking") ||
                     lowerQuestion.Contains("expert") || lowerQuestion.Contains("professional"))
                return "Advisory Services";
            else if (lowerQuestion.Contains("quiz") || lowerQuestion.Contains("learn") || lowerQuestion.Contains("education") ||
                     lowerQuestion.Contains("course") || lowerQuestion.Contains("tutorial"))
                return "Education & Learning";
            else if (lowerQuestion.Contains("voucher") || lowerQuestion.Contains("reward") || lowerQuestion.Contains("points") ||
                     lowerQuestion.Contains("gamification") || lowerQuestion.Contains("character"))
                return "Rewards & Gamification";
            else if (lowerQuestion.Contains("forum") || lowerQuestion.Contains("discussion") || lowerQuestion.Contains("community") ||
                     lowerQuestion.Contains("chat") || lowerQuestion.Contains("social"))
                return "Community & Social";
            else if (lowerQuestion.Contains("dashboard") || lowerQuestion.Contains("profile") || lowerQuestion.Contains("navigation") ||
                     lowerQuestion.Contains("homepage") || lowerQuestion.Contains("login"))
                return "Platform Navigation";
            else if (lowerQuestion.Contains("hello") || lowerQuestion.Contains("hi") || lowerQuestion.Contains("hey") ||
                     lowerQuestion.Contains("help") || lowerQuestion.Contains("start"))
                return "General Greeting";
            else if (lowerQuestion.Contains("language") || lowerQuestion.Contains("chinese") || lowerQuestion.Contains("malay") ||
                     lowerQuestion.Contains("tamil") || lowerQuestion.Contains("español"))
                return "Language Support";
            else
                return "General Inquiry";
        }

        // Enhanced language detection for better multi-language support
        private static string DetectLanguage(string question)
        {
            // Simple language detection based on common patterns
            if (ContainsChinese(question))
                return "Chinese";
            else if (question.ToLower().Contains("selamat") || question.ToLower().Contains("terima kasih") ||
                     question.ToLower().Contains("tolong"))
                return "Malay";
            else if (question.ToLower().Contains("vanakkam") || question.ToLower().Contains("nandri"))
                return "Tamil";
            else if (question.ToLower().Contains("hola") || question.ToLower().Contains("gracias") ||
                     question.ToLower().Contains("ayuda"))
                return "Spanish";
            else if (question.ToLower().Contains("bonjour") || question.ToLower().Contains("merci") ||
                     question.ToLower().Contains("aide"))
                return "French";
            else if (question.ToLower().Contains("guten") || question.ToLower().Contains("danke") ||
                     question.ToLower().Contains("hilfe"))
                return "German";
            else if (question.ToLower().Contains("こんにちは") || question.ToLower().Contains("ありがとう"))
                return "Japanese";
            else if (question.ToLower().Contains("안녕") || question.ToLower().Contains("감사"))
                return "Korean";
            else
                return "English";
        }

        // Helper method to detect Chinese characters
        private static bool ContainsChinese(string text)
        {
            foreach (char c in text)
            {
                if (c >= 0x4e00 && c <= 0x9fff) // Basic CJK Unified Ideographs range
                    return true;
            }
            return false;
        }

        #endregion
    }
}