using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace bipj
{
    public partial class PerformanceAnalysis : System.Web.UI.Page
    {
        private string connectionString;
        private string openAiApiKey;

        protected async void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            openAiApiKey = ConfigurationManager.AppSettings["OpenAI_API_Key"];

            if (!IsPostBack)
            {
                await GenerateReportAsync();
            }
        }

        private async Task GenerateReportAsync()
        {
            pnlLoading.Visible = true;
            pnlReport.Visible = false;

            try
            {
                // 1. Collect comprehensive data
                var dashboardData = GetDashboardData();
                var engagementMetrics = GetEngagementMetrics();
                var performanceMetrics = GetPerformanceMetrics();
                var tableSchemas = GetTableSchemas();

                var prompt = BuildEnhancedPrompt(dashboardData, engagementMetrics, performanceMetrics, tableSchemas);

                // 2. Call OpenAI API with enhanced settings
                var reportText = await CallOpenAIApiWithHttpClient(prompt);

                // 3. Process and display the structured report
                DisplayStructuredReport(reportText);
            }
            catch (Exception ex)
            {
                litHealthSummary.Text = $"<div class='finding-card'><div class='finding-header'><div class='finding-icon negative'><i class='bi bi-exclamation-triangle'></i></div><h3 class='finding-title'>Error Generating Report</h3></div><p class='finding-description'>Unable to generate analysis: {ex.Message}</p></div>";
                pnlLoading.Visible = false;
                pnlReport.Visible = true;
            }
        }

        private async Task<string> CallOpenAIApiWithHttpClient(string prompt)
        {
            if (string.IsNullOrEmpty(openAiApiKey))
            {
                throw new InvalidOperationException("OpenAI API Key is not configured.");
            }

            var requestBody = new
            {
                model = "gpt-4", // Using GPT-4 for better analysis
                messages = new[]
                {
                    new {
                        role = "system",
                        content = @"You are a senior business intelligence analyst specializing in fintech and educational platforms. 
                                   Your expertise includes user engagement analysis, feature adoption metrics, educational effectiveness, 
                                   and platform optimization strategies. Provide detailed, actionable insights with specific recommendations.
                                   
                                   Format your response EXACTLY as follows:
                                   HEALTH_SCORE: [number between 0-100]
                                   
                                   SUMMARY: [2-3 sentences about overall platform health]
                                   
                                   FINDINGS:
                                   [Each finding should be in this format:]
                                   FINDING_TYPE: [POSITIVE|NEGATIVE|NEUTRAL|INFO]
                                   TITLE: [Brief title]
                                   DESCRIPTION: [Detailed explanation]
                                   METRICS: [Key numbers/percentages]
                                   RECOMMENDATIONS: [Bullet point recommendations]
                                   ---
                                   [Next finding...]"
                    },
                    new { role = "user", content = prompt }
                },
                temperature = 0.3, // Lower temperature for more consistent analysis
                max_tokens = 2500  // Increased for detailed analysis
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(90); // Increased timeout

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    return result.choices[0].message.content.ToString();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"OpenAI Error: {response.StatusCode} - {responseString}");
                    throw new HttpRequestException($"OpenAI API call failed with status code {response.StatusCode}");
                }
            }
        }

        private Dictionary<string, object> GetDashboardData()
        {
            var data = new Dictionary<string, object>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Core user metrics
                data["TotalUsers"] = GetScalar(conn, "SELECT COUNT(*) FROM [User]");
                data["ActiveUsers"] = GetScalar(conn, "SELECT COUNT(*) FROM [User] WHERE LastLoginDate >= DATEADD(day, -30, GETDATE())");
                data["NewUsersThisMonth"] = GetScalar(conn, "SELECT COUNT(*) FROM [User] WHERE MONTH(CreatedDate) = MONTH(GETDATE()) AND YEAR(CreatedDate) = YEAR(GETDATE())");
                data["NewUsersLastMonth"] = GetScalar(conn, "SELECT COUNT(*) FROM [User] WHERE MONTH(CreatedDate) = MONTH(DATEADD(month, -1, GETDATE())) AND YEAR(CreatedDate) = YEAR(DATEADD(month, -1, GETDATE()))");

                // Feature adoption
                data["TotalAdvisors"] = GetScalar(conn, "SELECT COUNT(*) FROM [Advisor] WHERE Status = 1");
                data["TotalBookings"] = GetScalar(conn, "SELECT COUNT(*) FROM [Booking]");
                data["BookingsThisMonth"] = GetScalar(conn, "SELECT COUNT(*) FROM [Booking] WHERE MONTH(CreatedAt) = MONTH(GETDATE()) AND YEAR(CreatedAt) = YEAR(GETDATE())");
                data["UsersWithJars"] = GetScalar(conn, "SELECT COUNT(DISTINCT UserId) FROM [Jars] WHERE IsDeleted = 0");
                data["TotalGoals"] = GetScalar(conn, "SELECT COUNT(*) FROM [Goals] WHERE IsArchived = 0");
                data["UsersWithGoals"] = GetScalar(conn, "SELECT COUNT(DISTINCT UserId) FROM [Goals] WHERE IsArchived = 0");

                // Education metrics
                data["TotalEducationModules"] = GetScalar(conn, "SELECT COUNT(*) FROM [EducationModules]");
                data["TotalEducationCompletions"] = GetScalar(conn, "SELECT COUNT(*) FROM [UserEducationProgress] WHERE CompletionPercentage = 100.00");
                data["EducationCompletionsThisMonth"] = GetScalar(conn, "SELECT COUNT(*) FROM [UserEducationProgress] WHERE CompletionPercentage = 100.00 AND MONTH(LastAccessed) = MONTH(GETDATE())");
                data["AvgEducationCompletion"] = GetScalar(conn, "SELECT AVG(ISNULL(CompletionPercentage, 0)) FROM [UserEducationProgress]");

                // Community engagement
                data["TotalPosts"] = GetScalar(conn, "SELECT COUNT(*) FROM [Post]");
                data["TotalComments"] = GetScalar(conn, "SELECT COUNT(*) FROM [Comment]");
                data["PostsThisMonth"] = GetScalar(conn, "SELECT COUNT(*) FROM [Post] WHERE MONTH(Post_DateTime) = MONTH(GETDATE())");
                data["CommentsThisMonth"] = GetScalar(conn, "SELECT COUNT(*) FROM [Comment] WHERE MONTH(Comment_DateTime) = MONTH(GETDATE())");
                data["ActiveForumUsers"] = GetScalar(conn, "SELECT COUNT(DISTINCT User_ID) FROM [Post] WHERE Post_DateTime >= DATEADD(day, -30, GETDATE())");
            }
            return data;
        }

        private Dictionary<string, object> GetEngagementMetrics()
        {
            var metrics = new Dictionary<string, object>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // User retention and engagement
                metrics["UserRetentionRate"] = GetScalar(conn, @"
                    SELECT CAST(COUNT(CASE WHEN LastLoginDate >= DATEADD(day, -7, GETDATE()) THEN 1 END) * 100.0 / COUNT(*) AS DECIMAL(5,2))
                    FROM [User] WHERE CreatedDate <= DATEADD(day, -7, GETDATE())");

                metrics["DailyActiveUsers"] = GetScalar(conn, "SELECT COUNT(DISTINCT UserId) FROM [UserViewedPages] WHERE CAST(ViewedDate AS DATE) = CAST(GETDATE() AS DATE)");

                // Feature utilization rates
                metrics["JarUtilizationRate"] = GetScalar(conn, @"
                    SELECT CAST(COUNT(DISTINCT j.UserId) * 100.0 / COUNT(DISTINCT u.Id) AS DECIMAL(5,2))
                    FROM [User] u LEFT JOIN [Jars] j ON u.Id = j.UserId AND j.IsDeleted = 0");

                metrics["GoalUtilizationRate"] = GetScalar(conn, @"
                    SELECT CAST(COUNT(DISTINCT g.UserId) * 100.0 / COUNT(DISTINCT u.Id) AS DECIMAL(5,2))
                    FROM [User] u LEFT JOIN [Goals] g ON u.Id = g.UserId AND g.IsArchived = 0");

                metrics["EducationParticipationRate"] = GetScalar(conn, @"
                    SELECT CAST(COUNT(DISTINCT uep.UserId) * 100.0 / COUNT(DISTINCT u.Id) AS DECIMAL(5,2))
                    FROM [User] u LEFT JOIN [UserEducationProgress] uep ON u.Id = uep.UserId");

                // Advisor engagement
                metrics["AdvisorBookingRate"] = GetScalar(conn, @"
                    SELECT CAST(COUNT(DISTINCT b.UserId) * 100.0 / COUNT(DISTINCT u.Id) AS DECIMAL(5,2))
                    FROM [User] u LEFT JOIN [Booking] b ON u.Id = b.UserId");

                metrics["AvgAdvisorRating"] = GetScalar(conn, "SELECT AVG(Rating) FROM [Advisor] WHERE Rating IS NOT NULL AND Status = 1");

                // Community participation
                metrics["ForumParticipationRate"] = GetScalar(conn, @"
                    SELECT CAST(COUNT(DISTINCT p.User_ID) * 100.0 / COUNT(DISTINCT u.Id) AS DECIMAL(5,2))
                    FROM [User] u LEFT JOIN [Post] p ON u.Id = p.User_ID");
            }
            return metrics;
        }

        private Dictionary<string, object> GetPerformanceMetrics()
        {
            var metrics = new Dictionary<string, object>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Growth metrics
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var lastMonth = DateTime.Now.AddMonths(-1).Month;
                var lastMonthYear = DateTime.Now.AddMonths(-1).Year;

                metrics["UserGrowthRate"] = CalculateGrowthRate(conn, "[User]", "CreatedDate");
                metrics["BookingGrowthRate"] = CalculateGrowthRate(conn, "[Booking]", "CreatedAt");
                metrics["PostGrowthRate"] = CalculateGrowthRate(conn, "[Post]", "Post_DateTime");

                // Completion and success rates
                metrics["EducationSuccessRate"] = GetScalar(conn, @"
                    SELECT CAST(COUNT(CASE WHEN CompletionPercentage = 100 THEN 1 END) * 100.0 / COUNT(*) AS DECIMAL(5,2))
                    FROM [UserEducationProgress] WHERE UserId IS NOT NULL");

                // Platform health indicators
                metrics["AdvisorUtilizationRate"] = GetScalar(conn, @"
                    SELECT CAST(COUNT(DISTINCT b.AdvisorId) * 100.0 / COUNT(DISTINCT a.AdvisorId) AS DECIMAL(5,2))
                    FROM [Advisor] a LEFT JOIN [Booking] b ON a.AdvisorId = b.AdvisorId WHERE a.Status = 1");

                metrics["UserEngagementScore"] = CalculateEngagementScore(conn);
            }
            return metrics;
        }

        private decimal CalculateGrowthRate(SqlConnection conn, string tableName, string dateColumn)
        {
            try
            {
                string query = $@"
                    SELECT 
                        COUNT(CASE WHEN MONTH({dateColumn}) = MONTH(GETDATE()) AND YEAR({dateColumn}) = YEAR(GETDATE()) THEN 1 END) as CurrentMonth,
                        COUNT(CASE WHEN MONTH({dateColumn}) = MONTH(DATEADD(month, -1, GETDATE())) AND YEAR({dateColumn}) = YEAR(DATEADD(month, -1, GETDATE())) THEN 1 END) as LastMonth
                    FROM {tableName}";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var current = reader.GetInt32(0);
                        var previous = reader.GetInt32(1);

                        if (previous > 0)
                        {
                            return Math.Round((decimal)(current - previous) * 100 / previous, 2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Growth rate calculation error: {ex.Message}");
            }

            return 0;
        }

        private decimal CalculateEngagementScore(SqlConnection conn)
        {
            try
            {
                // Composite engagement score based on multiple factors
                string query = @"
                    SELECT 
                        COUNT(DISTINCT CASE WHEN LastLoginDate >= DATEADD(day, -7, GETDATE()) THEN u.Id END) * 1.0 / COUNT(DISTINCT u.Id) * 30 as LoginScore,
                        COUNT(DISTINCT p.User_ID) * 1.0 / COUNT(DISTINCT u.Id) * 25 as ForumScore,
                        COUNT(DISTINCT j.UserId) * 1.0 / COUNT(DISTINCT u.Id) * 20 as JarScore,
                        COUNT(DISTINCT g.UserId) * 1.0 / COUNT(DISTINCT u.Id) * 15 as GoalScore,
                        COUNT(DISTINCT b.UserId) * 1.0 / COUNT(DISTINCT u.Id) * 10 as BookingScore
                    FROM [User] u
                    LEFT JOIN [Post] p ON u.Id = p.User_ID AND p.Post_DateTime >= DATEADD(day, -30, GETDATE())
                    LEFT JOIN [Jars] j ON u.Id = j.UserId AND j.IsDeleted = 0
                    LEFT JOIN [Goals] g ON u.Id = g.UserId AND g.IsArchived = 0
                    LEFT JOIN [Booking] b ON u.Id = b.UserId AND b.CreatedAt >= DATEADD(day, -30, GETDATE())";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var loginScore = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
                        var forumScore = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);
                        var jarScore = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
                        var goalScore = reader.IsDBNull(3) ? 0 : reader.GetDouble(3);
                        var bookingScore = reader.IsDBNull(4) ? 0 : reader.GetDouble(4);

                        return Math.Round((decimal)(loginScore + forumScore + jarScore + goalScore + bookingScore), 2);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Engagement score calculation error: {ex.Message}");
            }

            return 50; // Default middle score
        }

        private string GetScalar(SqlConnection conn, string query)
        {
            try
            {
                using (var cmd = new SqlCommand(query, conn))
                {
                    var result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "0";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Query execution error: {ex.Message}");
                return "0";
            }
        }

        private string GetTableSchemas()
        {
            return @"
                Database Schema for Financial Literacy Platform:
                
                Core Tables:
                - User: Stores user account information (Id, Name, Email, CreatedDate, LastLoginDate, UserType)
                - Advisor: Financial advisors available for booking (AdvisorId, Name, Status, Rating, RatingCount)
                - Booking: Advisor consultation bookings (BookingId, UserId, AdvisorId, SessionType, CreatedAt)
                
                Financial Tools:
                - Jars: Digital savings jars for budgeting (JarId, UserId, JarName, Amount, IsDeleted)
                - Goals: Financial goals set by users (GoalId, UserId, GoalName, TargetAmount, IsArchived)
                - Portfolio: Investment portfolio tracking (Id, UserId, InvestmentType, Amount)
                - InsurancePlan: Insurance planning tool (Id, UserID, PlanType, CreatedAt)
                
                Education System:
                - EducationModules: Available learning modules (Id, Name, BriefDescription, Category)
                - UserEducationProgress: Learning progress tracking (Id, UserId, ModuleId, CompletionPercentage, LastAccessed)
                
                Community Features:
                - Post: Forum posts by users (Post_ID, User_ID, Content, Post_DateTime)
                - Comment: Comments on forum posts (Comment_ID, User_ID, Post_ID, Comment_DateTime)
                - Like: Likes on posts (Like_ID, User_ID, Post_ID, Like_DateTime)
                
                Activity Tracking:
                - UserViewedPages: Page view analytics (Id, UserId, PageName, ViewedDate)
                - JarTransactions: Jar transaction history (Id, JarId, Amount, Date, TransactionType)
                - GoalTransactions: Goal-related transactions (Id, GoalId, Amount, Date)";
        }

        private string BuildEnhancedPrompt(Dictionary<string, object> dashboardData, Dictionary<string, object> engagementMetrics, Dictionary<string, object> performanceMetrics, string schemas)
        {
            var prompt = new StringBuilder();

            prompt.AppendLine("COMPREHENSIVE FINANCIAL LITERACY PLATFORM ANALYSIS");
            prompt.AppendLine("=================================================");
            prompt.AppendLine();
            prompt.AppendLine("You are analyzing a comprehensive financial literacy platform with the following capabilities:");
            prompt.AppendLine("- Digital jar system for budgeting and savings");
            prompt.AppendLine("- Financial goal setting and tracking");
            prompt.AppendLine("- Educational modules and progress tracking");
            prompt.AppendLine("- Professional financial advisor consultations");
            prompt.AppendLine("- Community forum for discussions");
            prompt.AppendLine("- Investment portfolio management");
            prompt.AppendLine("- Insurance planning tools");
            prompt.AppendLine();

            prompt.AppendLine("CURRENT PLATFORM DATA:");
            prompt.AppendLine("---------------------");
            foreach (var item in dashboardData)
            {
                prompt.AppendLine($"• {item.Key}: {item.Value}");
            }
            prompt.AppendLine();

            prompt.AppendLine("ENGAGEMENT & UTILIZATION METRICS:");
            prompt.AppendLine("---------------------------------");
            foreach (var item in engagementMetrics)
            {
                prompt.AppendLine($"• {item.Key}: {item.Value}%");
            }
            prompt.AppendLine();

            prompt.AppendLine("PERFORMANCE & GROWTH METRICS:");
            prompt.AppendLine("-----------------------------");
            foreach (var item in performanceMetrics)
            {
                prompt.AppendLine($"• {item.Key}: {item.Value}%");
            }
            prompt.AppendLine();

            prompt.AppendLine("DATABASE STRUCTURE:");
            prompt.AppendLine("------------------");
            prompt.AppendLine(schemas);
            prompt.AppendLine();

            prompt.AppendLine("ANALYSIS REQUIREMENTS:");
            prompt.AppendLine("---------------------");
            prompt.AppendLine("1. Calculate an overall platform health score (0-100) considering:");
            prompt.AppendLine("   - User engagement and retention rates");
            prompt.AppendLine("   - Feature adoption across all tools");
            prompt.AppendLine("   - Educational completion rates");
            prompt.AppendLine("   - Community participation levels");
            prompt.AppendLine("   - Growth trends and momentum");
            prompt.AppendLine();
            prompt.AppendLine("2. Identify 4-6 key findings covering:");
            prompt.AppendLine("   - POSITIVE aspects (what's working well)");
            prompt.AppendLine("   - NEGATIVE issues (areas needing immediate attention)");
            prompt.AppendLine("   - NEUTRAL observations (noteworthy patterns)");
            prompt.AppendLine("   - INFO insights (opportunities for optimization)");
            prompt.AppendLine();
            prompt.AppendLine("3. For each finding, provide:");
            prompt.AppendLine("   - Clear, actionable title");
            prompt.AppendLine("   - Detailed analysis with specific data points");
            prompt.AppendLine("   - Relevant metrics that support your conclusion");
            prompt.AppendLine("   - 3-4 specific, implementable recommendations");
            prompt.AppendLine();
            prompt.AppendLine("4. Focus on:");
            prompt.AppendLine("   - Cross-feature synergies and gaps");
            prompt.AppendLine("   - User journey optimization");
            prompt.AppendLine("   - Revenue and engagement opportunities");
            prompt.AppendLine("   - Scalability and sustainability concerns");
            prompt.AppendLine("   - Competitive positioning in fintech education");
            prompt.AppendLine();
            prompt.AppendLine("Provide detailed, data-driven insights that will help platform administrators make informed strategic decisions.");

            return prompt.ToString();
        }

        private void DisplayStructuredReport(string reportText)
        {
            try
            {
                // Extract health score
                var healthScore = ExtractHealthScore(reportText);

                // Register JavaScript to create the health chart
                var chartScript = $"setTimeout(() => {{ window.initializeHealthChart({healthScore}); }}, 100);";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HealthChartScript", chartScript, true);

                // Parse and format the structured report
                var (healthSummary, findings) = ParseStructuredReport(reportText);

                // Set the content
                litHealthSummary.Text = healthSummary;
                litKeyFindings.Text = findings;

                pnlLoading.Visible = false;
                pnlReport.Visible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Report display error: {ex.Message}");

                // Fallback display
                litHealthSummary.Text = "Unable to parse the detailed report structure. Here's the raw analysis:";
                litKeyFindings.Text = $"<div class='finding-card'><pre style='white-space: pre-wrap; font-family: inherit;'>{reportText}</pre></div>";

                pnlLoading.Visible = false;
                pnlReport.Visible = true;
            }
        }

        private int ExtractHealthScore(string reportText)
        {
            var match = Regex.Match(reportText, @"HEALTH_SCORE:\s*(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }

            // Fallback: look for percentage in text
            match = Regex.Match(reportText, @"(\d+)%.*health", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }

            return 75; // Default fallback
        }

        private (string healthSummary, string findings) ParseStructuredReport(string reportText)
        {
            var healthSummary = "";
            var findings = new StringBuilder();

            try
            {
                // Extract summary
                var summaryMatch = Regex.Match(reportText, @"SUMMARY:\s*(.*?)(?=FINDINGS:|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (summaryMatch.Success)
                {
                    healthSummary = summaryMatch.Groups[1].Value.Trim();
                }

                // Extract findings
                var findingsSection = Regex.Match(reportText, @"FINDINGS:(.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (findingsSection.Success)
                {
                    var findingsText = findingsSection.Groups[1].Value;
                    var individualFindings = findingsText.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var finding in individualFindings)
                    {
                        if (string.IsNullOrWhiteSpace(finding)) continue;

                        var findingHtml = ParseIndividualFinding(finding.Trim());
                        if (!string.IsNullOrEmpty(findingHtml))
                        {
                            findings.AppendLine(findingHtml);
                        }
                    }
                }

                // If structured parsing failed, create a fallback structure
                if (findings.Length == 0)
                {
                    findings.AppendLine(CreateFallbackFindings(reportText));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Report parsing error: {ex.Message}");
                findings.AppendLine(CreateFallbackFindings(reportText));
            }

            return (healthSummary, findings.ToString());
        }

        private string ParseIndividualFinding(string findingText)
        {
            try
            {
                var typeMatch = Regex.Match(findingText, @"FINDING_TYPE:\s*(POSITIVE|NEGATIVE|NEUTRAL|INFO)", RegexOptions.IgnoreCase);
                var titleMatch = Regex.Match(findingText, @"TITLE:\s*(.*?)(?=DESCRIPTION:|$)", RegexOptions.IgnoreCase);
                var descMatch = Regex.Match(findingText, @"DESCRIPTION:\s*(.*?)(?=METRICS:|RECOMMENDATIONS:|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var metricsMatch = Regex.Match(findingText, @"METRICS:\s*(.*?)(?=RECOMMENDATIONS:|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var recsMatch = Regex.Match(findingText, @"RECOMMENDATIONS:\s*(.*?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (!titleMatch.Success) return "";

                var type = typeMatch.Success ? typeMatch.Groups[1].Value.ToLower() : "info";
                var title = titleMatch.Groups[1].Value.Trim();
                var description = descMatch.Success ? descMatch.Groups[1].Value.Trim() : "";
                var metrics = metricsMatch.Success ? metricsMatch.Groups[1].Value.Trim() : "";
                var recommendations = recsMatch.Success ? recsMatch.Groups[1].Value.Trim() : "";

                var iconClass = type == "positive" ? "bi-check-circle" :
                               type == "negative" ? "bi-exclamation-triangle" :
                               type == "neutral" ? "bi-info-circle" : "bi-lightbulb";

                var html = new StringBuilder();
                html.AppendLine($"<div class='finding-card'>");
                html.AppendLine($"  <div class='finding-header'>");
                html.AppendLine($"    <div class='finding-icon {type}'><i class='{iconClass}'></i></div>");
                html.AppendLine($"    <h3 class='finding-title'>{title}</h3>");
                html.AppendLine($"  </div>");

                if (!string.IsNullOrEmpty(description))
                {
                    html.AppendLine($"  <p class='finding-description'>{description}</p>");
                }

                if (!string.IsNullOrEmpty(metrics))
                {
                    var metricItems = metrics.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (metricItems.Length > 0)
                    {
                        html.AppendLine($"  <div class='finding-metrics'>");
                        foreach (var metric in metricItems.Take(3)) // Limit to 3 metrics
                        {
                            html.AppendLine($"    <span class='metric-item'>{metric.Trim()}</span>");
                        }
                        html.AppendLine($"  </div>");
                    }
                }

                if (!string.IsNullOrEmpty(recommendations))
                {
                    html.AppendLine($"  <div class='recommendations'>");
                    html.AppendLine($"    <div class='recommendations-title'><i class='bi bi-arrow-right-circle'></i> Recommendations</div>");

                    var recItems = recommendations.Split(new[] { '\n', '•', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    if (recItems.Length > 0)
                    {
                        html.AppendLine($"    <ul>");
                        foreach (var rec in recItems)
                        {
                            var cleanRec = rec.Trim();
                            if (!string.IsNullOrEmpty(cleanRec))
                            {
                                html.AppendLine($"      <li>{cleanRec}</li>");
                            }
                        }
                        html.AppendLine($"    </ul>");
                    }

                    html.AppendLine($"  </div>");
                }

                html.AppendLine($"</div>");

                return html.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Individual finding parsing error: {ex.Message}");
                return "";
            }
        }

        private string CreateFallbackFindings(string reportText)
        {
            // Create a simple fallback structure when parsing fails
            return $@"
                <div class='finding-card'>
                    <div class='finding-header'>
                        <div class='finding-icon info'><i class='bi bi-info-circle'></i></div>
                        <h3 class='finding-title'>AI Analysis Report</h3>
                    </div>
                    <div class='finding-description'>
                        <pre style='white-space: pre-wrap; font-family: inherit; font-size: 0.95rem; line-height: 1.6;'>{reportText}</pre>
                    </div>
                </div>";
        }
    }
}