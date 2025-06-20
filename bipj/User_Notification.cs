using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace bipj
{
    public class User_Notification
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private string _Notification_ID;
        private string _Action;
        private string _Action_ID;
        private string _Post_ID;
        private string _Status;
        private string _User_Name;
        private string _User_Profile;
        private string _Text;
        private string _DateTime;

        public User_Notification()
        {
        }

        // create notification
        public User_Notification(string action, string action_id, string post_id)
        {
            _Action = action;
            _Action_ID = action_id;
            _Post_ID = post_id;
        }

        // retrieve user notification
        public User_Notification(string notification_id, string action, string action_id, string post_id, string status, string name, string profile, string text, string datetime)
        {
            _Notification_ID = notification_id;
            _Action = action;
            _Action_ID = action_id;
            _Post_ID = post_id;
            _Status = status;
            User_Name = name;
            User_Profile = profile;
            _Text = text;
            _DateTime = datetime;
        }

        public string Notification_ID
        {
            get { return _Notification_ID; }
            set { _Notification_ID = value; }
        }

        public string Action
        {
            get { return _Action; }
            set { _Action = value; }
        }
        public string Action_ID
        {
            get { return _Action_ID; }
            set { _Action_ID = value; }
        }

        public string Post_ID
        {
            get { return _Post_ID; }
            set { _Post_ID = value; }
        }

        public string Status
        {
            get { return _Status; }
            set { _Status = value; }
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

        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        public string DateTime
        {
            get { return _DateTime; }
            set { _DateTime = value; }
        }


        public int NotificationInsert()
        {
            int result = 0;

            string queryStr = "INSERT INTO Notification(Action, Action_ID, Post_ID, Status)"
                            + "VALUES (@Action, @Action_ID, @Post_ID, @Status)";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.Parameters.AddWithValue("@Action", this.Action);
            cmd.Parameters.AddWithValue("@Action_ID", this.Action_ID);
            cmd.Parameters.AddWithValue("@Post_ID", this.Post_ID);
            cmd.Parameters.AddWithValue("@Status", "Unread");

            conn.Open();
            result += cmd.ExecuteNonQuery();
            conn.Close();

            return result;
        }

        public List<User_Notification> GetNotificationsByUserID(string user_id)
        {
            string notification_id, action, action_id, post_id, status, name, profile, text, datetime;

            List<User_Notification> notification_list = new List<User_Notification>();

            // filter only the post belongs to user
            string queryStr = "SELECT * FROM Notification n " +
                            "LEFT OUTER JOIN Post p ON n.Post_ID = p.Post_ID " +
                            "WHERE p.User_ID = @User_ID " +
                            "ORDER BY Notification_ID DESC";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@User_ID", user_id);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                notification_id = dr["Notification_ID"].ToString();
                action = dr["Action"].ToString();
                action_id = dr["Action_ID"].ToString();
                post_id = dr["Post_ID"].ToString();
                status = dr["Status"].ToString();

                if (action == "Like")
                {
                    string queryStr1 = "SELECT * FROM [Like] l " +
                        "LEFT OUTER JOIN [User] u ON u.Id = l.User_ID " +
                        "WHERE Like_ID = @Action_ID";

                    SqlConnection conn1 = new SqlConnection(_connStr);
                    SqlCommand cmd1 = new SqlCommand(queryStr1, conn1);
                    cmd1.Parameters.AddWithValue("@Action_ID", action_id);

                    conn1.Open();
                    SqlDataReader dr1 = cmd1.ExecuteReader();

                    while (dr1.Read())
                    {
                        name = dr1["Name"].ToString();
                        profile = dr1["Profile"].ToString();
                        text = null;
                        datetime = dr1["Like_DateTime"].ToString();

                        User_Notification user_notification = new User_Notification(notification_id, action, action_id, post_id, status, name, profile, text, datetime);
                        notification_list.Add(user_notification);
                    }

                    conn1.Close();
                    dr1.Close();
                    dr1.Dispose();

                }
                else if (action == "Comment")
                {
                    string queryStr2 = "SELECT * FROM [Comment] c " +
                        "LEFT OUTER JOIN [User] u ON u.Id = c.User_ID " +
                        "WHERE Comment_ID = @Action_ID";

                    SqlConnection conn2 = new SqlConnection(_connStr);
                    SqlCommand cmd2 = new SqlCommand(queryStr2, conn2);
                    cmd2.Parameters.AddWithValue("@Action_ID", action_id);

                    conn2.Open();
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    while (dr2.Read())
                    {
                        name = dr2["Name"].ToString();
                        profile = dr2["Profile"].ToString();
                        text = dr2["Text"].ToString();
                        datetime = dr2["Comment_DateTime"].ToString();

                        User_Notification user_notification = new User_Notification(notification_id, action, action_id, post_id, status, name, profile, text, datetime);
                        notification_list.Add(user_notification);
                    }

                    conn2.Close();
                    dr2.Close();
                    dr2.Dispose();

                }
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return notification_list;
        }

        public void NotificationDelete(string action, string action_id)
        {
            string queryStr = "DELETE FROM Notification WHERE Action = @Action AND Action_ID = @Action_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@Action_ID", action_id);

            conn.Open();
            int nofRow = 0;
            nofRow = cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}