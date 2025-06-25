using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace bipj
{
    public class User_Voucher
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private string _User_Voucher_ID;
        private string _Staff_Voucher_ID;
        private string _Company_Name;
        private string _Description;
        private string _Expiry_Date;
        private string _User_ID;

        public User_Voucher()
        {
        }

        public User_Voucher(string staff_voucher_id, string description, string expiry_date, string user_id)
        {
            Staff_Voucher_ID = staff_voucher_id;
            Description = description;
            Expiry_Date = expiry_date;
            User_ID = user_id;
        }

        public string User_Voucher_ID
        {
            get { return _User_Voucher_ID; }
            set { _User_Voucher_ID = value; }
        }

        public string Staff_Voucher_ID
        {
            get { return _Staff_Voucher_ID; }
            set { _Staff_Voucher_ID = value; }
        }

        public string Company_Name
        {
            get { return _Company_Name; }
            set { _Company_Name = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        public string Expiry_Date
        {
            get { return _Expiry_Date; }
            set { _Expiry_Date = value; }
        }
        public string User_ID
        {
            get { return _User_ID; }
            set { _User_ID = value; }
        }


        public int VoucherInsert()
        {
            int result = 0;

            string queryStr = "INSERT INTO User_Voucher(Staff_Voucher_ID, Description, Expiry_Date, User_ID)"
                            + "VALUES (@Staff_Voucher_ID, @Description, @Expiry_Date, @User_ID)";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.Parameters.AddWithValue("@Staff_Voucher_ID", this.Staff_Voucher_ID);
            cmd.Parameters.AddWithValue("@Description", this.Description);
            cmd.Parameters.AddWithValue("@Expiry_Date", this.Expiry_Date);
            cmd.Parameters.AddWithValue("@User_ID", this.User_ID);

            conn.Open();
            result += cmd.ExecuteNonQuery();
            conn.Close();

            return result;
        }

        public void PointUpdate(string user_id, int point)
        {
            string queryStr = "UPDATE [User] SET" +
                            " Point = @Point " +
                            " WHERE Id = @User_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@User_ID", user_id);
            cmd.Parameters.AddWithValue("@Point", point);
           
            conn.Open();
            int nofRow = 0;
            nofRow = cmd.ExecuteNonQuery();
            conn.Close();

        }

        public int GetUserPoint(string user_id)
        {
            int user_points = 0;

            string queryStr1 = "SELECT * FROM [User] WHERE Id = @User_ID";

            SqlConnection conn1 = new SqlConnection(_connStr);
            SqlCommand cmd1 = new SqlCommand(queryStr1, conn1);
            cmd1.Parameters.AddWithValue("@User_ID", user_id);

            conn1.Open();
            SqlDataReader dr1 = cmd1.ExecuteReader();

            while (dr1.Read())
            {
                user_points = int.Parse(dr1["Point"].ToString());
            }

            conn1.Close();
            dr1.Close();
            dr1.Dispose();

            return user_points;
        }

    }
}