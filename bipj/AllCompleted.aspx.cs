using System;

namespace bipj
{
    public partial class AllCompleted : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnStartHere_Click(object sender, EventArgs e)
        {
            // Redirect to AllTopics or wherever you want users to start
            Response.Redirect("AllTopics.aspx");
        }
    }
}
