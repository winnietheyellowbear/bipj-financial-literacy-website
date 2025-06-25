using System;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class AllTopics : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnViewDetails_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            string topic = btn?.CommandArgument;

            switch (topic)
            {
                case "Budgeting":
                    Response.Redirect("TopicBudgeting.aspx");
                    break;
                case "Investing":
                    Response.Redirect("TopicInvesting.aspx");
                    break;
                case "Debt":
                    Response.Redirect("TopicDebt.aspx");
                    break;
                case "Tax":
                    Response.Redirect("TopicTax.aspx");
                    break;
                case "Credit":
                    Response.Redirect("TopicCredit.aspx");
                    break;
                case "Risk":
                    Response.Redirect("TopicRisk.aspx");
                    break;
                case "Retirement":
                    Response.Redirect("TopicRetirement.aspx");
                    break;
                case "Goals":
                    Response.Redirect("TopicGoals.aspx");
                    break;
                default:
                    // Optionally, handle unknown cases
                    Response.Redirect("Education.aspx");
                    break;
            }
        }
    }
}
