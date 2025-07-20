using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class ProfileQR : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Loginpage.aspx");
                return;
            }

            if (!IsPostBack)
            {
                int qrUserId = Convert.ToInt32(Request.QueryString["userId"]);
                int currentUserId = Convert.ToInt32(Session["UserId"]);

                if (qrUserId == currentUserId)
                {
                    // Load editable profile
                    Response.Redirect("Profile.aspx");
                }
                else
                {
                    // Load view-only profile
                    Response.Redirect("ViewSpecificProfile.aspx?userId=" + qrUserId);
                }
            }
        }
    }
}