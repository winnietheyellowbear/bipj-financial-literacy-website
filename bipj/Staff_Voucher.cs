using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;


namespace bipj
{
    public class Staff_Voucher
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private string _Voucher_ID;
        private string _Company_Logo;
        private string _Company_Name;
        private string _Description;
        private string _Discount;
        private string _Validity;
        private int _Points_Required;

        public Staff_Voucher()
        {
        }

        // create voucher
        public Staff_Voucher(string company_logo, string company_name, string description, string validity, int points_required)
        {
            Company_Logo = company_logo;
            Company_Name = company_name;
            Description = description;
            Validity = validity;
            Points_Required = points_required;
        }

        // retrieve voucher
        public Staff_Voucher(string voucher_id, string company_logo, string company_name, string description, string validity, int points_required)
        {
            Voucher_ID = voucher_id;
            Company_Logo = company_logo;
            Company_Name = company_name;
            Description = description;
            Validity = validity;
            Points_Required = points_required;
        }

        public string Voucher_ID
        {
            get { return _Voucher_ID; }
            set { _Voucher_ID = value; }
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

        public string Validity
        {
            get { return _Validity; }
            set { _Validity = value; }
        }
        public int Points_Required
        {
            get { return _Points_Required; }
            set { _Points_Required = value; }
        }

        public int VoucherInsert()
        {
            int result = 0;

            string queryStr = "INSERT INTO Staff_Voucher(Company_Logo, Company_Name, Description, Validity, Points_Required)"
                            + "VALUES (@Company_Logo, @Company_Name, @Description, @Validity, @Points_Required)";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.Parameters.AddWithValue("@Company_Logo", this.Company_Logo);
            cmd.Parameters.AddWithValue("@Company_Name", this.Company_Name);
            cmd.Parameters.AddWithValue("@Description", this.Description);
            cmd.Parameters.AddWithValue("@Validity", this.Validity);
            cmd.Parameters.AddWithValue("@Points_Required", this.Points_Required);

            conn.Open();
            result += cmd.ExecuteNonQuery();
            conn.Close();

            return result;
        }

        public List<Staff_Voucher> GetAllVouchers()
        {
            string voucher_id, company_logo, company_name, description, validity;
            int points_required;
            List<Staff_Voucher> voucher_list = new List<Staff_Voucher>();

            string queryStr = "SELECT * FROM Staff_Voucher";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
           
            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                voucher_id = dr["Voucher_ID"].ToString();
                company_logo = dr["Company_Logo"].ToString();
                company_name = dr["Company_Name"].ToString();
                description = dr["Description"].ToString();
                validity = dr["Validity"].ToString();
                points_required = int.Parse(dr["Points_Required"].ToString());

                Staff_Voucher staff_voucher = new Staff_Voucher(voucher_id, company_logo, company_name, description, validity, points_required);
                voucher_list.Add(staff_voucher);
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return voucher_list;
        }

        public (bool isPointEnough, int userPoints, int pointsRequired) IsPointEnough(string voucher_id, string user_id)
        {
            int user_points = 0;

            Staff_Voucher staff_voucher = new Staff_Voucher();
            staff_voucher = staff_voucher.GetVoucherByVoucherID(voucher_id);

            // retrieve user details
            User_Voucher user_voucher = new User_Voucher();
            user_points = user_voucher.GetUserPoint(user_id);

            if (user_points > staff_voucher.Points_Required)
            {
                return (true, user_points, staff_voucher.Points_Required);
            }
            else
            {
                return (false, user_points, staff_voucher.Points_Required);
            }
        }


        public Staff_Voucher GetVoucherByVoucherID(string voucher_id)
        {
            string company_logo, company_name, description, validity;
            int points_required;

            Staff_Voucher staff_voucher = new Staff_Voucher();   

            string queryStr = "SELECT * FROM Staff_Voucher WHERE Voucher_ID = @Voucher_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@Voucher_ID", voucher_id);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                company_logo = dr["Company_Logo"].ToString();
                company_name = dr["Company_Name"].ToString();
                description = dr["Description"].ToString();
                validity = dr["Validity"].ToString();
                points_required = int.Parse(dr["Points_Required"].ToString());

                staff_voucher = new Staff_Voucher(voucher_id, company_logo, company_name, description, validity, points_required);
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return staff_voucher;
        }
    }
}