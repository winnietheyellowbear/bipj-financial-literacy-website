using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace bipj
{
    public class User_Comment
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private string _Comment_ID;
        private string _Text;
        private string _User_ID;
        private string _Post_ID;
        private string _Comment_DateTime;

        private string _User_Profile;
        private string _User_Name;

        public User_Comment()
        {
        }

        // create comment
        public User_Comment(string text, string user_id, string post_id)
        {
            _Text = text;
            _User_ID = user_id;
            _Post_ID = post_id;
        }

        // retrieve comment
        public User_Comment(string comment_id, string text, string user_id, string post_id, string comment_datetime, string profile, string name)
        {
            _Comment_ID = comment_id;
            _Text = text;
            _User_ID = user_id;
            _Post_ID = post_id;
            _Comment_DateTime = comment_datetime;
            _User_Profile = profile;
            _User_Name = name;
        }

        public string Comment_ID
        {
            get { return _Comment_ID; }
            set { _Comment_ID = value; }
        }

        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        public string User_ID
        {
            get { return _User_ID; }
            set { _User_ID = value; }
        }

        public string Post_ID
        {
            get { return _Post_ID; }
            set { _Post_ID = value; }
        }

        public string Comment_DateTime
        {
            get { return _Comment_DateTime; }
            set { _Comment_DateTime = value; }
        }

        public string User_Profile
        {
            get { return _User_Profile; }
            set { _User_Profile = value; }
        }

        public string User_Name
        {
            get { return _User_Name; }
            set { _User_Name = value; }
        }

        public void CommentInsert()
        {

            string queryStr = "INSERT INTO Comment(Text, User_ID, Post_ID, Comment_DateTime) " +
                  "OUTPUT INSERTED.Comment_ID " +  // return the new Comment_ID
                  "VALUES (@Text, @User_ID, @Post_ID, @Comment_DateTime)";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.Parameters.AddWithValue("@Text", this.Text);
            cmd.Parameters.AddWithValue("@User_ID", this.User_ID);
            cmd.Parameters.AddWithValue("@Post_ID", this.Post_ID);

            DateTime currentDateTime = new DateTime(2025, 6, 15, 13, 45, 0);
            cmd.Parameters.AddWithValue("@Comment_DateTime", currentDateTime);

            conn.Open();
            string comment_id = cmd.ExecuteScalar().ToString();
            conn.Close();

            // insert notification
            User_Post user_post = new User_Post();
            string user_id = user_post.GetPostUserID(this.Post_ID);

            if (user_id != this.User_ID)
            {
                User_Notification user_notification = new User_Notification("Comment", comment_id, this.Post_ID);
                user_notification.NotificationInsert();
            }

        }


        public List<User_Comment> GetCommentsByPostID(string post_id)
        {
            string comment_id, text, user_id, comment_datetime, profile, name;
            List<User_Comment> comment_list = new List<User_Comment>();

            string queryStr = "SELECT * FROM Comment c LEFT OUTER JOIN [User] u ON c.User_ID = u.Id WHERE c.Post_ID = @Post_ID ORDER BY c.Comment_ID DESC";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@Post_ID", post_id);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                comment_id = dr["Comment_ID"].ToString();
                text = dr["Text"].ToString();
                user_id = dr["User_ID"].ToString();
                comment_datetime = dr["Comment_DateTime"].ToString();
                profile = dr["Profile"].ToString();
                name = dr["Name"].ToString();

                User_Comment user_comment = new User_Comment(comment_id, text, user_id, post_id, comment_datetime, profile, name);
                comment_list.Add(user_comment);
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return comment_list;
        }

        public int CommentDelete(string comment_id)
        {
            string queryStr = "DELETE FROM Comment WHERE Comment_ID = @Comment_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@Comment_ID", comment_id);
        
            conn.Open();
            int nofRow = 0;
            nofRow = cmd.ExecuteNonQuery();
            conn.Close();

            User_Notification user_notification = new User_Notification();
            user_notification.NotificationDelete("Comment", comment_id);

            return nofRow;
        }
    }
}