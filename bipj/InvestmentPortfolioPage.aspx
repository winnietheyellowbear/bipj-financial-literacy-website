<<<<<<< HEAD
﻿<%@ Page Title="Portfolio Builder" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InvestmentPortfolioPage.aspx.cs" Inherits="bipj.InvestmentPortfolioPage" Async="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        .asset-detail-label {
            font-weight: bold;
            color: #555;
        }
        .portfolio-grid th {
            background-color: #f2f2f2;
=======
﻿<%@ Page Title="Portfolio Builder" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" Async="true" CodeBehind="InvestmentPortfolioPage.aspx.cs" Inherits="bipj.InvestmentPortfolioPage" %>

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
>>>>>>> 07a4dbcf93962d5594baae011952de56a750ffbd
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<<<<<<< HEAD
    <div class="container-fluid mt-4">
        <div class="row">
            <div class="col-12">
                <asp:Label ID="lblPortfolioName" runat="server" CssClass="h2"></asp:Label>
                <hr />
            </div>
        </div>
        <div class="row">
            <div class="col-lg-7">
                <h4><i class="fa fa-search"></i> Find an Asset</h4>
                <div class="input-group mb-3">
                    <asp:TextBox ID="txtAssetSymbol" runat="server" CssClass="form-control" placeholder="Enter Stock Symbol (e.g., AAPL, TSLA) or Crypto (e.g., BTC/USD)"></asp:TextBox>
                    <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="btn btn-primary" />
                </div>
                <asp:Label ID="lblSearchStatus" runat="server" ForeColor="Red"></asp:Label>
                <asp:Panel ID="pnlAssetDetails" runat="server" Visible="false" CssClass="card mt-3">
                    <div class="card-body">
                        <h4 class="card-title">
                            <asp:Literal ID="litAssetName" runat="server"></asp:Literal>
                            (<asp:Literal ID="litAssetSymbol" runat="server"></asp:Literal>)
                        </h4>
                        <p class="card-text"><asp:Literal ID="litAssetDescription" runat="server"></asp:Literal></p>
                        <div class="row">
                            <div class="col-md-6"><span class="asset-detail-label">Current Price:</span> $<asp:Literal ID="litCurrentPrice" runat="server"></asp:Literal></div>
                            <div class="col-md-6"><span class="asset-detail-label">Sector:</span> <asp:Literal ID="litSector" runat="server"></asp:Literal></div>
                            <div class="col-md-6"><span class="asset-detail-label">Asset Type:</span> <asp:Literal ID="litAssetType" runat="server"></asp:Literal></div>
                            <div class="col-md-6"><span class="asset-detail-label">Geography:</span> <asp:Literal ID="litGeography" runat="server"></asp:Literal></div>
                        </div>
                        <div class="mt-4">
                            <canvas id="priceChart"></canvas>
                        </div>
                        <div class="text-center mt-2">
                            <button type="button" id="btn30d" class="btn btn-sm btn-outline-secondary">30 Days</button>
                            <button type="button" id="btn1y" class="btn btn-sm btn-outline-secondary">1 Year</button>
                            <button type="button" id="btnForecast" class="btn btn-sm btn-outline-primary">Toggle 7-Day Forecast</button>
                        </div>
                        <div class="input-group mt-4">
                            <span class="input-group-text">Quantity</span>
                            <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" TextMode="Number" step="0.0001" Text="1"></asp:TextBox>
                            <asp:Button ID="btnAddAsset" runat="server" Text="Add to Portfolio" OnClick="btnAddAsset_Click" CssClass="btn btn-success" />
                        </div>
                        <asp:Label ID="lblAddStatus" runat="server" CssClass="mt-2 d-block" ForeColor="Red"></asp:Label>
                    </div>
                </asp:Panel>
            </div>
            <div class="col-lg-5">
                <h4><i class="fa fa-briefcase"></i> Current Portfolio</h4>
                <asp:GridView ID="gvPortfolioAssets" runat="server" AutoGenerateColumns="false" CssClass="table table-hover portfolio-grid" GridLines="None" OnRowDeleting="gvPortfolioAssets_RowDeleting">
                    <Columns>
                        <asp:BoundField DataField="Symbol" HeaderText="Symbol" />
                        <asp:BoundField DataField="AssetName" HeaderText="Name" />
                        <asp:BoundField DataField="Quantity" HeaderText="Quantity" DataFormatString="{0:N4}" />
                        <asp:BoundField DataField="PurchasedPrice" HeaderText="Purchase Price" DataFormatString="{0:C}" />
                        <asp:BoundField DataField="PurchasedAt" HeaderText="Date Added" DataFormatString="{0:g}" />
                        <asp:CommandField ShowDeleteButton="true" DeleteText="Remove" ControlStyle-CssClass="btn btn-danger btn-sm" />
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="alert alert-info">This portfolio is empty. Use the search tool to add assets.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
                <div class="text-end mt-3">
                     <asp:Button ID="btnGoToDashboard" runat="server" Text="Analyze Portfolio →" OnClick="btnGoToDashboard_Click" CssClass="btn btn-info" />
                </div>
            </div>
        </div>
    </div>
    <asp:HiddenField ID="hfCurrentSymbol" runat="server" />
</asp:Content>

<%-- ✅ FIX 1: The entire JavaScript block has been moved to the 'scripts' ContentPlaceHolder. --%>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script type="text/javascript">
        // Global chart variable
        let priceChart;
        let showForecast = false;

        // Function to call C# WebMethod and get chart data
        function loadChartData(symbol, timePeriod, includeForecast) {
            // The PageMethods object will now exist because this script loads after the ScriptManager's scripts.
            PageMethods.GetChartData(symbol, timePeriod, includeForecast, function (responseString) {
                // We need to parse the JSON string that comes back from the server
                const response = JSON.parse(responseString);
                renderChart(response);
            }, function (error) {
                console.error("Error loading chart data: " + error.responseText);
                alert('Could not load chart data. Please check the console for errors.');
            });
        }

        // Function to render the chart using Chart.js
        function renderChart(data) {
            const ctx = document.getElementById('priceChart').getContext('2d');

            if (priceChart) {
                priceChart.destroy(); // Clear previous chart instance
            }

            priceChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: data.Labels,
                    datasets: [
                        {
                            label: 'Historical Price',
                            data: data.HistoricalPrices,
                            borderColor: 'rgb(75, 192, 192)',
                            tension: 0.1,
                            pointRadius: 1
                        },
                        // Conditionally add the forecast dataset
                        ...(data.ForecastPrices && data.ForecastPrices.length > 0 ? [{
                            label: '7-Day Forecast',
                            data: data.ForecastPrices,
                            borderColor: 'rgb(255, 99, 132)',
                            borderDash: [5, 5], // Dotted line
                            tension: 0.1,
                            pointRadius: 2,
                            pointBackgroundColor: 'rgb(255, 99, 132)'
                        }] : [])
                    ]
                },
                options: {
                    scales: {
                        y: {
                            ticks: {
                                // Format Y-axis as currency
                                callback: function (value, index, values) {
                                    return '$' + value.toFixed(2);
                                }
                            }
                        }
                    },
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'top',
                        },
                        title: {
                            display: true,
                            text: 'Asset Price History'
                        }
=======
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
>>>>>>> 07a4dbcf93962d5594baae011952de56a750ffbd
                    }
                }
            });
        }
