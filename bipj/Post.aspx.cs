using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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


        protected void btn_publish_Click(object sender, EventArgs e)
        {
            int result = 0;

            List<string> imagePaths = new List<string>();
            List<string> videoPaths = new List<string>();

            //string images = null;
            //string videos = null;
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

            //if (img_post.HasFile)
            //{
            //    string fileName = Path.GetFileName(img_post.FileName);
            //    string savePath = Server.MapPath("~/Forum/Images/") + fileName;
            //    img_post.SaveAs(savePath);
            //    images = "~/Forum/Images/" + fileName;    
            //}

            //if (video_post.HasFile)
            //{
            //    string fileName = Path.GetFileName(video_post.FileName);
            //    string savePath = Server.MapPath("~/Forum/Videos/") + fileName;
            //    video_post.SaveAs(savePath);
            //    videos = "~/Forum/Videos/" + fileName; 
            //}

            User_Post user_post = new User_Post(images, videos, text, category, user_id);
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