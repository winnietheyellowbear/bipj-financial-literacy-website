using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using static System.Net.Mime.MediaTypeNames;

namespace bipj
{
    public class User_Like
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private string _Like_ID;
        private string _Post_ID;
        private string _User_ID;
        private string _User_Name;
        private string _User_Profile;

        public User_Like()
        {
        }

        // create like
        public User_Like(string post_id, string user_id)
        {
            _Post_ID = post_id;
            _User_ID = user_id;
        }
        public User_Like(string like_id, string post_id, string user_id, string profile, string name)
        {
            _Like_ID = like_id;
            _Post_ID = post_id;
            _User_ID = user_id;
            _User_Profile = profile;
            _User_Name = name;
        }

        public string Like_ID
        {
            get { return _Like_ID; }
            set { _Like_ID = value; }
        }

        public string Post_ID
        {
            get { return _Post_ID; }
            set { _Post_ID = value; }
        }

        public string User_ID
        {
            get { return _User_ID; }
            set { _User_ID = value; }
        }

        public string User_Name
        {
            get { return _User_Name; }
            set { _User_Name = value; }
        }

        public string User_Profile
        {
            get { return _User_Profile; }
            set { _User_Profile = value; }
        }


        public void LikeInsert()
        {
            int result = 0;

            User_Like user_like = new User_Like();
            result = user_like.IsPostLiked(this.Post_ID, this.User_ID);

            if (result == 0)
            {
                string queryStr = "INSERT INTO [Like](User_ID, Post_ID, Like_DateTime) " +
                                  "OUTPUT INSERTED.Like_ID " +
                                  "VALUES (@User_ID, @Post_ID, @Like_DateTime)";

                SqlConnection conn = new SqlConnection(_connStr);
                SqlCommand cmd = new SqlCommand(queryStr, conn);

                cmd.Parameters.AddWithValue("@User_ID", this.User_ID);
                cmd.Parameters.AddWithValue("@Post_ID", this.Post_ID);

                DateTime currentDateTime = new DateTime(2025, 6, 15, 13, 45, 0);
                cmd.Parameters.AddWithValue("@Like_DateTime", currentDateTime);

                conn.Open();
                string like_id = cmd.ExecuteScalar().ToString();
                conn.Close();


                // insert notification
                User_Post user_post = new User_Post();
                string user_id = user_post.GetPostByUserID(this.Post_ID);

                if (user_id != this.User_ID)
                {
                    User_Notification user_notification = new User_Notification("Like", like_id, this.Post_ID);
                    user_notification.NotificationInsert();
                }

            }
            else if (result == 1)
            {
                string queryStr = "DELETE FROM [Like] WHERE Post_ID = @post_id AND User_ID = @user_id";

                SqlConnection conn = new SqlConnection(_connStr);
                SqlCommand cmd = new SqlCommand(queryStr, conn);

                cmd.Parameters.AddWithValue("@post_id", this.Post_ID);
                cmd.Parameters.AddWithValue("@user_id", this.User_ID);

                conn.Open();

                int nofRow = 0;
                nofRow = cmd.ExecuteNonQuery();

                conn.Close();

                User_Notification user_notification = new User_Notification();
                user_notification.NotificationDelete("Like", this.Post_ID);
            }

        }

        public int IsPostLiked(string post_id, string user_id)
        {
            string queryStr = "SELECT * FROM [Like] WHERE Post_ID = @Post_ID AND User_ID = @User_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@Post_ID", post_id);
            cmd.Parameters.AddWithValue("@User_ID", user_id);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            int result = 0;

            while (dr.Read())
            {
                result = 1;
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return result;
        }

        public List<User_Like> GetLikesByPostID(string post_id)
        {
            string like_id, user_id, profile, name;
            List<User_Like> like_list = new List<User_Like>();

            string queryStr = "SELECT * FROM [Like] l " +
                "LEFT OUTER JOIN [User] u ON u.Id = l.User_ID WHERE Post_ID = @Post_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@Post_ID", post_id);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {

                like_id = dr["Like_ID"].ToString();
                user_id = dr["User_ID"].ToString();
                profile = dr["Profile"].ToString();
                name = dr["Name"].ToString();

                User_Like user_like = new User_Like(like_id, post_id, user_id, profile, name);
                like_list.Add(user_like);
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return like_list;
        }

  
    }
}