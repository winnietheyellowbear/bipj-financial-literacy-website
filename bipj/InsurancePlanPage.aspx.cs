using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class InsurancePlanPage : System.Web.UI.Page
    {
        private int GetCurrentUserID()
        {
            if (Session["UserID"] != null)
            {
                return Convert.ToInt32(Session["UserID"]);
            }
            // For testing purposes, returning a default user.
            return 1;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session["UserID"] = 1; // Demo purposes
                BindInsurancePlans();
            }
        }

        private void BindInsurancePlans()
        {
            int userId = GetCurrentUserID();
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                // UPDATED TABLE NAME
                using (SqlCommand cmd = new SqlCommand("SELECT PlanID, PlanName, CreatedAt FROM InsurancePlan WHERE UserID = @UserID ORDER BY CreatedAt DESC", con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        rptInsurancePlans.DataSource = dt;
                        rptInsurancePlans.DataBind();

                        pnlNoPlans.Visible = dt.Rows.Count == 0;
                    }
                }
            }
        }

        protected void btnCreateNewPlan_Click(object sender, EventArgs e)
        {
            // Redirect to the form page remains the same.
            Response.Redirect("InsuranceFormPage.aspx");
        }

        protected void rptInsurancePlans_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int planId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "View")
            {
                // UPDATED REDIRECT: Go to the details/dashboard page
                Response.Redirect($"InsuranceDashboardPage.aspx?PlanID={planId}");
            }
            else if (e.CommandName == "Edit")
            {
                Response.Redirect($"InsuranceFormPage.aspx?PlanID={planId}");
            }
            else if (e.CommandName == "Delete")
            {
                DeletePlan(planId);
                BindInsurancePlans();
            }
        }

        private void DeletePlan(int planId)
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                // Since we used ON DELETE CASCADE, we only need to delete from the parent table.
                // The database will handle deleting related records in child tables.
                using (SqlCommand cmd = new SqlCommand("DELETE FROM InsurancePlan WHERE PlanID = @PlanID", con))
                {
                    cmd.Parameters.AddWithValue("@PlanID", planId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}