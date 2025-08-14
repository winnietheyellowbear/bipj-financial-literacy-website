using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                    LoadPortfolioData();
                }
                else
                {
                    Response.Redirect("InvestmentPage.aspx");
                }
            }
        }

        private void LoadPortfolioData()
        {
            using (SqlConnection con = new SqlConnection(DbConstr))
            {
                // Get Portfolio Name
                string nameQuery = "SELECT PortfolioName FROM Portfolios WHERE PortfolioID = @PortfolioID";
                SqlCommand nameCmd = new SqlCommand(nameQuery, con);
                nameCmd.Parameters.AddWithValue("@PortfolioID", PortfolioID);
                con.Open();
                object result = nameCmd.ExecuteScalar();
                if (result != null)
                {
                    lblPortfolioName.Text = result.ToString();
                }
                con.Close();

                // Get Assets in Portfolio
                string assetsQuery = @"
                    SELECT a.Symbol, a.AssetName, pa.Quantity, pa.PurchasedPrice, pa.PurchasedAt
                    FROM PortfolioAssets pa
                    JOIN Assets a ON pa.AssetID = a.AssetID
                    WHERE pa.PortfolioID = @PortfolioID
                    ORDER BY pa.PurchasedAt DESC";
                SqlDataAdapter sda = new SqlDataAdapter(assetsQuery, con);
                sda.SelectCommand.Parameters.AddWithValue("@PortfolioID", PortfolioID);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                gvPortfolioAssets.DataSource = dt;
                gvPortfolioAssets.DataBind();
            }
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

                    // Use ScriptManager to call the JS function to initialize the chart
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "initChart", $"initializeChart('{asset.Symbol}');", true);
                }
                else
                {
                    pnlAssetDetails.Visible = false;
                    lblSearchStatus.Text = "Asset not found or API error.";
                }
            }
            catch (Exception ex)
            {
                pnlAssetDetails.Visible = false;
                lblSearchStatus.Text = "An error occurred: " + ex.Message;
            }
        }

        private async Task<AssetData> GetAssetData(string symbol)
        {
            // 1. Check Cache
            using (var con = new SqlConnection(DbConstr))
            {
                string query = "SELECT AssetID, AssetName, Symbol, AssetType, Description, Sector, Geography, LastApiUpdate FROM Assets WHERE Symbol = @Symbol";
                var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Symbol", symbol);
                await con.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    var lastUpdate = reader["LastApiUpdate"] as DateTime?;
                    // Use cache if updated within the last 4 hours
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
                        // Still need to fetch the real-time price
                        string priceUrl = $"https://api.twelvedata.com/quote?symbol={symbol}&apikey={ApiKey}";
                        var priceResponse = await client.GetStringAsync(priceUrl);
                        var priceData = JsonConvert.DeserializeObject<QuoteResponse>(priceResponse);
                        asset.CurrentPrice = priceData?.Close ?? 0;
                        return asset;
                    }
                }
            }

            // 2. Not in cache or stale, call API
            string profileUrl = $"https://api.twelvedata.com/profile?symbol={symbol}&apikey={ApiKey}";
            string quoteUrl = $"https://api.twelvedata.com/quote?symbol={symbol}&apikey={ApiKey}";

            var profileTask = client.GetStringAsync(profileUrl);
            var quoteTask = client.GetStringAsync(quoteUrl);
            await Task.WhenAll(profileTask, quoteTask);

            var profile = JsonConvert.DeserializeObject<ProfileResponse>(await profileTask);
            var quote = JsonConvert.DeserializeObject<QuoteResponse>(await quoteTask);

            if (profile == null || quote == null) return null;

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

            // 3. Update Cache
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
            // Fetch historical data (from cache or API)
            List<HistoricalPrice> prices = GetHistoricalPrices(symbol).Result;

            var chartData = new ChartData();
            var pricesToChart = new List<HistoricalPrice>();

            // Filter data based on time period
            DateTime startDate = timePeriod == "1y" ? DateTime.Today.AddYears(-1) : DateTime.Today.AddDays(-30);
            pricesToChart = prices.Where(p => p.Date >= startDate).OrderBy(p => p.Date).ToList();

            chartData.Labels = pricesToChart.Select(p => p.Date.ToString("MMM dd")).ToList();
            chartData.HistoricalPrices = pricesToChart.Select(p => p.Price).ToList();

            if (includeForecast)
            {
                // Calculate forecast and add to the chart data
                var forecast = CalculateForecast(pricesToChart, 7);
                // Pad historical data with nulls so forecast starts where history ends
                int historyCount = chartData.HistoricalPrices.Count;
                chartData.ForecastPrices = new List<decimal?>();
                for (int i = 0; i < historyCount - 1; i++)
                {
                    chartData.ForecastPrices.Add(null);
                }
                // Add the last historical price as the starting point for the forecast line
                chartData.ForecastPrices.Add(chartData.HistoricalPrices.LastOrDefault());
                chartData.ForecastPrices.AddRange(forecast.Select(p => (decimal?)p.Price));

                // Add future dates to labels
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
                // Get AssetID and check if we need to fetch new data
                string assetQuery = "SELECT AssetID FROM Assets WHERE Symbol = @Symbol";
                var assetCmd = new SqlCommand(assetQuery, con);
                assetCmd.Parameters.AddWithValue("@Symbol", symbol);
                await con.OpenAsync();
                object assetIdResult = await assetCmd.ExecuteScalarAsync();

                if (assetIdResult == null) return prices; // Should not happen if search was done first
                assetId = (int)assetIdResult;

                // Check for most recent price date in cache
                string latestDateQuery = "SELECT MAX(PriceDate) FROM AssetPriceHistory WHERE AssetID = @AssetID";
                var dateCmd = new SqlCommand(latestDateQuery, con);
                dateCmd.Parameters.AddWithValue("@AssetID", assetId);
                object lastDateResult = await dateCmd.ExecuteScalarAsync();

                // Fetch from API if cache is empty or stale (more than a day old)
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
                            var mergeCmd = new SqlCommand(mergeQuery, con);
                            mergeCmd.Parameters.AddWithValue("@AssetID", assetId);
                            mergeCmd.Parameters.AddWithValue("@PriceDate", value.Datetime);
                            mergeCmd.Parameters.AddWithValue("@ClosePrice", value.Close);
                            await mergeCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Read all cached prices
                string selectQuery = "SELECT PriceDate, ClosePrice FROM AssetPriceHistory WHERE AssetID = @AssetID ORDER BY PriceDate";
                var selectCmd = new SqlCommand(selectQuery, con);
                selectCmd.Parameters.AddWithValue("@AssetID", assetId);
                using (var reader = await selectCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        prices.Add(new HistoricalPrice { Date = (DateTime)reader["PriceDate"], Price = (decimal)reader["ClosePrice"] });
                    }
                }
            }
            return prices;
        }

        // This is a simplified but realistic forecasting model
        private static List<HistoricalPrice> CalculateForecast(List<HistoricalPrice> historicalData, int daysToForecast)
        {
            var forecast = new List<HistoricalPrice>();
            if (historicalData.Count < 20) return forecast; // Need enough data for a meaningful trend

            // ✅ FIX 1: Replaced TakeLast(20) with a more compatible method.
            var recentPrices = historicalData.Skip(Math.Max(0, historicalData.Count - 20)).Select(p => p.Price).ToList();
            decimal alpha = 2.0m / (recentPrices.Count + 1); // Smoothing factor
            decimal ema = recentPrices[0];
            for (int i = 1; i < recentPrices.Count; i++)
            {
                ema = (recentPrices[i] * alpha) + (ema * (1 - alpha));
            }
            decimal lastPrice = historicalData.Last().Price;

            // ✅ FIX 2: Added parentheses to Average() to execute the method.
            decimal dailyTrend = (ema - recentPrices.Average()) / recentPrices.Count; // Simple trend direction

            DateTime lastDate = historicalData.Last().Date;
            for (int i = 1; i <= daysToForecast; i++)
            {
                lastPrice += dailyTrend; // Project next price based on trend
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

            try
            {
                // We need AssetId, CurrentPrice, and PortfolioID
                string symbol = hfCurrentSymbol.Value;
                var assetData = await GetAssetData(symbol); // Re-fetch to ensure we have ID and latest price

                if (assetData != null)
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
                    LoadPortfolioData(); // Refresh the grid view
                }
            }
            catch (Exception ex)
            {
                lblAddStatus.Text = "Error adding asset: " + ex.Message;
            }
        }

        protected void btnGoToDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect($"InvestmentDashboardPage.aspx?id={PortfolioID}");
        }

        #region Helper Classes
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

        public class ChartData
        {
            public List<string> Labels { get; set; }
            public List<decimal> HistoricalPrices { get; set; }
            public List<decimal?> ForecastPrices { get; set; } // Nullable for gaps in data series
        }

        public class HistoricalPrice
        {
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
        }

        // Classes for deserializing TwelveData API responses
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
