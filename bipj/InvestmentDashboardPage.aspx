<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InvestmentDashboardPage.aspx.cs" Inherits="bipj.InvestmentDashboardPage" Async="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        .metric-card {
            border-left-width: 5px;
            border-radius: .35rem;
        }
        .metric-value {
            font-size: 2rem;
            font-weight: 700;
        }
        .positive {
            color: #1cc88a; /* Green */
        }
        .negative {
            color: #e74a3b; /* Red */
        }
        .stars {
            color: #f6c23e; /* Yellow */
            font-size: 1.5rem;
        }
        .heatmap-table {
            border-collapse: collapse;
            width: 100%;
        }
        .heatmap-table th, .heatmap-table td {
            border: 1px solid #ddd;
            padding: 8px;
            text-align: center;
        }
        .heatmap-table th {
            background-color: #f2f2f2;
            font-weight: bold;
            writing-mode: vertical-rl;
            text-orientation: mixed;
            padding: 10px 5px;
        }
        .heatmap-table td {
            color: #333;
            font-weight: 500;
        }
    </style>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    
    <!-- Chart.js -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    
    <!-- Animate.css -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/animate.css/4.1.1/animate.min.css" rel="stylesheet">
    
    <style>
        /* ===========================
           GLOBAL STYLES
        =========================== */
        :root {
            --primary-gradient: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --success-gradient: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            --danger-gradient: linear-gradient(135deg, #eb3349 0%, #f45c43 100%);
            --info-gradient: linear-gradient(135deg, #2193b0 0%, #6dd5ed 100%);
            --dark-gradient: linear-gradient(135deg, #141e30 0%, #243b55 100%);
            
            --primary-color: #667eea;
            --secondary-color: #764ba2;
            --success-color: #38ef7d;
            --danger-color: #f45c43;
            --info-color: #6dd5ed;
            
            --transition-fast: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            --transition-medium: 0.5s cubic-bezier(0.4, 0, 0.2, 1);
            
            --shadow-sm: 0 2px 10px rgba(0,0,0,0.08);
            --shadow-md: 0 4px 20px rgba(0,0,0,0.1);
            --shadow-lg: 0 10px 40px rgba(0,0,0,0.15);
        }

        body {
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
            min-height: 100vh;
        }

        /* ===========================
           PORTFOLIO CARDS
        =========================== */
        .portfolio-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
            gap: 2rem;
            margin: 2rem 0;
        }

        .portfolio-card {
            background: white;
            border-radius: 1rem;
            overflow: hidden;
            position: relative;
            transition: var(--transition-fast);
            box-shadow: var(--shadow-sm);
            cursor: pointer;
        }

        .portfolio-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 5px;
            background: var(--primary-gradient);
            transform: scaleX(0);
            transition: transform var(--transition-fast);
            transform-origin: left;
        }

        .portfolio-card:hover::before {
            transform: scaleX(1);
        }

        .portfolio-card:hover {
            transform: translateY(-5px);
            box-shadow: var(--shadow-lg);
        }

        .portfolio-card-header {
            padding: 1.5rem;
            background: var(--primary-gradient);
            color: white;
        }

        .portfolio-card-body {
            padding: 1.5rem;
        }

        .portfolio-stats {
            display: flex;
            justify-content: space-around;
            padding: 1rem 0;
            border-top: 1px solid #f0f0f0;
            margin-top: 1rem;
        }

        .stat-item {
            text-align: center;
        }

        .stat-value {
            font-size: 1.5rem;
            font-weight: 700;
            color: var(--primary-color);
        }

        .stat-label {
            font-size: 0.875rem;
            color: #666;
            margin-top: 0.25rem;
        }

        /* ===========================
           ASSET SEARCH BAR
        =========================== */
        .asset-search-container {
            position: relative;
            margin-bottom: 2rem;
        }

        .asset-search-input {
            width: 100%;
            padding: 1rem 3rem 1rem 1.5rem;
            border: 2px solid transparent;
            border-radius: 0.75rem;
            font-size: 1.1rem;
            background: white;
            box-shadow: var(--shadow-md);
            transition: var(--transition-fast);
        }

        .asset-search-input:focus {
            outline: none;
            border-color: var(--primary-color);
            box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
        }

        .search-icon {
            position: absolute;
            right: 1.5rem;
            top: 50%;
            transform: translateY(-50%);
            color: #999;
        }

        .search-suggestions {
            position: absolute;
            top: calc(100% + 0.5rem);
            left: 0;
            right: 0;
            background: white;
            border-radius: 0.75rem;
            box-shadow: var(--shadow-lg);
            max-height: 400px;
            overflow-y: auto;
            z-index: 1000;
            opacity: 0;
            visibility: hidden;
            transform: translateY(-10px);
            transition: var(--transition-fast);
        }

        .search-suggestions.active {
            opacity: 1;
            visibility: visible;
            transform: translateY(0);
        }

        .suggestion-item {
            padding: 1rem 1.5rem;
            display: flex;
            justify-content: space-between;
            align-items: center;
            cursor: pointer;
            transition: background var(--transition-fast);
        }

        .suggestion-item:hover {
            background: #f8f9fa;
        }

        .suggestion-symbol {
            font-weight: 700;
            color: var(--primary-color);
        }

        .suggestion-name {
            color: #666;
            font-size: 0.875rem;
        }

        .suggestion-price {
            font-weight: 600;
            color: #333;
        }

        /* ===========================
           PRICE CHART
        =========================== */
        .chart-container {
            background: white;
            border-radius: 1rem;
            padding: 2rem;
            box-shadow: var(--shadow-md);
            margin-bottom: 2rem;
        }

        .chart-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1.5rem;
        }

        .chart-title {
            font-size: 1.25rem;
            font-weight: 700;
            color: #333;
        }

        .chart-controls {
            display: flex;
            gap: 0.5rem;
        }

        .chart-btn {
            padding: 0.5rem 1rem;
            background: white;
            border: 2px solid #e0e0e0;
            border-radius: 0.5rem;
            color: #666;
            font-weight: 600;
            cursor: pointer;
            transition: var(--transition-fast);
        }

        .chart-btn:hover {
            border-color: var(--primary-color);
            color: var(--primary-color);
        }

        .chart-btn.active {
            background: var(--primary-gradient);
            color: white;
            border-color: transparent;
        }

        /* ===========================
           DASHBOARD METRICS
        =========================== */
        .metric-cards {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 1.5rem;
            margin-bottom: 2rem;
        }

        .metric-card {
            background: white;
            border-radius: 1rem;
            padding: 1.5rem;
            position: relative;
            overflow: hidden;
            box-shadow: var(--shadow-sm);
            transition: var(--transition-fast);
        }

        .metric-card:hover {
            transform: translateY(-3px);
            box-shadow: var(--shadow-md);
        }

        .metric-card::after {
            content: '';
            position: absolute;
            top: 0;
            right: 0;
            width: 100px;
            height: 100px;
            background: var(--primary-gradient);
            opacity: 0.1;
            border-radius: 50%;
            transform: translate(30px, -30px);
        }

        .metric-icon {
            width: 50px;
            height: 50px;
            border-radius: 0.75rem;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.5rem;
            color: white;
            margin-bottom: 1rem;
        }

        .metric-icon.primary {
            background: var(--primary-gradient);
        }

        .metric-icon.success {
            background: var(--success-gradient);
        }

        .metric-icon.danger {
            background: var(--danger-gradient);
        }

        .metric-icon.info {
            background: var(--info-gradient);
        }

        .metric-value {
            font-size: 2rem;
            font-weight: 700;
            color: #333;
            margin-bottom: 0.5rem;
        }

        .metric-label {
            color: #999;
            font-size: 0.875rem;
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .metric-change {
            position: absolute;
            top: 1.5rem;
            right: 1.5rem;
            padding: 0.25rem 0.75rem;
            border-radius: 2rem;
            font-size: 0.875rem;
            font-weight: 600;
        }

        .metric-change.positive {
            background: rgba(56, 239, 125, 0.1);
            color: var(--success-color);
        }

        .metric-change.negative {
            background: rgba(244, 92, 67, 0.1);
            color: var(--danger-color);
        }

        /* ===========================
           LOADING STATES
        =========================== */
        .skeleton {
            animation: skeleton-loading 1s linear infinite alternate;
        }

        @keyframes skeleton-loading {
            0% {
                background-color: #f0f0f0;
            }
            100% {
                background-color: #e0e0e0;
            }
        }

        .spinner {
            width: 40px;
            height: 40px;
            border: 4px solid #f0f0f0;
            border-top-color: var(--primary-color);
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }

        @keyframes spin {
            to {
                transform: rotate(360deg);
            }
        }

        /* ===========================
           TOOLTIPS
        =========================== */
        .tooltip-custom {
            position: relative;
            cursor: help;
        }

        .tooltip-custom::after {
            content: attr(data-tooltip);
            position: absolute;
            bottom: 125%;
            left: 50%;
            transform: translateX(-50%);
            background: #333;
            color: white;
            padding: 0.5rem 1rem;
            border-radius: 0.5rem;
            font-size: 0.875rem;
            white-space: nowrap;
            opacity: 0;
            visibility: hidden;
            transition: var(--transition-fast);
            z-index: 1000;
        }

        .tooltip-custom:hover::after {
            opacity: 1;
            visibility: visible;
        }

        /* ===========================
           NOTIFICATIONS
        =========================== */
        .notification {
            position: fixed;
            top: 2rem;
            right: 2rem;
            padding: 1rem 1.5rem;
            background: white;
            border-radius: 0.75rem;
            box-shadow: var(--shadow-lg);
            z-index: 9999;
            transform: translateX(400px);
            transition: transform var(--transition-medium);
        }

        .notification.show {
            transform: translateX(0);
        }

        .notification.success {
            border-left: 4px solid var(--success-color);
        }

        .notification.error {
            border-left: 4px solid var(--danger-color);
        }

        .notification.info {
            border-left: 4px solid var(--info-color);
        }

        /* ===========================
           RESPONSIVE DESIGN
        =========================== */
        @media (max-width: 768px) {
            .portfolio-grid {
                grid-template-columns: 1fr;
                gap: 1rem;
            }

            .metric-cards {
                grid-template-columns: 1fr;
            }

            .chart-controls {
                flex-wrap: wrap;
            }

            .notification {
                right: 1rem;
                left: 1rem;
            }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <div>
                <h1 class="h2 mb-0">Investment Dashboard</h1>
                <asp:Label ID="lblPortfolioName" runat="server" CssClass="h5 text-muted" />
            </div>
            <asp:HyperLink ID="hlBackToBuilder" runat="server" CssClass="btn btn-secondary">← Back to Builder</asp:HyperLink>
        </div>

        <!-- Top Row Metrics -->
        <div class="row">
            <div class="col-md-3 mb-4">
                <div class="card metric-card border-left-primary shadow h-100 py-2">
                    <div class="card-body">
                        <div class="text-xs font-weight-bold text-primary text-uppercase mb-1">Principal Amount</div>
                        <div class="metric-value"><asp:Literal ID="litPrincipal" runat="server" /></div>
                    </div>
                </div>
            </div>
            <div class="col-md-3 mb-4">
                <div class="card metric-card border-left-success shadow h-100 py-2">
                    <div class="card-body">
                        <div class="text-xs font-weight-bold text-success text-uppercase mb-1">Current Value</div>
                        <div class="metric-value"><asp:Literal ID="litCurrentValue" runat="server" /></div>
                    </div>
                </div>
            </div>
            <div class="col-md-3 mb-4">
                <div class="card metric-card border-left-info shadow h-100 py-2">
                    <div class="card-body">
                        <div class="text-xs font-weight-bold text-info text-uppercase mb-1">Return on Investment (ROI)</div>
                        <div class="metric-value"><asp:Literal ID="litROI" runat="server" /></div>
                    </div>
                </div>
            </div>
            <div class="col-md-3 mb-4">
                <div class="card metric-card border-left-warning shadow h-100 py-2">
                    <div class="card-body">
                        <div class="text-xs font-weight-bold text-warning text-uppercase mb-1">Net Profit / Loss</div>
                        <div class="metric-value"><asp:Literal ID="litNetProfit" runat="server" /></div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Second Row: Risk & Charts -->
        <div class="row">
            <div class="col-lg-6 mb-4">
                <div class="card shadow h-100">
                    <div class="card-header py-3">
                        <h6 class="m-0 font-weight-bold text-primary">Portfolio Risk Profile</h6>
                    </div>
                    <div class="card-body">
                        <h4 class="small font-weight-bold">Volatility Score <span class="float-right stars"><asp:Literal ID="litVolatility" runat="server" /></span></h4>
                        <p class="text-muted small">Based on the standard deviation of the portfolio's historical daily returns. Higher stars mean more price fluctuation.</p>
                        <hr />
                        <h4 class="small font-weight-bold">Risk Score <span class="float-right stars"><asp:Literal ID="litRiskScore" runat="server" /></span></h4>
                        <p class="text-muted small">An estimated score based on the types of assets in the portfolio (e.g., cryptocurrencies are higher risk than bonds) and overall volatility.</p>
                    </div>
                </div>
            </div>
            <div class="col-lg-6 mb-4">
                <div class="card shadow h-100">
                    <div class="card-header py-3">
                        <h6 class="m-0 font-weight-bold text-primary">Portfolio Exposure</h6>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-6 text-center">
                                <strong>Sector Exposure</strong>
                                <canvas id="sectorChart"></canvas>
                            </div>
                            <div class="col-md-6 text-center">
                                <strong>Geographical Exposure</strong>
                                <canvas id="geoChart"></canvas>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Third Row: Correlation Matrix -->
        <div class="row">
            <div class="col-12 mb-4">
                <div class="card shadow">
                    <div class="card-header py-3">
                        <h6 class="m-0 font-weight-bold text-primary">Asset Correlation Matrix</h6>
                        <p class="small text-muted mb-0">Shows how closely the prices of assets in your portfolio move together (based on the last 90 days of returns). 
                            <span style="background-color: #d4edda; padding: 2px;">Green (near +1)</span> indicates they move in the same direction. 
                            <span style="background-color: #f8d7da; padding: 2px;">Red (near -1)</span> indicates they move in opposite directions. Values near 0 mean no correlation.</p>
                    </div>
                    <div class="card-body overflow-auto">
                        <asp:Repeater ID="rptCorrelationMatrix" runat="server">
                            <HeaderTemplate>
                                <table class="heatmap-table">
                                    <thead>
                                        <tr>
                                            <th></th> <!-- Empty top-left corner -->
                            </HeaderTemplate>
                            <ItemTemplate>
                                <%-- This part renders the column headers (asset symbols) --%>
                                <th><%# Eval("Symbol") %></th>
                            </ItemTemplate>
                            <FooterTemplate>
                                        </tr>
                                    </thead>
                                    <tbody>
                            </FooterTemplate>
                        </asp:Repeater>

                        <asp:Repeater ID="rptCorrelationRows" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td style="font-weight:bold; background-color:#f2f2f2;"><%# Eval("Symbol") %></td>
                                    <asp:Repeater ID="rptCorrelationCells" runat="server" DataSource='<%# Eval("Correlations") %>'>
                                        <ItemTemplate>
                                            <td style='background-color: <%# GetColorForCorrelation((double)Container.DataItem) %>'>
                                                <%# ((double)Container.DataItem).ToString("F2") %>
                                            </td>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                    </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        // This function will be called from the C# code-behind to render the charts
        function renderPieCharts(sectorData, geoData) {
            // Sector Chart
            const sectorCtx = document.getElementById('sectorChart');
            if (sectorCtx) {
                new Chart(sectorCtx, {
                    type: 'doughnut',
                    data: sectorData,
                    options: { responsive: true, plugins: { legend: { position: 'bottom' } } }
                });
            }

            // Geography Chart
            const geoCtx = document.getElementById('geoChart');
            if (geoCtx) {
                new Chart(geoCtx, {
                    type: 'doughnut',
                    data: geoData,
                    options: { responsive: true, plugins: { legend: { position: 'bottom' } } }
                });
            }
        }
    </script>
</asp:Content>