<<<<<<< HEAD

        // Event Listeners for buttons
        document.addEventListener('DOMContentLoaded', function () {
            const btn30d = document.getElementById('btn30d');
            const btn1y = document.getElementById('btn1y');
            const btnForecast = document.getElementById('btnForecast');
            const symbolField = document.getElementById('<%= hfCurrentSymbol.ClientID %>');

            if (btn30d) {
                btn30d.addEventListener('click', function () {
                    if (symbolField.value) {
                        loadChartData(symbolField.value, '30d', showForecast);
                    }
                });
            }

            if (btn1y) {
                btn1y.addEventListener('click', function () {
                    if (symbolField.value) {
                        loadChartData(symbolField.value, '1y', showForecast);
                    }
                });
            }

            if (btnForecast) {
                btnForecast.addEventListener('click', function () {
                    if (symbolField.value) {
                        showForecast = !showForecast; // Toggle the state
                        loadChartData(symbolField.value, '30d', showForecast);
                    }
                });
            }
        });

        // This function is called from C# to initialize the chart after a search
        function initializeChart(symbol) {
             const symbolField = document.getElementById('<%= hfCurrentSymbol.ClientID %>');
            symbolField.value = symbol;
            showForecast = false; // Reset forecast view on new search
            loadChartData(symbol, '30d', false);
        }
    </script>
</asp:Content>
=======
        // ✅ FIXED: Removed the automatic call on DOMContentLoaded to prevent race conditions.
        // The server will now explicitly call drawChart() when it's ready.
    </script>
</asp:Content>
>>>>>>> 07a4dbcf93962d5594baae011952de56a750ffbd
