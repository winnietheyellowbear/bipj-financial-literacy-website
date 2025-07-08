using System;

namespace bipj
{
    public partial class Booking4 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // nothing to pre-load
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            // go back to step 3
            Response.Redirect("Booking3.aspx");
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // save into Session for later steps
            Session["BookingName"] = txtName.Text.Trim();
            Session["BookingEmail"] = txtEmail.Text.Trim();
            Session["BookingFocus"] = txtFocus.Text.Trim();

            Response.Redirect("Booking5.aspx");
        }
    }
}
