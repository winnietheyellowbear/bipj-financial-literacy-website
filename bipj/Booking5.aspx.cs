using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using GemBox.Email.Calendar;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace bipj
{
    public partial class Booking5 : System.Web.UI.Page
    {
        // these back fields get populated in Page_Load
        protected string Type, AdvisorName, AdvisorCategory, DateTimeSlot, ClientName, ClientEmail, Focus;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Always populate fields from session, not just on first load
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
                // Render literals on the page (only on first load)
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

                // 2) Email to client (with .ics attachment)
                try
                {
                    SendClientEmail(
                        toEmail: ClientEmail,
                        clientName: ClientName,
                        advisorName: AdvisorName,
                        advisorCategory: AdvisorCategory,
                        sessionType: Type,
                        dateTime: DateTimeSlot,
                        focus: Focus,
                        bookingDateTime: booking.BookingDateTime
                    );
                }
                catch (Exception ex)
                {
                    // TODO: log ex.Message
                }

                // 3) Email to advisor (with .ics attachment)
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
                            focus: Focus,
                            bookingDateTime: booking.BookingDateTime
                        );
                    }
                    catch (Exception ex)
                    {
                        // TODO: log ex.Message
                    }
                }

                // 4) Create Google Calendar event (for record-keeping only)
                try
                {
                    CreateGoogleCalendarEvent(
                        clientName: ClientName,
                        clientEmail: ClientEmail,
                        advisorName: AdvisorName,
                        advisorEmail: advEmail,
                        sessionType: Type,
                        dateTime: booking.BookingDateTime,
                        focus: Focus
                    );
                }
                catch (Exception ex)
                {
                    // TODO: log ex.Message - calendar creation failed but booking still confirmed
                }

                // 5) UI feedback
                btnConfirm.Visible = false;
                ClientScript.RegisterStartupScript(
                    GetType(),
                    "confirmation",
                    $"alert('Your session is confirmed! Booking ID: {bookingId}. Calendar invite sent! Please check your email for details.');",
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

        #region Google Calendar Integration

        private void CreateGoogleCalendarEvent(
            string clientName,
            string clientEmail,
            string advisorName,
            string advisorEmail,
            string sessionType,
            DateTime dateTime,
            string focus)
        {
            try
            {
                // 1) Load service account credentials
                var serviceAcctPath = Server.MapPath("~/App_Data/google-service-account.json");

                if (!File.Exists(serviceAcctPath))
                {
                    throw new FileNotFoundException("Google service account credentials not found");
                }

                GoogleCredential credential;
                using (var stream = new FileStream(serviceAcctPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(CalendarService.Scope.Calendar);
                }

                // 2) Build the Calendar service
                var calendarService = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "FinClarity Booking System"
                });

                // 3) Create the event (Note: Service accounts cannot send invites)
                // FIX: Explicitly specify which Event class to use
                var eventObj = new Google.Apis.Calendar.v3.Data.Event()
                {
                    Summary = $"FinClarity {sessionType} Session",
                    Description = BuildEventDescription(clientName, advisorName, sessionType, focus),
                    Start = new EventDateTime()
                    {
                        DateTime = dateTime,
                        TimeZone = "Asia/Singapore"
                    },
                    End = new EventDateTime()
                    {
                        DateTime = dateTime.AddHours(1), // Assuming 1-hour sessions
                        TimeZone = "Asia/Singapore"
                    },
                    Location = "FinClarity Office",

                    // Add attendees for record-keeping (invites won't be sent by service account)
                    Attendees = new List<EventAttendee>()
                    {
                        new EventAttendee()
                        {
                            Email = clientEmail,
                            DisplayName = clientName,
                            ResponseStatus = "needsAction"
                        }
                    },

                    // Add reminders
                    Reminders = new Google.Apis.Calendar.v3.Data.Event.RemindersData()
                    {
                        UseDefault = false,
                        Overrides = new List<EventReminder>()
                        {
                            new EventReminder() { Method = "email", Minutes = 24 * 60 }, // 24 hours
                            new EventReminder() { Method = "popup", Minutes = 30 }      // 30 minutes
                        }
                    }
                };

                // If advisor email is available, add them as attendee too
                if (!string.IsNullOrEmpty(advisorEmail))
                {
                    eventObj.Attendees.Add(new EventAttendee()
                    {
                        Email = advisorEmail,
                        DisplayName = advisorName,
                        ResponseStatus = "accepted"
                    });
                }

                // 4) Insert event into calendar
                var request = calendarService.Events.Insert(eventObj, "48cb45ce9d2a750fe1ff6f88e59818ee22f2a1fb6c01c81a8dd396c9b4ff7525@group.calendar.google.com");
                // Note: SendUpdates won't work for service accounts
                request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.None;

                var createdEvent = request.Execute();

                // TODO: Optionally save the Google Calendar event ID to your database
                // for future reference (e.g., if you need to update/cancel the event)
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the booking process
                // TODO: Implement proper logging
                throw new Exception($"Failed to create Google Calendar event: {ex.Message}", ex);
            }
        }

        private string BuildEventDescription(string clientName, string advisorName, string sessionType, string focus)
        {
            var description = new StringBuilder();
            description.AppendLine($"FinClarity {sessionType} Session");
            description.AppendLine();
            description.AppendLine($"Client: {clientName}");
            description.AppendLine($"Advisor: {advisorName}");

            if (!string.IsNullOrEmpty(focus))
            {
                description.AppendLine($"Focus: {focus}");
            }

            description.AppendLine();
            description.AppendLine("Generated by FinClarity Booking System");

            return description.ToString();
        }

        #endregion

        #region Email Helpers

        private DateTime ParseDateTime(string dateStr, string timeStr)
        {
            try
            {
                if (DateTime.TryParse(dateStr, out DateTime date))
                {
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

            return DateTime.Now;
        }

        private void SendClientEmail(
            string toEmail,
            string clientName,
            string advisorName,
            string advisorCategory,
            string sessionType,
            string dateTime,
            string focus,
            DateTime bookingDateTime)
        {
            var msg = new MailMessage
            {
                From = new MailAddress("no-reply@yourdomain.com", "FinClarity Team"),
                Subject = "🎉 Your FinClarity Session Is Confirmed!",
                IsBodyHtml = true
            };
            msg.To.Add(toEmail);

            // 📅 HTML Email Body
            var sb = new StringBuilder();
            sb.AppendLine($"<p>Hi {clientName},</p>");
            sb.AppendLine($"<p>Your <strong>{sessionType}</strong> session with <strong>{advisorName}</strong> ({advisorCategory}) has been confirmed.</p>");
            sb.AppendLine($"<p><strong>Date &amp; Time:</strong> {dateTime}</p>");
            if (!string.IsNullOrEmpty(focus))
                sb.AppendLine($"<p><strong>Focus:</strong> {focus}</p>");
            sb.AppendLine("<p>📎 <strong>Calendar Invite:</strong> We've attached a calendar invitation file (.ics). Click on it to add this session to your calendar (works with Google Calendar, Outlook, Apple Calendar, etc.).</p>");
            sb.AppendLine("<p>We look forward to seeing you!</p>");
            sb.AppendLine("<br/><p>— The FinClarity Team</p>");

            msg.Body = sb.ToString();

            // 🗓️ Generate and attach ICS file (Calendar Invite)
            try
            {
                string calendarText = BuildIcsContent(clientName, advisorName, advisorCategory, sessionType, focus, bookingDateTime);
                var calendarBytes = Encoding.UTF8.GetBytes(calendarText);
                var calendarStream = new MemoryStream(calendarBytes);

                var attachment = new Attachment(calendarStream, "FinClarity-Session.ics", "text/calendar");
                msg.Attachments.Add(attachment);
            }
            catch (Exception ex)
            {
                // TODO: log ex.Message - continue without attachment if ICS generation fails
            }

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
            string focus,
            DateTime bookingDateTime)
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
            sb.AppendLine("<p>📎 <strong>Calendar:</strong> This appointment has been added to the FinClarity calendar. A calendar invite (.ics file) is attached for your convenience.</p>");
            sb.AppendLine("<p>Please prepare accordingly.</p>");
            sb.AppendLine("<br/><p>— The FinClarity Team</p>");

            msg.Body = sb.ToString();

            // 🗓️ Generate and attach ICS file (Calendar Invite)
            try
            {
                string calendarText = BuildIcsContent(clientName, advisorName, advisorCategory, sessionType, focus, bookingDateTime);
                var calendarBytes = Encoding.UTF8.GetBytes(calendarText);
                var calendarStream = new MemoryStream(calendarBytes);

                var attachment = new Attachment(calendarStream, "FinClarity-Session.ics", "text/calendar");
                msg.Attachments.Add(attachment);
            }
            catch (Exception ex)
            {
                // TODO: log ex.Message - continue without attachment if ICS generation fails
            }

            using (var smtp = new SmtpClient()) smtp.Send(msg);
        }

        /// <summary>
        /// Generates an ICS (iCalendar) file content for calendar invites
        /// </summary>
        private string BuildIcsContent(string clientName, string advisorName, string advisorCategory, string sessionType, string focus, DateTime bookingDateTime)
        {
            // Convert to UTC for ICS format
            string start = bookingDateTime.ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
            string end = bookingDateTime.AddHours(1).ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
            string created = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//FinClarity//Booking System//EN");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("METHOD:REQUEST");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{Guid.NewGuid()}@finclarity.com");
            sb.AppendLine($"DTSTAMP:{created}");
            sb.AppendLine($"DTSTART:{start}");
            sb.AppendLine($"DTEND:{end}");
            sb.AppendLine($"SUMMARY:FinClarity {sessionType} Session");

            // Build description
            var description = new StringBuilder();
            description.Append($"Session with {advisorName} ({advisorCategory})");
            if (!string.IsNullOrEmpty(focus))
                description.Append($"\\nFocus: {focus}");
            description.Append($"\\nClient: {clientName}");
            description.Append("\\nGenerated by FinClarity Booking System");

            sb.AppendLine($"DESCRIPTION:{description.ToString()}");
            sb.AppendLine("LOCATION:FinClarity Office");
            sb.AppendLine("STATUS:CONFIRMED");
            sb.AppendLine("SEQUENCE:0");
            sb.AppendLine("PRIORITY:5");
            sb.AppendLine("CLASS:PUBLIC");
            sb.AppendLine("TRANSP:OPAQUE");

            // Add reminders
            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine("TRIGGER:-PT24H");
            sb.AppendLine("REPEAT:0");
            sb.AppendLine("ACTION:EMAIL");
            sb.AppendLine($"SUMMARY:Reminder: FinClarity {sessionType} Session Tomorrow");
            sb.AppendLine($"DESCRIPTION:Don't forget your FinClarity session with {advisorName} tomorrow!");
            sb.AppendLine("END:VALARM");

            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine("TRIGGER:-PT30M");
            sb.AppendLine("REPEAT:0");
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine($"SUMMARY:FinClarity {sessionType} Session in 30 minutes");
            sb.AppendLine("END:VALARM");

            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        #endregion
    }
}