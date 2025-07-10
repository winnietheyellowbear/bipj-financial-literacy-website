using System;
using System.Web.UI;

namespace bipj
{
    public partial class Booking1 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // nothing needed here
        }

        /// <summary>
        /// Both card buttons call this handler.
        /// We pull the CommandArgument ("Individual" or "Group"),
        /// store it, and redirect to Step 2.
        /// </summary>
        protected void btnSelect_Click(object sender, EventArgs e)
        {
            var button = (System.Web.UI.WebControls.Button)sender;
            var sessionType = button.CommandArgument;

            // Save choice in Session
            Session["BookingType"] = sessionType;

            // Move on to Booking2.aspx
            Response.Redirect("Booking2.aspx");
        }
    }
}
