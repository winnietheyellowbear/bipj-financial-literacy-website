using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace bipj
{
    public partial class InsuranceFormPage : System.Web.UI.Page
    {
        // Property to easily get the PlanID from the query string. Returns 0 if not in edit mode.
        private int PlanID => Convert.ToInt32(Request.QueryString["PlanID"]);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (PlanID > 0)
                {
                    // If a PlanID exists, we are in "Edit Mode"
                    LoadPlanData();
                }
            }
        }

        private void LoadPlanData()
        {
            litFormTitle.Text = "Edit Your Insurance Profile";
            btnSubmit.Text = "Update My Recommendations";

            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT p.PlanName, r.* FROM InsurancePlan p INNER JOIN InsuranceFormResponse r ON p.PlanID = r.PlanID WHERE p.PlanID = @PlanID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", PlanID);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Pre-fill form fields with data from the database
                            txtPlanName.Text = reader["PlanName"].ToString();
                            txtAge.Text = reader["Age"].ToString();
                            ddlGender.SelectedValue = reader["Gender"].ToString();
                            txtOccupation.Text = reader["Occupation"].ToString();
                            txtAnnualIncome.Text = reader["AnnualIncome"].ToString();
                            chkHasDependents.Checked = Convert.ToBoolean(reader["HasDependents"]);
                            pnlDependents.Visible = chkHasDependents.Checked;
                            txtNumberOfDependents.Text = reader["NumberOfDependents"].ToString();
                            ddlMaritalStatus.SelectedValue = reader["MaritalStatus"].ToString();
                            txtLifestyle.Text = reader["LifestyleDescription"].ToString();
                            txtFinancialGoals.Text = reader["FinancialGoals"].ToString();
                            ddlRiskTolerance.SelectedValue = reader["RiskTolerance"].ToString();
                            txtExistingCoverage.Text = reader["ExistingCoverage"].ToString();
                            txtHealthStatus.Text = reader["HealthStatus"].ToString();

                            // Create the "snapshot" of the original data and store it
                            hfOriginalData.Value = CreateDataSnapshot();
                        }
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlanID > 0)
                {
                    // EDIT MODE
                    HandleUpdate();
                }
                else
                {
                    // CREATE MODE
                    HandleCreate();
                }
            }
            catch (Exception ex)
            {
                ShowError("An unexpected error occurred. Please contact support.");
            }
        }

        private void HandleCreate()
        {
            int userId = GetCurrentUserID();
            int newPlanId = CreateInsurancePlan(userId);

            if (newPlanId > 0)
            {
                SaveFormResponses(newPlanId);
                Response.Redirect($"InsuranceDashboardPage.aspx?PlanID={newPlanId}&new=true");
            }
            else
            {
                ShowError("Could not create the insurance plan. Please try again.");
            }
        }

        private void HandleUpdate()
        {
            string originalData = hfOriginalData.Value;
            string currentData = CreateDataSnapshot();

            if (originalData == currentData)
            {
                // No changes were made, just redirect back to the dashboard
                Response.Redirect($"InsuranceDashboardPage.aspx?PlanID={PlanID}");
            }
            else
            {
                // Data has changed, so update the database and clear the cache
                UpdateInsurancePlan();
                ClearCache(PlanID);
                // Redirect with the 'edited=true' flag to force a new API call
                Response.Redirect($"InsuranceDashboardPage.aspx?PlanID={PlanID}&edited=true");
            }
        }

        private string CreateDataSnapshot()
        {
            // Concatenate all form values into a single string for comparison
            return string.Join("|",
                txtPlanName.Text.Trim(),
                txtAge.Text.Trim(),
                ddlGender.SelectedValue,
                txtOccupation.Text.Trim(),
                txtAnnualIncome.Text.Trim(),
                chkHasDependents.Checked.ToString(),
                chkHasDependents.Checked ? txtNumberOfDependents.Text.Trim() : "0",
                ddlMaritalStatus.SelectedValue,
                txtLifestyle.Text.Trim(),
                txtFinancialGoals.Text.Trim(),
                ddlRiskTolerance.SelectedValue,
                txtExistingCoverage.Text.Trim(),
                txtHealthStatus.Text.Trim()
            );
        }

        private void UpdateInsurancePlan()
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                // Update the plan name in the parent table
                string planQuery = "UPDATE InsurancePlan SET PlanName = @PlanName, LastUpdatedAt = GETDATE() WHERE PlanID = @PlanID";
                using (SqlCommand cmd = new SqlCommand(planQuery, con))
                {
                    cmd.Parameters.AddWithValue("@PlanName", txtPlanName.Text.Trim());
                    cmd.Parameters.AddWithValue("@PlanID", PlanID);
                    cmd.ExecuteNonQuery();
                }

                // Update the form responses in the child table
                string responseQuery = @"UPDATE InsuranceFormResponse SET 
                    Age = @Age, Gender = @Gender, Occupation = @Occupation, AnnualIncome = @AnnualIncome, 
                    HasDependents = @HasDependents, NumberOfDependents = @NumberOfDependents, MaritalStatus = @MaritalStatus, 
                    LifestyleDescription = @Lifestyle, FinancialGoals = @FinancialGoals, RiskTolerance = @RiskTolerance, 
                    ExistingCoverage = @ExistingCoverage, HealthStatus = @HealthStatus 
                    WHERE PlanID = @PlanID";
                using (SqlCommand cmd = new SqlCommand(responseQuery, con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", PlanID);
                    cmd.Parameters.AddWithValue("@Age", Convert.ToInt32(txtAge.Text.Trim()));
                    cmd.Parameters.AddWithValue("@Gender", ddlGender.SelectedValue);
                    cmd.Parameters.AddWithValue("@Occupation", txtOccupation.Text.Trim());
                    cmd.Parameters.AddWithValue("@AnnualIncome", Convert.ToDecimal(txtAnnualIncome.Text.Trim()));
                    cmd.Parameters.AddWithValue("@HasDependents", chkHasDependents.Checked);
                    cmd.Parameters.AddWithValue("@NumberOfDependents", chkHasDependents.Checked ? Convert.ToInt32(txtNumberOfDependents.Text.Trim()) : 0);
                    cmd.Parameters.AddWithValue("@MaritalStatus", ddlMaritalStatus.SelectedValue);
                    cmd.Parameters.AddWithValue("@Lifestyle", txtLifestyle.Text.Trim());
                    cmd.Parameters.AddWithValue("@FinancialGoals", txtFinancialGoals.Text.Trim());
                    cmd.Parameters.AddWithValue("@RiskTolerance", ddlRiskTolerance.SelectedValue);
                    cmd.Parameters.AddWithValue("@ExistingCoverage", txtExistingCoverage.Text.Trim());
                    cmd.Parameters.AddWithValue("@HealthStatus", txtHealthStatus.Text.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ClearCache(int planId)
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                // Delete from both cache tables
                new SqlCommand("DELETE FROM InsuranceRecommendation WHERE PlanID = @PlanID", con)
                    .ExecuteNonQuery(new SqlParameter("@PlanID", planId));
                new SqlCommand("DELETE FROM InsurancePolicyComparison WHERE PlanID = @PlanID", con)
                    .ExecuteNonQuery(new SqlParameter("@PlanID", planId));
            }
        }

        // --- Methods below are for CREATE mode and are mostly unchanged ---

        protected void chkHasDependents_CheckedChanged(object sender, EventArgs e)
        {
            pnlDependents.Visible = chkHasDependents.Checked;
        }

        private int GetCurrentUserID()
        {
            if (Session["UserID"] != null)
            {
                return Convert.ToInt32(Session["UserID"]);
            }
            return 1;
        }

        private int CreateInsurancePlan(int userId)
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "INSERT INTO InsurancePlan (UserID, PlanName, CreatedAt, LastUpdatedAt) OUTPUT INSERTED.PlanID VALUES (@UserID, @PlanName, GETDATE(), GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@PlanName", txtPlanName.Text.Trim());
                    con.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private void SaveFormResponses(int planId)
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = @"INSERT INTO InsuranceFormResponse
                               (PlanID, Age, Gender, Occupation, AnnualIncome, HasDependents, NumberOfDependents, MaritalStatus, LifestyleDescription, FinancialGoals, RiskTolerance, ExistingCoverage, HealthStatus)
                               VALUES (@PlanID, @Age, @Gender, @Occupation, @AnnualIncome, @HasDependents, @NumberOfDependents, @MaritalStatus, @Lifestyle, @FinancialGoals, @RiskTolerance, @ExistingCoverage, @HealthStatus)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", planId);
                    cmd.Parameters.AddWithValue("@Age", Convert.ToInt32(txtAge.Text.Trim()));
                    cmd.Parameters.AddWithValue("@Gender", ddlGender.SelectedValue);
                    cmd.Parameters.AddWithValue("@Occupation", txtOccupation.Text.Trim());
                    cmd.Parameters.AddWithValue("@AnnualIncome", Convert.ToDecimal(txtAnnualIncome.Text.Trim()));
                    cmd.Parameters.AddWithValue("@HasDependents", chkHasDependents.Checked);
                    cmd.Parameters.AddWithValue("@NumberOfDependents", chkHasDependents.Checked ? Convert.ToInt32(txtNumberOfDependents.Text.Trim()) : 0);
                    cmd.Parameters.AddWithValue("@MaritalStatus", ddlMaritalStatus.SelectedValue);
                    cmd.Parameters.AddWithValue("@Lifestyle", txtLifestyle.Text.Trim());
                    cmd.Parameters.AddWithValue("@FinancialGoals", txtFinancialGoals.Text.Trim());
                    cmd.Parameters.AddWithValue("@RiskTolerance", ddlRiskTolerance.SelectedValue);
                    cmd.Parameters.AddWithValue("@ExistingCoverage", txtExistingCoverage.Text.Trim());
                    cmd.Parameters.AddWithValue("@HealthStatus", txtHealthStatus.Text.Trim());
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ShowError(string message)
        {
            litError.Text = $"<div class='mt-4 p-4 bg-red-100 border border-red-400 text-red-700 rounded'>{message}</div>";
            litError.Visible = true;
        }
    }
    // Helper extension method to simplify adding parameters
    public static class SqlCommandExtensions
    {
        public static void ExecuteNonQuery(this SqlCommand cmd, SqlParameter parameter)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.Add(parameter);
            cmd.ExecuteNonQuery();
        }
    }
}
