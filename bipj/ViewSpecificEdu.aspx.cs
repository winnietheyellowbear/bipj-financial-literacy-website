

using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Services;
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
        [System.Web.Services.WebMethod]
        public static string GetAIResponse(string question, string topic)
        {
            string apiKey = "change here replace here";

            string prompt = $"You are assisting a learner in the topic of \"{topic}\". " +
                            $"Only answer questions that are related to this topic. " +
                            $"If the question is not related to this topic, remind the student it's out of scope — but still give a brief answer. " +
                            $"Here is the student's question: \"{question}\"";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "system", content = $"You are a helpful tutor restricted to the topic: {topic}" },
            new { role = "user", content = prompt }
        },
                temperature = 0.7
            };

            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync("https://api.openai.com/v1/chat/completions", content).Result;

                var responseString = response.Content.ReadAsStringAsync().Result;
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);

                return result.choices[0].message.content.ToString();
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
                        ltPageContent.Text = reader["Content"].ToString(); 

                    }
                }
                int userId = Convert.ToInt32(Session["UserId"]);

                // Step 1: Mark this page as viewed
                using (SqlCommand markViewedCmd = new SqlCommand(@"
    IF NOT EXISTS (
        SELECT 1 FROM UserViewedPages WHERE UserId = @UserId AND PageId = @PageId
    )
    BEGIN
        INSERT INTO UserViewedPages (UserId, PageId) VALUES (@UserId, @PageId)
    END", conn))
                {
                    markViewedCmd.Parameters.AddWithValue("@UserId", userId);
                    markViewedCmd.Parameters.AddWithValue("@PageId", PageId);
                    markViewedCmd.ExecuteNonQuery();
                }

                // Step 2: Recalculate progress
                int totalPages = 0, viewedPages = 0;

                // Total pages in module
                using (SqlCommand totalPagesCmd = new SqlCommand(@"
    SELECT COUNT(*) FROM EducationPages 
    WHERE SubTopicId IN (
        SELECT Id FROM EducationSubTopics WHERE ModuleId = @ModuleId
    )", conn))
                {
                    totalPagesCmd.Parameters.AddWithValue("@ModuleId", ModuleId);
                    totalPages = (int)totalPagesCmd.ExecuteScalar();
                }

                // Viewed pages by user for module
                using (SqlCommand viewedPagesCmd = new SqlCommand(@"
    SELECT COUNT(*) FROM UserViewedPages 
    WHERE UserId = @UserId AND PageId IN (
        SELECT Id FROM EducationPages 
        WHERE SubTopicId IN (
            SELECT Id FROM EducationSubTopics WHERE ModuleId = @ModuleId
        )
    )", conn))
                {
                    viewedPagesCmd.Parameters.AddWithValue("@UserId", userId);
                    viewedPagesCmd.Parameters.AddWithValue("@ModuleId", ModuleId);
                    viewedPages = (int)viewedPagesCmd.ExecuteScalar();
                }

                int progress = totalPages > 0 ? (viewedPages * 100) / totalPages : 0;

                // Step 3: Update UserEducationProgress
                using (SqlCommand updateProgressCmd = new SqlCommand(@"
    IF EXISTS (SELECT 1 FROM UserEducationProgress WHERE UserId = @UserId AND ModuleId = @ModuleId)
        UPDATE UserEducationProgress 
        SET CompletionPercentage = @Progress, LastAccessed = GETDATE() 
        WHERE UserId = @UserId AND ModuleId = @ModuleId
    ELSE
        INSERT INTO UserEducationProgress (UserId, ModuleId, CompletionPercentage, LastAccessed)
        VALUES (@UserId, @ModuleId, @Progress, GETDATE())", conn))
                {
                    updateProgressCmd.Parameters.AddWithValue("@UserId", userId);
                    updateProgressCmd.Parameters.AddWithValue("@ModuleId", ModuleId);
                    updateProgressCmd.Parameters.AddWithValue("@Progress", progress);
                    updateProgressCmd.ExecuteNonQuery();
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
