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
        public string user_id = "5";
        public string user_type = "Staff";

        public List<User_Post> post_list = new List<User_Post>();
        User_Post user_post = new User_Post();

        List<User_Like> like_list = new List<User_Like>();
        User_Like user_like = new User_Like();

        User_Comment user_comment = new User_Comment();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session["Discussion_Search"] = null;
                Session["Discussion_Filter"] = null;

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


        protected async void btn_comment_AI_suggestion_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string text = btn.CommandArgument;

            // Get the comment TextBox from the same RepeaterItem
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;
            TextBox textbox = (TextBox)item.FindControl("tb_text");
            string comment = textbox.Text;
           
            string suggestion = await user_post.Comment_AI_Suggestion(text, comment);
            Label label = (Label)item.FindControl("lbl_AISuggestion");
            label.Text = suggestion;

            UpdatePanel_Post.Update();
        }


        protected void btn_delete_comment_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string comment_id = btn.CommandArgument;

            user_comment.CommentDelete(comment_id);

            Update_Panel();
        }

        protected void Search(object sender, EventArgs e)
        {
            string search = searchInput.Text.Trim();
            string category = categoryFilter.SelectedValue;

            Session["Discussion_Search"] = search;
            Session["Discussion_Filter"] = category;

            Update_Panel();
        }

        protected void Update_Panel()
        {
            if ((Session["Discussion_Search"] != null) || (Session["Disucssion_Filter"] != null))
            {
                string search = Session["Discussion_Search"].ToString();
                string category = Session["Discussion_Filter"].ToString();
                post_list = user_post.GetSearchPosts(search, category, user_id);
            }
            else
            {
                post_list = user_post.GetAllPosts(user_id);
            }

            Post.DataSource = post_list;
            Post.DataBind();
            UpdatePanel_Post.Update();
        }

        public int GetLikeCount(string post_id)
        {
            like_list = user_like.GetLikesByPostID(post_id);
            return like_list.Count;
        }
    }
}