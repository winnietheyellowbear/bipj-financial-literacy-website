// File: Booking.aspx.cs
using System;
using System.Collections.Generic;
using System.Web.Script.Services;
using System.Web.Services;
using bipj;                            // for Advisor
using Net.Pkcs11Interop.HighLevelAPI40;
using BookingModel = bipj.Models.Booking;  // alias to avoid clash with page class

namespace bipj
{
    public partial class Booking : System.Web.UI.Page
    {
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                
                // 1) Load all approved advisors (Status = 1)
                var advisors = Advisor.GetByStatus(1);
                rptAdvisors.DataSource = advisors;
                rptAdvisors.DataBind();
               
            }
        }

        /// <summary>
        /// AJAX endpoint: returns a list of "HH:mm" strings for already‐booked slots.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<string> GetBookedSlots(int advisorId, string date)
        {
            // Parse incoming date ("yyyy-MM-dd")
            if (!DateTime.TryParse(date, out var dt))
                return new List<string>();

            // Fetch the booked DateTimes and convert to “HH:mm”
            var booked = BookingModel.GetBookedSlots(advisorId, dt);
            var times = new List<string>();
            foreach (var slot in booked)
                times.Add(slot.ToString("HH:mm"));

            return times;
        }

        /// <summary>
        /// AJAX endpoint: saves a new booking. Returns true if insert succeeded.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static bool SaveBooking(int userId,
                                       int advisorId,
                                       string sessionType,
                                       string date,
                                       string time,
                                       string focus)
        {
            // Combine date + time into a single DateTime
            if (!DateTime.TryParse($"{date} {time}", out var dt))
                return false;

            var booking = new BookingModel
            {
                UserId = userId,
                AdvisorId = advisorId,
                SessionType = sessionType,
                BookingDateTime = dt,
                Focus = focus,
                Status = 0,                   // Pending
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Insert returns new BookingId; success if > 0
            return booking.Insert() > 0;
        }
    }
}
