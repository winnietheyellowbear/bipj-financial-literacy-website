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

    }
}