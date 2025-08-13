using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;

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
            set { ViewState["Topics"] = value; }
        }

        // ---- store uploaded image path in SESSION (not ViewState) ----
        private string UploadedImageFile
        {
            get { return (string)(Session["UploadedImageFile"] ?? ""); }
            set { Session["UploadedImageFile"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Topics = new List<Topic>();
                // fresh page -> clear any previous image path for safety
                UploadedImageFile = "";
            }
            RenderTopics();
        }

        // Save file (if present) to ~/Images/education and stash relative path in Session.
        // Returns true if a file was saved this call.
        private bool TrySaveImageFromFileUpload()
        {
            if (!fileUploadImage.HasFile) return false;

            string extension = Path.GetExtension(fileUploadImage.FileName).ToLowerInvariant();
            string[] allowedExts = { ".jpg", ".jpeg", ".png", ".gif" };
            if (Array.IndexOf(allowedExts, extension) < 0)
            {
                lblMessage.Text = "Please upload an image file (.jpg, .png, .gif).";
                return false;
            }

            string folder = Server.MapPath("~/Images/education/");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string safeName = Path.GetFileNameWithoutExtension(fileUploadImage.FileName);
            string uniqueName = $"{safeName}_{Guid.NewGuid():N}{extension}";
            string savePath = Path.Combine(folder, uniqueName);

            fileUploadImage.SaveAs(savePath);
            UploadedImageFile = $"Images/education/{uniqueName}";
            return true;
        }

        protected void btnInsertImage_Click(object sender, EventArgs e)
        {
            if (TrySaveImageFromFileUpload())
            {
                lblMessage.Text = "Image uploaded successfully!";
            }
            else if (!fileUploadImage.HasFile)
            {
                lblMessage.Text = "Please select an image to upload.";
            }
        }

        protected void btnAddTopic_Click(object sender, EventArgs e)
        {
            // If user selected a file but didn't click Insert yet, save it now so we don't lose it on this postback.
            if (string.IsNullOrWhiteSpace(UploadedImageFile))
                TrySaveImageFromFileUpload();

            SaveDynamicValues();
            Topics.Add(new Topic { TopicName = "", Pages = new List<string>() });
            RenderTopics();
        }

        protected void btnAddPage_Click(object sender, EventArgs e)
        {
            // Same auto-save protection here
            if (string.IsNullOrWhiteSpace(UploadedImageFile))
                TrySaveImageFromFileUpload();

            SaveDynamicValues();
            var btn = (Button)sender;
            int topicIndex = int.Parse(btn.CommandArgument);
            Topics[topicIndex].Pages.Add("");
            RenderTopics();
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            SaveDynamicValues();

            // Final fallback: if no path stored yet but a file is currently selected, save it now.
            if (string.IsNullOrWhiteSpace(UploadedImageFile))
                TrySaveImageFromFileUpload();

            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                string insertModule = @"
INSERT INTO EducationModules (Name, BriefDescription, ImageUrl, IndeptDescription)
VALUES (@Name, @BriefDescription, @ImageUrl, @IndeptDescription);
SELECT SCOPE_IDENTITY();";

                int moduleId = 0;
                using (var cmd = new SqlCommand(insertModule, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", txtModuleName.Text.Trim());
                    cmd.Parameters.AddWithValue("@BriefDescription", txtBriefDesc.Text.Trim());

                    object imageParam = string.IsNullOrWhiteSpace(UploadedImageFile)
                        ? (object)DBNull.Value
                        : UploadedImageFile;
                    cmd.Parameters.AddWithValue("@ImageUrl", imageParam);

                    cmd.Parameters.AddWithValue("@IndeptDescription", txtIndeptDesc.Text.Trim());

                    moduleId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (var topic in Topics)
                {
                    string insertSubTopic = @"
INSERT INTO EducationSubTopics (ModuleId, Name)
VALUES (@ModuleId, @Name);
SELECT SCOPE_IDENTITY();";
                    int subTopicId = 0;
                    using (var cmd = new SqlCommand(insertSubTopic, conn))
                    {
                        cmd.Parameters.AddWithValue("@ModuleId", moduleId);
                        cmd.Parameters.AddWithValue("@Name", (topic.TopicName ?? "").Trim());
                        subTopicId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    foreach (var page in topic.Pages)
                    {
                        string insertPage = @"
INSERT INTO EducationPages (SubTopicId, Title, Content)
VALUES (@SubTopicId, @Title, @Content);";
                        using (var cmd = new SqlCommand(insertPage, conn))
                        {
                            cmd.Parameters.AddWithValue("@SubTopicId", subTopicId);
                            cmd.Parameters.AddWithValue("@Title", (page ?? "").Trim());
                            cmd.Parameters.AddWithValue("@Content", "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            lblMessage.Text = "Education module created successfully!";
            // Optionally clear the session image after success
            UploadedImageFile = "";
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
                    CssClass = "form-control mb-2"
                };
                topicBox.Attributes["placeholder"] = "Topic name";
                topicPanel.Controls.Add(topicBox);

                // Pages for this topic
                for (int j = 0; j < Topics[i].Pages.Count; j++)
                {
                    var pageBox = new TextBox
                    {
                        ID = $"txtPage_{i}_{j}",
                        Text = Topics[i].Pages[j],
                        CssClass = "form-control mb-2"
                    };
                    pageBox.Attributes["placeholder"] = "Page name";
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
