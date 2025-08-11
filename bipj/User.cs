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
        private string _Phone_Number;

        public User()
        {
        }

      
        public string Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        public string Phone_Number
        {
            get { return _Phone_Number; }
            set { _Phone_Number = value; }
        }

        public List<string> GetUsersPhoneNumber()
        {
            List<string> phone_number_list = new List<string>();
           
            string queryStr = "SELECT * FROM [User]";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                if(dr["PhoneNumber"] != null)
                {
                    phone_number_list.Add(dr["PhoneNumber"].ToString());
                }
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return phone_number_list;
        }

    }
}