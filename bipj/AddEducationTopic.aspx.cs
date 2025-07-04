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
            SaveDynamicValues();

            // Here you can use Topics, txtModuleName.Text, etc to insert into your database!
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
