using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;


namespace bipj
{
    public class User_Post
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private string _Post_ID;
        private string _Images;
        private string _Videos;
        private string _Text;
        private string _Category;
        private string _User_ID;
        private string _Post_DateTime;
        private string _Last_Update_DateTime;
        private string _Name;
        private string _Profile;
        private string _Type;
        private bool _Like_Status;
        private List<User_Comment> _Comments_List;
        private List<string> _Images_List;
        private List<string> _Videos_List;

        string apiKey = "";

        public User_Post()
        {
        }

        // create post
        public User_Post(string images, string videos, string text, string category, string user_id)
        {
            _Images = images;
            _Videos = videos;
            _Text = text;
            _Category = category;
            _User_ID = user_id;
        }

        // retrieve post
        public User_Post(string post_id, List<string> images, List<string> videos, string text, string category, string user_id, string post_datetime, string last_update_datetime, string name, string profile, string type, bool like_status, List<User_Comment> comments_list)
        {
            _Post_ID = post_id;
            _Images_List = images;
            _Videos_List = videos;
            _Text = text;
            _Category = category;
            _User_ID = user_id;
            _Post_DateTime = post_datetime;
            _Last_Update_DateTime = last_update_datetime;
            _Name = name;
            _Profile = profile;
            _Type = type;
            _Like_Status = like_status;
            _Comments_List = comments_list;
        }

        // retrieve post for update
        public User_Post(string post_id, List<string> images, List<string> videos, string text, string category)
        {
            _Post_ID = post_id;
            _Images_List = images;
            _Videos_List = videos;
            _Text = text;
            _Category = category;
        }

        // update post
        public User_Post(string post_id, string images, string videos, string text, string category, string user_id)
        {
            _Post_ID = post_id;
            _Images = images;
            _Videos = videos;
            _Text = text;
            _Category = category;
            _User_ID = user_id;
        }

        public string Post_ID
        {
            get { return _Post_ID; }
            set { _Post_ID = value; }
        }

        public string Images
        {
            get { return _Images; }
            set { _Images = value; }
        }

        public string Videos
        {
            get { return _Videos; }
            set { _Videos = value; }
        }

        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        public string Category
        {
            get { return _Category; }
            set { _Category = value; }
        }

        public string User_ID
        {
            get { return _User_ID; }
            set { _User_ID = value; }
        }

        public string Post_DateTime
        {
            get { return _Post_DateTime; }
            set { _Post_DateTime = value; }
        }

        public string Last_Update_DateTime
        {
            get { return _Last_Update_DateTime; }
            set { _Last_Update_DateTime = value; }
        }

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        public string Profile
        {
            get { return _Profile; }
            set { _Profile = value; }
        }

        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        public bool Like_Status
        {
            get { return _Like_Status; }
            set { _Like_Status = value; }
        }

        public List<User_Comment> Comments_List
        {
            get { return _Comments_List; }
            set { _Comments_List = value; }
        }

        public List<string> Images_List
        {
            get { return _Images_List; }
            set { _Images_List = value; }
        }

        public List<string> Videos_List
        {
            get { return _Videos_List; }
            set { _Videos_List = value; }
        }


        public int PostInsert()
        {
            int result = 0;

            string queryStr = "INSERT INTO Post(Images, Videos, Text, Category, User_ID, Post_DateTime)"
                            + "VALUES (@Images, @Videos, @Text, @Category, @User_ID, @Post_DateTime)";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.Parameters.Add(new SqlParameter("@Images", string.IsNullOrWhiteSpace(this.Images) ? (object)DBNull.Value : this.Images));
            cmd.Parameters.Add(new SqlParameter("@Videos", string.IsNullOrWhiteSpace(this.Videos) ? (object)DBNull.Value : this.Videos));
            cmd.Parameters.AddWithValue("@Text", this.Text);
            cmd.Parameters.AddWithValue("@Category", this.Category);
            cmd.Parameters.AddWithValue("@User_ID", this.User_ID);

            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = currentDateTime.ToString("dd MMM yyyy hh:mm tt");
            cmd.Parameters.AddWithValue("@Post_DateTime", formattedDateTime);

            conn.Open();
            result += cmd.ExecuteNonQuery();
            conn.Close();

            return result;
        }

