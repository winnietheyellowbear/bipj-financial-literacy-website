using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
<<<<<<< HEAD
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
=======
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
>>>>>>> 07a4dbcf93962d5594baae011952de56a750ffbd

namespace bipj
{
    public partial class InvestmentPortfolioPage : System.Web.UI.Page
    {
        private static readonly string ApiKey = "cc404b6f2d2240f3b60772749ae6ea11";
<<<<<<< HEAD

        private static readonly string DbConstr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
        private static readonly HttpClient client = new HttpClient();

=======
        private static readonly string DbConstr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
        private static readonly HttpClient client = new HttpClient();

        // Property to hold PortfolioID from URL
>>>>>>> 07a4dbcf93962d5594baae011952de56a750ffbd
        private int PortfolioID
        {
            get { return ViewState["PortfolioID"] != null ? (int)ViewState["PortfolioID"] : 0; }
            set { ViewState["PortfolioID"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
<<<<<<< HEAD
                if (int.TryParse(Request.QueryString["id"], out int portfolioId))
                {
                    PortfolioID = portfolioId;
                    RegisterAsyncTask(new PageAsyncTask(LoadPortfolioDataAsync));
                }
                else
                {
                    Response.Redirect("InvestmentPage.aspx");
                }
            }
=======
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

            // Call the new helper method to get asset details (from cache or API)
            AssetDetails details = await GetAssetDetailsAsync(symbol);

            if (details != null)
            {
                lblPrice.Text = $"Current Price for {symbol}: ${details.Price:F2}";
                lblAssetDescription.Text = details.Name;

                ViewState["CurrentPrice"] = details.Price;
                ViewState["CurrentSymbol"] = symbol;
                ViewState["CurrentAssetName"] = details.Name;

                await LoadHistoryAndDrawChart(symbol, 30);
                btnForecast.Enabled = true;
            }
            else
            {
                lblPrice.Text = "Price not available for this symbol. Please try another (e.g., AAPL, BTC/USD).";
                btnForecast.Enabled = false;
            }
        }

        private async Task<AssetDetails> GetAssetDetailsAsync(string symbol)
        {
            using (var con = new SqlConnection(DbConstr))
            {
                string query = "SELECT AssetName, LastApiUpdate FROM Assets WHERE Symbol = @Symbol";
                var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Symbol", symbol);
                await con.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var lastUpdate = reader["LastApiUpdate"] as DateTime?;
                        if (lastUpdate.HasValue && lastUpdate > DateTime.UtcNow.AddHours(-1))
                        {
                            string assetName = reader["AssetName"].ToString();
                            string priceUrl = $"https://api.twelvedata.com/price?symbol={symbol}&apikey={ApiKey}";
                            var priceResponse = await client.GetStringAsync(priceUrl);
                            JObject priceData = JObject.Parse(priceResponse);
                            if (decimal.TryParse(priceData["price"]?.ToString(), out decimal price))
                            {
                                return new AssetDetails { Name = assetName, Price = price };
                            }
                        }
                    }
                }
            }

            string profileUrl = $"https://api.twelvedata.com/profile?symbol={symbol}&apikey={ApiKey}";
            string priceUrlApi = $"https://api.twelvedata.com/price?symbol={symbol}&apikey={ApiKey}";

            var profileTask = client.GetStringAsync(profileUrl);
            var priceTask = client.GetStringAsync(priceUrlApi);
            await Task.WhenAll(profileTask, priceTask);

            JObject profileData = JObject.Parse(await profileTask);
            JObject priceDataApi = JObject.Parse(await priceTask);

            string freshName = profileData["name"]?.ToString();
            string freshPriceStr = priceDataApi["price"]?.ToString();

