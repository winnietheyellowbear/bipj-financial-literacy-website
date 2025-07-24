using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.UI;

namespace bipj
{
    public partial class Education : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadRecentModules();
                LoadCompletedModules();
            }

        }
        private void LoadRecentModules()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            int userId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
            SELECT TOP 3 em.Id, em.Name, em.BriefDescription, em.ImageUrl
            FROM UserEducationProgress uep
            INNER JOIN EducationModules em ON uep.ModuleId = em.Id
            WHERE uep.UserId = @UserId
            ORDER BY uep.LastAccessed DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptRecentModules.DataSource = dt;
                rptRecentModules.DataBind();

                pnlNoRecent.Visible = dt.Rows.Count == 0;
                rptRecentModules.Visible = dt.Rows.Count > 0;
            }
        }
        private void LoadCompletedModules()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            int userId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
            SELECT em.Id, em.Name, em.BriefDescription, em.ImageUrl
            FROM UserEducationProgress uep
            INNER JOIN EducationModules em ON uep.ModuleId = em.Id
            WHERE uep.UserId = @UserId AND uep.CompletionPercentage >= 100";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptCompletedModules.DataSource = dt;
                rptCompletedModules.DataBind();
            }
        }
        protected void btnShowAllTopics_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllTopics.aspx");
        }

        protected void btnViewAllRecent_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllRecent.aspx");
        }

        protected void btnViewAllCompleted_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllCompleted.aspx");
        }
    }
}