        public List<User_Post> GetAllPosts(string user_id)
        {
            string post_id, images, videos, text, category, post_datetime, last_update_datetime, name, profile, type;
            bool like_status;
            List<string> images_list = new List<string>();
            List<string> videos_list = new List<string>();

            User_Like user_like = new User_Like();
            User_Comment user_Comment = new User_Comment();
            List<User_Comment> comments_list = new List<User_Comment>();
            List<User_Post> post_list = new List<User_Post>();

            string queryStr = "SELECT * FROM Post p LEFT OUTER JOIN [User] u ON p.User_ID = u.Id ORDER BY p.Post_ID DESC";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                post_id = dr["Post_ID"].ToString();
                images = dr["Images"].ToString();
                videos = dr["Videos"].ToString();
                text = dr["Text"].ToString();
                category = dr["Category"].ToString();
                post_datetime = dr["Post_DateTime"].ToString();
                last_update_datetime = dr["Last_Update_DateTime"].ToString();
                name = dr["Name"].ToString();
                profile = dr["Profile"].ToString();
                type = dr["Type"].ToString();

                images_list = images.Split(',').ToList();
                videos_list = videos.Split(',').ToList();

                like_status = user_like.IsPostLiked(post_id, user_id);
                comments_list = user_Comment.GetCommentsByPostID(post_id);

                User_Post user_post = new User_Post(post_id, images_list, videos_list, text, category, user_id, post_datetime, last_update_datetime, name, profile, type, like_status, comments_list);
                post_list.Add(user_post);
            }
            conn.Close();
            dr.Close();
            dr.Dispose();