            // ✅ FIXED: The logic now prioritizes getting a valid price. If a name isn't available from the profile,
            // it uses the symbol as a fallback, allowing assets like cryptocurrencies to be processed correctly.
            if (decimal.TryParse(freshPriceStr, out decimal freshPrice))
            {
                // Use the profile name if available, otherwise default to the symbol itself.
                string finalAssetName = string.IsNullOrEmpty(freshName) ? symbol : freshName;

                using (var con = new SqlConnection(DbConstr))
                {
                    string upsertQuery = @"
                        MERGE Assets AS target
                        USING (SELECT @Symbol AS Symbol) AS source ON (target.Symbol = source.Symbol)
                        WHEN MATCHED THEN 
                            UPDATE SET AssetName = @AssetName, LastApiUpdate = @LastApiUpdate
                        WHEN NOT MATCHED THEN 
                            INSERT (Symbol, AssetName, LastApiUpdate) VALUES (@Symbol, @AssetName, @LastApiUpdate);";

                    using (var cmd = new SqlCommand(upsertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Symbol", symbol);
                        cmd.Parameters.AddWithValue("@AssetName", finalAssetName);
                        cmd.Parameters.AddWithValue("@LastApiUpdate", DateTime.UtcNow);
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return new AssetDetails { Name = finalAssetName, Price = freshPrice };
            }

            return null;
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
            var historicalData = await GetHistoricalPricesAsync(symbol, days);

            if (historicalData != null && historicalData.Any())
            {
                // Data is returned with newest first, so we reverse it for the chart.
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
            var prices = new List<HistoricalPrice>();
            int assetId = await GetOrCreateAssetIdAsync(symbol, symbol); // Ensure asset exists

            using (var con = new SqlConnection(DbConstr))
            {
                await con.OpenAsync();
                // Check for the most recent date in our cache for this asset
                string latestDateQuery = "SELECT MAX(PriceDate) FROM AssetPriceHistory WHERE AssetID = @AssetID";
                var dateCmd = new SqlCommand(latestDateQuery, con);
                dateCmd.Parameters.AddWithValue("@AssetID", assetId);
                object lastDateResult = await dateCmd.ExecuteScalarAsync();

                // If our cache is up-to-date (has today's or yesterday's price), we can try to use it.
                if (lastDateResult != DBNull.Value && ((DateTime)lastDateResult) >= DateTime.Today.AddDays(-1))
                {
                    string cacheQuery = "SELECT TOP (@Days) PriceDate, ClosePrice FROM AssetPriceHistory WHERE AssetID = @AssetID ORDER BY PriceDate DESC";
                    var cmd = new SqlCommand(cacheQuery, con);
                    cmd.Parameters.AddWithValue("@AssetID", assetId);
                    cmd.Parameters.AddWithValue("@Days", days);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            prices.Add(new HistoricalPrice { Date = (DateTime)reader["PriceDate"], Price = (decimal)reader["ClosePrice"] });
                        }
                    }
                    // If we found enough data in the cache, we're done!
                    if (prices.Count >= days) return prices;
                }

                // CACHE MISS/STALE: We need to call the API.
                string url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval=1day&outputsize={days}&apikey={ApiKey}";
                var response = await client.GetStringAsync(url);
                JObject data = JObject.Parse(response);

                if (data["values"] != null)
                {
                    prices.Clear(); // Clear any partial data we might have read from a stale cache
                    var values = data["values"].Select(item => new HistoricalPrice
                    {
                        Date = DateTime.Parse(item["datetime"].ToString()),
                        Price = decimal.Parse(item["close"].ToString(), CultureInfo.InvariantCulture)
                    }).OrderByDescending(p => p.Date).Take(days).ToList();

                    // Update our database cache with the new data
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
                    return values;
                }
            }
            return prices; // Return whatever we have, even if it's empty
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

        private class AssetDetails
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        private class HistoricalPrice
        {
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
>>>>>>> 07a4dbcf93962d5594baae011952de56a750ffbd
        }

        // ✅ FIXED: Restructured to use a single connection and removed the non-existent 'CloseAsync' call.
        private async Task LoadPortfolioDataAsync()
        {
            using (SqlConnection con = new SqlConnection(DbConstr))
            {
                await con.OpenAsync();

                // Get Portfolio Name
                string nameQuery = "SELECT PortfolioName FROM Portfolios WHERE PortfolioID = @PortfolioID";
                using (SqlCommand nameCmd = new SqlCommand(nameQuery, con))
                {
                    nameCmd.Parameters.AddWithValue("@PortfolioID", PortfolioID);
                    object result = await nameCmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        lblPortfolioName.Text = result.ToString();
                    }
                }

                // Get Assets in Portfolio (reusing the same open connection)
                string assetsQuery = @"
                    SELECT pa.PortfolioAssetID, a.Symbol, a.AssetName, pa.Quantity, pa.PurchasedPrice, pa.PurchasedAt
                    FROM PortfolioAssets pa
                    JOIN Assets a ON pa.AssetID = a.AssetID
                    WHERE pa.PortfolioID = @PortfolioID
                    ORDER BY pa.PurchasedAt DESC";

                using (SqlDataAdapter sda = new SqlDataAdapter(assetsQuery, con))
                {
                    sda.SelectCommand.Parameters.AddWithValue("@PortfolioID", PortfolioID);
                    DataTable dt = new DataTable();
                    sda.Fill(dt);

                    gvPortfolioAssets.DataKeyNames = new string[] { "PortfolioAssetID" };
                    gvPortfolioAssets.DataSource = dt;
                    gvPortfolioAssets.DataBind();
                }
            } // The connection is automatically closed here by the 'using' statement.
        }

        protected async void btnSearch_Click(object sender, EventArgs e)
        {
            string symbol = txtAssetSymbol.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(symbol))
            {
                lblSearchStatus.Text = "Please enter an asset symbol.";
                return;
            }

            try
            {
                AssetData asset = await GetAssetData(symbol);
                if (asset != null)
                {
                    pnlAssetDetails.Visible = true;
                    lblSearchStatus.Text = "";
                    litAssetName.Text = asset.AssetName;
                    litAssetSymbol.Text = asset.Symbol;
                    litCurrentPrice.Text = asset.CurrentPrice.ToString("F2");
                    litAssetDescription.Text = asset.Description;
                    litSector.Text = asset.Sector;
                    litAssetType.Text = asset.AssetType;
                    litGeography.Text = asset.Geography;
                    hfCurrentSymbol.Value = asset.Symbol;
                    ViewState["CurrentAsset"] = asset;

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "initChart", $"initializeChart('{asset.Symbol}');", true);
                }
                else
                {
                    pnlAssetDetails.Visible = false;
                    lblSearchStatus.Text = "Asset not found or API error.";
                    ViewState["CurrentAsset"] = null;
                }
            }
            catch (Exception ex)
            {
                pnlAssetDetails.Visible = false;
                lblSearchStatus.Text = "An error occurred: " + ex.Message;
                ViewState["CurrentAsset"] = null;
            }
        }

