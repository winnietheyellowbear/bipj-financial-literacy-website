<%@ Page Title="Admin Dashboard" Language="C#" MasterPageFile="~/Staff_Nav.Master" AutoEventWireup="true" CodeBehind="AdminPage.aspx.cs" Inherits="bipj.AdminPage" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Inter', sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            color: #333;
        }

        /* Content Layout - keeping your original sidebar structure */
        .content-wrapper {
            display: flex;
            align-items: flex-start;
            margin-top: 20px;
        }

        /* Sidebar - keeping your original styling */
        .sidebar {
            background: #333;
            color: #fff;
            width: 200px;
            padding: 20px;
            flex: 0 0 200px;
            border-radius: 10px;
            margin-right: 20px;
        }

        .sidebar ul {
            list-style: none;
            margin: 0;
            padding: 0;
        }

        .sidebar li {
            margin-bottom: 12px;
        }

        .sidebar li.active a,
        .sidebar li a:hover {
            background: #575757;
        }

        .sidebar a {
            color: #fff;
            text-decoration: none;
            display: block;
            padding: 8px;
            border-radius: 4px;
            transition: background .2s;
        }

        /* Main Content */
        .main-content {
            flex: 1;
            padding: 0 24px;
        }

        .header {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            padding: 1.5rem 2rem;
            border-radius: 20px;
            margin-bottom: 2rem;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .header h1 {
            color: #333;
            font-weight: 700;
            font-size: 2rem;
            margin-bottom: 0.5rem;
        }

        .header p {
            color: #666;
            font-size: 1.1rem;
        }

        .analyze-button {
            background: linear-gradient(135deg, #667eea, #764ba2);
            color: #fff;
            padding: 10px 20px;
            border-radius: 8px;
            text-decoration: none;
            font-weight: 600;
            transition: background 0.3s ease, transform 0.3s ease;
            box-shadow: 0 4px 15px rgba(118, 75, 162, 0.3);
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .analyze-button:hover {
            background: linear-gradient(135deg, #764ba2, #667eea);
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(118, 75, 162, 0.4);
        }

        /* KPI Cards - Modified for 4 cards */
        .kpi-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 1.5rem;
            margin-bottom: 2rem;
        }

        .kpi-card {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 1.5rem;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
            transition: transform 0.3s ease, box-shadow 0.3s ease;
            position: relative;
            overflow: hidden;
        }

        .kpi-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 4px;
            background: linear-gradient(90deg, #667eea, #764ba2);
        }

        .kpi-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 12px 48px rgba(0, 0, 0, 0.15);
        }

        .kpi-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1rem;
        }

        .kpi-icon {
            width: 50px;
            height: 50px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.5rem;
            color: white;
        }

        .kpi-value {
            font-size: 2rem;
            font-weight: 700;
            color: #333;
            margin-bottom: 0.25rem;
        }

        .kpi-label {
            color: #666;
            font-size: 0.9rem;
            font-weight: 500;
        }

        .kpi-change {
            display: flex;
            align-items: center;
            font-size: 0.85rem;
            margin-top: 0.5rem;
        }

        .kpi-change.positive {
            color: #10b981;
        }

        .kpi-change.negative {
            color: #ef4444;
        }

        /* Chart Sections - Modified for 3 charts */
        .chart-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(500px, 1fr));
            gap: 2rem;
            margin-bottom: 2rem;
        }

        .chart-section {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 2rem;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
        }

        .chart-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1.5rem;
        }

        .chart-title {
            font-size: 1.3rem;
            font-weight: 600;
            color: #333;
        }

        .chart-container {
            position: relative;
            height: 350px;
            width: 100%;
        }

        /* Color variants */
        .bg-blue { background: linear-gradient(135deg, #667eea, #764ba2); }
        .bg-green { background: linear-gradient(135deg, #10b981, #059669); }
        .bg-purple { background: linear-gradient(135deg, #8b5cf6, #7c3aed); }
        .bg-orange { background: linear-gradient(135deg, #f59e0b, #d97706); }
        .bg-pink { background: linear-gradient(135deg, #ec4899, #db2777); }
        .bg-teal { background: linear-gradient(135deg, #14b8a6, #0d9488); }

        /* Responsive */
        @media (max-width: 1024px) {
            .content-wrapper {
                flex-direction: column;
            }
            
            .sidebar {
                width: 100%;
                margin-bottom: 20px;
                margin-right: 0;
            }
            
            .chart-grid {
                grid-template-columns: 1fr;
            }
        }

        @media (max-width: 768px) {
            .kpi-grid {
                grid-template-columns: 1fr;
            }
            
            .main-content {
                padding: 1rem;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="content-wrapper">
        <div class="sidebar">
            <ul>
                <li class="active"><a href="AdminPage.aspx"><i class="bi bi-speedometer2"></i> Performance Dashboard</a></li>
                <li><a href="ApproveAdvisor.aspx"><i class="bi bi-people"></i> Manage Advisors</a></li>
                <li><a href="ManageEducation.aspx"><i class="bi bi-book"></i> Manage Education Topics</a></li>
                <li><a href="ManageTests.aspx"><i class="bi bi-patch-question"></i> Manage Tests</a></li>
            </ul>
        </div>

        <div class="main-content">
            <div class="header">
                <div>
                    <h1>Performance Dashboard</h1>
                    <p>Monitor your platform's health and user engagement metrics</p>
                </div>
                <a href="PerformanceAnalysis.aspx" class="analyze-button">
                    <i class="bi bi-bar-chart-line"></i> Analyze
                </a>
            </div>

            <!-- 4 Small KPI Cards -->
            <div class="kpi-grid">
                <div class="kpi-card">
                    <div class="kpi-header">
                        <div class="kpi-icon bg-green">
                            <i class="bi bi-graph-up-arrow"></i>
                        </div>
                    </div>
                    <div class="kpi-value" id="activeUsers">0</div>
                    <div class="kpi-label">Active Users (30 days)</div>
                    <div class="kpi-change positive" id="activeUserGrowthChange">
                        <i class="bi bi-arrow-up"></i>
                        Loading...
                    </div>
                </div>

                <div class="kpi-card">
                    <div class="kpi-header">
                        <div class="kpi-icon bg-purple">
                            <i class="bi bi-piggy-bank"></i>
                        </div>
                    </div>
                    <div class="kpi-value" id="jarUsers">0</div>
                    <div class="kpi-label">Users Using Jar System</div>
                    <div class="kpi-change positive" id="jarAdoptionChange">
                        <i class="bi bi-arrow-up"></i>
                        Loading...
                    </div>
                </div>

                <div class="kpi-card">
                    <div class="kpi-header">
                        <div class="kpi-icon bg-teal">
                            <i class="bi bi-target"></i>
                        </div>
                    </div>
                    <div class="kpi-value" id="goalUsers">0</div>
                    <div class="kpi-label">Users Using Financial Goals</div>
                    <div class="kpi-change positive" id="goalGrowthChange">
                        <i class="bi bi-arrow-up"></i>
                        Loading...
                    </div>
                </div>

                <div class="kpi-card">
                    <div class="kpi-header">
                        <div class="kpi-icon bg-orange">
                            <i class="bi bi-calendar-check"></i>
                        </div>
                    </div>
                    <div class="kpi-value" id="advisorBookings">0</div>
                    <div class="kpi-label">User Bookings</div>
                    <div class="kpi-change positive" id="bookingGrowthChange">
                        <i class="bi bi-arrow-up"></i>
                        Loading...
                    </div>
                </div>
            </div>

            <!-- 3 Large Charts -->
            <div class="chart-grid">
                <div class="chart-section">
                    <div class="chart-header">
                        <h3 class="chart-title">User Growth & Engagement</h3>
                    </div>
                    <div class="chart-container">
                        <canvas id="userGrowthChart"></canvas>
                    </div>
                </div>

                <div class="chart-section">
                    <div class="chart-header">
                        <h3 class="chart-title">Financial Tools Usage</h3>
                    </div>
                    <div class="chart-container">
                        <canvas id="toolsUsageChart"></canvas>
                    </div>
                </div>

                <div class="chart-section">
                    <div class="chart-header">
                        <h3 class="chart-title">Community Engagement</h3>
                    </div>
                    <div class="chart-container">
                        <canvas id="forumChart"></canvas>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>