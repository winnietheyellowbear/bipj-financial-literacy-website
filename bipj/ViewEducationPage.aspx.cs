using System;
using System.Data;
using System.Data.SqlClient;

namespace bipj
{
    public partial class ViewEducationPage : System.Web.UI.Page
    {
        protected string TopicForAssistant = "Financial Literacy"; // default fallback
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadModules();
            }
            var moduleIdStr = Request.QueryString["moduleId"];
            if (int.TryParse(moduleIdStr, out int moduleId))
            {
                TopicForAssistant = GetModuleName(moduleId) ?? TopicForAssistant;
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
        private string GetModuleName(int moduleId)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
            using (var cmd = new System.Data.SqlClient.SqlCommand(
                "SELECT Name FROM EducationModules WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", moduleId);
                conn.Open();
                var obj = cmd.ExecuteScalar();
                return obj == null ? null : obj.ToString();
            }
        }
    }
}