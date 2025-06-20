using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class EditMyPost : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string post_id = Session["Post_ID"].ToString();

            if (!IsPostBack)
            {
                User_Post user_post = new User_Post();
                user_post = user_post.GetPostByPostID(post_id);

                Image.DataSource = user_post.Images_List;
                Image.DataBind();

                Video.DataSource = user_post.Videos_List;
                Video.DataBind();

                Session["ImagesList"] = user_post.Images_List;
                Session["VideosList"] = user_post.Videos_List;

                tb_text.Text = user_post.Text;
                radiobtn_category.SelectedValue = user_post.Category;
            }
        }

        protected void btn_remove_image_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string image = btn.CommandArgument;

            var images = Session["ImagesList"] as List<string>;

            images.Remove(image); // Remove image from the list
            Session["ImagesList"] = images; // Save it back to session
            
            Image.DataSource = images;
            Image.DataBind();

            UpdatePanel_Image.Update();
        }

        protected void btn_remove_video_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string video = btn.CommandArgument;

            var videos = Session["VideosList"] as List<string>;

            videos.Remove(video); 
            Session["VideosList"] = videos; 

            Video.DataSource = videos;
            Video.DataBind();

            UpdatePanel_Video.Update();
        }

        protected void btn_update_Click(object sender, EventArgs e)
        {
            int result = 0;

            // Get existing session lists or create new ones
            List<string> imagePaths = Session["ImagesList"] as List<string> ?? new List<string>();
            List<string> videoPaths = Session["VideosList"] as List<string> ?? new List<string>();

            string text = tb_text.Text;
            string category = radiobtn_category.SelectedValue;
            string user_id = "2";
            string post_id = Session["Post_ID"].ToString();

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

            User_Post user_post = new User_Post(post_id, images, videos, text, category, user_id);
            result = user_post.PostUpdate();

            if (result > 0)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Post updated. 😊'); window.location='Post.aspx';", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Failed to update post. 😞');", true);
            }
        }
    }
}