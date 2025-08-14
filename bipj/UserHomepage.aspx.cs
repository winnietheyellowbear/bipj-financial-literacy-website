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

namespace bipj
{
    public partial class UserHomepage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Any page load logic can go here
        }

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

        // Method to get user context (can be enhanced with session data)
        private static string GetUserContext()
        {
            try
            {
                // This can be enhanced to include user session data like:
                // - User's preferred language
                // - Their progress in financial literacy
                // - Previously completed quizzes
                // - Current goals or interests
                // For now, returning empty string
                return "";
            }
            catch
            {
                return "";
            }
        }

        // Method to suggest personalized content based on user history
        private static string GetPersonalizedSuggestions(string category)
        {
            try
            {
                // This can be enhanced to provide personalized suggestions based on:
                // - User's question category
                // - Their previous interactions
                // - Popular content in their category
                // - Their learning progress

                switch (category.ToLower())
                {
                    case "budgeting & savings":
                        return "💡 Tip: Check out our budgeting calculator in the Dashboard! Many users found it super helpful for tracking expenses! 📊✨";
                    case "investment":
                        return "🚀 Fun fact: Did you know our investment portfolio builder has helped over 1000 users create their first investment plan? Want to be next? 📈";
                    case "education & learning":
                        return "🎮 Pro tip: Our financial literacy quizzes are not only educational but also earn you points for character customization! Learning + fun = win! 🏆";
                    default:
                        return "🌟 Don't forget to explore our community forum where users share amazing financial tips daily! 💬";
                }
            }
            catch
            {
                return "";
            }
        }
    }
}