            return post_list;
        }

        public List<User_Post> GetPostsByUserID(string user_id)
        {
            string post_id, images, videos, text, category, post_datetime, last_update_datetime, name, profile, type;
            bool like_status;
            List<string> images_list = new List<string>();
            List<string> videos_list = new List<string>();

            User_Like user_like = new User_Like();
            User_Comment user_Comment = new User_Comment();
            List<User_Comment> comments_list = new List<User_Comment>();
            List<User_Post> post_list = new List<User_Post>();

            string queryStr = "SELECT * FROM Post p LEFT OUTER JOIN [User] u ON p.User_ID = u.Id WHERE p.User_ID = @User_ID ORDER BY p.Post_ID DESC";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@User_ID", user_id);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                post_id = dr["Post_ID"].ToString();
                images = dr["Images"].ToString();
                videos = dr["Videos"].ToString();
                text = dr["Text"].ToString();
                category = dr["Category"].ToString();
                post_datetime = dr["Post_DateTime"].ToString();
                last_update_datetime = dr["Last_Update_DateTime"].ToString();
                name = dr["Name"].ToString();
                profile = dr["Profile"].ToString();
                type = dr["Type"].ToString();

                images_list = images.Split(',').ToList();
                videos_list = videos.Split(',').ToList();

                like_status = user_like.IsPostLiked(post_id, user_id);
                comments_list = user_Comment.GetCommentsByPostID(post_id);

                User_Post user_post = new User_Post(post_id, images_list, videos_list, text, category, user_id, post_datetime, last_update_datetime, name, profile, type, like_status, comments_list);
                post_list.Add(user_post);
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return post_list;
        }

        public User_Post GetPostByPostID(string post_id, string user_id)
        {
            string images, videos, text, category, post_datetime, last_update_datetime, name, profile, type;
            bool like_status;
            List<string> images_list = new List<string>();
            List<string> videos_list = new List<string>();

            User_Post user_post = new User_Post();
            User_Like user_like = new User_Like();
            User_Comment user_Comment = new User_Comment();
            List<User_Comment> comments_list = new List<User_Comment>();


            string queryStr = "SELECT * FROM Post p LEFT OUTER JOIN [User] u ON p.User_ID = u.Id WHERE Post_ID = @Post_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@Post_ID", post_id);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                images = dr["Images"].ToString();
                videos = dr["Videos"].ToString();
                text = dr["Text"].ToString();
                category = dr["Category"].ToString();
                post_datetime = dr["Post_DateTime"].ToString();
                last_update_datetime = dr["Last_Update_DateTime"].ToString();
                name = dr["Name"].ToString();
                profile = dr["Profile"].ToString();
                type = dr["Type"].ToString();

                images_list = images.Split(',').ToList();
                videos_list = videos.Split(',').ToList();
                
                like_status = user_like.IsPostLiked(post_id, user_id);
                comments_list = user_Comment.GetCommentsByPostID(post_id);

                user_post = new User_Post(post_id, images_list, videos_list, text, category, user_id, post_datetime, last_update_datetime, name, profile, type, like_status, comments_list);
                return user_post;
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return user_post;
        }

        public string GetPostUserID(string post_id)
        {
            string user_id = null;

            string queryStr = "SELECT * FROM Post WHERE Post_ID = @Post_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@Post_ID", post_id);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                user_id = dr["User_ID"].ToString();
                return user_id;
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return user_id;
        }

        public int PostDelete(string post_id)
        {
            string queryStr = "DELETE FROM Post WHERE Post_ID = @post_id";
            string queryStr1 = "DELETE FROM [Like] WHERE Post_ID = @post_id";
            string queryStr2 = "DELETE FROM Comment WHERE Post_ID = @post_id";
            string queryStr3 = "DELETE FROM Notification WHERE Post_ID = @post_id";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            SqlCommand cmd1 = new SqlCommand(queryStr1, conn);
            SqlCommand cmd2 = new SqlCommand(queryStr2, conn);
            SqlCommand cmd3 = new SqlCommand(queryStr3, conn);

            cmd.Parameters.AddWithValue("@post_id", post_id);
            cmd1.Parameters.AddWithValue("@post_id", post_id);
            cmd2.Parameters.AddWithValue("@post_id", post_id);
            cmd3.Parameters.AddWithValue("@post_id", post_id);

            conn.Open();
            int nofRow = 0;
            nofRow = cmd.ExecuteNonQuery() + cmd1.ExecuteNonQuery() + cmd2.ExecuteNonQuery() + cmd3.ExecuteNonQuery();
            conn.Close();

            return nofRow;

        }

        public int PostUpdate()
        {
            string queryStr = "UPDATE Post SET" +
                            " Images = @Images, " +
                            " Videos = @Videos, " +
                            " Text = @Text, " +
                            " Category = @Category, " +
                            " Last_Update_DateTime = @Last_Update_DateTime " +
                            " WHERE Post_ID = @Post_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@Post_ID", this.Post_ID);
            cmd.Parameters.AddWithValue("@Images", this.Images);
            cmd.Parameters.AddWithValue("@Videos", this.Videos);
            cmd.Parameters.AddWithValue("@Text", this.Text);
            cmd.Parameters.AddWithValue("@Category", this.Category);

            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = currentDateTime.ToString("dd MMM yyyy hh:mm tt");
            cmd.Parameters.AddWithValue("@Last_Update_DateTime", formattedDateTime);

            conn.Open();
            int nofRow = 0;
            nofRow = cmd.ExecuteNonQuery();
            conn.Close();

            return nofRow;
        }

        public List<User_Post> GetSearchPosts(string searchInput, string filterInput, string user_id)
        {
            User_Post user_post = new User_Post();
            List<User_Post> post_list = new List<User_Post>();

            string queryStr = "SELECT * FROM Post p LEFT OUTER JOIN [User] u ON p.User_ID = u.Id";

            if (!string.IsNullOrEmpty(searchInput) && filterInput != "category")
            {
                queryStr += " WHERE (Text LIKE @searchInput OR Name LIKE @searchInput) AND Category = @category";
            }
            else if (!string.IsNullOrEmpty(searchInput))
            {
                queryStr += " WHERE (Text LIKE @searchInput OR Name LIKE @searchInput)";
            }
            else if (filterInput != "category")
            {
                queryStr += " WHERE Category = @category";
            }

            queryStr += " ORDER BY p.Post_ID DESC";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);

            if (!string.IsNullOrEmpty(searchInput) && filterInput != "category")
            {
                cmd.Parameters.AddWithValue("@searchInput", "%" + searchInput + "%");
                cmd.Parameters.AddWithValue("@category", filterInput);
            }

            else if (!string.IsNullOrEmpty(searchInput))
            {
                cmd.Parameters.AddWithValue("@searchInput", "%" + searchInput + "%");

            }
            else if (filterInput != "category")
            {
                cmd.Parameters.AddWithValue("@category", filterInput);
            }

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                string post_id = dr["Post_ID"].ToString();

                user_post = user_post.GetPostByPostID(post_id, user_id);
                post_list.Add(user_post);
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            if ((string.IsNullOrEmpty(searchInput) || (searchInput == "")) && (filterInput == "category"))
            {
                post_list = user_post.GetAllPosts(user_id);
            }

            return post_list;
        }


        public async Task<string> ContentModerator(string text)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");  

                var requestBody = new
                {
                    model = "gpt-4",  
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are a content moderator. Respond ONLY with 'Yes' if the text contains rude, offensive, disrespectful, or insulting language, even if it's mild. Respond 'No' otherwise."
                        },
                        new
                        {
                            role = "user",
                            content = $"Is this content inappropriate? '{text}'"
                        }
                    }
                };

                // Send the request to the OpenAI API
                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions",
                    new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));

                // Read the response content
                var responseString = await response.Content.ReadAsStringAsync();

                // Parse the JSON response to get the assistant's reply
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                string resultText = responseObject.choices[0].message.content.ToString().Trim();

                if (resultText.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    return "Yes";
                }
                else
                {
                    return "No";
                }
            }
        }

        public async Task<string> Comment_AI_Suggestion(string text, string comment)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-4", 
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are a staff member of a financial literacy website, commenting on a post in a forum. Your task is to evaluate the user's comment and suggest improvements if necessary. The format of your response should be as follows:\n\n" +
                                      "1. 'evaluation of comment (if any):' - Evaluate the user's comment and mention any improvements or praise if it's good.\n" +
                                      "2. 'suggested comment:' - If the comment could be improved, provide a suggested response that is professional, encouraging, and informative about financial literacy."
                        },
                        new
                        {
                            role = "user",
                            content = $"Given the following text post by the user, please evaluate my comment and suggest a new one if necessary. The post text is: '{text}'. My response is: '{comment}'"
                        }
                    }
                };

                // Send the request to the OpenAI API
                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions",
                    new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));

                // Read the response content
                var responseString = await response.Content.ReadAsStringAsync();

                // Parse the JSON response to get the assistant's reply
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                string suggestion = responseObject.choices[0].message.content.ToString().Trim();

                return suggestion;
            }
        }

    }
}