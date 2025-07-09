using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    [Serializable]
    public class Topic
    {
        public string TopicName { get; set; }
        public List<string> Pages { get; set; } = new List<string>();

    }
    public partial class AddEducationTopic : System.Web.UI.Page
    {
        private List<Topic> Topics
        {
            get
            {
                if (ViewState["Topics"] == null)
                    ViewState["Topics"] = new List<Topic>();
                return (List<Topic>)ViewState["Topics"];
            }
            set
            {
                ViewState["Topics"] = value;
            }
        }
        private string UploadedImageFile
        {
            get { return ViewState["UploadedImageFile"] as string ?? ""; }
            set { ViewState["UploadedImageFile"] = value; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Topics = new List<Topic>();
            }
            RenderTopics();
        }

        protected void btnInsertImage_Click(object sender, EventArgs e)
        {
            if (fileUploadImage.HasFile)
            {
                // Only allow certain file types for security
                string extension = System.IO.Path.GetExtension(fileUploadImage.FileName).ToLower();
                string[] allowedExts = { ".jpg", ".jpeg", ".png", ".gif" };
                if (Array.IndexOf(allowedExts, extension) < 0)
                {
                    lblMessage.Text = "Please upload an image file (.jpg, .png, .gif).";
                    return;
                }

                // Save the file
                string fileName = System.IO.Path.GetFileName(fileUploadImage.FileName);
                string savePath = Server.MapPath("~/images/education/") + fileName;
                fileUploadImage.SaveAs(savePath);

                // Store the file name for saving module later
                UploadedImageFile = "images/education/" + fileName;

                lblMessage.Text = "Image uploaded successfully!";
            }
            else
            {
                lblMessage.Text = "Please select an image to upload.";
            }
        }


        protected void btnAddTopic_Click(object sender, EventArgs e)
        {
            SaveDynamicValues();
            Topics.Add(new Topic { TopicName = "", Pages = new List<string>() });
            RenderTopics();
        }

        protected void btnAddPage_Click(object sender, EventArgs e)
        {
            SaveDynamicValues();
            var btn = (Button)sender;
            int topicIndex = int.Parse(btn.CommandArgument);
            Topics[topicIndex].Pages.Add("");
            RenderTopics();
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            SaveDynamicValues(); // Ensure all inputs are current

            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

            using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
            {
                conn.Open();

                // 1. Insert EducationModule
                string insertModule = @"
            INSERT INTO EducationModules (Name, BriefDescription, ImageUrl, IndeptDescription)
            VALUES (@Name, @BriefDescription, @ImageUrl, @IndeptDescription);
            SELECT SCOPE_IDENTITY();";
                int moduleId = 0;
                using (var cmd = new System.Data.SqlClient.SqlCommand(insertModule, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", txtModuleName.Text.Trim());
                    cmd.Parameters.AddWithValue("@BriefDescription", txtBriefDesc.Text.Trim());
                    cmd.Parameters.AddWithValue("@ImageUrl", UploadedImageFile); // If blank, just empty string
                    cmd.Parameters.AddWithValue("@IndeptDescription", txtIndeptDesc.Text.Trim());

                    moduleId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 2. For each Topic, insert EducationSubTopic, then its pages
                foreach (var topic in Topics)
                {
                    // Insert EducationSubTopic
                    string insertSubTopic = @"
                INSERT INTO EducationSubTopics (ModuleId, Name)
                VALUES (@ModuleId, @Name);
                SELECT SCOPE_IDENTITY();";
                    int subTopicId = 0;
                    using (var cmd = new System.Data.SqlClient.SqlCommand(insertSubTopic, conn))
                    {
                        cmd.Parameters.AddWithValue("@ModuleId", moduleId);
                        cmd.Parameters.AddWithValue("@Name", topic.TopicName.Trim());
                        subTopicId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Insert each Page for this SubTopic
                    foreach (var page in topic.Pages)
                    {
                        string insertPage = @"
                    INSERT INTO EducationPages (SubTopicId, Title, Content)
                    VALUES (@SubTopicId, @Title, @Content);";
                        using (var cmd = new System.Data.SqlClient.SqlCommand(insertPage, conn))
                        {
                            cmd.Parameters.AddWithValue("@SubTopicId", subTopicId);
                            cmd.Parameters.AddWithValue("@Title", page.Trim());
                            cmd.Parameters.AddWithValue("@Content", ""); // Or extend to allow page content
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            lblMessage.Text = "Education module created successfully!";
            // Optionally: clear the form and/or redirect
        }

        private void RenderTopics()
        {
            phTopics.Controls.Clear();

            for (int i = 0; i < Topics.Count; i++)
            {
                var topicPanel = new Panel { CssClass = "mb-3 p-2", ID = "panelTopic" + i };

                // Topic Name
                var topicBox = new TextBox
                {
                    ID = $"txtTopic_{i}",
                    Text = Topics[i].TopicName,
                    CssClass = "form-control mb-2",
                    Attributes = { ["placeholder"] = "Topic name" }
                };
                topicPanel.Controls.Add(topicBox);

                // Pages for this topic
                for (int j = 0; j < Topics[i].Pages.Count; j++)
                {
                    var pageBox = new TextBox
                    {
                        ID = $"txtPage_{i}_{j}",
                        Text = Topics[i].Pages[j],
                        CssClass = "form-control mb-2",
                        Attributes = { ["placeholder"] = "Page name" }
                    };
                    topicPanel.Controls.Add(pageBox);
                }

                // Add Page Button
                var btnAddPage = new Button
                {
                    ID = $"btnAddPage_{i}",
                    CssClass = "btn btn-outline-primary btn-sm mb-2",
                    Text = "+ Page",
                    CommandArgument = i.ToString()
                };
                btnAddPage.Click += btnAddPage_Click;
                topicPanel.Controls.Add(btnAddPage);

                phTopics.Controls.Add(topicPanel);
            }
        }

        private void SaveDynamicValues()
        {
            for (int i = 0; i < Topics.Count; i++)
            {
                var topicBox = (TextBox)phTopics.FindControl($"txtTopic_{i}");
                if (topicBox != null)
                    Topics[i].TopicName = topicBox.Text;

                for (int j = 0; j < Topics[i].Pages.Count; j++)
                {
                    var pageBox = (TextBox)phTopics.FindControl($"txtPage_{i}_{j}");
                    if (pageBox != null)
                        Topics[i].Pages[j] = pageBox.Text;
                }
            }
        }

    }
}
