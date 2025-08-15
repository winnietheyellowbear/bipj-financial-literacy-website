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
