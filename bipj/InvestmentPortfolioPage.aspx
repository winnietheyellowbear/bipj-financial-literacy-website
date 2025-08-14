<%@ Page Title="Portfolio Builder" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" Async="true" CodeBehind="InvestmentPortfolioPage.aspx.cs" Inherits="bipj.InvestmentPortfolioPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- This content placeholder is for styles and head scripts --%>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        .card-header {
            background-color: #f8f9fa;
            font-weight: bold;
        }
        .card {
            box-shadow: 0 4px 8px 0 rgba(0,0,0,0.2);
            transition: 0.3s;
        }
        .card:hover {
            box-shadow: 0 8px 16px 0 rgba(0,0,0,0.2);
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <div>
                <h1 class="h2 mb-0">Portfolio Builder</h1>
                <asp:Label ID="lblPortfolioName" runat="server" CssClass="h5 text-muted" />
            </div>
            <asp:Button ID="btnGoToDashboard" runat="server" Text="Analyze Portfolio →" OnClick="btnGoToDashboard_Click" CssClass="btn btn-info" />
        </div>

        <div class="card mb-4">
            <div class="card-header">
                <i class="fa fa-search"></i> Find an Asset
            </div>
            <div class="card-body">
                <div class="input-group">
                    <asp:TextBox ID="txtSymbol" runat="server" CssClass="form-control" placeholder="Enter Asset Symbol (e.g. AAPL, BTC/USD)" />
                    <asp:Button ID="btnGetPrice" runat="server" Text="Get Details" OnClick="btnGetPrice_Click" CssClass="btn btn-primary" />
                </div>
                <asp:Label ID="lblPrice" runat="server" CssClass="d-block mt-2 fw-bold" />
                <asp:Label ID="lblAssetDescription" runat="server" CssClass="text-muted d-block" />
            </div>
        </div>

        <div class="card mb-4">
            <div class="card-header">
                <i class="fa fa-line-chart"></i> Price Trend Chart
            </div>
            <div class="card-body">
                <div style="position: relative; height: 400px;">
                    <canvas id="priceChart"></canvas>
                </div>
                <div class="mt-3 text-center">
                    <asp:Button ID="btnViewMonth" runat="server" Text="1-Month History" OnClick="btnViewMonth_Click" CssClass="btn btn-outline-secondary me-2" />
                    <asp:Button ID="btnViewYear" runat="server" Text="1-Year History" OnClick="btnViewYear_Click" CssClass="btn btn-outline-secondary me-2" />
                    <asp:Button ID="btnForecast" runat="server" Text="Toggle 7-Day Forecast" OnClick="btnForecast_Click" CssClass="btn btn-outline-success" />
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-lg-6">
                <div class="card mb-4">
                    <div class="card-header">
                        <i class="fa fa-plus-circle"></i> Add Asset to Portfolio
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <label for="<%=txtQuantity.ClientID%>" class="form-label fw-bold">Enter Quantity:</label>
                            <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" TextMode="Number" step="any" />
                        </div>
                        <asp:Button ID="btnAddAsset" runat="server" Text="Add to Portfolio" CssClass="btn btn-success w-100" OnClick="btnAddAsset_Click" />
                        <asp:Literal ID="litMessage" runat="server" />
                    </div>
                </div>
            </div>
            <div class="col-lg-6">
                <div class="card mb-4">
                    <div class="card-header">
                        <i class="fa fa-briefcase"></i> Current Portfolio Assets
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvPortfolioAssets" runat="server" AutoGenerateColumns="False" DataKeyNames="PortfolioAssetID" OnRowCommand="gvPortfolioAssets_RowCommand" CssClass="table table-hover">
                            <Columns>
                                <asp:BoundField DataField="Symbol" HeaderText="Symbol" />
                                <asp:BoundField DataField="Quantity" HeaderText="Quantity" DataFormatString="{0:N4}" />
                                <asp:BoundField DataField="PurchasedPrice" HeaderText="Buy Price" DataFormatString="{0:C}" />
                                <asp:BoundField DataField="PurchasedAt" HeaderText="Added At" DataFormatString="{0:g}" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Button runat="server" CommandName="DeleteAsset" CommandArgument='<%# Container.DataItemIndex %>' Text="Remove" CssClass="btn btn-danger btn-sm" OnClientClick="return confirm('Are you sure?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                             <EmptyDataTemplate>
                                <div class="alert alert-info">This portfolio is empty.</div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>

        <%-- Hidden Fields to pass data from server to client-side JavaScript --%>
        <asp:HiddenField ID="hfPriceLabels" runat="server" />
        <asp:HiddenField ID="hfPriceData" runat="server" />
        <asp:HiddenField ID="hfForecastData" runat="server" />
        <asp:HiddenField ID="hfForecastUpper" runat="server" />
        <asp:HiddenField ID="hfForecastLower" runat="server" />
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        function drawChart() {
            // Safely parse JSON from hidden fields, defaulting to empty arrays
            const labels = JSON.parse(document.getElementById('<%= hfPriceLabels.ClientID %>').value || "[]");
            const priceData = JSON.parse(document.getElementById('<%= hfPriceData.ClientID %>').value || "[]");
            const forecastData = JSON.parse(document.getElementById('<%= hfForecastData.ClientID %>').value || "[]");
            const forecastUpper = JSON.parse(document.getElementById('<%= hfForecastUpper.ClientID %>').value || "[]");
            const forecastLower = JSON.parse(document.getElementById('<%= hfForecastLower.ClientID %>').value || "[]");

            const ctx = document.getElementById('priceChart').getContext('2d');

            if (window.priceChartInstance) {
                window.priceChartInstance.destroy();
            }

            const datasets = [
                {
                    label: 'Historical Price',
                    data: priceData,
                    borderColor: '#4361ee',
                    backgroundColor: 'rgba(67, 97, 238, 0.1)',
                    borderWidth: 2,
                    fill: false,
                    tension: 0.1,
                    pointRadius: 2
                }
            ];

            if (forecastData.length > 0) {
                const nulls = Array(priceData.length - 1).fill(null); // Pad to align forecast
                nulls.push(priceData[priceData.length - 1]); // connect the line

                datasets.push({
                    label: 'Forecast',
                    data: [...nulls, ...forecastData],
                    borderColor: '#f72585',
                    borderDash: [5, 5],
                    borderWidth: 2,
                    fill: false,
                    tension: 0.1,
                    pointRadius: 3
                });

                datasets.push({
                    label: 'Volatility Range',
                    data: [...nulls, ...forecastUpper],
                    borderColor: 'rgba(255, 99, 132, 0.2)',
                    borderWidth: 1,
                    fill: '+1',
                    backgroundColor: 'rgba(255, 99, 132, 0.1)',
                    pointRadius: 0
                });

                datasets.push({
                    label: '', // Hidden label for the lower band
                    data: [...nulls, ...forecastLower],
                    borderColor: 'rgba(255, 99, 132, 0.2)',
                    borderWidth: 1,
                    fill: false,
                    pointRadius: 0
                });
            }

            window.priceChartInstance = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: datasets
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'top',
                            labels: {
                                filter: item => item.label // Hides the empty label for the lower band
                            }
                        },
                        title: { display: true, text: 'Price Trend with Forecast' },
                        tooltip: { mode: 'index', intersect: false }
                    },
                    interaction: { mode: 'nearest', axis: 'x', intersect: false },
                    scales: {
                        x: { title: { display: true, text: "Date" } },
                        y: { title: { display: true, text: "Price" }, beginAtZero: false }
                    }
                }
            });
        }
        // ✅ FIXED: Removed the automatic call on DOMContentLoaded to prevent race conditions.
        // The server will now explicitly call drawChart() when it's ready.
    </script>
</asp:Content>