        private async Task<AssetData> GetAssetData(string symbol)
        {
            using (var con = new SqlConnection(DbConstr))
            {
                string query = "SELECT AssetID, AssetName, Symbol, AssetType, Description, Sector, Geography, LastApiUpdate FROM Assets WHERE Symbol = @Symbol";
                var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Symbol", symbol);
                await con.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var lastUpdate = reader["LastApiUpdate"] as DateTime?;
                    if (lastUpdate.HasValue && lastUpdate > DateTime.UtcNow.AddHours(-4))
                    {
                        var asset = new AssetData
                        {
                            AssetId = (int)reader["AssetID"],
                            AssetName = reader["AssetName"].ToString(),
                            Symbol = reader["Symbol"].ToString(),
                            AssetType = reader["AssetType"].ToString(),
                            Description = reader["Description"].ToString(),
                            Sector = reader["Sector"].ToString(),
                            Geography = reader["Geography"].ToString(),
                        };
                        string priceUrl = $"https://api.twelvedata.com/quote?symbol={symbol}&apikey={ApiKey}";
                        var priceResponse = await client.GetStringAsync(priceUrl);
                        var priceData = JsonConvert.DeserializeObject<QuoteResponse>(priceResponse);
                        asset.CurrentPrice = priceData?.Close ?? 0;
                        return asset;
                    }
                }
            }

