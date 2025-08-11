using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class AllTopics : System.Web.UI.Page
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
        protected string GetModuleImageUrl(object urlObj)
        {
            // default/fallback
            var fallback = ResolveUrl("~/images/default-module.png");

            if (urlObj == null || urlObj == DBNull.Value) return fallback;

            var s = urlObj.ToString();
            if (string.IsNullOrWhiteSpace(s)) return fallback;

            // normalize to app-rooted and resolve
            if (s.StartsWith("~/") || s.StartsWith("/"))
                return ResolveUrl(s);

            return ResolveUrl("~/" + s);
        }

    }
}
