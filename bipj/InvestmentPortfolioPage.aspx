<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InvestmentPortfolioPage.aspx.cs" Inherits="bipj.InvestmentPortfolioPage" Async="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        .asset-detail-label {
            font-weight: bold;
            color: #555;
        }
        .portfolio-grid th {
            background-color: #f2f2f2;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid mt-4">
        <div class="row">
            <div class="col-12">
                <asp:Label ID="lblPortfolioName" runat="server" CssClass="h2"></asp:Label>
                <hr />
            </div>
        </div>

        <div class="row">
            <%-- Left Column: Search, Details, and Chart --%>
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

                        <%-- Chart Area --%>
                        <div class="mt-4">
                            <canvas id="priceChart"></canvas>
                        </div>
                        <div class="text-center mt-2">
                            <button type="button" id="btn30d" class="btn btn-sm btn-outline-secondary">30 Days</button>
                            <button type="button" id="btn1y" class="btn btn-sm btn-outline-secondary">1 Year</button>
                            <button type="button" id="btnForecast" class="btn btn-sm btn-outline-primary">Toggle 7-Day Forecast</button>
                        </div>

                        <%-- Add to Portfolio Section --%>
                        <div class="input-group mt-4">
                            <span class="input-group-text">Quantity</span>
                            <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" TextMode="Number" step="0.0001" Text="1"></asp:TextBox>
                            <asp:Button ID="btnAddAsset" runat="server" Text="Add to Portfolio" OnClick="btnAddAsset_Click" CssClass="btn btn-success" />
                        </div>
                        <asp:Label ID="lblAddStatus" runat="server" CssClass="mt-2 d-block" ForeColor="Red"></asp:Label>
                    </div>
                </asp:Panel>
            </div>

            <%-- Right Column: Current Portfolio --%>
            <div class="col-lg-5">
                <h4><i class="fa fa-briefcase"></i> Current Portfolio</h4>
                <asp:GridView ID="gvPortfolioAssets" runat="server" AutoGenerateColumns="false" CssClass="table table-hover portfolio-grid" GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="Symbol" HeaderText="Symbol" />
                        <asp:BoundField DataField="AssetName" HeaderText="Name" />
                        <asp:BoundField DataField="Quantity" HeaderText="Quantity" DataFormatString="{0:N4}" />
                        <asp:BoundField DataField="PurchasedPrice" HeaderText="Purchase Price" DataFormatString="{0:C}" />
                        <asp:BoundField DataField="PurchasedAt" HeaderText="Date Added" DataFormatString="{0:g}" />
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

    <%-- Hidden field to store the currently searched symbol for JavaScript --%>
    <asp:HiddenField ID="hfCurrentSymbol" runat="server" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script type="text/javascript">
        // Global chart variable
        let priceChart;
        let showForecast = false;

        // Function to call C# WebMethod and get chart data
        function loadChartData(symbol, timePeriod, includeForecast) {
            PageMethods.GetChartData(symbol, timePeriod, includeForecast, function (response) {
                renderChart(response);
            }, function (error) {
                console.error("Error loading chart data: " + error.responseText);
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
                    }
                }
            });
        }
        
        // Event Listeners for buttons
        document.addEventListener('DOMContentLoaded', function () {
            // These event listeners will be active after a postback as well
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
                        // Reload the chart with the last used time period
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