            string profileUrl = $"https://api.twelvedata.com/profile?symbol={symbol}&apikey={ApiKey}";
            string quoteUrl = $"https://api.twelvedata.com/quote?symbol={symbol}&apikey={ApiKey}";

            var profileTask = client.GetStringAsync(profileUrl);
            var quoteTask = client.GetStringAsync(quoteUrl);
            await Task.WhenAll(profileTask, quoteTask);

            var profile = JsonConvert.DeserializeObject<ProfileResponse>(await profileTask);
            var quote = JsonConvert.DeserializeObject<QuoteResponse>(await quoteTask);

            if (profile == null || quote == null || string.IsNullOrEmpty(profile.Name)) return null;

            var newAsset = new AssetData
            {
                Symbol = symbol,
                AssetName = profile.Name,
                Description = profile.Description,
                AssetType = profile.Type,
                Geography = profile.Country,
                Sector = profile.Sector,
                CurrentPrice = quote.Close
            };

            using (var con = new SqlConnection(DbConstr))
            {
                string upsertQuery = @"
                    MERGE Assets AS target
                    USING (SELECT @Symbol AS Symbol) AS source
                    ON (target.Symbol = source.Symbol)
                    WHEN MATCHED THEN
                        UPDATE SET AssetName = @AssetName, Description = @Description, AssetType = @AssetType, Geography = @Geography, Sector = @Sector, LastApiUpdate = @LastApiUpdate
                    WHEN NOT MATCHED THEN
                        INSERT (Symbol, AssetName, Description, AssetType, Geography, Sector, LastApiUpdate)
                        VALUES (@Symbol, @AssetName, @Description, @AssetType, @Geography, @Sector, @LastApiUpdate);
                    SELECT AssetID FROM Assets WHERE Symbol = @Symbol;";

                var cmd = new SqlCommand(upsertQuery, con);
                cmd.Parameters.AddWithValue("@Symbol", newAsset.Symbol);
                cmd.Parameters.AddWithValue("@AssetName", (object)newAsset.AssetName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", (object)newAsset.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AssetType", (object)newAsset.AssetType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Geography", (object)newAsset.Geography ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Sector", (object)newAsset.Sector ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LastApiUpdate", DateTime.UtcNow);

                await con.OpenAsync();
                newAsset.AssetId = (int)await cmd.ExecuteScalarAsync();
            }
            return newAsset;
        }


        [WebMethod]
        public static string GetChartData(string symbol, string timePeriod, bool includeForecast)
        {
            List<HistoricalPrice> prices = GetHistoricalPrices(symbol).Result;
            var chartData = new ChartData();
            var pricesToChart = new List<HistoricalPrice>();
            DateTime startDate = timePeriod == "1y" ? DateTime.Today.AddYears(-1) : DateTime.Today.AddDays(-30);
            pricesToChart = prices.Where(p => p.Date >= startDate).OrderBy(p => p.Date).ToList();

            chartData.Labels = pricesToChart.Select(p => p.Date.ToString("MMM dd")).ToList();
            chartData.HistoricalPrices = pricesToChart.Select(p => p.Price).ToList();

            if (includeForecast && pricesToChart.Any())
            {
                var forecast = CalculateForecast(pricesToChart, 7);
                int historyCount = chartData.HistoricalPrices.Count;
                chartData.ForecastPrices = new List<decimal?>();
                for (int i = 0; i < historyCount - 1; i++)
                {
                    chartData.ForecastPrices.Add(null);
                }
                chartData.ForecastPrices.Add(chartData.HistoricalPrices.LastOrDefault());
                chartData.ForecastPrices.AddRange(forecast.Select(p => (decimal?)p.Price));

                DateTime lastDate = pricesToChart.Last().Date;
                for (int i = 1; i <= 7; i++)
                {
                    chartData.Labels.Add(lastDate.AddDays(i).ToString("MMM dd"));
                }
            }
            return new JavaScriptSerializer().Serialize(chartData);
        }

