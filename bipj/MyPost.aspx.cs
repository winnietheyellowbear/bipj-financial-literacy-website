using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class MyPost : System.Web.UI.Page
    {
        public string user_id;

        public List<User_Post> post_list = new List<User_Post>();
        User_Post user_post = new User_Post();

        User_Like user_like = new User_Like();
        List<User_Like> like_list = new List<User_Like>();
        
        User_Comment user_comment = new User_Comment();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Loginpage.aspx");
            }
            else
            {
                user_id = Session["UserId"].ToString();
            }

            if (!IsPostBack)
            {
                Update_Panel();
            }
        }

        protected void btn_like_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string post_id = btn.CommandArgument;

            user_like = new User_Like(post_id, user_id);
            user_like.LikeInsert();

            Update_Panel();
        }

        protected void btn_comment_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string post_id = btn.CommandArgument;

            // Get the comment TextBox from the same RepeaterItem
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;
            TextBox textbox = (TextBox)item.FindControl("tb_text");
            string text = textbox.Text;

            user_comment = new User_Comment(text, user_id, post_id);
            user_comment.CommentInsert();

            Update_Panel();
        }

        protected void btn_delete_comment_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string comment_id = btn.CommandArgument;

            user_comment.CommentDelete(comment_id);

            Update_Panel();
        }

        protected void btn_delete_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string post_id = btn.CommandArgument;

            user_post.PostDelete(post_id);

            Update_Panel();
        }

        protected void Update_Panel()
        {
            post_list = user_post.GetPostsByUserID(user_id);

            Post.DataSource = post_list;
            Post.DataBind();
            UpdatePanel_Post.Update();
        }

        public int GetLikeCount(string post_id)
        {
            like_list = user_like.GetLikesByPostID(post_id);
            return like_list.Count;
        }

        protected void btn_edit_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string post_id = btn.CommandArgument;

            Response.Redirect("EditMyPost.aspx?post_id=" + post_id);
        }

    }
}