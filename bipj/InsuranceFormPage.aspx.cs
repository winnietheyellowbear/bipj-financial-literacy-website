using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;

namespace bipj
{
    public partial class InsuranceFormPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void chkHasDependents_CheckedChanged(object sender, EventArgs e)
        {
            pnlDependents.Visible = chkHasDependents.Checked;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                int userId = GetCurrentUserID();
                int newPlanId = CreateInsurancePlan(userId);

                if (newPlanId > 0)
                {
                    SaveFormResponses(newPlanId);
                    // UPDATED REDIRECT: Go to the new results page
                    Response.Redirect($"InsuranceDashboardPage.aspx?PlanID={newPlanId}&new=true");
                }
                else
                {
                    ShowError("Could not create the insurance plan. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError("An unexpected error occurred. Please contact support.");
            }
        }

        private int GetCurrentUserID()
        {
            if (Session["UserID"] != null)
            {
                return Convert.ToInt32(Session["UserID"]);
            }
            return 1; // Fallback for testing
        }

        private int CreateInsurancePlan(int userId)
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                // UPDATED TABLE NAME
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
                // UPDATED TABLE NAME
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
}