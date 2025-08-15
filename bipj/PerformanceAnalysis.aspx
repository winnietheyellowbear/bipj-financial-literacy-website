<%@ Page Title="Performance Analysis" Language="C#" MasterPageFile="~/Staff_Nav.Master" AutoEventWireup="true" CodeBehind="PerformanceAnalysis.aspx.cs" Inherits="bipj.PerformanceAnalysis" Async="true" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chartjs-plugin-datalabels"></script>
    <style>
        body {
            font-family: 'Inter', sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            color: #333;
            margin: 0;
            padding: 0;
        }

        .content-wrapper {
            display: flex;
            align-items: flex-start;
            margin: 20px;
            gap: 20px;
            min-height: calc(100vh - 40px);
        }

        .sidebar {
            background: rgba(51, 51, 51, 0.95);
            backdrop-filter: blur(10px);
            color: #fff;
            width: 250px;
            padding: 25px;
            flex: 0 0 250px;
            border-radius: 15px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
        }
        
        .sidebar ul {
            list-style: none;
            margin: 0;
            padding: 0;
        }

        .sidebar li {
            margin-bottom: 8px;
        }

        .sidebar li.active a {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            transform: translateX(5px);
        }

        .sidebar li a:hover {
            background: rgba(255, 255, 255, 0.1);
            transform: translateX(3px);
        }

        .sidebar a {
            color: #fff;
            text-decoration: none;
            display: flex;
            align-items: center;
            padding: 12px 16px;
            border-radius: 10px;
            transition: all 0.3s ease;
            font-weight: 500;
        }

        .sidebar a i {
            margin-right: 12px;
            font-size: 1.1rem;
            width: 20px;
        }

        .main-content {
            flex: 1;
            padding: 0;
        }

        .header {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(15px);
            padding: 2rem;
            border-radius: 20px;
            margin-bottom: 25px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
            border: 1px solid rgba(255, 255, 255, 0.2);
        }

        .header h1 {
            color: #333;
            font-weight: 700;
            font-size: 2.2rem;
            margin-bottom: 0.5rem;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .header p {
            color: #666;
            font-size: 1.1rem;
            margin: 0;
        }

        .analysis-container {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(15px);
            border-radius: 20px;
            padding: 0;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
            border: 1px solid rgba(255, 255, 255, 0.2);
            min-height: 600px;
            overflow: hidden;
        }
        
        .loading-container {
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            height: 600px;
            text-align: center;
            color: #666;
        }
        
        .loading-spinner {
            border: 4px solid rgba(102, 126, 234, 0.2);
            border-left-color: #667eea;
            border-radius: 50%;
            width: 60px;
            height: 60px;
            animation: spin 1s linear infinite;
            margin-bottom: 1.5rem;
        }

        .loading-text {
            font-size: 1.1rem;
            font-weight: 500;
        }

        .report-content {
            padding: 0;
        }

        .health-score-section {
            background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
            padding: 2.5rem;
            border-bottom: 1px solid rgba(0, 0, 0, 0.05);
        }

        .health-score-container {
            display: flex;
            align-items: center;
            gap: 3rem;
        }
        
        .health-chart-container {
            width: 200px;
            height: 200px;
            flex-shrink: 0;
            position: relative;
        }

        .health-chart-container canvas {
            filter: drop-shadow(0 4px 8px rgba(0, 0, 0, 0.1));
        }

        .health-info {
            flex: 1;
        }

        .health-info h2 {
            font-size: 2rem;
            color: #1e293b;
            margin-bottom: 1rem;
            font-weight: 700;
        }

        .health-summary {
            font-size: 1.1rem;
            line-height: 1.6;
            color: #475569;
        }

        .findings-section {
            padding: 2.5rem;
        }

        .findings-title {
            font-size: 1.8rem;
            color: #1e293b;
            margin-bottom: 2rem;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .findings-title i {
            color: #667eea;
        }

        .findings-content {
            display: grid;
            gap: 2rem;
        }

        .finding-card {
            background: #f8fafc;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            padding: 1.5rem;
            transition: all 0.3s ease;
        }

        .finding-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
        }

        .finding-header {
            display: flex;
            align-items: center;
            gap: 12px;
            margin-bottom: 1rem;
        }

        .finding-icon {
            width: 40px;
            height: 40px;
            border-radius: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.2rem;
            color: white;
        }

        .finding-icon.positive { background: linear-gradient(135deg, #10b981 0%, #059669 100%); }
        .finding-icon.negative { background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); }
        .finding-icon.neutral { background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); }
        .finding-icon.info { background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); }

        .finding-title {
            font-size: 1.2rem;
            font-weight: 600;
            color: #1e293b;
            margin: 0;
        }

        .finding-description {
            color: #64748b;
            line-height: 1.6;
            margin-bottom: 1rem;
        }

        .finding-metrics {
            display: flex;
            gap: 1rem;
            margin-bottom: 1rem;
        }

        .metric-item {
            background: rgba(102, 126, 234, 0.1);
            padding: 8px 12px;
            border-radius: 6px;
            font-size: 0.9rem;
            color: #667eea;
            font-weight: 500;
        }

        .recommendations {
            background: #f0f9ff;
            border: 1px solid #bae6fd;
            border-radius: 8px;
            padding: 1rem;
        }

        .recommendations-title {
            font-weight: 600;
            color: #0c4a6e;
            margin-bottom: 0.5rem;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .recommendations ul {
            margin: 0;
            padding-left: 1.2rem;
            color: #0c4a6e;
        }

        .recommendations li {
            margin-bottom: 0.3rem;
            line-height: 1.4;
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        /* Responsive Design */
        @media (max-width: 1024px) {
            .content-wrapper {
                flex-direction: column;
                margin: 15px;
            }
            
            .sidebar {
                width: 100%;
                margin-bottom: 20px;
                flex: none;
            }
            
            .health-score-container {
                flex-direction: column;
                text-align: center;
                gap: 2rem;
            }
            
            .health-chart-container {
                width: 150px;
                height: 150px;
            }

            .findings-content {
                gap: 1.5rem;
            }
        }

        @media (max-width: 768px) {
            .content-wrapper {
                margin: 10px;
            }

            .header {
                padding: 1.5rem;
            }

            .header h1 {
                font-size: 1.8rem;
            }

            .health-score-section,
            .findings-section {
                padding: 2rem;
            }

            .finding-card {
                padding: 1.2rem;
            }

            .findings-content {
                gap: 1rem;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="content-wrapper">
        <div class="sidebar">
            <ul>
                <li><a href="AdminPage.aspx"><i class="bi bi-speedometer2"></i> Performance Dashboard</a></li>
                <li class="active"><a href="PerformanceAnalysis.aspx"><i class="bi bi-bar-chart-line"></i> AI Analysis</a></li>
                <li><a href="ApproveAdvisor.aspx"><i class="bi bi-people"></i> Manage Advisors</a></li>
                <li><a href="ManageEducation.aspx"><i class="bi bi-book"></i> Manage Education Topics</a></li>
                <li><a href="ManageTests.aspx"><i class="bi bi-patch-question"></i> Manage Tests</a></li>
            </ul>
        </div>

        <div class="main-content">
            <div class="header">
                <h1><i class="bi bi-robot"></i> AI Performance Analysis</h1>
                <p>Generate comprehensive insights and recommendations for your financial literacy platform using advanced AI analysis.</p>
            </div>

            <div class="analysis-container">
                <asp:Panel ID="pnlLoading" runat="server" CssClass="loading-container">
                    <div class="loading-spinner"></div>
                    <p class="loading-text">AI is analyzing your platform data...</p>
                    <small style="color: #94a3b8;">This may take a few moments</small>
                </asp:Panel>

                <asp:Panel ID="pnlReport" runat="server" Visible="false" CssClass="report-content">
                    <div class="health-score-section">
                        <div class="health-score-container">
                            <div class="health-chart-container">
                                <canvas id="healthScoreChart"></canvas>
                            </div>
                            <div class="health-info">
                                <h2>Platform Health Score</h2>
                                <div class="health-summary">
                                    <asp:Literal ID="litHealthSummary" runat="server"></asp:Literal>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class="findings-section">
                        <h2 class="findings-title">
                            <i class="bi bi-lightbulb"></i>
                            Key Insights & Recommendations
                        </h2>
                        <div class="findings-content">
                            <asp:Literal ID="litKeyFindings" runat="server"></asp:Literal>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>
    
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            // Global chart configuration
            Chart.defaults.font.family = 'Inter';
            Chart.defaults.color = '#64748b';
        });

        // Enhanced circular health score chart
        function createHealthScoreChart(healthScore) {
            const ctx = document.getElementById('healthScoreChart').getContext('2d');

            // Determine color based on health score
            let primaryColor, secondaryColor, textColor;
            if (healthScore >= 80) {
                primaryColor = '#10b981'; // Green
                secondaryColor = '#6ee7b7';
                textColor = '#065f46';
            } else if (healthScore >= 60) {
                primaryColor = '#f59e0b'; // Yellow
                secondaryColor = '#fbbf24';
                textColor = '#92400e';
            } else {
                primaryColor = '#ef4444'; // Red
                secondaryColor = '#fca5a5';
                textColor = '#991b1b';
            }

            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: ['Health Score', 'Remaining'],
                    datasets: [{
                        data: [healthScore, 100 - healthScore],
                        backgroundColor: [primaryColor, '#f1f5f9'],
                        borderWidth: 0,
                        cutout: '75%'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: { display: false },
                        tooltip: { enabled: false }
                    },
                    animation: {
                        animateRotate: true,
                        duration: 2000,
                        easing: 'easeOutCubic'
                    }
                },
                plugins: [{
                    afterDraw: function (chart) {
                        const ctx = chart.ctx;
                        const centerX = chart.width / 2;
                        const centerY = chart.height / 2;

                        // Draw the percentage text
                        ctx.save();
                        ctx.textAlign = 'center';
                        ctx.textBaseline = 'middle';
                        ctx.fillStyle = textColor;
                        ctx.font = 'bold 2.5rem Inter';
                        ctx.fillText(healthScore + '%', centerX, centerY - 5);

                        // Draw the label
                        ctx.fillStyle = '#94a3b8';
                        ctx.font = '0.9rem Inter';
                        ctx.fillText('Health Score', centerX, centerY + 25);
                        ctx.restore();
                    }
                }]
            });
        }

        // Function to be called from C# code-behind
        window.initializeHealthChart = function (score) {
            createHealthScoreChart(score);
        };
    </script>
</asp:Content>
