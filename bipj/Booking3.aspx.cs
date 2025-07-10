using System;
using System.Collections.Generic;
using System.Web.Script.Services;
using System.Web.Services;

namespace bipj
{
    public partial class Booking3 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // nothing needed here for now
        }

        /// <summary>
        /// AJAX endpoint: returns a list of "HH:mm" strings for already‐booked slots.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<string> GetBookedSlots(int advisorId, string date)
        {
            if (!DateTime.TryParse(date, out var dt))
                return new List<string>();

            // Fetch the booked DateTimes and convert to “HH:mm”
            var booked = Models.Booking.GetBookedSlots(advisorId, dt);
            var times = new List<string>();
            foreach (var slot in booked)
                times.Add(slot.ToString("HH:mm"));

            return times;
        }

        /// <summary>
        /// Back → Booking2
        /// </summary>
        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("Booking2.aspx");
        }

        /// <summary>
        /// Next → Booking4 (stores date & time in Session)
        /// </summary>
        protected void btnNext_Click(object sender, EventArgs e)
        {
            // These hidden fields must exist in Booking3.aspx:
            // <asp:HiddenField ID="hfDate" runat="server" />
            // <asp:HiddenField ID="hfTime" runat="server" />
            var date = hfDate.Value;
            var time = hfTime.Value;

            if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(time))
            {
                // Use the exact keys Booking5.aspx.cs will look up:
                Session["BookingDate"] = date;
                Session["BookingTime"] = time;
            }

            Response.Redirect("Booking4.aspx");
        }
    }
}
