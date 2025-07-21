using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace bipj
{
    public partial class ReviewAdvisor : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Optional: load advisor info based on BookingId from query string
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string userEmail = Session["UserEmail"] as string;
            string comment = txtComment.Text.Trim();
            int rating = int.TryParse(hfRating.Value, out int r) ? r : 0;

            int bookingId = 0;
            int.TryParse(Request.QueryString["bookingId"], out bookingId);

            if (rating <= 0 || string.IsNullOrEmpty(userEmail) || bookingId == 0)
            {
                // Invalid data
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    INSERT INTO AdvisorReview (BookingId, Rating, Comment, CreatedAt)
                    VALUES (@BookingId, @Rating, @Comment, GETDATE());";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@BookingId", bookingId);
                    cmd.Parameters.AddWithValue("@Rating", rating);
                    cmd.Parameters.AddWithValue("@Comment", (object)comment ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // Redirect or notify success
            Response.Redirect("ReminderPage.aspx?success=1");
        }
    }
}
