using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
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
            DateTime lastUpdated;
            using (var con = new SqlConnection(DbConstr))
            {
                var cmd = new SqlCommand("SELECT LastUpdatedAt, PortfolioName FROM Portfolios WHERE PortfolioID = @PortfolioID", con);
                cmd.Parameters.AddWithValue("@PortfolioID", portfolioId);
                await con.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        return;
                    }
                    lastUpdated = (DateTime)reader["LastUpdatedAt"];
                    lblPortfolioName.Text = reader["PortfolioName"].ToString();
                }
            }

            string cacheKey = $"dashboard_{portfolioId}_{lastUpdated:yyyyMMddHHmmss}";
            var cachedData = HttpRuntime.Cache[cacheKey] as DashboardData;

            if (cachedData != null)
            {
                BindDataToUI(cachedData);
                return;
            }

            var portfolioAssets = await GetPortfolioAssetsAsync(portfolioId);
            if (!portfolioAssets.Any()) return;

            await GetAndUpdateCurrentPricesAsync(portfolioAssets);

            var historicalData = await GetPortfolioHistoricalDataAsync(portfolioAssets, 90);

            var correlationMatrix = CalculateCorrelationMatrix(portfolioAssets, historicalData);

            var dashboardData = new DashboardData(portfolioAssets, historicalData, correlationMatrix);

            HttpRuntime.Cache.Insert(cacheKey, dashboardData, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(12));

            BindDataToUI(dashboardData);
        }

        private void BindDataToUI(DashboardData data)
        {
            litPrincipal.Text = data.Principal.ToString("C");
            litCurrentValue.Text = data.CurrentValue.ToString("C");
            string roiClass = data.ROI >= 0 ? "positive" : "negative";
            litROI.Text = $"<span class='{roiClass}'>{data.ROI:P2}</span>";
            string netProfitClass = data.NetProfit >= 0 ? "positive" : "negative";
            litNetProfit.Text = $"<span class='{netProfitClass}'>{data.NetProfit:C}</span>";

            litVolatility.Text = GenerateStars(data.VolatilityScore);
            litRiskScore.Text = GenerateStars(data.RiskScore);

            rptCorrelationMatrix.DataSource = data.Assets;
            rptCorrelationMatrix.DataBind();
            rptCorrelationRows.DataSource = data.CorrelationMatrix;
            rptCorrelationRows.DataBind();

            RenderExposureCharts(data.Assets, data.CurrentValue);
        }

        private async Task<List<PortfolioAsset>> GetPortfolioAssetsAsync(int portfolioId)
        {
            var portfolioAssets = new List<PortfolioAsset>();
            using (var con = new SqlConnection(DbConstr))
            {
                string query = @"SELECT pa.Quantity, pa.PurchasedPrice, a.AssetID, a.Symbol, a.AssetType, a.Sector, a.Geography
                                 FROM PortfolioAssets pa JOIN Assets a ON pa.AssetID = a.AssetID
                                 WHERE pa.PortfolioID = @PortfolioID";
                var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PortfolioID", portfolioId);
                await con.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
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
            return portfolioAssets;
        }

        #region Calculations & Data Fetching

        private async Task GetAndUpdateCurrentPricesAsync(List<PortfolioAsset> assets)
        {
            var assetsToUpdate = new List<PortfolioAsset>();
            using (var con = new SqlConnection(DbConstr))
            {
                await con.OpenAsync();
                foreach (var asset in assets)
                {
                    string query = "SELECT LastPrice, PriceLastUpdate FROM Assets WHERE AssetID = @AssetID";
                    var cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@AssetID", asset.AssetId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var priceLastUpdate = reader["PriceLastUpdate"] as DateTime?;
                            if (priceLastUpdate.HasValue && priceLastUpdate.Value.Date >= DateTime.UtcNow.Date)
                            {
                                asset.CurrentPrice = (decimal)reader["LastPrice"];
                            }
                            else
                            {
                                assetsToUpdate.Add(asset);
                            }
                        }
                        else
                        {
                            assetsToUpdate.Add(asset);
                        }
                    }
                }
            }

            if (assetsToUpdate.Any())
            {
                string symbols = string.Join(",", assetsToUpdate.Select(a => a.Symbol));
                string url = $"https://api.twelvedata.com/price?symbol={symbols}&apikey={ApiKey}";
                var response = await client.GetStringAsync(url);
                JToken data = JToken.Parse(response);
                if (data.Type == JTokenType.Object)
                {
                    var prices = new Dictionary<string, decimal>();
                    if (data["price"] != null)
                    {
                        prices[assetsToUpdate.First().Symbol] = data["price"].Value<decimal>();
                    }
                    else
                    {
                        prices = data.ToObject<Dictionary<string, JObject>>().ToDictionary(kvp => kvp.Key, kvp => kvp.Value["price"].Value<decimal>());
                    }
                    using (var con = new SqlConnection(DbConstr))
                    {
                        await con.OpenAsync();
                        foreach (var asset in assetsToUpdate)
                        {
                            if (prices.TryGetValue(asset.Symbol, out decimal newPrice))
                            {
                                asset.CurrentPrice = newPrice;
                                string updateQuery = "UPDATE Assets SET LastPrice = @LastPrice, PriceLastUpdate = @PriceLastUpdate WHERE AssetID = @AssetID";
                                using (var updateCmd = new SqlCommand(updateQuery, con))
                                {
                                    updateCmd.Parameters.AddWithValue("@LastPrice", newPrice);
                                    updateCmd.Parameters.AddWithValue("@PriceLastUpdate", DateTime.UtcNow);
                                    updateCmd.Parameters.AddWithValue("@AssetID", asset.AssetId);
                                    await updateCmd.ExecuteNonQueryAsync();
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task<Dictionary<int, List<decimal>>> GetPortfolioHistoricalDataAsync(List<PortfolioAsset> assets, int days)
        {
            var allHistory = new Dictionary<int, List<decimal>>();
            foreach (var asset in assets)
            {
                var prices = await GetHistoricalPricesAsync(asset.AssetId, asset.Symbol, days);
                allHistory[asset.AssetId] = prices.Select(p => p.Price).ToList();
            }
            return allHistory;
        }

        private async Task<List<HistoricalPrice>> GetHistoricalPricesAsync(int assetId, string symbol, int days)
        {
            using (var con = new SqlConnection(DbConstr))
            {
                await con.OpenAsync();

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

                DateTime latestDateInCache = prices.Any() ? prices.Max(p => p.Date) : DateTime.MinValue;
                if (prices.Count >= days && latestDateInCache.Date >= DateTime.UtcNow.Date.AddDays(-1))
                {
                    return prices;
                }

                string url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval=1day&outputsize=400&apikey={ApiKey}";
                var response = await client.GetStringAsync(url);
                var apiData = JsonConvert.DeserializeObject<TimeSeriesResponse>(response);
                if (apiData?.Values != null)
                {
                    var values = apiData.Values.Select(item => new HistoricalPrice
                    {
                        Date = item.Datetime,
                        Price = item.Close
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
                    return values.OrderByDescending(p => p.Date).Take(days).ToList();
                }
            }
            return new List<HistoricalPrice>();
        }

        private List<CorrelationRow> CalculateCorrelationMatrix(List<PortfolioAsset> assets, Dictionary<int, List<decimal>> historicalData)
        {
            var historicalReturns = new Dictionary<string, List<decimal>>();
            foreach (var asset in assets)
            {
                if (historicalData.ContainsKey(asset.AssetId))
                {
                    var prices = historicalData[asset.AssetId];
                    var returns = new List<decimal>();
                    for (int i = 0; i < prices.Count - 1; i++)
                    {
                        if (prices[i + 1] != 0) returns.Add((prices[i] - prices[i + 1]) / prices[i + 1]);
                    }
                    historicalReturns[asset.Symbol] = returns;
                }
            }
            var matrix = new List<CorrelationRow>();
            foreach (var assetA in assets)
            {
                var row = new CorrelationRow { Symbol = assetA.Symbol };
                foreach (var assetB in assets)
                {
                    if (historicalReturns.ContainsKey(assetA.Symbol) && historicalReturns.ContainsKey(assetB.Symbol))
                    {
                        row.Correlations.Add(CalculatePearsonCorrelation(historicalReturns[assetA.Symbol], historicalReturns[assetB.Symbol]));
                    }
                    else
                    {
                        row.Correlations.Add(0);
                    }
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
            var sectorExposure = assets.GroupBy(a => a.Sector)
                                       .Select(g => new { Name = g.Key, Value = g.Sum(a => a.Quantity * a.CurrentPrice) })
                                       .OrderByDescending(x => x.Value)
                                       .ToList();
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
            score = Math.Max(0, Math.Min(5, score));
            return new string('★', score) + new string('☆', 5 - score);
        }

        // ✅ REFACTORED: This method now calculates a smooth gradient from red to green.
        public string GetColorForCorrelation(double correlation)
        {
            // Clamp the value between -1 and 1 to handle any edge cases.
            correlation = Math.Max(-1.0, Math.Min(1.0, correlation));

            int r, g, b;

            if (correlation >= 0)
            {
                // Positive correlation: Interpolate from White (255, 255, 255) to Green (34, 139, 34)
                // We use a slightly darker green for better visibility.
                r = (int)(255 - (255 - 34) * correlation);
                g = (int)(255 - (255 - 139) * correlation);
                b = (int)(255 - (255 - 34) * correlation);
            }
            else
            {
                // Negative correlation: Interpolate from Red (220, 20, 60) to White (255, 255, 255)
                // We use a crimson red. The interpolation factor is the absolute value.
                double factor = Math.Abs(correlation);
                r = (int)(255 - (255 - 220) * factor);
                g = (int)(255 - (255 - 20) * factor);
                b = (int)(255 - (255 - 60) * factor);
            }

            return $"#{r:X2}{g:X2}{b:X2}";
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

        [Serializable]
        private class HistoricalPrice
        {
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
        }

        [Serializable]
        public class DashboardData
        {
            public decimal Principal { get; }
            public decimal CurrentValue { get; }
            public decimal NetProfit { get; }
            public decimal ROI { get; }
            public int VolatilityScore { get; }
            public int RiskScore { get; }
            public List<PortfolioAsset> Assets { get; }
            public List<CorrelationRow> CorrelationMatrix { get; }

            public DashboardData(List<PortfolioAsset> assets, Dictionary<int, List<decimal>> historicalData, List<CorrelationRow> correlationMatrix)
            {
                Assets = assets;
                CorrelationMatrix = correlationMatrix;
                Principal = assets.Sum(a => a.Quantity * a.PurchasedPrice);
                CurrentValue = assets.Sum(a => a.Quantity * a.CurrentPrice);
                NetProfit = CurrentValue - Principal;
                ROI = (Principal == 0) ? 0 : (NetProfit / Principal);
                VolatilityScore = CalculateVolatility(historicalData);
                RiskScore = CalculateRiskScore(assets, VolatilityScore);
            }

            private int CalculateVolatility(Dictionary<int, List<decimal>> historicalData)
            {
                if (historicalData == null || !historicalData.Any()) return 0;
                var portfolioReturns = new List<decimal>();
                int maxPoints = historicalData.Values.Min(v => v.Count);
                if (maxPoints < 2) return 0;
                for (int i = 0; i < maxPoints - 1; i++)
                {
                    decimal yesterdayValue = historicalData.Sum(kvp => kvp.Value[i + 1]);
                    decimal todayValue = historicalData.Sum(kvp => kvp.Value[i]);
                    if (yesterdayValue != 0) portfolioReturns.Add((todayValue - yesterdayValue) / yesterdayValue);
                }
                if (!portfolioReturns.Any()) return 0;
                decimal avgReturn = portfolioReturns.Average();
                double variance = (double)portfolioReturns.Sum(r => (r - avgReturn) * (r - avgReturn)) / portfolioReturns.Count;
                double stdDev = Math.Sqrt(variance);
                if (stdDev > 0.05) return 5;
                if (stdDev > 0.03) return 4;
                if (stdDev > 0.02) return 3;
                if (stdDev > 0.01) return 2;
                if (stdDev > 0) return 1;
                return 0;
            }

            private int CalculateRiskScore(List<PortfolioAsset> assets, int volatilityScore)
            {
                if (!assets.Any()) return 0;
                double totalValue = (double)assets.Sum(a => a.Quantity * a.CurrentPrice);
                if (totalValue == 0) return 0;
                double weightedRisk = 0;
                foreach (var asset in assets)
                {
                    int assetRisk = 3;
                    switch (asset.AssetType?.ToLower())
                    {
                        case "cryptocurrency": assetRisk = 5; break;
                        case "bond": case "government bond": assetRisk = 1; break;
                        case "real estate": case "reit": assetRisk = 2; break;
                    }
                    weightedRisk += assetRisk * ((double)(asset.Quantity * asset.CurrentPrice) / totalValue);
                }
                return (int)Math.Round((weightedRisk + volatilityScore) / 2.0, MidpointRounding.AwayFromZero);
            }
        }
        #endregion
    }
}
