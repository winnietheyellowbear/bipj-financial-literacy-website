using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bipj
{
    public partial class InsuranceComparisonPage : System.Web.UI.Page
    {
        private int PlanID => Convert.ToInt32(Request.QueryString["PlanID"]);
        private static readonly string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key=";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (PlanID == 0)
                {
                    Response.Redirect("InsurancePlanPage.aspx");
                    return;
                }
                RegisterAsyncTask(new PageAsyncTask(LoadAnalysisAsync));
            }
        }

        private async Task LoadAnalysisAsync()
        {
            try
            {
                await LoadPlanName();

                // ✅ CACHING LOGIC IMPLEMENTED
                // Check if a refresh is forced (e.g., after an edit)
                bool forceRefresh = Request.QueryString["edited"] == "true";

                string analysisJson = null;

                if (!forceRefresh)
                {
                    analysisJson = GetAnalysisFromCache(PlanID);
                }

                // If no cached data exists or a refresh is forced, call the API
                if (string.IsNullOrEmpty(analysisJson))
                {
                    string userProfile = BuildPromptFromForm();
                    string policyJson = GetComparisonFromCache(PlanID);

                    if (string.IsNullOrEmpty(userProfile) || string.IsNullOrEmpty(policyJson))
                    {
                        ShowError("Could not retrieve the necessary data to perform an analysis. Please go back to the dashboard and try again.");
                        return;
                    }

                    analysisJson = await GenerateFinalAnalysisAsync(userProfile, policyJson);

                    // Cache the new result
                    CacheAnalysis(PlanID, analysisJson);
                }

                ProcessAnalysisResponse(analysisJson);
            }
            catch (Exception ex)
            {
                ShowError($"An unexpected error occurred during analysis: {ex.Message}");
            }
            finally
            {
                pnlLoading.Visible = false;
                pnlResults.Visible = true;
            }
        }

        private void ProcessAnalysisResponse(string json)
        {
            if (string.IsNullOrEmpty(json) || json.StartsWith("<p"))
            {
                ShowError(json);
                return;
            }
            try
            {
                var analysisResult = JsonConvert.DeserializeObject<List<PolicyAnalysis>>(json);
                if (analysisResult == null || !analysisResult.Any())
                {
                    ShowError("The AI returned a response, but it could not be structured into a final analysis.");
                    return;
                }
                rptAnalysis.DataSource = analysisResult;
                rptAnalysis.DataBind();
            }
            catch (JsonException jex)
            {
                ShowError($"Error parsing the AI's final analysis: {jex.Message}.<br/><br/><strong>Raw AI Response:</strong><pre>{Server.HtmlEncode(json)}</pre>");
            }
        }

        private async Task<string> GenerateFinalAnalysisAsync(string userProfile, string policyJson)
        {
            string prompt = $@"
                As an expert financial advisor in Singapore, analyze the following data.
                You are given a user's profile and a list of pre-selected, suitable insurance policies grouped by category.
                Your task is to review the policies WITHIN EACH CATEGORY and select the single best policy for the user.
                Provide a detailed justification for each choice, explaining why it is superior to the other options for this specific user.

                Return your response ONLY as a raw JSON array of objects. Do not include any introductory text, backticks, or markdown formatting.
                Each object in the array must have the following exact keys: 'insuranceType' (string), 'bestPolicyName' (string), and 'justification' (string).

                ---
                USER PROFILE:
                {userProfile}
                ---
                PRE-SELECTED POLICIES:
                {policyJson}
                ---
            ";
            return await CallGeminiApi(prompt);
        }

        private async Task<string> CallGeminiApi(string prompt)
        {
            string apiKey = "AIzaSyAJRjb5r2BlhZQTurmQ9z7ltpCcS3S49xA"; // Replace with your key management method
            string requestUrl = GeminiApiUrl + apiKey;

            using (var client = new HttpClient())
            {
                var payload = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                string jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Using a simplified call here, but you could add the retry logic from the other page
                var response = await client.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(responseBody);
                    return jObject["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    return $"<p class='text-danger'>API Error: {response.ReasonPhrase}. Details: {errorBody}</p>";
                }
            }
        }

        private string GetAnalysisFromCache(int planId)
        {
            return GetContentFromCache("InsurancePolicyAnalysis", "AnalysisText", planId);
        }

        private void CacheAnalysis(int planId, string content)
        {
            // Only cache valid JSON content, not error messages
            if (!string.IsNullOrEmpty(content) && !content.StartsWith("<p"))
            {
                CacheContentInDb("InsurancePolicyAnalysis", "AnalysisText", planId, content);
            }
        }

        private void CacheContentInDb(string tableName, string columnName, int planId, string content)
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (var con = new SqlConnection(constr))
            {
                // Use an UPSERT-like logic: delete existing cache then insert new
                con.Open();
                using (var delCmd = new SqlCommand($"DELETE FROM {tableName} WHERE PlanID = @PlanID", con))
                {
                    delCmd.Parameters.AddWithValue("@PlanID", planId);
                    delCmd.ExecuteNonQuery();
                }
                using (var insCmd = new SqlCommand($"INSERT INTO {tableName} (PlanID, {columnName}) VALUES (@PlanID, @Content)", con))
                {
                    insCmd.Parameters.AddWithValue("@PlanID", planId);
                    insCmd.Parameters.AddWithValue("@Content", content);
                    insCmd.ExecuteNonQuery();
                }
            }
        }

        private string GetComparisonFromCache(int planId)
        {
            return GetContentFromCache("InsurancePolicyComparison", "ComparisonText", planId);
        }

        private string GetContentFromCache(string tableName, string columnName, int planId)
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (var con = new SqlConnection(constr))
            {
                using (var cmd = new SqlCommand($"SELECT {columnName} FROM {tableName} WHERE PlanID = @PlanID", con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", planId);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        private string BuildPromptFromForm()
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (var con = new SqlConnection(constr))
            {
                using (var cmd = new SqlCommand("SELECT * FROM InsuranceFormResponse WHERE PlanID = @PlanID", con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", PlanID);
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
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
                            return sb.ToString();
                        }
                    }
                }
            }
            return string.Empty;
        }

        private async Task LoadPlanName()
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (var con = new SqlConnection(constr))
            {
                using (var cmd = new SqlCommand("SELECT PlanName FROM InsurancePlan WHERE PlanID = @PlanID", con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", PlanID);
                    await con.OpenAsync();
                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null) litPlanName.Text = result.ToString();
                }
            }
        }

        protected void btnBackToDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect($"InsuranceDashboardPage.aspx?PlanID={PlanID}");
        }

        private void ShowError(string message)
        {
            pnlLoading.Visible = false;
            pnlResults.Visible = false;
            pnlError.Visible = true;
            litErrorMessage.Text = message;
        }
    }
}
