using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
            if (Panel2.Visible) // logged in
            {
                imgNavProfile.ImageUrl = GetUserProfileImgUrl(Session["UserId"]);
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
        private string GetUserProfileImgUrl(object userIdObj)
        {
            var fallback = ResolveUrl("~/images/profile_default.png");
            if (userIdObj == null) return fallback;

            string url = null;
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString))
            using (var cmd = new SqlCommand("SELECT Profile FROM [User] WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", Convert.ToInt32(userIdObj));
                conn.Open();
                var o = cmd.ExecuteScalar();
                url = o == null ? null : o.ToString();
            }
            if (string.IsNullOrWhiteSpace(url)) return fallback;

            // Normalize (DB might store "Profileuploads/..." without leading ~/)
            if (url.StartsWith("~/") || url.StartsWith("/")) return ResolveUrl(url) + "?v=" + Guid.NewGuid().ToString("N");
            return ResolveUrl("~/" + url) + "?v=" + Guid.NewGuid().ToString("N");
        }
    }
}