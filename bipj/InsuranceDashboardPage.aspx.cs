using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class InsuranceDashboardPage : System.Web.UI.Page
    {
        private int PlanID => Convert.ToInt32(Request.QueryString["PlanID"]);

        private static readonly string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key=";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (PlanID == 0)
            {
                Response.Redirect("InsurancePlanPage.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadPlanName();
                RegisterAsyncTask(new PageAsyncTask(LoadRecommendationsAsync));
            }
        }

        private async Task LoadRecommendationsAsync()
        {
            bool isNew = Request.QueryString["new"] == "true";
            pnlLoading.Visible = isNew;
            pnlResults.Visible = !isNew;

            try
            {
                // ✅ FIXED: Now calls the real API method for general recommendations
                string generalRec = await GetOrGenerateRecommendationAsync(
                    GetRecommendationFromCache,
                    (prompt) => GenerateGeminiResponseAsync("Based on the following user profile, recommend suitable insurance types, desired coverage amounts for each, and a suggested budget allocation as a percentage of their income. Present it clearly with headings and paragraphs.\n\n" + prompt),
                    CacheGeneralRecommendation
                );
                litGeneralRecommendation.Text = generalRec;

                // ✅ FIXED: Now calls the real API method for policy comparisons
                string policyComp = await GetOrGenerateRecommendationAsync(
                    GetComparisonFromCache,
                    (prompt) => GenerateGeminiResponseAsync("Based on the following user profile, recommend three real, existing insurance policies from well-known providers in Singapore. Compare them on key features, premiums, and benefits to explain which is the best fit. Format the output in clear sections.\n\n" + prompt),
                    CachePolicyComparison
                );
                litPolicyComparison.Text = policyComp;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load recommendations. The AI service might be unavailable or an error occurred: {ex.Message}");
            }
            finally
            {
                pnlLoading.Visible = false;
                pnlResults.Visible = true;
            }
        }

        private async Task<string> GenerateGeminiResponseAsync(string fullPrompt)
        {
            string apiKey = "INSERT_JH_API_KEY";
            if (string.IsNullOrEmpty(apiKey))
            {
                return "<p class='text-danger'>Error: The hardcoded Gemini API key is missing in InsuranceDashboardPage.aspx.cs.</p>";
            }

            string requestUrl = GeminiApiUrl + apiKey;

            using (var client = new HttpClient())
            {
                var payload = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = fullPrompt } } }
                    }
                };

                string jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Parse the JSON to extract the generated text
                    var jObject = JObject.Parse(responseBody);
                    var text = jObject["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                    // Simple conversion from markdown-like text to HTML
                    return text?.Replace("\n", "<br />").Replace("**", "<strong>").Replace("</strong>", "</strong>");
                }
                else
                {
                    // Return the error message from the API if available
                    string errorBody = await response.Content.ReadAsStringAsync();
                    return $"<p class='text-danger'>API Error: {response.ReasonPhrase}. Details: {errorBody}</p>";
                }
            }
        }

        private delegate Task<string> GenerateContentAsync(string prompt);
        private delegate void CacheContent(int planId, string content);

        private async Task<string> GetOrGenerateRecommendationAsync(Func<int, string> getFromCache, GenerateContentAsync generate, CacheContent cache)
        {
            string cachedContent = getFromCache(PlanID);
            if (!string.IsNullOrEmpty(cachedContent))
            {
                return cachedContent;
            }

            string prompt = BuildPromptFromForm();
            string generatedContent = await generate(prompt);
            cache(PlanID, generatedContent);
            return generatedContent;
        }

        private string BuildPromptFromForm()
        {
            string prompt = string.Empty;
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                // UPDATED TABLE NAME
                string query = "SELECT * FROM InsuranceFormResponse WHERE PlanID = @PlanID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", PlanID);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"Age: {reader["Age"]}");
                            sb.AppendLine($"Gender: {reader["Gender"]}");
                            sb.AppendLine($"Occupation: {reader["Occupation"]}");
                            sb.AppendLine($"Annual Income: ${reader["AnnualIncome"]}");
                            sb.AppendLine($"Marital Status: {reader["MaritalStatus"]}");
                            sb.AppendLine($"Has Dependents: {reader["HasDependents"]}, Number: {reader["NumberOfDependents"]}");
                            sb.AppendLine($"Health Status: {reader["HealthStatus"]}");
                            sb.AppendLine($"Lifestyle: {reader["LifestyleDescription"]}");
                            sb.AppendLine($"Financial Goals: {reader["FinancialGoals"]}");
                            sb.AppendLine($"Risk Tolerance: {reader["RiskTolerance"]}");
                            sb.AppendLine($"Existing Coverage: {reader["ExistingCoverage"]}");
                            prompt = sb.ToString();
                        }
                    }
                }
            }
            return prompt;
        }

        private string GetRecommendationFromCache(int planId)
        {
            // UPDATED TABLE NAME
            return GetContentFromCache("InsuranceRecommendation", "RecommendationText", planId);
        }

        private string GetComparisonFromCache(int planId)
        {
            // UPDATED TABLE NAME
            return GetContentFromCache("InsurancePolicyComparison", "ComparisonText", planId);
        }

        private string GetContentFromCache(string tableName, string columnName, int planId)
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand($"SELECT {columnName} FROM {tableName} WHERE PlanID = @PlanID", con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", planId);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        private void CacheGeneralRecommendation(int planId, string content)
        {
            // UPDATED TABLE NAME
            CacheContentInDb("InsuranceRecommendation", "RecommendationText", planId, content);
        }

        private void CachePolicyComparison(int planId, string content)
        {
            // UPDATED TABLE NAME
            CacheContentInDb("InsurancePolicyComparison", "ComparisonText", planId, content);
        }

        private void CacheContentInDb(string tableName, string columnName, int planId, string content)
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand($"INSERT INTO {tableName} (PlanID, {columnName}) VALUES (@PlanID, @Content)", con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", planId);
                    cmd.Parameters.AddWithValue("@Content", content);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadPlanName()
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                // UPDATED TABLE NAME
                using (SqlCommand cmd = new SqlCommand("SELECT PlanName FROM InsurancePlan WHERE PlanID = @PlanID", con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", PlanID);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null) litPlanName.Text = result.ToString();
                }
            }
        }

        protected void btnBackToPlans_Click(object sender, EventArgs e)
        {
            // UPDATED REDIRECT
            Response.Redirect("InsurancePlanPage.aspx");
        }

        protected void btnViewComparison_Click(object sender, EventArgs e)
        {
            Response.Redirect($"InsuranceComparisonPage.aspx?PlanID={PlanID}");
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litErrorMessage.Text = message;
        }
    }
}