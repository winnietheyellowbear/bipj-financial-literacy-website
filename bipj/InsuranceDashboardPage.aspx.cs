using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (PlanID == 0)
            {
                // UPDATED REDIRECT: Go back to the new hub page
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
                string generalRec = await GetOrGenerateRecommendationAsync(
                    GetRecommendationFromCache,
                    GenerateGeneralRecommendationAsync,
                    CacheGeneralRecommendation
                );
                litGeneralRecommendation.Text = generalRec;

                string policyComp = await GetOrGenerateRecommendationAsync(
                    GetComparisonFromCache,
                    GeneratePolicyComparisonAsync,
                    CachePolicyComparison
                );
                litPolicyComparison.Text = policyComp;
            }
            catch (Exception ex)
            {
                ShowError("Failed to load recommendations. The AI service might be unavailable.");
            }
            finally
            {
                pnlLoading.Visible = false;
                pnlResults.Visible = true;
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

        private async Task<string> GenerateGeneralRecommendationAsync(string prompt)
        {
            string geminiPrompt = "Based on the following user profile, recommend suitable insurance types, desired coverage amounts for each, and a suggested budget allocation as a percentage of their income. Present it clearly.\n\n" + prompt;
            await Task.Delay(2000);
            return "<h3>Recommended Insurance Portfolio</h3><p><strong>Health Insurance:</strong> Given your active lifestyle and age, a comprehensive health plan with a coverage of at least $500,000 is recommended. This should cover hospitalization and critical illnesses. Budget: 8% of income.</p><p><strong>Term Life Insurance:</strong> To protect your dependents, a term life policy of $1,000,000 is advisable. This ensures their financial stability. Budget: 3% of income.</p><p><strong>Disability Insurance:</strong> To protect your income in case of an accident, long-term disability insurance covering 60% of your income is a wise choice. Budget: 2% of income.</p>";
        }

        private async Task<string> GeneratePolicyComparisonAsync(string prompt)
        {
            string geminiPrompt = "Based on the following user profile, recommend three real, existing insurance policies from well-known providers. Compare them on key features, premiums, and benefits to explain which is the best fit.\n\n" + prompt;
            await Task.Delay(3000);
            return "<h3>Top Policy Recommendations</h3><p><strong>Policy A (Global Health):</strong> Excellent coverage but higher premium. Best for frequent travelers.</p><p><strong>Policy B (SecureLife Term):</strong> Most affordable term life plan with great riders. Best for budget-conscious users.</p><p><strong>Policy C (IncomeShield Disability):</strong> Comprehensive disability coverage from a reputable provider. Best overall value.</p><p><strong>Recommendation:</strong> For your profile, <strong>Policy B</strong> offers the best balance of cost and protection for your family's needs.</p>";
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