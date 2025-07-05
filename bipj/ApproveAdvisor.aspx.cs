using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Mail;
using System.Text;


namespace bipj
{
    public partial class ApproveAdvisor : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                BindPendingAdvisors();
        }

        private void BindPendingAdvisors()
        {
            // Fetch all advisors with Status = 0 (pending)
            var pending = Advisor.GetByStatus(0);
            rptPending.DataSource = pending;
            rptPending.DataBind();
        }

        // Approve: set Status = 1
        protected void btnApprove_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int advisorId = int.Parse(btn.CommandArgument);

            // 1) mark as approved
            Advisor.UpdateStatus(advisorId, 1);

            // 2) send the “approved” email
            var adv = Advisor.GetById(advisorId);
            if (adv != null)
            {
                try
                {
                    SendApprovalEmail(adv.Email, adv.Name);
                }
                catch (Exception ex)
                {
                    // TODO: log failure (ex.Message)
                }
            }

            // 3) rebind grid
            BindPendingAdvisors();
        }

        // Delete: remove advisor record entirely
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int id = int.Parse(btn.CommandArgument);
            var adv = Advisor.GetById(id);
            if (adv != null)
                adv.Delete();
            BindPendingAdvisors();
        }

        // View: redirect to a placeholder view page
        protected void btnView_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int id = int.Parse(btn.CommandArgument);
            Response.Redirect($"ViewAdvisor.aspx?advisorId={id}");
        }

        #region Helpers

        private void SendApprovalEmail(string toEmail, string advisorName)
        {
            var msg = new MailMessage();
            msg.To.Add(toEmail);
            msg.Subject = "🎉 Your FinClarity Advisor Application Has Been Approved!";
            msg.IsBodyHtml = true;

            var sb = new StringBuilder();
            sb.AppendLine($"<p>Dear {advisorName},</p>");
            sb.AppendLine("<p>Congratulations! Your application to become an advisor on <strong>FinClarity</strong> has been approved.</p>");
            sb.AppendLine("<p>You can now set your availability, and start receiving client booking requests.</p>");
            sb.AppendLine("<p>If you have any questions, feel free to reply to this email or contact support at <a href=\"mailto:support@finclarity.com\">support@finclarity.com</a>.</p>");
            sb.AppendLine("<p>Welcome aboard and happy advising!</p>");
            sb.AppendLine("<br/><p>— The FinClarity Team</p>");

            msg.Body = sb.ToString();
            using (var smtp = new SmtpClient())
            {
                smtp.Send(msg);
            }
        }

        #endregion

        // Renders ★★½☆☆ etc.
        public string GenerateStars(decimal rating)
        {
            int full = (int)Math.Floor(rating);
            bool half = rating - full >= 0.5m;
            int empty = 5 - full - (half ? 1 : 0);

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < full; i++) sb.Append("<i class='fas fa-star'></i>");
            if (half) sb.Append("<i class='fas fa-star-half-alt'></i>");
            for (int i = 0; i < empty; i++) sb.Append("<i class='far fa-star'></i>");
            return sb.ToString();
        }

        // Emits <li>Specialty</li> for each non-null specialty
        public string GetSpecialtiesList(object s1, object s2, object s3)
        {
            var items = new List<string>();
            if (s1 != null) items.Add($"<li>{s1}</li>");
            if (s2 != null) items.Add($"<li>{s2}</li>");
            if (s3 != null) items.Add($"<li>{s3}</li>");
            return string.Join("", items);
        }
    }
}