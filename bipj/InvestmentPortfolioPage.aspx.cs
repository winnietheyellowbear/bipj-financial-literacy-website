using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        private int PortfolioID
        {
            get { return ViewState["PortfolioID"] != null ? (int)ViewState["PortfolioID"] : 0; }
            set { ViewState["PortfolioID"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (int.TryParse(Request.QueryString["id"], out int portfolioId))
                {
                    PortfolioID = portfolioId;
                    RegisterAsyncTask(new PageAsyncTask(LoadInitialDataAsync));
                }
                else
                {
                    Response.Redirect("InvestmentPage.aspx");
                }
            }
        }

        private async Task LoadInitialDataAsync()
        {
            await LoadPortfolioAssetsAsync();
            hfPriceLabels.Value = "[]";
            hfPriceData.Value = "[]";
            hfForecastData.Value = "[]";
            hfForecastUpper.Value = "[]";
            hfForecastLower.Value = "[]";
            btnForecast.Enabled = false;
            ScriptManager.RegisterStartupScript(this, GetType(), "drawInitialChart", "drawChart();", true);
        }

        protected async void btnGetPrice_Click(object sender, EventArgs e)
        {
            string symbol = txtSymbol.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(symbol)) return;

            AssetFullDetails details = await GetAssetDetailsAsync(symbol);

            if (details != null)
            {
                lblPrice.Text = $"Current Price for {symbol}: ${details.Price:F2}";
                lblAssetDescription.Text = details.Description;
                ViewState["CurrentAsset"] = details;

                await LoadHistoryAndDrawChart(symbol, 30);
                btnForecast.Enabled = true;
            }
            else
            {
                lblPrice.Text = "Price not available for this symbol. Please try another (e.g., AAPL, BTC/USD).";
                btnForecast.Enabled = false;
            }
        }

        private async Task<AssetFullDetails> GetAssetDetailsAsync(string symbol)
        {
            // Step 1: Check cache. If price is from today, return cached data.
            using (var con = new SqlConnection(DbConstr))
            {
                string query = "SELECT AssetName, Description, Sector, Geography, AssetType, LastPrice, PriceLastUpdate FROM Assets WHERE Symbol = @Symbol";
                var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Symbol", symbol);
                await con.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var priceLastUpdate = reader["PriceLastUpdate"] as DateTime?;
                        if (priceLastUpdate.HasValue && priceLastUpdate.Value.Date >= DateTime.UtcNow.Date)
                        {
                            return new AssetFullDetails
                            {
                                Symbol = symbol,
                                Name = reader["AssetName"].ToString(),
                                Price = (decimal)reader["LastPrice"],
                                Description = FormatDescription(reader["Description"]?.ToString(), reader["AssetType"]?.ToString(), reader["Geography"]?.ToString(), reader["Sector"]?.ToString()),
                                AssetType = reader["AssetType"]?.ToString(),
                                Sector = reader["Sector"]?.ToString(),
                                Geography = reader["Geography"]?.ToString()
                            };
                        }
                    }
                }
            }

            // Step 2: CACHE MISS. Call APIs individually for resilience.
            decimal? freshPrice = null;
            JObject profileData = null;

            try
            {
                string priceUrlApi = $"https://api.twelvedata.com/price?symbol={symbol}&apikey={ApiKey}";
                var priceJson = await client.GetStringAsync(priceUrlApi);
                JObject priceDataApi = JObject.Parse(priceJson);
                if (decimal.TryParse(priceDataApi["price"]?.ToString(), out decimal p))
                {
                    freshPrice = p;
                }
            }
            catch { /* Price fetch failed, will return null later */ }

            if (freshPrice == null) return null;

            try
            {
                string profileUrl = $"https://api.twelvedata.com/profile?symbol={symbol}&apikey={ApiKey}";
                var profileJson = await client.GetStringAsync(profileUrl);
                profileData = JObject.Parse(profileJson);
            }
            catch { /* Profile fetch failed, we can proceed without it */ }

            var details = new AssetFullDetails
            {
                Symbol = symbol,
                Name = profileData?["name"]?.ToString() ?? symbol,
                Price = freshPrice.Value,
                Description = FormatDescription(profileData?["description"]?.ToString(), profileData?["type"]?.ToString(), profileData?["country"]?.ToString(), profileData?["sector"]?.ToString()),
                AssetType = profileData?["type"]?.ToString(),
                Sector = profileData?["sector"]?.ToString(),
                Geography = profileData?["country"]?.ToString()
            };

            // Step 3: Update database with whatever data we successfully fetched.
            using (var con = new SqlConnection(DbConstr))
            {
                string upsertQuery = @"
                    MERGE Assets AS target
                    USING (SELECT @Symbol AS Symbol) AS source ON (target.Symbol = source.Symbol)
                    WHEN MATCHED THEN 
                        UPDATE SET AssetName = @AssetName, Description = @Description, Sector = @Sector, Geography = @Geography, AssetType = @AssetType, LastApiUpdate = @LastApiUpdate, LastPrice = @LastPrice, PriceLastUpdate = @PriceLastUpdate
                    WHEN NOT MATCHED THEN 
                        INSERT (Symbol, AssetName, Description, Sector, Geography, AssetType, LastApiUpdate, LastPrice, PriceLastUpdate) 
                        VALUES (@Symbol, @AssetName, @Description, @Sector, @Geography, @AssetType, @LastApiUpdate, @LastPrice, @PriceLastUpdate);";

                using (var cmd = new SqlCommand(upsertQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Symbol", details.Symbol);
                    cmd.Parameters.AddWithValue("@AssetName", (object)details.Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)profileData?["description"]?.ToString() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Sector", (object)details.Sector ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Geography", (object)details.Geography ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AssetType", (object)details.AssetType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastApiUpdate", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@LastPrice", details.Price);
                    cmd.Parameters.AddWithValue("@PriceLastUpdate", DateTime.UtcNow);
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            return details;
        }

        private string FormatDescription(string description, string type, string country, string sector)
        {
            if (string.IsNullOrWhiteSpace(description) && string.IsNullOrWhiteSpace(type) && string.IsNullOrWhiteSpace(country) && string.IsNullOrWhiteSpace(sector))
            {
                return "<i>Asset description not applicable.</i>";
            }
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(description))
            {
                sb.Append(description);
            }
            var details = new List<string>();
            if (!string.IsNullOrWhiteSpace(type)) details.Add($"Type: {type}");
            if (!string.IsNullOrWhiteSpace(country)) details.Add($"Country: {country}");
            if (!string.IsNullOrWhiteSpace(sector)) details.Add($"Sector: {sector}");
            if (details.Any())
            {
                sb.Append($"<br/><br/><strong>{string.Join(" | ", details)}</strong>");
            }
            return sb.ToString();
        }

        protected async void btnAddAsset_Click(object sender, EventArgs e)
        {
            var assetDetails = ViewState["CurrentAsset"] as AssetFullDetails;
            string quantityStr = txtQuantity.Text.Trim();
            if (assetDetails == null || !decimal.TryParse(quantityStr, out decimal quantity))
            {
                litMessage.Text = "<p class='text-danger mt-2'>Missing data. Please search for an asset and enter a valid quantity.</p>";
                return;
            }
            int assetId = await GetOrCreateAssetIdAsync(assetDetails.Symbol, assetDetails.Name);
            using (SqlConnection conn = new SqlConnection(DbConstr))
            {
                string insertQuery = @"INSERT INTO PortfolioAssets (PortfolioID, AssetID, Quantity, PurchasedPrice, PurchasedAt)
                                       VALUES (@PortfolioID, @AssetID, @Quantity, @PurchasedPrice, GETDATE());
                                       UPDATE Portfolios SET LastUpdatedAt = GETDATE() WHERE PortfolioID = @PortfolioID;";
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@PortfolioID", this.PortfolioID);
                    cmd.Parameters.AddWithValue("@AssetID", assetId);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@PurchasedPrice", assetDetails.Price);
                    try
                    {
                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        litMessage.Text = "<p class='text-success mt-2'>Asset added successfully!</p>";
                        txtQuantity.Text = "";
                        await LoadPortfolioAssetsAsync();
                    }
                    catch (Exception ex)
                    {
                        litMessage.Text = $"<p class='text-danger mt-2'>Error saving to DB: {ex.Message}</p>";
                    }
                }
            }
        }

        protected async void btnViewMonth_Click(object sender, EventArgs e)
        {
            var assetDetails = ViewState["CurrentAsset"] as AssetFullDetails;
            if (assetDetails != null)
            {
                await LoadHistoryAndDrawChart(assetDetails.Symbol, 30);
            }
        }

        protected async void btnViewYear_Click(object sender, EventArgs e)
        {
            var assetDetails = ViewState["CurrentAsset"] as AssetFullDetails;
            if (assetDetails != null)
            {
                await LoadHistoryAndDrawChart(assetDetails.Symbol, 365);
            }
        }

        protected void btnForecast_Click(object sender, EventArgs e)
        {
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
            var labels = JsonConvert.DeserializeObject<List<string>>(hfPriceLabels.Value);
            DateTime lastDate = DateTime.Parse(labels.Last());
            for (int i = 1; i <= 7; i++) { labels.Add(lastDate.AddDays(i).ToString("yyyy-MM-dd")); }
            hfPriceLabels.Value = JsonConvert.SerializeObject(labels);
            ScriptManager.RegisterStartupScript(this, GetType(), "drawChart", "drawChart();", true);
        }

        protected async void gvPortfolioAssets_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteAsset")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int portfolioAssetId = Convert.ToInt32(gvPortfolioAssets.DataKeys[rowIndex].Value);
                using (SqlConnection conn = new SqlConnection(DbConstr))
                {
                    string deleteQuery = "DELETE FROM PortfolioAssets WHERE PortfolioAssetID = @PortfolioAssetID";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PortfolioAssetID", portfolioAssetId);
                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await LoadPortfolioAssetsAsync();
            }
        }

        private async Task LoadHistoryAndDrawChart(string symbol, int days)
        {
            var historicalData = await GetHistoricalPricesAsync(symbol, days);
            if (historicalData != null && historicalData.Any())
            {
                historicalData.Reverse();
                var labels = historicalData.Select(item => item.Date.ToString("yyyy-MM-dd")).ToList();
                var prices = historicalData.Select(item => item.Price).ToList();
                hfPriceLabels.Value = JsonConvert.SerializeObject(labels);
                hfPriceData.Value = JsonConvert.SerializeObject(prices);
                hfForecastData.Value = "[]";
                hfForecastUpper.Value = "[]";
                hfForecastLower.Value = "[]";
                ScriptManager.RegisterStartupScript(this, GetType(), "drawChart", "drawChart();", true);
            }
        }

        private async Task<List<HistoricalPrice>> GetHistoricalPricesAsync(string symbol, int days)
        {
            int assetId = await GetOrCreateAssetIdAsync(symbol, symbol);
            using (var con = new SqlConnection(DbConstr))
            {
                await con.OpenAsync();

                // First, try to read from the cache.
                string cacheQuery = "SELECT TOP (@Days) PriceDate, ClosePrice FROM AssetPriceHistory WHERE AssetID = @AssetID ORDER BY PriceDate DESC";
                var cmd = new SqlCommand(cacheQuery, con);
                cmd.Parameters.AddWithValue("@AssetID", assetId);
                cmd.Parameters.AddWithValue("@Days", days);
                var prices = new List<HistoricalPrice>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        prices.Add(new HistoricalPrice { Date = (DateTime)reader["PriceDate"], Price = (decimal)reader["ClosePrice"] });
                    }
                }

                // ✅ FIXED: This is the corrected logic. We check if the data we found is fresh and sufficient.
                // We only call the API if the cache is empty OR the newest item is from before yesterday OR we don't have enough data points.
                DateTime latestDateInCache = prices.Any() ? prices.Max(p => p.Date) : DateTime.MinValue;
                if (!prices.Any() || latestDateInCache.Date < DateTime.UtcNow.Date.AddDays(-1) || prices.Count < days)
                {
                    // CACHE MISS/STALE: Fetch a large, fixed amount of data to stock our cache.
                    string url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval=1day&outputsize=400&apikey={ApiKey}";
                    var response = await client.GetStringAsync(url);
                    JObject data = JObject.Parse(response);
                    if (data["values"] != null)
                    {
                        var values = data["values"].Select(item => new HistoricalPrice
                        {
                            Date = DateTime.Parse(item["datetime"].ToString()),
                            Price = decimal.Parse(item["close"].ToString(), CultureInfo.InvariantCulture)
                        }).ToList();

                        var mergeQuery = @"
                            MERGE AssetPriceHistory AS target
                            USING (SELECT @AssetID AS AssetID, @PriceDate AS PriceDate) AS source ON (target.AssetID = source.AssetID AND target.PriceDate = source.PriceDate)
                            WHEN NOT MATCHED THEN INSERT (AssetID, PriceDate, ClosePrice) VALUES (@AssetID, @PriceDate, @ClosePrice);";
                        foreach (var value in values)
                        {
                            using (var mergeCmd = new SqlCommand(mergeQuery, con))
                            {
                                mergeCmd.Parameters.AddWithValue("@AssetID", assetId);
                                mergeCmd.Parameters.AddWithValue("@PriceDate", value.Date);
                                mergeCmd.Parameters.AddWithValue("@ClosePrice", value.Price);
                                await mergeCmd.ExecuteNonQueryAsync();
                            }
                        }
                        // After updating the cache, return the requested number of days from the newly fetched data.
                        return values.OrderByDescending(p => p.Date).Take(days).ToList();
                    }
                }

                // CACHE HIT: If we get here, it means the data we read from the cache initially was good enough.
                return prices;
            }
        }

        private async Task LoadPortfolioAssetsAsync()
        {
            using (SqlConnection conn = new SqlConnection(DbConstr))
            {
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
                string upsertQuery = @"
                    MERGE Assets AS target
                    USING (SELECT @Symbol AS Symbol) AS source ON (target.Symbol = source.Symbol)
                    WHEN NOT MATCHED THEN INSERT (Symbol, AssetName) VALUES (@Symbol, @AssetName);
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

        [Serializable]
        private class AssetFullDetails
        {
            public string Symbol { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string Description { get; set; }
            public string AssetType { get; set; }
            public string Sector { get; set; }
            public string Geography { get; set; }
        }

        [Serializable]
        private class HistoricalPrice
        {
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
        }
    }
}
