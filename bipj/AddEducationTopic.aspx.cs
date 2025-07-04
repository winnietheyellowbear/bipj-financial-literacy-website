using System;
using System.Web.UI;

namespace bipj
{
    public partial class AddEducationTopic : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // You can put page-load logic here
        }

        // Event handler for the Insert Image button
        protected void btnInsertImage_Click(object sender, EventArgs e)
        {
            // Example logic for file upload
            if (fileUploadImage.HasFile)
            {
                // Save file to /images/education/ or similar
                string fileName = System.IO.Path.GetFileName(fileUploadImage.FileName);
                string savePath = Server.MapPath("~/images/education/") + fileName;
                fileUploadImage.SaveAs(savePath);

                // Optionally, set the image textbox value to the saved path
                txtImage.Text = "images/education/" + fileName;

                // Optionally, show a message (use a Label control in your .aspx)
                // lblMessage.Text = "Image uploaded successfully!";
            }
            else
            {
                // lblMessage.Text = "Please select an image to upload.";
            }
        }

        // You can add more handlers, for example:
        protected void btnAddTopic_Click(object sender, EventArgs e)
        {
            // Add logic for dynamically adding topics
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            // Add logic for saving the whole module with topics/pages to the DB
        }
    }
}
