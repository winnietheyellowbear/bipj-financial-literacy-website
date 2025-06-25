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
        private string _Company_Logo;
        private string _Company_Name;
        private string _Description;
        private string _Expiry_Date;
        private string _User_ID;

        public User_Voucher()
        {
        }

        public string User_Voucher_ID
        {
            get { return _User_Voucher_ID; }
            set { _User_Voucher_ID = value; }
        }

        public string Company_Logo
        {
            get { return _Company_Logo; }
            set { _Company_Logo = value; }
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

            string queryStr = "INSERT INTO User_Voucher(User_Voucher_ID, Company_Logo, Company_Name, Description, Expiry_Date, User_ID)"
                            + "VALUES (@User_Voucher_ID, @Company_Logo, @Company_Name, @Description, @Expiry_Date, @User_ID)";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.Parameters.AddWithValue("@User_Voucher_ID", this.User_Voucher_ID);
            cmd.Parameters.AddWithValue("@Company_Logo", this.Company_Logo);
            cmd.Parameters.AddWithValue("@Company_Name", this.Company_Name);
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
            string queryStr = "UPDATE User SET" +
                            " Point = @Point " +
                            " WHERE User_ID = @User_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@User_ID", user_id);
            cmd.Parameters.AddWithValue("@Point", point);
           
            conn.Open();
            int nofRow = 0;
            nofRow = cmd.ExecuteNonQuery();
            conn.Close();

        }
    }
}