using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace bipj
{
    public partial class EditEducationPage : System.Web.UI.Page
    {
        protected int ModuleId
        {
            get { int id; int.TryParse(Request.QueryString["moduleId"], out id); return id; }
        }

        protected int PageId
        {
            get { int id; int.TryParse(Request.QueryString["pageId"], out id); return id; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSideNav();
                LoadPageContent();
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
                            txtContent.Text = reader["Content"].ToString();
                        }
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = "UPDATE EducationPages SET Title=@Title, Content=@Content WHERE Id=@Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", txtPageTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@Content", txtContent.Text.Trim());
                    cmd.Parameters.AddWithValue("@Id", PageId);
                    cmd.ExecuteNonQuery();
                }
            }
            lblMessage.Text = "Page updated successfully!";
            // Optionally reload side nav if you want
            LoadSideNav();
        }

    }
}
