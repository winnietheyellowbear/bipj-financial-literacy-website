using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class EditEducationPage : System.Web.UI.Page
    {
        protected int ModuleId => int.TryParse(Request.QueryString["moduleId"], out int id) ? id : 0;
        protected int PageId => int.TryParse(Request.QueryString["pageId"], out int id) ? id : 0;

        private string ConnStr => ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Optional: require staff login
            if (Session["UserId"] == null || !"Staff".Equals(Session["UserType"] as string, StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = Server.UrlEncode(Request.RawUrl);
                Response.Redirect("Loginpage.aspx?returnUrl=" + returnUrl + "&msg=login_required");
                return;
            }

            if (!IsPostBack)
            {
                if (ModuleId <= 0)
                {
                    lblMessage.CssClass = "text-danger";
                    lblMessage.Text = "ERROR: No ModuleId specified.";
                    return;
                }

                LoadSideNav();

                if (PageId > 0)
                    LoadPageContent();
                else
                    LoadFirstPageOfModule();
            }
        }

        private void LoadFirstPageOfModule()
        {
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 p.Id
                FROM EducationPages p
                JOIN EducationSubTopics s ON p.SubTopicId = s.Id
                WHERE s.ModuleId = @ModuleId
                ORDER BY p.Id;", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@ModuleId", ModuleId);
                var firstId = cmd.ExecuteScalar();
                if (firstId != null)
                {
                    Response.Redirect($"EditEducationPage.aspx?moduleId={ModuleId}&pageId={firstId}");
                }
                else
                {
                    lblMessage.CssClass = "text-warning";
                    lblMessage.Text = "No pages exist for this module.";
                }
            }
        }

        private sealed class TopicInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<PageInfo> Pages { get; set; } = new List<PageInfo>();
        }

        private sealed class PageInfo
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int ModuleId { get; set; }
        }

        private void LoadSideNav()
        {
            var topics = new List<TopicInfo>();

            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                // Fetch topics
                using (var cmd = new SqlCommand("SELECT Id, Name FROM EducationSubTopics WHERE ModuleId=@m ORDER BY Name;", conn))
                {
                    cmd.Parameters.AddWithValue("@m", ModuleId);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            topics.Add(new TopicInfo
                            {
                                Id = (int)r["Id"],
                                Name = r["Name"].ToString()
                            });
                        }
                    }
                }

                // Fetch pages per topic
                foreach (var t in topics)
                {
                    using (var cmd = new SqlCommand("SELECT Id, Title FROM EducationPages WHERE SubTopicId=@s ORDER BY Id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@s", t.Id);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                t.Pages.Add(new PageInfo
                                {
                                    Id = (int)r["Id"],
                                    Title = r["Title"].ToString(),
                                    ModuleId = ModuleId
                                });
                            }
                        }
                    }
                }
            }

            rptTopics.DataSource = topics;
            rptTopics.DataBind();
        }

        protected void rptTopics_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var topic = (TopicInfo)e.Item.DataItem;
            var rptPages = (Repeater)e.Item.FindControl("rptPages");
            rptPages.DataSource = topic.Pages;
            rptPages.DataBind();
        }

        protected void rptPages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var page = (PageInfo)e.Item.DataItem;
            var link = (HyperLink)e.Item.FindControl("lnkPage");

            link.Text = HttpUtility.HtmlEncode(page.Title);
            link.NavigateUrl = $"EditEducationPage.aspx?moduleId={page.ModuleId}&pageId={page.Id}";

            // Highlight current page
            if (page.Id == PageId)
                link.CssClass += " active";
        }

        private void LoadPageContent()
        {
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("SELECT Title, Content FROM EducationPages WHERE Id=@id;", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@id", PageId);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        txtPageTitle.Text = r["Title"].ToString();
                        // Store raw HTML in hidden field; CKEditor will load it.
                        hfEditorContent.Value = r["Content"].ToString();
                        lblMessage.Text = string.Empty;
                    }
                    else
                    {
                        lblMessage.CssClass = "text-danger";
                        lblMessage.Text = "Page not found.";
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (PageId <= 0)
            {
                lblMessage.CssClass = "text-danger";
                lblMessage.Text = "Error: No page selected.";
                return;
            }

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("UPDATE EducationPages SET Title=@t, Content=@c WHERE Id=@id;", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@t", txtPageTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@c", hfEditorContent.Value ?? string.Empty); // HTML from CKEditor
                cmd.Parameters.AddWithValue("@id", PageId);
                cmd.ExecuteNonQuery();
            }

            lblMessage.CssClass = "text-success";
            lblMessage.Text = "Page updated successfully!";
        }
    }
}
