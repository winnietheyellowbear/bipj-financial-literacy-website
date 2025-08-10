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
                // This is now a placeholder, as the main logic is handled by the new structured method.
                // We keep it to show the policy comparison, which is still text-based.
                string generalRecJson = await GetOrGenerateRecommendationAsync(
                   GetRecommendationFromCache,
                   (prompt) => GenerateGeminiJsonResponseAsync(prompt),
                   CacheGeneralRecommendation
               );

                // Process the structured JSON response
                ProcessStructuredRecommendations(generalRecJson);


                // This part remains for the second, text-based API call for policy comparisons.
                string policyComp = await GetOrGenerateRecommendationAsync(
                    GetComparisonFromCache,
                    (prompt) => GenerateGeminiTextResponseAsync("Based on the following user profile, recommend three real, existing insurance policies from well-known providers in Singapore. Compare them on key features, premiums, and benefits to explain which is the best fit. Format the output in clear sections using markdown.\n\n" + prompt),
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

        private void ProcessStructuredRecommendations(string json)
        {
            if (string.IsNullOrEmpty(json) || json.StartsWith("<p"))
            {
                ShowError("Could not retrieve or parse structured recommendations from the AI service.");
                return;
            }

            try
            {
                var recommendations = JsonConvert.DeserializeObject<List<InsuranceRecommendation>>(json);

                // Bind the list of recommendation objects to the repeater for the cards
                rptRecommendations.DataSource = recommendations;
                rptRecommendations.DataBind();

                // Prepare data for the pie chart and budget display
                var chartData = recommendations.Select(r => new { label = r.Type, value = r.BudgetPercentage }).ToList();
                string chartDataJson = JsonConvert.SerializeObject(chartData);

                // Build the simple number display for the budget
                var budgetStringBuilder = new StringBuilder();
                budgetStringBuilder.Append("<p class='text-muted'><strong>Budget Breakdown:</strong> ");
                budgetStringBuilder.Append(string.Join(" | ", recommendations.Select(r => $"{r.Type}: {r.BudgetPercentage}%")));
                budgetStringBuilder.Append("</p>");
                litBudgetNumbers.Text = budgetStringBuilder.ToString();

                // Call the JavaScript function to draw the chart
                ScriptManager.RegisterStartupScript(this, this.GetType(), "drawChartScript", $"drawBudgetChart({chartDataJson});", true);
            }
            catch (JsonException jex)
            {
                ShowError($"Error parsing the AI's JSON response: {jex.Message}. Raw response: <pre>{json}</pre>");
            }
        }

        private async Task<string> GenerateGeminiJsonResponseAsync(string userProfile)
        {
            string jsonPrompt = $@"
                Based on the following user profile, provide insurance recommendations.
                Return your response ONLY as a raw JSON array of objects. Do not include any introductory text, backticks, or markdown formatting.
                Each object in the array must have the following exact keys: 'type' (string), 'coverage' (string), 'explanation' (string), and 'budgetPercentage' (integer).
                Example: [{{""type"": ""Life Insurance"", ""coverage"": ""$500,000"", ""explanation"": ""This is crucial..."", ""budgetPercentage"": 10}}]

                User Profile:
                {userProfile}
            ";
            return await CallGeminiApi(jsonPrompt, isJsonOutput: true);
        }

        private async Task<string> GenerateGeminiTextResponseAsync(string fullPrompt)
        {
            return await CallGeminiApi(fullPrompt, isJsonOutput: false);
        }

        // A single, reusable method to call the Gemini API
        private async Task<string> CallGeminiApi(string prompt, bool isJsonOutput)
        {
            string apiKey = "AIzaSyAJRjb5r2BlhZQTurmQ9z7ltpCcS3S49xA";
            if (string.IsNullOrEmpty(apiKey))
            {
                return "<p class='text-danger'>Error: The hardcoded Gemini API key is missing.</p>";
            }

            string requestUrl = GeminiApiUrl + apiKey;

            using (var client = new HttpClient())
            {
                var payload = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                string jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(responseBody);
                    var text = jObject["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                    // If we expect plain text, format it for HTML display. Otherwise, return the raw JSON string.
                    return isJsonOutput ? text : text?.Replace("\n", "<br />").Replace("**", "<strong>").Replace("</strong>", "</strong>");
                }
                else
                {
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