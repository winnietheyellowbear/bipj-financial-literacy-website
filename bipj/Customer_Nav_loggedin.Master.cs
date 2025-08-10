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

            ScriptManager.ScriptResourceMapping.AddDefinition("jquery",
                new ScriptResourceDefinition
                {
                    Path = "https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js",
                    DebugPath = "https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.js",
                    CdnPath = "https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js",
                    CdnDebugPath = "https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.js"
                });

            if (Session["UserType"] != null)
            {
                user_type = Session["UserType"].ToString();

                if (user_type == "Staff")
                {
                    Panel1.Visible = true;
                }
                
                Panel2.Visible = true;
            }
            else
            {
                Panel3.Visible = true;
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