using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;



namespace bipj
{
    public class User
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private string _Id;
        private int _Point;
        private string _Type;

        public User()
        {
        }

        // create like
        public User(string id, int point, string type)
        {
            Id = id;
            Point = point;
            Type = type;
        }
      
        public string Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        public int Point
        {
            get { return _Point; }
            set { _Point = value; }
        }

        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        public User GetUserByPostID(string user_id)
        {
            string type;
            int point;
            User user = new User();

            string queryStr = "SELECT * FROM User WHERE Id = @User_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@User_ID", user_id);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                type = dr["Type"].ToString();
                point = int.Parse(dr["Point"].ToString());
                
                user = new User(user_id, point, type);
                
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return user;

        }

    }
}