using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

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
        private bool _Like_Status;

        private List<string> _Images_List;
        private List<string> _Videos_List;

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
        public User_Post(string post_id, List<string> images, List<string> videos, string text, string category, string user_id, string post_datetime, string last_update_datetime, string name, string profile, bool like_status)
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
            _Like_Status = like_status;
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

        public bool Like_Status
        {
            get { return _Like_Status; }
            set { _Like_Status = value; }
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

            DateTime currentDateTime = new DateTime(2025, 6, 15, 13, 45, 0);
            cmd.Parameters.AddWithValue("@Post_DateTime", currentDateTime);

            conn.Open();
            result += cmd.ExecuteNonQuery();
            conn.Close();

            return result;
        }

        public List<User_Post> GetAllPosts()
        {
            string post_id, images, videos, text, category, user_id, post_datetime, last_update_datetime, name, profile;
            bool like_status;
            List<string> images_list = new List<string>();
            List<string> videos_list = new List<string>();

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
                user_id = dr["User_ID"].ToString();
                post_datetime = dr["Post_DateTime"].ToString();
                last_update_datetime = dr["Last_Update_DateTime"].ToString();
                name = dr["Name"].ToString();
                profile = dr["Profile"].ToString();

                User_Like user_like = new User_Like();
                string User_ID = "2";
                int result = user_like.IsPostLiked(post_id, User_ID);

                if (result == 1)
                {
                    like_status = true;
                }
                else
                {
                    like_status = false;
                }

                images_list = images.Split(',').ToList();
                videos_list = videos.Split(',').ToList();

                User_Post user_post = new User_Post(post_id, images_list, videos_list, text, category, user_id, post_datetime, last_update_datetime, name, profile, like_status);
                post_list.Add(user_post);
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return post_list;
        }

        public List<User_Post> GetPostsByUserID(string user_id)
        {
            string post_id, images, videos, text, category, post_datetime, last_update_datetime, name, profile;
            bool like_status;
            List<string> images_list = new List<string>();
            List<string> videos_list = new List<string>();

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

                // like
                User_Like user_like = new User_Like();
                int result = user_like.IsPostLiked(post_id, user_id);

                if (result == 1)
                {
                    like_status = true;
                }
                else
                {
                    like_status = false;
                }

                images_list = images.Split(',').ToList();
                videos_list = videos.Split(',').ToList();

                User_Post user_post = new User_Post(post_id, images_list, videos_list, text, category, user_id, post_datetime, last_update_datetime, name, profile, like_status);
                post_list.Add(user_post);
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return post_list;
        }


        public User_Post GetPostByPostID(string post_id)
        {
            string images, videos, text, category;
            List<string> images_list = new List<string>();
            List<string> videos_list = new List<string>();

            User_Post user_post = new User_Post();

            string queryStr = "SELECT * FROM Post WHERE Post_ID = @Post_ID";

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
                
                images_list = images.Split(',').ToList();
                videos_list = videos.Split(',').ToList();

                user_post = new User_Post(post_id, images_list, videos_list, text, category);
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

            DateTime datetime = DateTime.Now;
            cmd.Parameters.AddWithValue("@Last_Update_DateTime", datetime);


            conn.Open();
            int nofRow = 0;
            nofRow = cmd.ExecuteNonQuery();
            conn.Close();

            return nofRow;
        }



    }
}