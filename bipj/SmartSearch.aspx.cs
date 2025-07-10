using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace bipj
{
    public partial class SmartSearch : System.Web.UI.Page
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
            if (!IsPostBack)
            {
                Load_Matched_Post();
            }
        }

        protected async void btnSearch_Click(object sender, EventArgs e)
        {
            string user_input = txtSearch.Text.Trim();

            // Prepare post summaries
            post_list = user_post.GetAllPosts();
            var post_summary_list = post_list.Select(p => $"Post {p.Post_ID}: {p.Text}").ToList();
            string combined_post_summary = string.Join("\n", post_summary_list);

            // Formulate the prompt for AI
            string prompt = $"User is interested in: '{user_input}'. " +
                            $"Here are the posts:\n{combined_post_summary}\n" +
                            $"Which posts best match the interest? Reply with a comma-separated list of Post_IDs only.";
            string result = await AI(prompt);

            var matched_id = result
                             .Split(',')
                             .Select(id => id.Trim())
                             .Where(id => !string.IsNullOrEmpty(id))
                             .ToList();
            var matched_post = post_list.Where(p => matched_id.Contains(p.Post_ID)).ToList();

            string redirectUrl = $"SmartSearch.aspx?post_id={HttpUtility.UrlEncode(string.Join(",", matched_id))}";
            string script = $"alert('Number of posts found: {matched_post.Count}'); window.location = '{redirectUrl}';";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertAndRedirect", script, true);
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
                comment_list = user_comment.GetCommentsByPostID(currentPost.Post_ID);
                Repeater commentRepeater = (Repeater)e.Item.FindControl("Comment");
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

            Load_Matched_Post();
        }


        protected void btn_delete_comment_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string comment_id = btn.CommandArgument;

            user_comment.CommentDelete(comment_id);

            Load_Matched_Post();

        }

        protected void Load_Matched_Post()
        {

            string post_id = Request.QueryString["post_id"];

            if (!string.IsNullOrEmpty(post_id))
            {
                // Decode the query string to handle any encoded characters
                var matched_id = HttpUtility.UrlDecode(post_id)
                    .Split(',')
                    .Select(id => id.Trim()) // Remove any spaces or other unwanted characters
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

                post_list = user_post.GetAllPosts();
                var matched_post = post_list.Where(p => matched_id.Contains(p.Post_ID)).ToList();

                Post.DataSource = matched_post;
                Post.DataBind();
            }
        }

        private async Task<string> AI(string prompt)
        {
            string apiKey = ""; // Replace with your OpenAI API key
            string endpoint = "https://api.openai.com/v1/chat/completions";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var request = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                var result = await response.Content.ReadAsStringAsync();

                dynamic jsonResult = JsonConvert.DeserializeObject(result);
                return jsonResult.choices[0].message.content.ToString().Trim();
            }
        }
    }
}
