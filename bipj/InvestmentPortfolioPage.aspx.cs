using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bipj
{
    public partial class InvestmentPortfolioPage : System.Web.UI.Page
    {
        private static readonly string ApiKey = "cc404b6f2d2240f3b60772749ae6ea11";
        private static readonly string DbConstr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
        private static readonly HttpClient client = new HttpClient();

        // Property to hold PortfolioID from URL
        private int PortfolioID
        {
            get { return ViewState["PortfolioID"] != null ? (int)ViewState["PortfolioID"] : 0; }
            set { ViewState["PortfolioID"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // ADAPTED: Get PortfolioID from URL, not Session.
                if (int.TryParse(Request.QueryString["id"], out int portfolioId))
                {
                    PortfolioID = portfolioId;
                    RegisterAsyncTask(new PageAsyncTask(LoadInitialDataAsync));
                }
                else
                {
                    // If no ID is provided, redirect to the main investment page.
                    Response.Redirect("InvestmentPage.aspx");
                }
            }
        }

        private async Task LoadInitialDataAsync()
        {
            await LoadPortfolioAssetsAsync();

            // Set initial chart to empty
            hfPriceLabels.Value = "[]";
            hfPriceData.Value = "[]";
            hfForecastData.Value = "[]";
            hfForecastUpper.Value = "[]";
            hfForecastLower.Value = "[]";
            btnForecast.Enabled = false;

            // ✅ FIXED: Explicitly call the JavaScript drawChart function after the server has prepared the hidden fields.
            ScriptManager.RegisterStartupScript(this, GetType(), "drawInitialChart", "drawChart();", true);
        }

        protected async void btnGetPrice_Click(object sender, EventArgs e)
        {
            string symbol = txtSymbol.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(symbol)) return;

            string priceUrl = $"https://api.twelvedata.com/price?symbol={symbol}&apikey={ApiKey}";
            string profileUrl = $"https://api.twelvedata.com/profile?symbol={symbol}&apikey={ApiKey}";

            var priceTask = client.GetStringAsync(priceUrl);
            var profileTask = client.GetStringAsync(profileUrl);
            await Task.WhenAll(priceTask, profileTask);

            var priceJson = await priceTask;
            var profileJson = await profileTask;

            JObject priceData = JObject.Parse(priceJson);
            JObject profileData = JObject.Parse(profileJson);

            string priceStr = priceData["price"]?.ToString();
            string assetName = profileData["name"]?.ToString();

            if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                lblPrice.Text = $"Current Price for {symbol}: ${price:F2}";
                lblAssetDescription.Text = string.IsNullOrEmpty(assetName) ? "Description not available." : assetName;

                // Use ViewState for state management, as it's more robust than Session for page-specific data.
                ViewState["CurrentPrice"] = price;
                ViewState["CurrentSymbol"] = symbol;
                ViewState["CurrentAssetName"] = string.IsNullOrEmpty(assetName) ? symbol : assetName;

                await LoadHistoryAndDrawChart(symbol, 30);
                btnForecast.Enabled = true;
            }
            else
            {
                lblPrice.Text = "Price not available for this symbol. Please try another (e.g., AAPL, BTC/USD).";
                btnForecast.Enabled = false;
            }
        }

        protected async void btnViewMonth_Click(object sender, EventArgs e)
        {
            string symbol = ViewState["CurrentSymbol"]?.ToString();
            if (!string.IsNullOrEmpty(symbol))
            {
                await LoadHistoryAndDrawChart(symbol, 30);
            }
        }

        protected async void btnViewYear_Click(object sender, EventArgs e)
        {
            string symbol = ViewState["CurrentSymbol"]?.ToString();
            if (!string.IsNullOrEmpty(symbol))
            {
                await LoadHistoryAndDrawChart(symbol, 365);
            }
        }

        protected void btnForecast_Click(object sender, EventArgs e)
        {
            // This logic now reads from hidden fields, as per the original design.
            var priceList = JsonConvert.DeserializeObject<List<decimal>>(hfPriceData.Value);
            var emaList = CalculateEMA(priceList, 10);
            if (emaList.Count < 2) return;

            var returns = new List<decimal>();
            for (int i = 1; i < priceList.Count; i++)
            {
                if (priceList[i - 1] != 0) returns.Add((priceList[i] - priceList[i - 1]) / priceList[i - 1]);
            }
            if (!returns.Any()) return;

            decimal avgReturn = returns.Average();
            decimal variance = returns.Average(r => (r - avgReturn) * (r - avgReturn));
            decimal stdDev = (decimal)Math.Sqrt((double)variance);

            var forecastCenter = new List<decimal>();
            var upperBand = new List<decimal>();
            var lowerBand = new List<decimal>();

            decimal slope = emaList.Last() - emaList[emaList.Count - 2];
            decimal lastEma = emaList.Last();

            for (int i = 1; i <= 7; i++)
            {
                lastEma += slope;
                decimal volatilityRange = lastEma * stdDev;
                forecastCenter.Add(lastEma);
                upperBand.Add(lastEma + volatilityRange);
                lowerBand.Add(lastEma - volatilityRange);
            }

            hfForecastData.Value = JsonConvert.SerializeObject(forecastCenter);
            hfForecastUpper.Value = JsonConvert.SerializeObject(upperBand);
            hfForecastLower.Value = JsonConvert.SerializeObject(lowerBand);

            // Append forecast dates to labels
            var labels = JsonConvert.DeserializeObject<List<string>>(hfPriceLabels.Value);
            DateTime lastDate = DateTime.Parse(labels.Last());
            for (int i = 1; i <= 7; i++) { labels.Add(lastDate.AddDays(i).ToString("yyyy-MM-dd")); }
            hfPriceLabels.Value = JsonConvert.SerializeObject(labels);

            ScriptManager.RegisterStartupScript(this, GetType(), "drawChart", "drawChart();", true);
        }

        protected async void btnAddAsset_Click(object sender, EventArgs e)
        {
            string symbol = ViewState["CurrentSymbol"]?.ToString();
            string assetName = ViewState["CurrentAssetName"]?.ToString();
            object priceObj = ViewState["CurrentPrice"];
            string quantityStr = txtQuantity.Text.Trim();

            if (string.IsNullOrEmpty(symbol) || priceObj == null || !decimal.TryParse(quantityStr, out decimal quantity))
            {
                litMessage.Text = "<p class='text-danger mt-2'>Missing data. Please search for an asset and enter a valid quantity.</p>";
                return;
            }

            decimal buyPrice = (decimal)priceObj;

            // ADAPTED: Ensure the asset exists in the 'Assets' table before adding to portfolio.
            int assetId = await GetOrCreateAssetIdAsync(symbol, assetName);

            using (SqlConnection conn = new SqlConnection(DbConstr))
            {
                // ADAPTED: Query now uses the correct table and column names.
                string insertQuery = @"INSERT INTO PortfolioAssets (PortfolioID, AssetID, Quantity, PurchasedPrice, PurchasedAt)
                                       VALUES (@PortfolioID, @AssetID, @Quantity, @PurchasedPrice, GETDATE());
                                       UPDATE Portfolios SET LastUpdatedAt = GETDATE() WHERE PortfolioID = @PortfolioID;";

                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@PortfolioID", this.PortfolioID);
                    cmd.Parameters.AddWithValue("@AssetID", assetId);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@PurchasedPrice", buyPrice);
                    try
                    {
                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        litMessage.Text = "<p class='text-success mt-2'>Asset added successfully!</p>";
                        txtQuantity.Text = "";
                        await LoadPortfolioAssetsAsync(); // Refresh GridView
                    }
                    catch (Exception ex)
                    {
                        litMessage.Text = $"<p class='text-danger mt-2'>Error saving to DB: {ex.Message}</p>";
                    }
                }
            }
        }

        protected async void gvPortfolioAssets_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteAsset")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int portfolioAssetId = Convert.ToInt32(gvPortfolioAssets.DataKeys[rowIndex].Value);

                using (SqlConnection conn = new SqlConnection(DbConstr))
                {
                    // ADAPTED: Deleting from the correct table using the correct primary key.
                    string deleteQuery = "DELETE FROM PortfolioAssets WHERE PortfolioAssetID = @PortfolioAssetID";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PortfolioAssetID", portfolioAssetId);
                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await LoadPortfolioAssetsAsync(); // Refresh table
            }
        }

        private async Task LoadHistoryAndDrawChart(string symbol, int days)
        {
            string url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval=1day&outputsize={days}&apikey={ApiKey}";
            var response = await client.GetStringAsync(url);
            JObject data = JObject.Parse(response);

            if (data["values"] != null)
            {
                var values = data["values"].Reverse().Take(days).ToList();
                var labels = values.Select(item => item["datetime"].ToString()).ToList();
                var prices = values.Select(item => decimal.Parse(item["close"].ToString(), CultureInfo.InvariantCulture)).ToList();

                hfPriceLabels.Value = JsonConvert.SerializeObject(labels);
                hfPriceData.Value = JsonConvert.SerializeObject(prices);
                // Clear forecast data on new history load
                hfForecastData.Value = "[]";
                hfForecastUpper.Value = "[]";
                hfForecastLower.Value = "[]";

                ScriptManager.RegisterStartupScript(this, GetType(), "drawChart", "drawChart();", true);
            }
        }

        private async Task LoadPortfolioAssetsAsync()
        {
            using (SqlConnection conn = new SqlConnection(DbConstr))
            {
                // ADAPTED: Query now joins with Assets table and uses correct column names.
                string query = @"SELECT pa.PortfolioAssetID, a.Symbol, pa.Quantity, pa.PurchasedPrice, pa.PurchasedAt 
                                 FROM PortfolioAssets pa
                                 JOIN Assets a ON pa.AssetID = a.AssetID
                                 WHERE pa.PortfolioID = @PortfolioID ORDER BY pa.PurchasedAt DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PortfolioID", this.PortfolioID);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        gvPortfolioAssets.DataSource = reader;
                        gvPortfolioAssets.DataBind();
                    }
                }

                // Also load the portfolio name
                string nameQuery = "SELECT PortfolioName FROM Portfolios WHERE PortfolioID = @PortfolioID";
                using (SqlCommand nameCmd = new SqlCommand(nameQuery, conn))
                {
                    nameCmd.Parameters.AddWithValue("@PortfolioID", this.PortfolioID);
                    object result = await nameCmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        lblPortfolioName.Text = result.ToString();
                    }
                }
            }
        }

        private async Task<int> GetOrCreateAssetIdAsync(string symbol, string assetName)
        {
            using (var con = new SqlConnection(DbConstr))
            {
                // ADAPTED: This logic is from your modern code, ensuring data integrity.
                string upsertQuery = @"
                    MERGE Assets AS target
                    USING (SELECT @Symbol AS Symbol) AS source ON (target.Symbol = source.Symbol)
                    WHEN NOT MATCHED THEN INSERT (Symbol, AssetName, LastApiUpdate) VALUES (@Symbol, @AssetName, GETDATE());
                    SELECT AssetID FROM Assets WHERE Symbol = @Symbol;";

                using (var cmd = new SqlCommand(upsertQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Symbol", symbol);
                    cmd.Parameters.AddWithValue("@AssetName", assetName);
                    await con.OpenAsync();
                    return (int)await cmd.ExecuteScalarAsync();
                }
            }
        }

        private List<decimal> CalculateEMA(List<decimal> prices, int period)
        {
            var ema = new List<decimal>();
            if (prices.Count < period) return ema;
            decimal multiplier = 2m / (period + 1);
            decimal previousEMA = prices.Take(period).Average();
            ema.Add(previousEMA);
            for (int i = period; i < prices.Count; i++)
            {
                decimal currentEMA = ((prices[i] - previousEMA) * multiplier) + previousEMA;
                ema.Add(currentEMA);
                previousEMA = currentEMA;
            }
            return ema;
        }

        protected void btnGoToDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect($"InvestmentDashboardPage.aspx?id={this.PortfolioID}");
        }
    }
}
