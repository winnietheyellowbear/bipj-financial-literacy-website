<%@ Page Title="My Investment Portfolios" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InvestmentPage.aspx.cs" Inherits="bipj.InvestmentPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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

        /* ✅ ADDED: This new rule makes the portfolio card titles bold. */
        .portfolio-card .card-title {
            font-weight: 700;
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
        <div class="card shadow-sm mb-4">
            <div class="card-body">
                <h5 class="card-title">Create a New Portfolio</h5>
                <div class="input-group">
                    <asp:TextBox ID="txtNewPortfolioName" runat="server" CssClass="form-control" placeholder="e.g., 'Retirement Fund', 'Tech Stocks'"></asp:TextBox>
                    <asp:Button ID="btnCreateNewPortfolio" runat="server" Text="Create Portfolio" OnClick="btnCreateNewPortfolio_Click"
                        CssClass="btn btn-primary" />
                </div>
                <asp:RequiredFieldValidator ID="rfvPortfolioName" runat="server" ControlToValidate="txtNewPortfolioName"
                    Display="Dynamic" CssClass="mt-1 d-block" />
            </div>
        </div>

        <h3 class="mb-3">My Existing Portfolios</h3>

        <asp:Panel ID="pnlEmptyData" runat="server" Visible="false" CssClass="text-center bg-white p-5 rounded shadow-sm">
            <p class="text-muted">You haven't created any investment portfolios yet.</p>
            <p class="mt-2 text-muted">Click the button above to get started!</p>
        </asp:Panel>

        <asp:Repeater ID="rptPortfolios" runat="server" OnItemCommand="rptPortfolios_ItemCommand">
            <HeaderTemplate>
                <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="col">
                    <%-- ✅ ADDED: The new 'portfolio-card' class for the hover effect. --%>
                    <div class="card h-100 shadow-sm portfolio-card">
                        <div class="card-body d-flex flex-column">
                            <h5 class="card-title"><%# Eval("PortfolioName") %></h5>
                            
                            <%-- ✅ MOVED: The button container is now below the title. --%>
                            <div class="mt-2 mb-3">
                                 <asp:Button ID="btnView" runat="server" Text="Edit" CommandName="View" CommandArgument='<%# Eval("PortfolioID") %>'
                                     CssClass="btn btn-success btn-sm me-2" />
                                <asp:Button ID="btnAnalyze" runat="server" Text="View Dashboard" CommandName="Analyze" CommandArgument='<%# Eval("PortfolioID") %>'
                                     CssClass="btn btn-info btn-sm me-2" />
                                 <asp:Button ID="btnDelete" runat="server" Text="Delete" CommandName="Delete" CommandArgument='<%# Eval("PortfolioID") %>'
                                     OnClientClick="return confirm('Are you sure you want to delete this portfolio? This will remove all assets it contains.');"
                                     CssClass="btn btn-danger btn-sm" />
                            </div>

                            <%-- ✅ MODIFIED: This div now pushes the descriptive text to the bottom of the card. --%>
                            <div class="mt-auto">
                                <p class="card-text"><%# Eval("Description") %></p>
                                <p class="card-text text-muted small">Last Updated: <%# Eval("LastUpdatedAt", "{0:MMMM d, yyyy}") %></p>
                            </div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:Repeater>
    </div>
</asp:Content>
