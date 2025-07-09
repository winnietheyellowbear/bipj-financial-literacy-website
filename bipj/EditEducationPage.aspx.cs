using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class EditEducationPage : System.Web.UI.Page
    {
        protected int ModuleId => int.TryParse(Request.QueryString["moduleId"], out int id) ? id : 0;
        protected int PageId => int.TryParse(Request.QueryString["pageId"], out int id) ? id : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (ModuleId > 0)
                {
                    LoadSideNav();

                    if (PageId > 0)
                    {
                        LoadPageContent();
                    }
                    else
                    {
                        LoadFirstPageOfModule();
                    }
                }
                else
                {
                    lblMessage.Text = "ERROR: No ModuleId specified";
                }
            }
        }

        private void LoadFirstPageOfModule()
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT TOP 1 p.Id 
                              FROM EducationPages p
                              JOIN EducationSubTopics s ON p.SubTopicId = s.Id
                              WHERE s.ModuleId = @ModuleId
                              ORDER BY p.Id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ModuleId", ModuleId);
                    var firstPageId = cmd.ExecuteScalar();

                    if (firstPageId != null)
                    {
                        Response.Redirect($"EditEducationPage.aspx?moduleId={ModuleId}&pageId={firstPageId}");
                    }
                    else
                    {
                        lblMessage.Text = "No pages exist for this module";
                    }
                }
            }
        }

        private void LoadSideNav()
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Fetch topics
                var topics = new List<dynamic>();
                string topicSql = "SELECT Id, Name FROM EducationSubTopics WHERE ModuleId=@ModuleId";

                using (var cmd = new SqlCommand(topicSql, conn))
                {
                    cmd.Parameters.AddWithValue("@ModuleId", ModuleId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topics.Add(new { Id = (int)reader["Id"], Name = reader["Name"].ToString() });
                        }
                    }
                }

                // For each topic, get its pages
                var rptData = new List<object>();
                foreach (var topic in topics)
                {
                    var pages = new List<object>();
                    string pageSql = "SELECT Id, Title FROM EducationPages WHERE SubTopicId=@SubTopicId";

                    using (var cmd = new SqlCommand(pageSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SubTopicId", topic.Id);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                pages.Add(new
                                {
                                    Id = (int)rdr["Id"],
                                    Title = rdr["Title"].ToString(),
                                    ModuleId = ModuleId,
                                    CurrentPageId = PageId
                                });
                            }
                        }
                    }

                    rptData.Add(new
                    {
                        TopicName = topic.Name,
                        Pages = pages
                    });
                }

                rptTopics.DataSource = rptData;
                rptTopics.DataBind();
            }
        }

        private void LoadPageContent()
        {
            if (PageId > 0)
            {
                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "SELECT Title, Content FROM EducationPages WHERE Id=@Id";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", PageId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtPageTitle.Text = reader["Title"].ToString();
                                hfEditorContent.Value = reader["Content"].ToString();
                            }
                        }
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (PageId > 0)
            {
                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "UPDATE EducationPages SET Title=@Title, Content=@Content WHERE Id=@Id";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", txtPageTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@Content", hfEditorContent.Value); // This is now JSON from Editor.js
                        cmd.Parameters.AddWithValue("@Id", PageId);
                        cmd.ExecuteNonQuery();
                    }
                }
                lblMessage.Text = "Page updated successfully!";
            }
            else
            {
                lblMessage.Text = "Error: No page selected";
            }
        }

        protected void rptTopics_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var dataItem = e.Item.DataItem;
                var propertyInfo = dataItem.GetType().GetProperty("Pages");
                var pages = propertyInfo.GetValue(dataItem, null) as IEnumerable<object>;

                var rptPages = (Repeater)e.Item.FindControl("rptPages");
                rptPages.DataSource = pages;
                rptPages.DataBind();
            }
        }
    }
}
