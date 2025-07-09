using System;
using System.Data;
using System.Data.SqlClient;

namespace bipj
{
    public partial class ViewEducationPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadModules();
            }
        }

        private void LoadModules()
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT Id, Name, BriefDescription, ImageUrl FROM EducationModules";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptModules.DataSource = dt;
                rptModules.DataBind();
            }
        }
    }
}