        private static async Task<List<HistoricalPrice>> GetHistoricalPrices(string symbol)
        {
            List<HistoricalPrice> prices = new List<HistoricalPrice>();
            int assetId;

            using (var con = new SqlConnection(DbConstr))
            {
                await con.OpenAsync();
                string assetQuery = "SELECT AssetID FROM Assets WHERE Symbol = @Symbol";
                using (var assetCmd = new SqlCommand(assetQuery, con))
                {
                    assetCmd.Parameters.AddWithValue("@Symbol", symbol);
                    object assetIdResult = await assetCmd.ExecuteScalarAsync();

                    if (assetIdResult == null) return prices;
                    assetId = (int)assetIdResult;
                }

                string latestDateQuery = "SELECT MAX(PriceDate) FROM AssetPriceHistory WHERE AssetID = @AssetID";
                using (var dateCmd = new SqlCommand(latestDateQuery, con))
                {
                    dateCmd.Parameters.AddWithValue("@AssetID", assetId);
                    object lastDateResult = await dateCmd.ExecuteScalarAsync();

                    if (lastDateResult == DBNull.Value || ((DateTime)lastDateResult) < DateTime.Today.AddDays(-1))
                    {
                        string url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval=1day&outputsize=365&apikey={ApiKey}";
                        var response = await client.GetStringAsync(url);
                        var apiData = JsonConvert.DeserializeObject<TimeSeriesResponse>(response);

                        if (apiData?.Values != null)
                        {
                            var mergeQuery = @"
                                MERGE AssetPriceHistory AS target
                                USING (SELECT @AssetID AS AssetID, @PriceDate AS PriceDate) AS source
                                ON (target.AssetID = source.AssetID AND target.PriceDate = source.PriceDate)
                                WHEN NOT MATCHED THEN
                                    INSERT (AssetID, PriceDate, ClosePrice) VALUES (@AssetID, @PriceDate, @ClosePrice);";

                            foreach (var value in apiData.Values)
                            {
                                using (var mergeCmd = new SqlCommand(mergeQuery, con))
                                {
                                    mergeCmd.Parameters.AddWithValue("@AssetID", assetId);
                                    mergeCmd.Parameters.AddWithValue("@PriceDate", value.Datetime);
                                    mergeCmd.Parameters.AddWithValue("@ClosePrice", value.Close);
                                    await mergeCmd.ExecuteNonQueryAsync();
                                }
                            }
                        }
                    }
                }

                string selectQuery = "SELECT PriceDate, ClosePrice FROM AssetPriceHistory WHERE AssetID = @AssetID ORDER BY PriceDate";
                using (var selectCmd = new SqlCommand(selectQuery, con))
                {
                    selectCmd.Parameters.AddWithValue("@AssetID", assetId);
                    using (var reader = await selectCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            prices.Add(new HistoricalPrice { Date = (DateTime)reader["PriceDate"], Price = (decimal)reader["ClosePrice"] });
                        }
                    }
                }
            }
            return prices;
        }

        private static List<HistoricalPrice> CalculateForecast(List<HistoricalPrice> historicalData, int daysToForecast)
        {
            var forecast = new List<HistoricalPrice>();
            if (historicalData.Count < 20) return forecast;

            var recentPrices = historicalData.Skip(Math.Max(0, historicalData.Count - 20)).Select(p => p.Price).ToList();
            decimal alpha = 2.0m / (recentPrices.Count + 1);
            decimal ema = recentPrices[0];
            for (int i = 1; i < recentPrices.Count; i++)
            {
                ema = (recentPrices[i] * alpha) + (ema * (1 - alpha));
            }
            decimal lastPrice = historicalData.Last().Price;
            decimal dailyTrend = (ema - recentPrices.Average()) / recentPrices.Count;

            DateTime lastDate = historicalData.Last().Date;
            for (int i = 1; i <= daysToForecast; i++)
            {
                lastPrice += dailyTrend;
                forecast.Add(new HistoricalPrice { Date = lastDate.AddDays(i), Price = lastPrice });
            }
            return forecast;
        }

        protected async void btnAddAsset_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtQuantity.Text, out decimal quantity) || quantity <= 0)
            {
                lblAddStatus.Text = "Please enter a valid positive quantity.";
                return;
            }

            var assetData = ViewState["CurrentAsset"] as AssetData;

            if (assetData != null)
            {
                try
                {
                    using (var con = new SqlConnection(DbConstr))
                    {
                        string query = @"
                            INSERT INTO PortfolioAssets (PortfolioID, AssetID, Quantity, PurchasedPrice, PurchasedAt)
                            VALUES (@PortfolioID, @AssetID, @Quantity, @PurchasedPrice, @PurchasedAt);
                            UPDATE Portfolios SET LastUpdatedAt = @PurchasedAt WHERE PortfolioID = @PortfolioID;";

                        var cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@PortfolioID", PortfolioID);
                        cmd.Parameters.AddWithValue("@AssetID", assetData.AssetId);
                        cmd.Parameters.AddWithValue("@Quantity", quantity);
                        cmd.Parameters.AddWithValue("@PurchasedPrice", assetData.CurrentPrice);
                        cmd.Parameters.AddWithValue("@PurchasedAt", DateTime.UtcNow);

                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                    lblAddStatus.Text = "";
                    txtQuantity.Text = "1";
                    await LoadPortfolioDataAsync();
                }
                catch (Exception ex)
                {
                    lblAddStatus.Text = "Error adding asset: " + ex.Message;
                }
            }
            else
            {
                lblAddStatus.Text = "Could not add asset. Please search for an asset again.";
            }
        }

        protected async void gvPortfolioAssets_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int portfolioAssetId = Convert.ToInt32(gvPortfolioAssets.DataKeys[e.RowIndex].Value);

            using (var con = new SqlConnection(DbConstr))
            {
                string query = "DELETE FROM PortfolioAssets WHERE PortfolioAssetID = @PortfolioAssetID";
                var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PortfolioAssetID", portfolioAssetId);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }

            await LoadPortfolioDataAsync();
        }

        protected void btnGoToDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect($"InvestmentDashboardPage.aspx?id={PortfolioID}");
        }

        #region Helper Classes
        [Serializable]
        public class AssetData
        {
            public int AssetId { get; set; }
            public string Symbol { get; set; }
            public string AssetName { get; set; }
            public string Description { get; set; }
            public string Sector { get; set; }
            public string Geography { get; set; }
            public string AssetType { get; set; }
            public decimal CurrentPrice { get; set; }
        }

        [Serializable]
        public class ChartData
        {
            public List<string> Labels { get; set; }
            public List<decimal> HistoricalPrices { get; set; }
            public List<decimal?> ForecastPrices { get; set; }
        }

        [Serializable]
        public class HistoricalPrice
        {
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
        }

        public class ProfileResponse
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Country { get; set; }
            public string Sector { get; set; }
            public string Type { get; set; }
        }
        public class QuoteResponse
        {
            [JsonProperty("close")]
            public decimal Close { get; set; }
        }
        public class TimeSeriesResponse
        {
            public List<TimeSeriesValue> Values { get; set; }
        }
        public class TimeSeriesValue
        {
            public DateTime Datetime { get; set; }
            public decimal Close { get; set; }
        }
        #endregion
    }
}
