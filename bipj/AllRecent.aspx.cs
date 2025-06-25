using System;

namespace bipj
{
    public partial class AllRecent : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnStartHere_Click(object sender, EventArgs e)
        {
            // Redirect to AllTopics or anywhere else you want
            Response.Redirect("AllTopics.aspx");
        }
    }
}
