using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class Staff_Nav : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Toggle login/logout UI
            bool loggedIn = (Session["UserId"] != null);
            pnlLoggedIn.Visible = loggedIn;
            pnlLoggedOut.Visible = !loggedIn;

            if (loggedIn && !IsPostBack)
            {
                imgStaffProfile.ImageUrl = GetUserProfileImgUrl(Session["UserId"]);
            }
        }

        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            // Clear session just like your customer master
            Session["UserId"] = null;
            Session["UserName"] = null;
            Session["UserType"] = null;
            Session["UserEmail"] = null;

            // Optional: if you ever switch to FormsAuth, also call FormsAuthentication.SignOut();
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

            // Normalize + cache-bust
            if (url.StartsWith("~/") || url.StartsWith("/"))
                return ResolveUrl(url) + "?v=" + Guid.NewGuid().ToString("N");

            return ResolveUrl("~/" + url) + "?v=" + Guid.NewGuid().ToString("N");
        }
    }
}