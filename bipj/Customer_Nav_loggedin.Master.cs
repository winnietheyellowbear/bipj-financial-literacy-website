using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class Customer_Nav_loggedin : System.Web.UI.MasterPage
    {
        public string user_type;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserType"] != null)
            {
                user_type = Session["UserType"].ToString();
            }
        }
        protected void btn_sign_out_Click(object sender, EventArgs e)
        {
            Session["UserId"] = null;
            Session["UserName"] = null;
            Session["UserType"] = null;
            Session["UserEmail"] = null;

            Response.Redirect("Loginpage.aspx");
        }
    }
}