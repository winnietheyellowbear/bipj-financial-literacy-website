using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace bipj
{
    public partial class Post : System.Web.UI.Page
    {
        
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected async void btn_publish_Click(object sender, EventArgs e)
        {
            int result = 0;

            List<string> imagePaths = new List<string>();
            List<string> videoPaths = new List<string>();

            string text = tb_text.Text;
            string category = radiobtn_category.SelectedValue;
            string user_id = "2";

            // Loop through uploaded files
            HttpFileCollection uploadedFiles = Request.Files;

            for (int i = 0; i < uploadedFiles.Count; i++)
            {
                HttpPostedFile file = uploadedFiles[i];

                if (file.ContentLength > 0)
                {
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    string filename = Path.GetFileName(file.FileName);

                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif")
                    {
                        string savePath = Server.MapPath("~/Forum/Images/" + filename);
                        file.SaveAs(savePath);
                        imagePaths.Add("~/Forum/Images/" + filename);
                    }
                    else if (ext == ".mp4" || ext == ".avi" || ext == ".mov" || ext == ".wmv")
                    {
                        string savePath = Server.MapPath("~/Forum/Videos/" + filename);
                        file.SaveAs(savePath);
                        videoPaths.Add("~/Forum/Videos/" + filename);
                    }
                }
            }

            // Convert list to comma-separated string (or store differently if your DB supports JSON, etc.)
            string images = string.Join(",", imagePaths);
            string videos = string.Join(",", videoPaths);

            User_Post user_post = new User_Post();

            // Call the asynchronous ContentModerator method
            string content_moderator = await user_post.ContentModerator(text);

            if (content_moderator == "Yes")
            {
                lbl_error_msg.Text = "Your post contains inappropriate text.";
                UpdatePanel.Update();
            }
            else
            {
                user_post = new User_Post(images, videos, text, category, user_id);
                result = user_post.PostInsert();

                if (result > 0)
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Post published. 😊'); window.location='Post.aspx';", true);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Failed to publish post. 😞');", true);
                }
            }
        }




    }
}