using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace badpjProject
{
    public partial class EnableFacialAuthentication : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            // Ensure the user is logged in; if not, redirect to login
            if (Session["UserId"] == null)
            {
                Response.Redirect("Login.aspx");
            }
            if (!IsPostBack)
            {
                UserLabel.Text = "Email: " + Session["UserEmail"].ToString();
            }
        }

        // Button click event to enroll the facial descriptor from the hidden field
        protected void btnEnroll_Click(object sender, EventArgs e)
        {
            string descriptorJson = hfDescriptor.Value;

            if (string.IsNullOrEmpty(descriptorJson))
            {
                lblMessage.Text = "No facial data captured. Please try again.";
                return;
            }

            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                string email = Session["UserEmail"].ToString();
                string connString = ConfigurationManager.ConnectionStrings["MyDBConnectionString"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Check if facial data already exists for this user
                    string checkSql = "SELECT COUNT(*) FROM UserFacialAuth WHERE UserId = @UserId";
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            // Update existing
                            string updateSql = "UPDATE UserFacialAuth SET FaceDescriptor = @FaceDescriptor, DateEnrolled = GETDATE() WHERE UserId = @UserId";
                            using (SqlCommand updateCmd = new SqlCommand(updateSql, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@FaceDescriptor", descriptorJson);
                                updateCmd.Parameters.AddWithValue("@UserId", userId);
                                updateCmd.ExecuteNonQuery();
                            }
                            lblMessage.Text = "Facial descriptor updated successfully for email: " + email;
                        }
                        else
                        {
                            // Insert new
                            string insertSql = "INSERT INTO UserFacialAuth (UserId, Login_Name, FaceDescriptor) VALUES (@UserId, @Login_Name, @FaceDescriptor)";
                            using (SqlCommand insertCmd = new SqlCommand(insertSql, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@UserId", userId);
                                insertCmd.Parameters.AddWithValue("@Login_Name", email);
                                insertCmd.Parameters.AddWithValue("@FaceDescriptor", descriptorJson);
                                insertCmd.ExecuteNonQuery();
                            }
                            lblMessage.Text = "Facial descriptor enrolled successfully for email: " + email;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error: " + ex.Message;
            }
        }

    }
}

