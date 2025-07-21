using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace bipj
{
    public partial class ReminderPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadReminders();
            }
        }

        private void LoadReminders()
        {
            string userEmail = Session["UserEmail"] as string;
            if (string.IsNullOrEmpty(userEmail))
            {
                rptNotifications.DataSource = new List<NotificationItem>
                {
                    new NotificationItem { Title = "Not Logged In", Message = "Please sign in to view your reminders." }
                };
                rptNotifications.DataBind();
                return;
            }

            List<NotificationItem> reminders = new List<NotificationItem>();
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT B.BookingId, B.BookingDateTime, B.Focus
                    FROM Booking B
                    INNER JOIN [User] U ON B.UserId = U.Id
                    WHERE U.Email = @Email
                    ORDER BY B.BookingDateTime DESC;";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", userEmail);
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int bookingId = dr.GetInt32(0);
                            DateTime sessionTime = dr.GetDateTime(1);
                            string focus = dr.IsDBNull(2) ? "N/A" : dr.GetString(2);

                            if (sessionTime > DateTime.Now)
                            {
                                reminders.Add(new NotificationItem
                                {
                                    Title = "📅 Upcoming Session",
                                    Message = $"You have a session booked on {sessionTime:ddd, dd MMM yyyy hh:mm tt}. Topic: {focus}"
                                });
                            }
                            else
                            {
                                reminders.Add(new NotificationItem
                                {
                                    Title = "📝 Review Your Session",
                                    Message = $"You had a session on {sessionTime:ddd, dd MMM yyyy hh:mm tt}. Please leave a review!",
                                    BookingId = bookingId
                                });
                            }
                        }
                    }
                }
            }

            if (reminders.Count == 0)
            {
                reminders.Add(new NotificationItem
                {
                    Title = "✅ No Pending Reminders",
                    Message = "You're all caught up!"
                });
            }

            rptNotifications.DataSource = reminders;
            rptNotifications.DataBind();
        }

        public class NotificationItem
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public int? BookingId { get; set; }
        }
    }
}
