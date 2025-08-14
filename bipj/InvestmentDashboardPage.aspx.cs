using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bipj
{
    public partial class InvestmentDashboardPage : System.Web.UI.Page
    {
        private static readonly string ApiKey = "cc404b6f2d2240f3b60772749ae6ea11";
        private static readonly string DbConstr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (int.TryParse(Request.QueryString["id"], out int portfolioId))
                {
                    hlBackToBuilder.NavigateUrl = $"~/InvestmentPortfolioPage.aspx?id={portfolioId}";
                    RegisterAsyncTask(new PageAsyncTask(() => LoadDashboardDataAsync(portfolioId)));
                }
                else
                {
                    Response.Redirect("InvestmentPage.aspx");
                }
            }
        }

        private async Task LoadDashboardDataAsync(int portfolioId)
        {
            // Step 1: Get all assets in the portfolio from our database.
            var portfolioAssets = new List<PortfolioAsset>();
            using (var con = new SqlConnection(DbConstr))
            {
                string query = @"SELECT p.PortfolioName, pa.Quantity, pa.PurchasedPrice, 
                                        a.AssetID, a.Symbol, a.AssetType, a.Sector, a.Geography
                                 FROM PortfolioAssets pa
                                 JOIN Assets a ON pa.AssetID = a.AssetID
                                 JOIN Portfolios p ON pa.PortfolioID = p.PortfolioID
                                 WHERE pa.PortfolioID = @PortfolioID";
                var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PortfolioID", portfolioId);
                await con.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (portfolioAssets.Count == 0) // Set portfolio name only once
                        {
                            lblPortfolioName.Text = reader["PortfolioName"].ToString();
                        }
                        portfolioAssets.Add(new PortfolioAsset
                        {
                            AssetId = (int)reader["AssetID"],
                            Symbol = reader["Symbol"].ToString(),
                            AssetType = reader["AssetType"]?.ToString() ?? "Unknown",
                            Sector = reader["Sector"]?.ToString() ?? "Unknown",
                            Geography = reader["Geography"]?.ToString() ?? "Unknown",
                            Quantity = (decimal)reader["Quantity"],
                            PurchasedPrice = (decimal)reader["PurchasedPrice"]
                        });
                    }
                }
            }

            if (!portfolioAssets.Any()) return; // Nothing to display

            // Step 2: Get the current price for each asset from the API.
            var priceTasks = portfolioAssets.Select(asset => GetCurrentPriceAsync(asset.Symbol)).ToList();
            var prices = await Task.WhenAll(priceTasks);
            for (int i = 0; i < portfolioAssets.Count; i++)
            {
                portfolioAssets[i].CurrentPrice = prices[i];
            }

            // Step 3: Perform all calculations.
            decimal principal = portfolioAssets.Sum(a => a.Quantity * a.PurchasedPrice);
            decimal currentValue = portfolioAssets.Sum(a => a.Quantity * a.CurrentPrice);
            decimal netProfit = currentValue - principal;
            decimal roi = (principal == 0) ? 0 : (netProfit / principal);

            // Step 4: Bind basic metrics to the UI.
            litPrincipal.Text = principal.ToString("C");
            litCurrentValue.Text = currentValue.ToString("C");
            string roiClass = roi >= 0 ? "positive" : "negative";
            litROI.Text = $"<span class='{roiClass}'>{roi:P2}</span>";
            string netProfitClass = netProfit >= 0 ? "positive" : "negative";
            litNetProfit.Text = $"<span class='{netProfitClass}'>{netProfit:C}</span>";

            // Step 5: Calculate and bind risk/volatility scores.
            var historicalData = await GetPortfolioHistoricalDataAsync(portfolioAssets, 90);
            var volatility = CalculateVolatility(historicalData);
            var riskScore = CalculateRiskScore(portfolioAssets, volatility);
            litVolatility.Text = GenerateStars(volatility);
            litRiskScore.Text = GenerateStars(riskScore);

            // Step 6: Calculate and render exposure pie charts.
            RenderExposureCharts(portfolioAssets, currentValue);

            // Step 7: Calculate and bind the correlation matrix.
            var correlationMatrix = await CalculateCorrelationMatrixAsync(portfolioAssets);
            rptCorrelationMatrix.DataSource = portfolioAssets; // For headers
            rptCorrelationMatrix.DataBind();
            rptCorrelationRows.DataSource = correlationMatrix;
            rptCorrelationRows.DataBind();
        }

        #region Calculations & Data Fetching

        private async Task<decimal> GetCurrentPriceAsync(string symbol)
        {
            // Simple API call to get the latest price for an asset.
            string url = $"https://api.twelvedata.com/price?symbol={symbol}&apikey={ApiKey}";
            var response = await client.GetStringAsync(url);
            var data = JObject.Parse(response);
            return data["price"]?.Value<decimal>() ?? 0;
        }

        private async Task<Dictionary<int, List<decimal>>> GetPortfolioHistoricalDataAsync(List<PortfolioAsset> assets, int days)
        {
            // Fetches historical data for all assets, using cache where possible.
            var allHistory = new Dictionary<int, List<decimal>>();
            foreach (var asset in assets)
            {
                allHistory[asset.AssetId] = await GetHistoricalPricesAsync(asset.AssetId, asset.Symbol, days);
            }
            return allHistory;
        }

        private async Task<List<decimal>> GetHistoricalPricesAsync(int assetId, string symbol, int days)
        {
            // This is our caching logic. Check the DB first before calling the API.
            var prices = new List<decimal>();
            using (var con = new SqlConnection(DbConstr))
            {
                await con.OpenAsync();
                string cacheQuery = "SELECT ClosePrice FROM AssetPriceHistory WHERE AssetID = @AssetID AND PriceDate >= @StartDate ORDER BY PriceDate DESC";
                var cmd = new SqlCommand(cacheQuery, con);
                cmd.Parameters.AddWithValue("@AssetID", assetId);
                cmd.Parameters.AddWithValue("@StartDate", DateTime.Today.AddDays(-days));
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync()) prices.Add((decimal)reader["ClosePrice"]);
                }

                if (prices.Count >= days) return prices; // Cache is sufficient

                // Cache is insufficient, call API and update cache.
                string url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval=1day&outputsize={days}&apikey={ApiKey}";
                var response = await client.GetStringAsync(url);
                var apiData = JsonConvert.DeserializeObject<TimeSeriesResponse>(response);

                if (apiData?.Values != null)
                {
                    prices.Clear();
                    var mergeQuery = @"
                        MERGE AssetPriceHistory AS target
                        USING (SELECT @AssetID AS AssetID, @PriceDate AS PriceDate) AS source ON (target.AssetID = source.AssetID AND target.PriceDate = source.PriceDate)
                        WHEN NOT MATCHED THEN INSERT (AssetID, PriceDate, ClosePrice) VALUES (@AssetID, @PriceDate, @ClosePrice);";

                    foreach (var value in apiData.Values.OrderByDescending(v => v.Datetime).Take(days))
                    {
                        prices.Add(value.Close);
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
            return prices;
        }

        private int CalculateVolatility(Dictionary<int, List<decimal>> historicalData)
        {
            // Calculate standard deviation of daily returns for the whole portfolio.
            if (historicalData == null || !historicalData.Any()) return 0;
            var portfolioReturns = new List<decimal>();
            int maxPoints = historicalData.Values.Min(v => v.Count);
            if (maxPoints < 2) return 0;

            for (int i = 0; i < maxPoints - 1; i++)
            {
                decimal yesterdayValue = historicalData.Sum(kvp => kvp.Value[i + 1]);
                decimal todayValue = historicalData.Sum(kvp => kvp.Value[i]);
                if (yesterdayValue != 0)
                {
                    portfolioReturns.Add((todayValue - yesterdayValue) / yesterdayValue);
                }
            }
            if (!portfolioReturns.Any()) return 0;
            decimal avgReturn = portfolioReturns.Average();
            double variance = (double)portfolioReturns.Sum(r => (r - avgReturn) * (r - avgReturn)) / portfolioReturns.Count;
            double stdDev = Math.Sqrt(variance);

            // Scale to a 0-5 star rating. These thresholds can be adjusted.
            if (stdDev > 0.05) return 5; // >5% daily swing = 5 stars
            if (stdDev > 0.03) return 4;
            if (stdDev > 0.02) return 3;
            if (stdDev > 0.01) return 2;
            if (stdDev > 0) return 1;
            return 0;
        }

        private int CalculateRiskScore(List<PortfolioAsset> assets, int volatilityScore)
        {
            // A weighted average based on asset type.
            if (!assets.Any()) return 0;
            double totalValue = (double)assets.Sum(a => a.Quantity * a.CurrentPrice);
            if (totalValue == 0) return 0;

            double weightedRisk = 0;
            foreach (var asset in assets)
            {
                int assetRisk = 3; // Default risk for stocks, ETFs, etc.
                switch (asset.AssetType?.ToLower())
                {
                    case "cryptocurrency": assetRisk = 5; break;
                    case "bond": case "government bond": assetRisk = 1; break;
                    case "real estate": case "reit": assetRisk = 2; break;
                }
                weightedRisk += assetRisk * ((double)(asset.Quantity * asset.CurrentPrice) / totalValue);
            }
            // Combine asset risk with market volatility.
            return (int)Math.Round((weightedRisk + volatilityScore) / 2.0, MidpointRounding.AwayFromZero);
        }

        private async Task<List<CorrelationRow>> CalculateCorrelationMatrixAsync(List<PortfolioAsset> assets)
        {
            var historicalReturns = new Dictionary<string, List<decimal>>();
            foreach (var asset in assets)
            {
                var prices = await GetHistoricalPricesAsync(asset.AssetId, asset.Symbol, 90);
                var returns = new List<decimal>();
                for (int i = 0; i < prices.Count - 1; i++)
                {
                    if (prices[i + 1] != 0) returns.Add((prices[i] - prices[i + 1]) / prices[i + 1]);
                }
                historicalReturns[asset.Symbol] = returns;
            }

            var matrix = new List<CorrelationRow>();
            foreach (var assetA in assets)
            {
                var row = new CorrelationRow { Symbol = assetA.Symbol };
                foreach (var assetB in assets)
                {
                    row.Correlations.Add(CalculatePearsonCorrelation(historicalReturns[assetA.Symbol], historicalReturns[assetB.Symbol]));
                }
                matrix.Add(row);
            }
            return matrix;
        }

        private double CalculatePearsonCorrelation(List<decimal> x, List<decimal> y)
        {
            int n = Math.Min(x.Count, y.Count);
            if (n < 2) return 0;
            double sumX = (double)x.Take(n).Sum();
            double sumY = (double)y.Take(n).Sum();
            double sumX2 = (double)x.Take(n).Sum(val => val * val);
            double sumY2 = (double)y.Take(n).Sum(val => val * val);
            double sumXY = 0;
            for (int i = 0; i < n; i++) sumXY += (double)(x[i] * y[i]);

            double numerator = n * sumXY - sumX * sumY;
            double denominator = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));
            return (denominator == 0) ? 0 : numerator / denominator;
        }

        #endregion

        #region UI Helpers

        private void RenderExposureCharts(List<PortfolioAsset> assets, decimal totalValue)
        {
            if (totalValue == 0) return;

            // Group by Sector
            var sectorExposure = assets.GroupBy(a => a.Sector)
                                       .Select(g => new { Name = g.Key, Value = g.Sum(a => a.Quantity * a.CurrentPrice) })
                                       .OrderByDescending(x => x.Value)
                                       .ToList();

            // Group by Geography
            var geoExposure = assets.GroupBy(a => a.Geography)
                                     .Select(g => new { Name = g.Key, Value = g.Sum(a => a.Quantity * a.CurrentPrice) })
                                     .OrderByDescending(x => x.Value)
                                     .ToList();

            var jsSerializer = new JavaScriptSerializer();
            string sectorJson = jsSerializer.Serialize(CreateChartJsData(sectorExposure.Select(s => s.Name), sectorExposure.Select(s => s.Value)));
            string geoJson = jsSerializer.Serialize(CreateChartJsData(geoExposure.Select(g => g.Name), geoExposure.Select(g => g.Value)));

            ScriptManager.RegisterStartupScript(this, GetType(), "renderCharts", $"renderPieCharts({sectorJson}, {geoJson});", true);
        }

        private object CreateChartJsData(IEnumerable<string> labels, IEnumerable<decimal> values)
        {
            return new
            {
                labels = labels,
                datasets = new[]
                {
                    new {
                        data = values,
                        backgroundColor = new[] { "#4e73df", "#1cc88a", "#36b9cc", "#f6c23e", "#e74a3b", "#858796", "#5a5c69" }
                    }
                }
            };
        }

        private string GenerateStars(int score)
        {
            score = Math.Max(0, Math.Min(5, score)); // Clamp score between 0 and 5
            return new string('★', score) + new string('☆', 5 - score);
        }

        // This public method is needed for the Repeater in the ASPX file to call.
        public string GetColorForCorrelation(double correlation)
        {
            // Strong positive correlation -> Green
            if (correlation > 0.7) return "#d4edda"; // Light green
            // Moderate positive
            if (correlation > 0.3) return "#e2e3e5"; // Light grey-green
            // Strong negative correlation -> Red
            if (correlation < -0.7) return "#f8d7da"; // Light red
            // Moderate negative
            if (correlation < -0.3) return "#f5c6cb"; // Lighter red
            // Weak or no correlation
            return "#ffffff"; // White
        }

        #endregion

        #region Helper Classes
        public class PortfolioAsset
        {
            public int AssetId { get; set; }
            public string Symbol { get; set; }
            public string AssetType { get; set; }
            public string Sector { get; set; }
            public string Geography { get; set; }
            public decimal Quantity { get; set; }
            public decimal PurchasedPrice { get; set; }
            public decimal CurrentPrice { get; set; }
        }
        public class CorrelationRow
        {
            public string Symbol { get; set; }
            public List<double> Correlations { get; set; } = new List<double>();
        }
        // Re-using TimeSeriesResponse from the other page for API deserialization
        public class TimeSeriesResponse
        {
            public string Status { get; set; }
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
