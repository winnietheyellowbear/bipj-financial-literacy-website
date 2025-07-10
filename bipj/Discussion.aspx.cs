using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace bipj
{
    public partial class Discussion : System.Web.UI.Page
    {
        public string user_id = "2";

        public List<User_Post> post_list = new List<User_Post>();
        User_Post user_post = new User_Post();

        List<User_Like> like_list = new List<User_Like>();
        User_Like user_like = new User_Like();

        List<User_Comment> comment_list = new List<User_Comment>();
        User_Comment user_comment = new User_Comment();

        protected void Page_Load(object sender, EventArgs e)
        {
            post_list = user_post.GetAllPosts();

            if (!IsPostBack)
            {
                Post.DataSource = post_list;
                Post.DataBind();
            }

        }

        protected void post_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Get the current post
                var currentPost = (User_Post)e.Item.DataItem;

                // -------------- like --------------
                like_list = user_like.GetLikesByPostID(currentPost.Post_ID);
                Label likeCountLabel = (Label)e.Item.FindControl("lbl_Like_Count");
                likeCountLabel.Text = like_list.Count.ToString();

                // -------------- comment --------------
                Repeater commentRepeater = (Repeater)e.Item.FindControl("Comment");
                comment_list = user_comment.GetCommentsByPostID(currentPost.Post_ID);
                commentRepeater.DataSource = comment_list;
                commentRepeater.DataBind();

            }
        }

        protected void btn_like_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string post_id = btn.CommandArgument;

            User_Like user_like = new User_Like(post_id, user_id);
            user_like.LikeInsert();

            like_list = user_like.GetLikesByPostID(post_id);
            if (user_like.IsPostLiked(post_id, user_id) == 1)
            {
                btn.CssClass = "btn-red";
                btn.Text = "Liked (" + like_list.Count.ToString() + ")";
            }
            else
            {
                btn.CssClass = "btn-blue";
                btn.Text = "Like (" + like_list.Count.ToString() + ")";
            }

            RepeaterItem item = (RepeaterItem)btn.NamingContainer;
            UpdatePanel updatePanel = (UpdatePanel)item.FindControl("UpdatePanel_Like");
            updatePanel.Update();

        }

        protected void btn_comment_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string post_id = btn.CommandArgument;
           
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;

            // Get the comment TextBox from the same RepeaterItem
            TextBox textbox = (TextBox)item.FindControl("tb_text");
            string text = textbox.Text;
            textbox.Text = "";

            User_Comment user_comment = new User_Comment(text, user_id, post_id);
            user_comment.CommentInsert();

            comment_list = user_comment.GetCommentsByPostID(post_id);

            Repeater commentRepeater = (Repeater)item.FindControl("Comment");
            UpdatePanel updatePanel = (UpdatePanel)item.FindControl("UpdatePanel_Comment");
            commentRepeater.DataSource = comment_list;
            commentRepeater.DataBind();
            updatePanel.Update();
        }

        protected void btn_delete_comment_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string comment_id = btn.CommandArgument;

            user_comment.CommentDelete(comment_id);

            post_list = user_post.GetAllPosts();
            Post.DataSource = post_list;
            Post.DataBind();
            UpdatePanel_Post.Update();

        }


    }
}