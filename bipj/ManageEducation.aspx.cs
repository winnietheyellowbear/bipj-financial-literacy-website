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
                LoadTopics();
        }

        private void LoadTopics()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT Id, Title, ImageUrl FROM EducationTopics";
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
            // Redirect to add topic page
            Response.Redirect("AddEducationTopic.aspx");
        }

        protected void btnDeleteTopics_Click(object sender, EventArgs e)
        {
            // Redirect to delete topic page (or show modal)
            Response.Redirect("DeleteEducationTopics.aspx");
        }
    }
}
