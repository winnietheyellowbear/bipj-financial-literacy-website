using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class AllProfile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Loginpage.aspx");
                return;
            }
            if (!IsPostBack)
            {
                LoadProfiles();
            }
        }

        private void LoadProfiles(string keyword = "")
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT Id, Name, Email, Point, JoinDate FROM [User]";

                // Search functionality
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    sql += " WHERE Name LIKE @keyword OR Email LIKE @keyword";
                }
                sql += " ORDER BY Name ASC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvProfiles.DataSource = dt;
                gvProfiles.DataBind();
            }
        }

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadProfiles(txtSearch.Text.Trim());
        }

        protected void gvProfiles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewProfile")
            {
                string userId = e.CommandArgument.ToString();
                // Change redirect to ViewSpecificProfile.aspx
                Response.Redirect("ViewSpecificProfile.aspx?userId=" + userId);
            }
        }


        protected void gvProfiles_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvProfiles.PageIndex = e.NewPageIndex;
            LoadProfiles(txtSearch.Text.Trim());
        }
    }
}
