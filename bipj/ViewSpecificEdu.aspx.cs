using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class ViewSpecificEdu : System.Web.UI.Page
    {
        protected int ModuleId => int.TryParse(Request.QueryString["moduleId"], out int id) ? id : 0;
        protected int PageId => int.TryParse(Request.QueryString["pageId"], out int id) ? id : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (ModuleId > 0)
                {
                    LoadModuleInfo();
                    LoadSideNav();

                    if (PageId > 0)
                    {
                        LoadPageContent();
                    }
                }
            }
        }

        private void LoadModuleInfo()
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT Name FROM EducationModules WHERE Id = @ModuleId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ModuleId", ModuleId);

                conn.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    ltModuleTitle.Text = result.ToString();
                }
            }
        }

        private void LoadSideNav()
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Get all subtopics for this module
                string topicSql = "SELECT Id, Name FROM EducationSubTopics WHERE ModuleId = @ModuleId";
                SqlDataAdapter topicDa = new SqlDataAdapter(topicSql, conn);
                topicDa.SelectCommand.Parameters.AddWithValue("@ModuleId", ModuleId);
                DataTable topicsDt = new DataTable();
                topicDa.Fill(topicsDt);

                // Create a DataTable with proper structure for binding
                DataTable rptData = new DataTable();
                rptData.Columns.Add("TopicName");
                rptData.Columns.Add("Pages", typeof(DataTable));

                foreach (DataRow topicRow in topicsDt.Rows)
                {
                    // Get all pages for this subtopic
                    string pageSql = "SELECT Id, Title, SubTopicId, @ModuleId AS ModuleId FROM EducationPages WHERE SubTopicId = @SubTopicId";
                    SqlDataAdapter pageDa = new SqlDataAdapter(pageSql, conn);
                    pageDa.SelectCommand.Parameters.AddWithValue("@SubTopicId", topicRow["Id"]);
                    pageDa.SelectCommand.Parameters.AddWithValue("@ModuleId", ModuleId);
                    DataTable pagesDt = new DataTable();
                    pageDa.Fill(pagesDt);

                    // Add to main data
                    DataRow newRow = rptData.NewRow();
                    newRow["TopicName"] = topicRow["Name"];
                    newRow["Pages"] = pagesDt;
                    rptData.Rows.Add(newRow);
                }

                rptTopics.DataSource = rptData;
                rptTopics.DataBind();
            }
        }

        private void LoadPageContent()
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT Title, Content FROM EducationPages WHERE Id = @PageId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PageId", PageId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pnlNoPageSelected.Visible = false;
                        pnlPageContent.Visible = true;

                        ltPageTitle.Text = reader["Title"].ToString();
                        hfPageContent.Value = reader["Content"].ToString(); // Store JSON in hidden field
                    }
                }
            }
        }

        protected void rptTopics_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView rowView = (DataRowView)e.Item.DataItem;
                DataTable pages = (DataTable)rowView["Pages"];

                Repeater rptPages = (Repeater)e.Item.FindControl("rptPages");
                rptPages.DataSource = pages;
                rptPages.DataBind();
            }
        }
    }
}