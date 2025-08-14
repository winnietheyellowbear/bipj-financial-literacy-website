using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class ViewSpecificEdu : System.Web.UI.Page
    {
        protected string TopicForAssistant = "Financial Literacy"; // default fallback
        protected int ModuleId => int.TryParse(Request.QueryString["moduleId"], out int id) ? id : 0;
        protected int PageId => int.TryParse(Request.QueryString["pageId"], out int id) ? id : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Require login
            if (Session["UserId"] == null)
            {
                var returnUrl = Server.UrlEncode(Request.RawUrl);
                Response.Redirect("Loginpage.aspx?returnUrl=" + returnUrl + "&msg=login_required");
                return;
            }

            if (!IsPostBack)
            {
                if (ModuleId <= 0)
                {
                    // If moduleId missing/invalid, go back to Education
                    Response.Redirect("Education.aspx");
                    return;
                }

                LoadModuleInfo();
                LoadSideNav();

                if (PageId > 0)
                    LoadPageContent();
                else
                {
                    pnlNoPageSelected.Visible = true;
                    pnlPageContent.Visible = false;
                }

            }
            var moduleIdStr = Request.QueryString["moduleId"];
            if (int.TryParse(moduleIdStr, out int moduleId))
            {
                TopicForAssistant = GetModuleName(moduleId) ?? TopicForAssistant;
            }
        }

        private string ConnStr => ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private void LoadModuleInfo()
        {
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("SELECT Name FROM EducationModules WHERE Id=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", ModuleId);
                conn.Open();
                var name = cmd.ExecuteScalar() as string;
                ltModuleTitle.Text = string.IsNullOrWhiteSpace(name) ? "Module" : name;
            }
        }

        private void LoadSideNav()
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                // 1) Get subtopics for the module
                var topics = new DataTable();
                using (var da = new SqlDataAdapter("SELECT Id, Name FROM EducationSubTopics WHERE ModuleId=@m ORDER BY Name", conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@m", ModuleId);
                    da.Fill(topics);
                }

                // 2) Build a table for binding (TopicName + Pages(DataTable))
                var navTable = new DataTable();
                navTable.Columns.Add("TopicId", typeof(int));
                navTable.Columns.Add("TopicName", typeof(string));
                navTable.Columns.Add("Pages", typeof(DataTable));

                foreach (DataRow t in topics.Rows)
                {
                    int subTopicId = Convert.ToInt32(t["Id"]);
                    var pages = new DataTable();
                    using (var daP = new SqlDataAdapter(
                        "SELECT Id, Title FROM EducationPages WHERE SubTopicId=@s ORDER BY Id", conn))
                    {
                        daP.SelectCommand.Parameters.AddWithValue("@s", subTopicId);
                        daP.Fill(pages);
                        // Attach moduleId to each page row at bind time (in child ItemDataBound)
                        pages.Columns.Add("ModuleId", typeof(int));
                        foreach (DataRow p in pages.Rows) p["ModuleId"] = ModuleId;
                    }

                    var row = navTable.NewRow();
                    row["TopicId"] = subTopicId;
                    row["TopicName"] = t["Name"].ToString();
                    row["Pages"] = pages;
                    navTable.Rows.Add(row);
                }

                rptTopics.DataSource = navTable;
                rptTopics.DataBind();
            }
        }

        private void LoadPageContent()
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                // 1) Load page content
                using (var cmd = new SqlCommand("SELECT Title, Content FROM EducationPages WHERE Id=@p", conn))
                {
                    cmd.Parameters.AddWithValue("@p", PageId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            pnlNoPageSelected.Visible = false;
                            pnlPageContent.Visible = true;

                            ltPageTitle.Text = r["Title"].ToString();
                            ltPageContent.Text = r["Content"].ToString(); // assume HTML stored
                        }
                        else
                        {
                            pnlNoPageSelected.Visible = true;
                            pnlPageContent.Visible = false;
                            return;
                        }
                    }
                }

                // 2) Progress tracking (mark viewed)
                int userId = Convert.ToInt32(Session["UserId"]);

                using (var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM UserViewedPages WHERE UserId=@u AND PageId=@p)
    INSERT INTO UserViewedPages(UserId, PageId) VALUES(@u, @p);", conn))
                {
                    cmd.Parameters.AddWithValue("@u", userId);
                    cmd.Parameters.AddWithValue("@p", PageId);
                    cmd.ExecuteNonQuery();
                }

                // 3) Recalculate completion for this module
                int totalPages = 0, viewedPages = 0;

                using (var cmd = new SqlCommand(@"
SELECT COUNT(*) FROM EducationPages 
WHERE SubTopicId IN (SELECT Id FROM EducationSubTopics WHERE ModuleId=@m)", conn))
                {
                    cmd.Parameters.AddWithValue("@m", ModuleId);
                    totalPages = (int)cmd.ExecuteScalar();
                }

                using (var cmd = new SqlCommand(@"
SELECT COUNT(*) FROM UserViewedPages 
WHERE UserId=@u AND PageId IN (
    SELECT Id FROM EducationPages 
    WHERE SubTopicId IN (SELECT Id FROM EducationSubTopics WHERE ModuleId=@m)
)", conn))
                {
                    cmd.Parameters.AddWithValue("@u", userId);
                    cmd.Parameters.AddWithValue("@m", ModuleId);
                    viewedPages = (int)cmd.ExecuteScalar();
                }

                int progress = totalPages > 0 ? (viewedPages * 100) / totalPages : 0;

                using (var cmd = new SqlCommand(@"
IF EXISTS (SELECT 1 FROM UserEducationProgress WHERE UserId=@u AND ModuleId=@m)
    UPDATE UserEducationProgress 
    SET CompletionPercentage=@pr, LastAccessed=GETDATE()
    WHERE UserId=@u AND ModuleId=@m
ELSE
    INSERT INTO UserEducationProgress (UserId, ModuleId, CompletionPercentage, LastAccessed)
    VALUES (@u, @m, @pr, GETDATE())", conn))
                {
                    cmd.Parameters.AddWithValue("@u", userId);
                    cmd.Parameters.AddWithValue("@m", ModuleId);
                    cmd.Parameters.AddWithValue("@pr", progress);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Bind child repeater with its pages
        protected void rptTopics_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var row = (DataRowView)e.Item.DataItem;
            var pages = (DataTable)row["Pages"];

            var child = (Repeater)e.Item.FindControl("rptPages");
            child.DataSource = pages;
            child.DataBind();
        }

        // Set the link text/url and active class per page row
        protected void rptPages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var data = (DataRowView)e.Item.DataItem;
            int id = Convert.ToInt32(data["Id"]);
            int moduleId = ModuleId;

            var link = (HyperLink)e.Item.FindControl("lnkPage");
            link.Text = HttpUtility.HtmlEncode(data["Title"].ToString());
            link.NavigateUrl = $"ViewSpecificEdu.aspx?moduleId={moduleId}&pageId={id}";

            // Highlight the current page
            if (id == PageId) link.CssClass += " active";
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
