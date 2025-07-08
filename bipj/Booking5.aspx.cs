using System;
using System.Net.Mail;
using System.Text;

namespace bipj
{
    public partial class Booking5 : System.Web.UI.Page
    {
        // these back fields get populated in Page_Load
        protected string Type, AdvisorName, AdvisorCategory, DateTimeSlot, ClientName, ClientEmail, Focus;

        protected void Page_Load(object sender, EventArgs e)
        {
            // FIXED: Always populate fields from session, not just on first load
            // 1) Pull everything from Session
            Type = Session["BookingType"]?.ToString() ?? "";
            AdvisorName = Session["BookingAdvisorName"]?.ToString() ?? "";
            AdvisorCategory = Session["BookingAdvisorCategory"]?.ToString() ?? "";
            var date = Session["bookingDate"]?.ToString() ?? "";
            var time = Session["bookingTime"]?.ToString() ?? "";
            ClientName = Session["BookingName"]?.ToString() ?? "";
            ClientEmail = Session["BookingEmail"]?.ToString() ?? "";
            Focus = Session["BookingFocus"]?.ToString() ?? "";
            DateTimeSlot = $"{date}, {time}";

            if (!IsPostBack)
            {
                // 2) Render literals on the page (only on first load)
                litType.Text = Type;
                litAdvisor.Text = $"{AdvisorName} ({AdvisorCategory})";
                litDateTime.Text = DateTimeSlot;
                litName.Text = ClientName;
                litEmail.Text = ClientEmail;
                litFocus.Text = Focus;
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("Booking4.aspx");
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) Save booking to database
                var booking = new Models.Booking
                {
                    UserId = Convert.ToInt32(Session["BookingUserId"] ?? Session["UserId"] ?? "0"),
                    AdvisorId = Convert.ToInt32(Session["BookingAdvisorId"] ?? "0"),
                    SessionType = Type,
                    BookingDateTime = ParseDateTime(Session["bookingDate"]?.ToString(), Session["bookingTime"]?.ToString()),
                    Focus = Focus,
                    Status = 1 // 1 = Confirmed
                };

                int bookingId = booking.Insert();

                // 2) Email to client
                try
                {
                    SendClientEmail(
                        toEmail: ClientEmail,
                        clientName: ClientName,
                        advisorName: AdvisorName,
                        advisorCategory: AdvisorCategory,
                        sessionType: Type,
                        dateTime: DateTimeSlot,
                        focus: Focus
                    );
                }
                catch (Exception ex)
                {
                    // TODO: log ex.Message
                }

                // 3) Email to advisor
                var advEmail = Session["BookingAdvisorEmail"]?.ToString();
                if (!string.IsNullOrEmpty(advEmail))
                {
                    try
                    {
                        SendAdvisorEmail(
                            toEmail: advEmail,
                            advisorName: AdvisorName,
                            clientName: ClientName,
                            clientEmail: ClientEmail,
                            advisorCategory: AdvisorCategory,
                            sessionType: Type,
                            dateTime: DateTimeSlot,
                            focus: Focus
                        );
                    }
                    catch (Exception ex)
                    {
                        // TODO: log ex.Message
                    }
                }

                // 4) UI feedback
                btnConfirm.Visible = false;
                ClientScript.RegisterStartupScript(
                    GetType(),
                    "confirmation",
                    $"alert('Your session is confirmed! Booking ID: {bookingId}. Please check your email for details.');",
                    true
                );
            }
            catch (Exception ex)
            {
                // Handle database save error
                ClientScript.RegisterStartupScript(
                    GetType(),
                    "error",
                    "alert('There was an error confirming your booking. Please try again.');",
                    true
                );
                // TODO: log ex.Message
            }
        }

        #region Email Helpers

        private DateTime ParseDateTime(string dateStr, string timeStr)
        {
            try
            {
                // Parse date (expected format: "2025-07-25" or similar)
                if (DateTime.TryParse(dateStr, out DateTime date))
                {
                    // Parse time (expected format: "13:00" or similar)
                    if (TimeSpan.TryParse(timeStr, out TimeSpan time))
                    {
                        return date.Date.Add(time);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: log ex.Message
            }

            // Fallback to current date/time if parsing fails
            return DateTime.Now;
        }

        private void SendClientEmail(
            string toEmail,
            string clientName,
            string advisorName,
            string advisorCategory,
            string sessionType,
            string dateTime,
            string focus)
        {
            var msg = new MailMessage
            {
                From = new MailAddress("no-reply@yourdomain.com", "FinClarity Team"),
                Subject = "🎉 Your FinClarity Session Is Confirmed!",
                IsBodyHtml = true
            };
            msg.To.Add(toEmail);

            var sb = new StringBuilder();
            sb.AppendLine($"<p>Hi {clientName},</p>");
            sb.AppendLine($"<p>Your <strong>{sessionType}</strong> session with <strong>{advisorName}</strong> ({advisorCategory}) has been confirmed.</p>");
            sb.AppendLine($"<p><strong>Date &amp; Time:</strong> {dateTime}</p>");
            if (!string.IsNullOrEmpty(focus))
                sb.AppendLine($"<p><strong>Focus:</strong> {focus}</p>");
            sb.AppendLine("<p>We look forward to seeing you!</p>");
            sb.AppendLine("<br/><p>— The FinClarity Team</p>");

            msg.Body = sb.ToString();
            using (var smtp = new SmtpClient()) smtp.Send(msg);
        }

        private void SendAdvisorEmail(
            string toEmail,
            string advisorName,
            string clientName,
            string clientEmail,
            string advisorCategory,
            string sessionType,
            string dateTime,
            string focus)
        {
            var msg = new MailMessage
            {
                From = new MailAddress("no-reply@yourdomain.com", "FinClarity Team"),
                Subject = "📅 New Booking on FinClarity",
                IsBodyHtml = true
            };
            msg.To.Add(toEmail);

            var sb = new StringBuilder();
            sb.AppendLine($"<p>Hi {advisorName},</p>");
            sb.AppendLine($"<p>You have a new <strong>{sessionType}</strong> booking ({advisorCategory}):</p>");
            sb.AppendLine("<ul>");
            sb.AppendLine($"<li><strong>Client:</strong> {clientName} ({clientEmail})</li>");
            sb.AppendLine($"<li><strong>Date &amp; Time:</strong> {dateTime}</li>");
            if (!string.IsNullOrEmpty(focus))
                sb.AppendLine($"<li><strong>Focus:</strong> {focus}</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("<p>Please prepare accordingly.</p>");
            sb.AppendLine("<br/><p>— The FinClarity Team</p>");

            msg.Body = sb.ToString();
            using (var smtp = new SmtpClient()) smtp.Send(msg);
        }

        #endregion
    }
}