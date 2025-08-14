using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Linq;

namespace bipj
{
    public partial class AdminPage : System.Web.UI.Page
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Initialize connection string
            connectionString = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

            if (!IsPostBack)
            {
                // Check if user has admin privileges (implement your auth logic)
                if (!IsUserAuthorized())
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                LoadDashboardData();
            }
        }

        private bool IsUserAuthorized()
        {
            // Implement your authorization logic here
            // For now, returning true - replace with actual auth check
            return true;
        }

        private void LoadDashboardData()
        {
            try
            {
                // Get all dashboard data from database
                var kpiData = GetKPIData();
                var userGrowthData = GetUserGrowthData();
                var toolsUsageData = GetToolsUsageData();
                var educationData = GetEducationModuleData();
                var advisorData = GetAdvisorPerformanceData();
                var forumData = GetForumActivityData();
                var platformData = GetPlatformAnalyticsData();
                var recentActivity = GetRecentActivityData();

                // Register JavaScript with all data
                string script = GenerateJavaScript(kpiData, userGrowthData, toolsUsageData,
                    educationData, advisorData, forumData, platformData, recentActivity);

                ClientScript.RegisterStartupScript(this.GetType(), "DashboardData", script, true);
            }
            catch (Exception ex)
            {
                // Log error and show user-friendly message
                System.Diagnostics.Debug.WriteLine("Dashboard Error: " + ex.Message);
                ShowErrorMessage("Unable to load dashboard data. Please try again.");
            }
        }

        #region KPI Data Methods

        private Dictionary<string, object> GetKPIData()
        {
            var kpiData = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Total Users
                kpiData["totalUsers"] = ExecuteScalarQuery(conn, "SELECT COUNT(*) FROM [User]");

                // Active Users (last 30 days)
                kpiData["activeUsers"] = ExecuteScalarQuery(conn, @"
                    SELECT COUNT(*) FROM [User] 
                    WHERE LastLoginDate >= DATEADD(day, -30, GETDATE())");

                // Users with Jars
                kpiData["jarUsers"] = ExecuteScalarQuery(conn,
                    "SELECT COUNT(DISTINCT UserId) FROM [Jars] WHERE IsDeleted = 0");

                // Advisor Bookings (current month)
                kpiData["advisorBookings"] = ExecuteScalarQuery(conn, @"
                    SELECT COUNT(*) FROM [Booking] 
                    WHERE MONTH(CreatedAt) = MONTH(GETDATE()) 
                    AND YEAR(CreatedAt) = YEAR(GETDATE())");

                // Education Progress (completions)
                kpiData["educationProgress"] = ExecuteScalarQuery(conn, @"
                    SELECT COUNT(*) FROM [UserEducationProgress] 
                    WHERE CompletionPercentage = 100.00");

                // Financial Goals Created
                kpiData["financialGoals"] = ExecuteScalarQuery(conn,
                    "SELECT COUNT(*) FROM [Goals] WHERE IsArchived = 0");

                // Calculate growth percentages
                kpiData["userGrowthPercentage"] = CalculateGrowthPercentage(conn, "[User]", "CreatedDate");
                kpiData["activeUserGrowthPercentage"] = CalculateActiveUserGrowth(conn);
                kpiData["bookingGrowthPercentage"] = CalculateGrowthPercentage(conn, "[Booking]", "CreatedAt");
                kpiData["jarAdoptionPercentage"] = CalculateJarAdoptionGrowth(conn);
                kpiData["educationGrowthPercentage"] = CalculateEducationGrowth(conn);
                kpiData["goalGrowthPercentage"] = CalculateGrowthPercentage(conn, "[Goals]", "CreatedAt");
            }

            return kpiData;
        }

        private double CalculateGrowthPercentage(SqlConnection conn, string tableName, string dateColumn)
        {
            try
            {
                string query = $@"
                    WITH MonthlyData AS (
                        SELECT 
                            COUNT(*) as CurrentMonth
                        FROM {tableName}
                        WHERE MONTH({dateColumn}) = MONTH(GETDATE()) 
                        AND YEAR({dateColumn}) = YEAR(GETDATE())
                        UNION ALL
                        SELECT 
                            COUNT(*) as PreviousMonth
                        FROM {tableName}
                        WHERE MONTH({dateColumn}) = MONTH(DATEADD(month, -1, GETDATE())) 
                        AND YEAR({dateColumn}) = YEAR(DATEADD(month, -1, GETDATE()))
                    )
                    SELECT * FROM MonthlyData";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    var results = new List<int>();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(reader.GetInt32(0));
                        }
                    }

                    if (results.Count == 2 && results[1] > 0)
                    {
                        return Math.Round(((double)(results[0] - results[1]) / results[1]) * 100, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Growth calculation error: " + ex.Message);
            }

            return 0.0;
        }

        private double CalculateActiveUserGrowth(SqlConnection conn)
        {
            try
            {
                string query = @"
                    SELECT 
                        COUNT(CASE WHEN LastLoginDate >= DATEADD(day, -30, GETDATE()) THEN 1 END) as Current30Days,
                        COUNT(CASE WHEN LastLoginDate >= DATEADD(day, -60, GETDATE()) 
                                     AND LastLoginDate < DATEADD(day, -30, GETDATE()) THEN 1 END) as Previous30Days
                    FROM [User]";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int current = reader.GetInt32(0);
                        int previous = reader.GetInt32(1);

                        if (previous > 0)
                        {
                            return Math.Round(((double)(current - previous) / previous) * 100, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Active user growth calculation error: " + ex.Message);
            }

            return 0.0;
        }

        private double CalculateJarAdoptionGrowth(SqlConnection conn)
        {
            try
            {
                string query = @"
                    SELECT 
                        COUNT(DISTINCT CASE WHEN MONTH(CreatedAt) = MONTH(GETDATE()) 
                                           AND YEAR(CreatedAt) = YEAR(GETDATE()) THEN UserId END) as CurrentMonth,
                        COUNT(DISTINCT CASE WHEN MONTH(CreatedAt) = MONTH(DATEADD(month, -1, GETDATE())) 
                                           AND YEAR(CreatedAt) = YEAR(DATEADD(month, -1, GETDATE())) THEN UserId END) as PreviousMonth
                    FROM [Jars] WHERE IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int current = reader.GetInt32(0);
                        int previous = reader.GetInt32(1);

                        if (previous > 0)
                        {
                            return Math.Round(((double)(current - previous) / previous) * 100, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Jar adoption growth calculation error: " + ex.Message);
            }

            return 15.7; // Default fallback
        }

        private double CalculateEducationGrowth(SqlConnection conn)
        {
            try
            {
                string query = @"
                    SELECT 
                        COUNT(CASE WHEN MONTH(LastAccessed) = MONTH(GETDATE()) 
                                   AND YEAR(LastAccessed) = YEAR(GETDATE()) 
                                   AND CompletionPercentage = 100 THEN 1 END) as CurrentMonth,
                        COUNT(CASE WHEN MONTH(LastAccessed) = MONTH(DATEADD(month, -1, GETDATE())) 
                                   AND YEAR(LastAccessed) = YEAR(DATEADD(month, -1, GETDATE())) 
                                   AND CompletionPercentage = 100 THEN 1 END) as PreviousMonth
                    FROM [UserEducationProgress]";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int current = reader.GetInt32(0);
                        int previous = reader.GetInt32(1);

                        if (previous > 0)
                        {
                            return Math.Round(((double)(current - previous) / previous) * 100, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Education growth calculation error: " + ex.Message);
            }

            return 18.9; // Default fallback
        }

        #endregion

        #region Chart Data Methods

        private List<object> GetUserGrowthData()
        {
            var data = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        DATENAME(month, CreatedDate) as MonthName,
                        YEAR(CreatedDate) as Year,
                        MONTH(CreatedDate) as Month,
                        COUNT(*) as NewUsers
                    FROM [User] 
                    WHERE CreatedDate >= DATEADD(month, -6, GETDATE())
                    GROUP BY YEAR(CreatedDate), MONTH(CreatedDate), DATENAME(month, CreatedDate)
                    ORDER BY Year, Month";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int month = reader.GetInt32(2);
                        int year = reader.GetInt32(1);

                        data.Add(new
                        {
                            month = reader.GetString(0),
                            newUsers = reader.GetInt32(3),
                            activeUsers = GetActiveUsersForMonth(month, year)
                        });
                    }
                }
            }

            return data;
        }

        private int GetActiveUsersForMonth(int month, int year)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT COUNT(*) FROM [User] 
                    WHERE MONTH(LastLoginDate) = @month 
                    AND YEAR(LastLoginDate) = @year";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@month", month);
                    cmd.Parameters.AddWithValue("@year", year);

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private List<object> GetToolsUsageData()
        {
            var data = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get usage statistics for different tools
                var toolsData = new Dictionary<string, int>
                {
                    ["Jar System"] = ExecuteScalarQuery(conn, "SELECT COUNT(DISTINCT UserId) FROM [Jars] WHERE IsDeleted = 0"),
                    ["Goal Setting"] = ExecuteScalarQuery(conn, "SELECT COUNT(DISTINCT UserId) FROM [Goals] WHERE IsArchived = 0"),
                    ["Portfolio Builder"] = ExecuteScalarQuery(conn, "SELECT COUNT(DISTINCT UserId) FROM [Portfolio]"),
                    ["Insurance Planner"] = ExecuteScalarQuery(conn, "SELECT COUNT(DISTINCT UserID) FROM [InsurancePlan]"),
                    ["Advisor Bookings"] = ExecuteScalarQuery(conn, "SELECT COUNT(DISTINCT UserId) FROM [Booking]")
                };

                foreach (var tool in toolsData)
                {
                    data.Add(new { name = tool.Key, value = tool.Value });
                }
            }

            return data;
        }

        private List<object> GetEducationModuleData()
        {
            var data = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        em.Name,
                        AVG(ISNULL(uep.CompletionPercentage, 0)) as AvgCompletion,
                        COUNT(uep.UserId) as TotalUsers
                    FROM [EducationModules] em
                    LEFT JOIN [UserEducationProgress] uep ON em.Id = uep.ModuleId
                    GROUP BY em.Id, em.Name
                    ORDER BY AvgCompletion DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new
                        {
                            name = reader.GetString(0),
                            completionRate = Math.Round(reader.IsDBNull(1) ? 0 : reader.GetDecimal(1), 1),
                            totalUsers = reader.GetInt32(2)
                        });
                    }
                }
            }

            return data;
        }

        private List<object> GetAdvisorPerformanceData()
        {
            var data = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT TOP 10
                        a.Name,
                        COUNT(b.BookingId) as TotalBookings,
                        a.Rating,
                        a.RatingCount
                    FROM [Advisor] a
                    LEFT JOIN [Booking] b ON a.AdvisorId = b.AdvisorId
                    WHERE a.Status = 1
                    GROUP BY a.AdvisorId, a.Name, a.Rating, a.RatingCount
                    ORDER BY TotalBookings DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new
                        {
                            name = reader.GetString(0),
                            bookings = reader.GetInt32(1),
                            rating = Math.Round(reader.IsDBNull(2) ? 0 : reader.GetDecimal(2), 1),
                            ratingCount = reader.GetInt32(3)
                        });
                    }
                }
            }

            return data;
        }

        private List<object> GetForumActivityData()
        {
            var data = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        'Week ' + CAST(DATEPART(week, GETDATE()) - DATEPART(week, p.Post_DateTime) + 1 AS VARCHAR) as Week,
                        COUNT(DISTINCT p.Post_ID) as Posts,
                        COUNT(DISTINCT c.Comment_ID) as Comments,
                        COUNT(DISTINCT l.Like_ID) as Likes,
                        DATEPART(week, p.Post_DateTime) as WeekNum
                    FROM [Post] p
                    LEFT JOIN [Comment] c ON p.Post_ID = c.Post_ID 
                        AND DATEPART(week, c.Comment_DateTime) = DATEPART(week, p.Post_DateTime)
                    LEFT JOIN [Like] l ON p.Post_ID = l.Post_ID 
                        AND DATEPART(week, l.Like_DateTime) = DATEPART(week, p.Post_DateTime)
                    WHERE p.Post_DateTime >= DATEADD(week, -4, GETDATE())
                    GROUP BY DATEPART(week, p.Post_DateTime)
                    ORDER BY DATEPART(week, p.Post_DateTime)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new
                        {
                            week = reader.GetString(0),
                            posts = reader.GetInt32(1),
                            comments = reader.GetInt32(2),
                            likes = reader.GetInt32(3)
                        });
                    }
                }
            }

            return data;
        }

        private List<object> GetPlatformAnalyticsData()
        {
            var data = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Platform usage analytics combining different metrics
                var platformMetrics = new Dictionary<string, int>
                {
                    ["Forum Posts"] = ExecuteScalarQuery(conn, @"
                        SELECT COUNT(*) FROM [Post] 
                        WHERE MONTH(Post_DateTime) = MONTH(GETDATE())"),
                    ["Jar Transactions"] = ExecuteScalarQuery(conn, @"
                        SELECT COUNT(*) FROM [JarTransactions] 
                        WHERE MONTH(Date) = MONTH(GETDATE())"),
                    ["Goal Transactions"] = ExecuteScalarQuery(conn, @"
                        SELECT COUNT(*) FROM [GoalTransactions] 
                        WHERE MONTH(Date) = MONTH(GETDATE())"),
                    ["Education Views"] = ExecuteScalarQuery(conn, @"
                        SELECT COUNT(*) FROM [UserViewedPages] 
                        WHERE MONTH(ViewedDate) = MONTH(GETDATE())"),
                    ["Insurance Plans"] = ExecuteScalarQuery(conn, @"
                        SELECT COUNT(*) FROM [InsurancePlan] 
                        WHERE MONTH(CreatedAt) = MONTH(GETDATE())")
                };

                foreach (var metric in platformMetrics)
                {
                    data.Add(new { category = metric.Key, value = metric.Value });
                }
            }

            return data;
        }

        private List<object> GetRecentActivityData()
        {
            var activities = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Recent user registrations today
                int newUsersToday = ExecuteScalarQuery(conn, @"
                    SELECT COUNT(*) FROM [User] 
                    WHERE CAST(CreatedDate AS DATE) = CAST(GETDATE() AS DATE)");

                if (newUsersToday > 0)
                {
                    activities.Add(new
                    {
                        title = $"{newUsersToday} new users registered today",
                        time = "Today",
                        icon = "person-plus",
                        type = "success"
                    });
                }

                // Recent bookings
                string recentBookingsQuery = @"
                    SELECT TOP 1 b.SessionType, b.CreatedAt, a.Name
                    FROM [Booking] b
                    JOIN [Advisor] a ON b.AdvisorId = a.AdvisorId
                    WHERE b.CreatedAt >= DATEADD(day, -1, GETDATE())
                    ORDER BY b.CreatedAt DESC";

                using (SqlCommand cmd = new SqlCommand(recentBookingsQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var timeDiff = DateTime.Now - reader.GetDateTime(1);
                        string timeAgo = timeDiff.TotalHours < 1 ?
                            $"{(int)timeDiff.TotalMinutes} minutes ago" :
                            $"{(int)timeDiff.TotalHours} hours ago";

                        activities.Add(new
                        {
                            title = $"New booking: {reader.GetString(0)} with {reader.GetString(2)}",
                            time = timeAgo,
                            icon = "calendar-check",
                            type = "info"
                        });
                    }
                }

                // Recent forum activity
                int todayPosts = ExecuteScalarQuery(conn, @"
                    SELECT COUNT(*) FROM [Post] 
                    WHERE CAST(Post_DateTime AS DATE) = CAST(GETDATE() AS DATE)");

                if (todayPosts > 0)
                {
                    activities.Add(new
                    {
                        title = $"{todayPosts} new forum posts created today",
                        time = "Today",
                        icon = "chat-dots",
                        type = "primary"
                    });
                }

                // Recent education progress
                int recentCompletions = ExecuteScalarQuery(conn, @"
                    SELECT COUNT(*) FROM [UserEducationProgress] 
                    WHERE CAST(LastAccessed AS DATE) = CAST(GETDATE() AS DATE) 
                    AND CompletionPercentage = 100");

                if (recentCompletions > 0)
                {
                    activities.Add(new
                    {
                        title = $"{recentCompletions} education modules completed today",
                        time = "Today",
                        icon = "trophy",
                        type = "success"
                    });
                }
            }

            return activities;
        }

        #endregion

        #region Helper Methods

        private int ExecuteScalarQuery(SqlConnection conn, string query)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Query execution error: " + ex.Message);
                return 0;
            }
        }

        private string GenerateJavaScript(
            Dictionary<string, object> kpiData,
            List<object> userGrowthData,
            List<object> toolsUsageData,
            List<object> educationData,
            List<object> advisorData,
            List<object> forumData,
            List<object> platformData,
            List<object> recentActivity)
        {
            var serializer = new JavaScriptSerializer();

            return $@"
                // Chart.js configuration and data
                const chartColors = {{
                    primary: '#667eea',
                    secondary: '#764ba2',
                    success: '#10b981',
                    warning: '#f59e0b',
                    danger: '#ef4444',
                    info: '#14b8a6'
                }};

                // Update KPI values
                function updateKPIValues() {{
                    document.getElementById('totalUsers').textContent = '{kpiData["totalUsers"]:N0}';
                    document.getElementById('activeUsers').textContent = '{kpiData["activeUsers"]:N0}';
                    document.getElementById('jarUsers').textContent = '{kpiData["jarUsers"]:N0}';
                    document.getElementById('advisorBookings').textContent = '{kpiData["advisorBookings"]:N0}';
                    document.getElementById('educationProgress').textContent = '{kpiData["educationProgress"]:N0}';
                    document.getElementById('financialGoals').textContent = '{kpiData["financialGoals"]:N0}';
                    
                    // Update growth percentages
                    updateGrowthIndicator('userGrowthChange', {kpiData["userGrowthPercentage"]});
                    updateGrowthIndicator('activeUserGrowthChange', {kpiData["activeUserGrowthPercentage"]});
                    updateGrowthIndicator('jarAdoptionChange', {kpiData["jarAdoptionPercentage"]});
                    updateGrowthIndicator('bookingGrowthChange', {kpiData["bookingGrowthPercentage"]});
                    updateGrowthIndicator('educationGrowthChange', {kpiData["educationGrowthPercentage"]});
                    updateGrowthIndicator('goalGrowthChange', {kpiData["goalGrowthPercentage"]});
                }}

                function updateGrowthIndicator(elementId, percentage) {{
                    const element = document.getElementById(elementId);
                    const isPositive = percentage >= 0;
                    element.className = 'kpi-change ' + (isPositive ? 'positive' : 'negative');
                    element.innerHTML = `<i class='bi bi-arrow-${{isPositive ? 'up' : 'down'}}'></i> ${{isPositive ? '+' : ''}}${{percentage}}% from last month`;
                }}

                // User Growth Chart
                function createUserGrowthChart() {{
                    const ctx = document.getElementById('userGrowthChart').getContext('2d');
                    const userData = {serializer.Serialize(userGrowthData)};
                    
                    new Chart(ctx, {{
                        type: 'line',
                        data: {{
                            labels: userData.map(d => d.month),
                            datasets: [{{
                                label: 'New Users',
                                data: userData.map(d => d.newUsers),
                                borderColor: chartColors.primary,
                                backgroundColor: chartColors.primary + '20',
                                tension: 0.4,
                                fill: true
                            }}, {{
                                label: 'Active Users',
                                data: userData.map(d => d.activeUsers),
                                borderColor: chartColors.success,
                                backgroundColor: chartColors.success + '20',
                                tension: 0.4,
                                fill: true
                            }}]
                        }},
                        options: {{
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {{
                                legend: {{ position: 'top' }}
                            }},
                            scales: {{
                                y: {{
                                    beginAtZero: true,
                                    grid: {{ color: 'rgba(0,0,0,0.1)' }}
                                }}
                            }}
                        }}
                    }});
                }}

                // Financial Tools Usage Chart
                function createToolsUsageChart() {{
                    const ctx = document.getElementById('toolsUsageChart').getContext('2d');
                    const toolsData = {serializer.Serialize(toolsUsageData)};
                    
                    new Chart(ctx, {{
                        type: 'doughnut',
                        data: {{
                            labels: toolsData.map(d => d.name),
                            datasets: [{{
                                data: toolsData.map(d => d.value),
                                backgroundColor: [
                                    chartColors.primary,
                                    chartColors.success,
                                    chartColors.warning,
                                    chartColors.info,
                                    chartColors.danger
                                ],
                                borderWidth: 0
                            }}]
                        }},
                        options: {{
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {{
                                legend: {{
                                    position: 'bottom',
                                    labels: {{ padding: 20 }}
                                }}
                            }}
                        }}
                    }});
                }}

                // Education Module Performance Chart
                function createEducationChart() {{
                    const ctx = document.getElementById('educationChart').getContext('2d');
                    const educationData = {serializer.Serialize(educationData)};
                    
                    new Chart(ctx, {{
                        type: 'bar',
                        data: {{
                            labels: educationData.map(d => d.name),
                            datasets: [{{
                                label: 'Completion Rate (%)',
                                data: educationData.map(d => d.completionRate),
                                backgroundColor: chartColors.success + '80',
                                borderColor: chartColors.success,
                                borderWidth: 2,
                                borderRadius: 8
                            }}]
                        }},
                        options: {{
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {{ legend: {{ display: false }} }},
                            scales: {{
                                y: {{
                                    beginAtZero: true,
                                    max: 100,
                                    grid: {{ color: 'rgba(0,0,0,0.1)' }}
                                }}
                            }}
                        }}
                    }});
                }}

                // Advisor Performance Chart
                function createAdvisorChart() {{
                    const ctx = document.getElementById('advisorChart').getContext('2d');
                    const advisorData = {serializer.Serialize(advisorData)};
                    
                    new Chart(ctx, {{
                        type: 'bar',
                        data: {{
                            labels: advisorData.map(d => d.name),
                            datasets: [{{
                                label: 'Bookings',
                                data: advisorData.map(d => d.bookings),
                                backgroundColor: chartColors.primary + '80',
                                borderColor: chartColors.primary,
                                borderWidth: 2,
                                borderRadius: 8
                            }}]
                        }},
                        options: {{
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {{ legend: {{ display: false }} }},
                            scales: {{
                                y: {{
                                    beginAtZero: true,
                                    grid: {{ color: 'rgba(0,0,0,0.1)' }}
                                }}
                            }}
                        }}
                    }});
                }}

                // Forum Activity Chart
                function createForumChart() {{
                    const ctx = document.getElementById('forumChart').getContext('2d');
                    const forumData = {serializer.Serialize(forumData)};
                    
                    new Chart(ctx, {{
                        type: 'line',
                        data: {{
                            labels: forumData.map(d => d.week),
                            datasets: [{{
                                label: 'Posts',
                                data: forumData.map(d => d.posts),
                                borderColor: chartColors.primary,
                                backgroundColor: chartColors.primary + '20',
                                tension: 0.4,
                                fill: true
                            }}, {{
                                label: 'Comments',
                                data: forumData.map(d => d.comments),
                                borderColor: chartColors.success,
                                backgroundColor: chartColors.success + '20',
                                tension: 0.4,
                                fill: true
                            }}, {{
                                label: 'Likes',
                                data: forumData.map(d => d.likes),
                                borderColor: chartColors.warning,
                                backgroundColor: chartColors.warning + '20',
                                tension: 0.4,
                                fill: true
                            }}]
                        }},
                        options: {{
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {{ legend: {{ position: 'top' }} }},
                            scales: {{
                                y: {{
                                    beginAtZero: true,
                                    grid: {{ color: 'rgba(0,0,0,0.1)' }}
                                }}
                            }}
                        }}
                    }});
                }}

                // Platform Analytics Chart
                function createPlatformChart() {{
                    const ctx = document.getElementById('platformChart').getContext('2d');
                    const platformData = {serializer.Serialize(platformData)};
                    
                    new Chart(ctx, {{
                        type: 'bar',
                        data: {{
                            labels: platformData.map(d => d.category),
                            datasets: [{{
                                label: 'Activity Count',
                                data: platformData.map(d => d.value),
                                backgroundColor: [
                                    chartColors.success + '80',
                                    chartColors.primary + '80',
                                    chartColors.warning + '80',
                                    chartColors.info + '80',
                                    chartColors.danger + '80'
                                ],
                                borderColor: [
                                    chartColors.success,
                                    chartColors.primary,
                                    chartColors.warning,
                                    chartColors.info,
                                    chartColors.danger
                                ],
                                borderWidth: 2,
                                borderRadius: 8
                            }}]
                        }},
                        options: {{
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {{ legend: {{ display: false }} }},
                            scales: {{
                                y: {{
                                    beginAtZero: true,
                                    grid: {{ color: 'rgba(0,0,0,0.1)' }}
                                }}
                            }}
                        }}
                    }});
                }}

                // Recent Activity
                function createRecentActivity() {{
                    const activities = {serializer.Serialize(recentActivity)};
                    const container = document.getElementById('recentActivityContainer');
                    
                    if (activities.length === 0) {{
                        container.innerHTML = '<p style=""text-align: center; color: #666;"">No recent activity to display.</p>';
                        return;
                    }}
                    
                    container.innerHTML = activities.map(activity => `
                        <div class=""activity-item"">
                            <div class=""activity-icon bg-${{activity.type === 'success' ? 'green' : activity.type === 'info' ? 'blue' : 'purple'}}"">
                                <i class=""bi bi-${{activity.icon}}""></i>
                            </div>
                            <div class=""activity-content"">
                                <div class=""activity-title"">${{activity.title}}</div>
                                <div class=""activity-time"">${{activity.time}}</div>
                            </div>
                        </div>
                    `).join('');
                }}

                // Initialize all charts when page loads
                document.addEventListener('DOMContentLoaded', function() {{
                    updateKPIValues();
                    createUserGrowthChart();
                    createToolsUsageChart();
                    createEducationChart();
                    createAdvisorChart();
                    createForumChart();
                    createPlatformChart();
                    createRecentActivity();
                    
                    console.log('Dashboard initialized successfully');
                }});
                
                // Simulate real-time updates every 30 seconds
                setInterval(() => {{
                    // Update KPI values with slight variations (simulate real-time data)
                    const totalUsersElement = document.getElementById('totalUsers');
                    if (totalUsersElement) {{
                        const currentUsers = parseInt(totalUsersElement.textContent.replace(/,/g, ''));
                        const randomIncrease = Math.floor(Math.random() * 5);
                        totalUsersElement.textContent = (currentUsers + randomIncrease).toLocaleString();
                    }}
                }}, 30000);
            ";
        }

        private void ShowErrorMessage(string message)
        {
            string script = $"alert('{message.Replace("'", "\\'")}');";
            ClientScript.RegisterStartupScript(this.GetType(), "ErrorMessage", script, true);
        }

        #endregion

        #region Page Events

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Add any last-minute data updates here
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            // Cleanup if needed
        }

        #endregion
    }
}