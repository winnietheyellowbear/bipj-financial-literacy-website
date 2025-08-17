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
                var forumData = GetForumActivityData();

                // Register JavaScript with all data (removed recent activity)
                string script = GenerateJavaScript(kpiData, userGrowthData, toolsUsageData, forumData);

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

                // Active Users (last 30 days)
                kpiData["activeUsers"] = ExecuteScalarQuery(conn, @"
                    SELECT COUNT(DISTINCT Id) FROM [User] 
                    WHERE LastLoginDate >= DATEADD(day, -30, GETDATE())");

                // Users with Jars
                kpiData["jarUsers"] = ExecuteScalarQuery(conn,
                    "SELECT COUNT(DISTINCT UserId) FROM [Jars] WHERE IsDeleted = 0");

                // Users with Goals
                kpiData["goalUsers"] = ExecuteScalarQuery(conn,
                    "SELECT COUNT(DISTINCT UserId) FROM [Goals] WHERE IsArchived = 0");

                // User Bookings (total count)
                kpiData["advisorBookings"] = ExecuteScalarQuery(conn,
                    "SELECT COUNT(*) FROM [Booking]");

                // Calculate proper growth percentages
                kpiData["activeUserGrowthPercentage"] = CalculateActiveUserGrowth(conn);
                kpiData["jarAdoptionPercentage"] = CalculateJarAdoptionGrowth(conn);
                kpiData["goalGrowthPercentage"] = CalculateGoalGrowth(conn);
                kpiData["bookingGrowthPercentage"] = CalculateBookingGrowth(conn);
            }

            return kpiData;
        }

        private double CalculateActiveUserGrowth(SqlConnection conn)
        {
            try
            {
                string query = @"
                    DECLARE @Current30Days INT, @Previous30Days INT;
                    
                    SELECT @Current30Days = COUNT(DISTINCT Id) 
                    FROM [User] 
                    WHERE LastLoginDate >= DATEADD(day, -30, GETDATE())
                    AND LastLoginDate IS NOT NULL;
                    
                    SELECT @Previous30Days = COUNT(DISTINCT Id) 
                    FROM [User] 
                    WHERE LastLoginDate >= DATEADD(day, -60, GETDATE()) 
                    AND LastLoginDate < DATEADD(day, -30, GETDATE())
                    AND LastLoginDate IS NOT NULL;
                    
                    SELECT @Current30Days as CurrentCount, @Previous30Days as PreviousCount;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int current = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        int previous = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                        if (previous > 0)
                        {
                            return Math.Round(((double)(current - previous) / previous) * 100, 1);
                        }
                        else if (current > 0)
                        {
                            return 100.0; // If no previous data but current exists, show 100% growth
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
                    DECLARE @Current30Days INT, @Previous30Days INT;
                    
                    SELECT @Current30Days = COUNT(DISTINCT UserId) 
                    FROM [Jars] 
                    WHERE CreatedAt >= DATEADD(day, -30, GETDATE()) 
                    AND IsDeleted = 0;
                    
                    SELECT @Previous30Days = COUNT(DISTINCT UserId) 
                    FROM [Jars] 
                    WHERE CreatedAt >= DATEADD(day, -60, GETDATE()) 
                    AND CreatedAt < DATEADD(day, -30, GETDATE()) 
                    AND IsDeleted = 0;
                    
                    SELECT @Current30Days as CurrentCount, @Previous30Days as PreviousCount;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int current = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        int previous = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                        if (previous > 0)
                        {
                            return Math.Round(((double)(current - previous) / previous) * 100, 1);
                        }
                        else if (current > 0)
                        {
                            return 100.0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Jar adoption growth calculation error: " + ex.Message);
            }

            return 0.0;
        }

        private double CalculateGoalGrowth(SqlConnection conn)
        {
            try
            {
                string query = @"
                    DECLARE @Current30Days INT, @Previous30Days INT;
                    
                    SELECT @Current30Days = COUNT(DISTINCT UserId) 
                    FROM [Goals] 
                    WHERE CreatedAt >= DATEADD(day, -30, GETDATE()) 
                    AND IsArchived = 0;
                    
                    SELECT @Previous30Days = COUNT(DISTINCT UserId) 
                    FROM [Goals] 
                    WHERE CreatedAt >= DATEADD(day, -60, GETDATE()) 
                    AND CreatedAt < DATEADD(day, -30, GETDATE()) 
                    AND IsArchived = 0;
                    
                    SELECT @Current30Days as CurrentCount, @Previous30Days as PreviousCount;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int current = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        int previous = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                        if (previous > 0)
                        {
                            return Math.Round(((double)(current - previous) / previous) * 100, 1);
                        }
                        else if (current > 0)
                        {
                            return 100.0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Goal growth calculation error: " + ex.Message);
            }

            return 0.0;
        }

        private double CalculateBookingGrowth(SqlConnection conn)
        {
            try
            {
                string query = @"
                    DECLARE @Current30Days INT, @Previous30Days INT;
                    
                    SELECT @Current30Days = COUNT(*) 
                    FROM [Booking] 
                    WHERE CreatedAt >= DATEADD(day, -30, GETDATE());
                    
                    SELECT @Previous30Days = COUNT(*) 
                    FROM [Booking] 
                    WHERE CreatedAt >= DATEADD(day, -60, GETDATE()) 
                    AND CreatedAt < DATEADD(day, -30, GETDATE());
                    
                    SELECT @Current30Days as CurrentCount, @Previous30Days as PreviousCount;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int current = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        int previous = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                        if (previous > 0)
                        {
                            return Math.Round(((double)(current - previous) / previous) * 100, 1);
                        }
                        else if (current > 0)
                        {
                            return 100.0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Booking growth calculation error: " + ex.Message);
            }

            return 0.0;
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
                            // Removed engagementScore from here
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
                    SELECT COUNT(DISTINCT Id) FROM [User] 
                    WHERE MONTH(LastLoginDate) = @month 
                    AND YEAR(LastLoginDate) = @year
                    AND LastLoginDate IS NOT NULL";

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

                // Get usage statistics for different financial tools
                var toolsData = new Dictionary<string, int>
                {
                    ["Jar System"] = ExecuteScalarQuery(conn, "SELECT COUNT(DISTINCT UserId) FROM [Jars] WHERE IsDeleted = 0"),
                    ["Goal Setting"] = ExecuteScalarQuery(conn, "SELECT COUNT(DISTINCT UserId) FROM [Goals] WHERE IsArchived = 0"),
                    ["Portfolio Builder"] = ExecuteScalarQuery(conn, "SELECT COUNT(DISTINCT UserID) FROM [Portfolios]"),
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

        private List<object> GetForumActivityData()
        {
            var data = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    WITH WeeklyData AS (
                        SELECT 
                            'Week ' + CAST(DATEDIFF(week, DATEADD(week, -4, GETDATE()), GETDATE()) + 1 - DATEDIFF(week, p.Post_DateTime, GETDATE()) AS VARCHAR) as Week,
                            DATEPART(week, p.Post_DateTime) as WeekNum,
                            p.Post_ID
                        FROM [Post] p
                        WHERE p.Post_DateTime >= DATEADD(week, -4, GETDATE())
                    )
                    SELECT 
                        wd.Week,
                        COUNT(DISTINCT wd.Post_ID) as Posts,
                        COUNT(DISTINCT c.Comment_ID) as Comments,
                        COUNT(DISTINCT l.Like_ID) as Likes
                    FROM WeeklyData wd
                    LEFT JOIN [Comment] c ON wd.Post_ID = c.Post_ID 
                        AND c.Comment_DateTime >= DATEADD(week, -4, GETDATE())
                    LEFT JOIN [Like] l ON wd.Post_ID = l.Post_ID 
                        AND l.Like_DateTime >= DATEADD(week, -4, GETDATE())
                    GROUP BY wd.Week, wd.WeekNum
                    ORDER BY wd.WeekNum";

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
            List<object> forumData)
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
                    document.getElementById('activeUsers').textContent = '{kpiData["activeUsers"]:N0}';
                    document.getElementById('jarUsers').textContent = '{kpiData["jarUsers"]:N0}';
                    document.getElementById('goalUsers').textContent = '{kpiData["goalUsers"]:N0}';
                    document.getElementById('advisorBookings').textContent = '{kpiData["advisorBookings"]:N0}';
                    
                    // Update growth percentages
                    updateGrowthIndicator('activeUserGrowthChange', {kpiData["activeUserGrowthPercentage"]});
                    updateGrowthIndicator('jarAdoptionChange', {kpiData["jarAdoptionPercentage"]});
                    updateGrowthIndicator('goalGrowthChange', {kpiData["goalGrowthPercentage"]});
                    updateGrowthIndicator('bookingGrowthChange', {kpiData["bookingGrowthPercentage"]});
                }}

                function updateGrowthIndicator(elementId, percentage) {{
                    const element = document.getElementById(elementId);
                    const isPositive = percentage >= 0;
                    element.className = 'kpi-change ' + (isPositive ? 'positive' : 'negative');
                    element.innerHTML = `<i class='bi bi-arrow-${{isPositive ? 'up' : 'down'}}'></i> ${{isPositive ? '+' : ''}}${{percentage}}% from last 30 days`;
                }}

                // User Growth & Engagement Chart (REMOVED ENGAGEMENT SCORE)
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
                            // REMOVED the engagement score dataset completely
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
                                borderWidth: 3,
                                borderColor: '#fff'
                            }}]
                        }},
                        options: {{
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {{
                                legend: {{
                                    position: 'bottom',
                                    labels: {{ 
                                        padding: 20,
                                        usePointStyle: true,
                                        font: {{
                                            size: 12
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    }});
                }}

                // Community Engagement Chart
                function createForumChart() {{
                    const ctx = document.getElementById('forumChart').getContext('2d');
                    const forumData = {serializer.Serialize(forumData)};
                    
                    new Chart(ctx, {{
                        type: 'bar',
                        data: {{
                            labels: forumData.map(d => d.week),
                            datasets: [{{
                                label: 'Posts',
                                data: forumData.map(d => d.posts),
                                backgroundColor: chartColors.primary + '80',
                                borderColor: chartColors.primary,
                                borderWidth: 2,
                                borderRadius: 8
                            }}, {{
                                label: 'Comments',
                                data: forumData.map(d => d.comments),
                                backgroundColor: chartColors.success + '80',
                                borderColor: chartColors.success,
                                borderWidth: 2,
                                borderRadius: 8
                            }}, {{
                                label: 'Likes',
                                data: forumData.map(d => d.likes),
                                backgroundColor: chartColors.warning + '80',
                                borderColor: chartColors.warning,
                                borderWidth: 2,
                                borderRadius: 8
                            }}]
                        }},
                        options: {{
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {{ 
                                legend: {{ 
                                    position: 'top',
                                    labels: {{
                                        usePointStyle: true,
                                        padding: 20
                                    }}
                                }}
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

                // Initialize all charts when page loads
                document.addEventListener('DOMContentLoaded', function() {{
                    updateKPIValues();
                    createUserGrowthChart();
                    createToolsUsageChart();
                    createForumChart();
                    
                    console.log('Dashboard initialized successfully');
                }});
                
                // Simulate real-time updates every 30 seconds
                setInterval(() => {{
                    // Update KPI values with slight variations (simulate real-time data)
                    const activeUsersElement = document.getElementById('activeUsers');
                    if (activeUsersElement) {{
                        const currentUsers = parseInt(activeUsersElement.textContent.replace(/,/g, ''));
                        const randomIncrease = Math.floor(Math.random() * 3);
                        if (randomIncrease > 0) {{
                            activeUsersElement.textContent = (currentUsers + randomIncrease).toLocaleString();
                        }}
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