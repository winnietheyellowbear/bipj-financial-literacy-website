using System;
using System.Web.UI;

namespace bipj
{
    public partial class Education : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnShowAllTopics_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllTopics.aspx");
        }

        protected void btnViewAllRecent_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllRecent.aspx");
        }

        protected void btnViewAllCompleted_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllCompleted.aspx");
        }
    }
}
