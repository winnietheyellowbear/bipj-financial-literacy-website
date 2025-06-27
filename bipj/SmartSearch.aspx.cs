using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bipj
{
    public partial class SmartSearch : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e) { }

        protected async void btnSearch_Click(object sender, EventArgs e)
        {
            string user_input = txtSearch.Text.Trim();

            // Get all posts
            User_Post user_post = new User_Post();
            List<User_Post> post_list = user_post.GetAllPosts();

            // Prepare a list of post summaries
            var post_summary_list = post_list.Select(p => $"Post {p.Post_ID}: {p.Text}").ToList();
            string combined_post_summary = string.Join("\n", post_summary_list);

            string prompt = $"User is interested in: '{user_input}'. " +
                $"Here are the posts:\n{combined_post_summary}\n" +
                $"Which posts best match the interest? Reply with a comma-separated list of Post_IDs only.";

            string matchedIDsRaw = await AskAI(prompt);

            // Split the AI response by comma, trim each part
            var matchedIDs = matchedIDsRaw.Split(',')
                                         .Select(id => id.Trim())
                                         .Where(id => !string.IsNullOrEmpty(id))
                                         .ToList();

            var matchedPosts = post_list.Where(p => matchedIDs.Contains(p.Post_ID)).ToList();

            if (matchedPosts.Any())
            {
                // Bind matched posts to the Repeater
                Post.DataSource = matchedPosts;
                Post.DataBind();

                UpdatePanel.Update();

            }
            else
            {
                Post.DataSource = null;
                Post.DataBind();

                UpdatePanel.Update();
            }

        }

        private async Task<string> AskAI(string prompt)
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
