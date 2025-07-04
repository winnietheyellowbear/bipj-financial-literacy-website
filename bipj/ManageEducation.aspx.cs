using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace bipj
{
    public partial class ManageEducation : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadModules();
        }

        private void LoadModules()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // You can join or subquery to get the subtopic count if you want.
                // Otherwise just select module fields only.
                string sql = @"
                    SELECT 
                        m.Id, 
                        m.Name, 
                        m.BriefDescription, 
                        m.ImageUrl,
                        m.IndeptDescription,
                        (SELECT COUNT(*) FROM EducationSubTopics s WHERE s.ModuleId = m.Id) AS SubTopicCount
                    FROM EducationModules m";
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
            }
            if (dt.Rows.Count == 0)
            {
                pnlNoTopics.Visible = true;
                pnlTopicsList.Visible = false;
                btnDeleteTopics.Enabled = false;
            }
            else
            {
                pnlNoTopics.Visible = false;
                pnlTopicsList.Visible = true;
                btnDeleteTopics.Enabled = true;
                rptTopics.DataSource = dt;
                rptTopics.DataBind();
            }
        }

        protected void btnAddTopic_Click(object sender, EventArgs e)
        {
            // Redirect to add module (main topic) page
            Response.Redirect("AddEducationTopic.aspx");
        }

        protected void btnDeleteTopics_Click(object sender, EventArgs e)
        {
            // Redirect to delete module page (or show modal)
            Response.Redirect("DeleteEducationTopics.aspx");
        }
    }
}
