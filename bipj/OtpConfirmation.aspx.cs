using System;
using System.Data.SqlClient;
using System.Configuration;

namespace bipj
{
    public partial class OtpConfirmation : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                lblMessage.CssClass = "alert alert-danger";
                lblMessage.Visible = false;
            }
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            string enteredOtp = txtOTP.Text.Trim();
            string sessionOtp = Session["OTP"]?.ToString();
            string name = Session["Register_Name"]?.ToString();
            string email = Session["Register_Email"]?.ToString();
            string password = Session["Register_Password"]?.ToString();

            if (string.IsNullOrEmpty(enteredOtp) || string.IsNullOrEmpty(sessionOtp) ||
                string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Session expired or invalid input.");
                return;
            }

            if (enteredOtp != sessionOtp)
            {
                ShowError("Invalid OTP. Please try again.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    int nextId = GetNextId(conn);
                    string insertSql = @"
                        INSERT INTO [User] 
                        (Id, Name, Email, Point, Profile, Bio, Password, UserType, IsEmailVerified, IsActive, CreatedDate)
                        VALUES 
                        (@Id, @Name, @Email, @Point, @Profile, @Bio, @Password, @UserType, @IsEmailVerified, @IsActive, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", nextId);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Point", 0);
                        cmd.Parameters.AddWithValue("@Profile", "");
                        cmd.Parameters.AddWithValue("@Bio", "");
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@UserType", "User");
                        cmd.Parameters.AddWithValue("@IsEmailVerified", true); // Set true, as verified!
                        cmd.Parameters.AddWithValue("@IsActive", true);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            // Clear session data
                            Session.Remove("OTP");
                            Session.Remove("Register_Name");
                            Session.Remove("Register_Email");
                            Session.Remove("Register_Password");

                            lblMessage.CssClass = "alert alert-success";
                            lblMessage.Text = "Registration successful! <a href='Loginpage.aspx'>Login here</a>.";
                            lblMessage.Visible = true;
                        }
                        else
                        {
                            ShowError("Failed to register. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("An error occurred: " + ex.Message);
            }
        }

        private int GetNextId(SqlConnection conn)
        {
            string sql = "SELECT ISNULL(MAX(Id), 0) + 1 FROM [User]";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                return (int)cmd.ExecuteScalar();
            }
        }

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "alert alert-danger";
            lblMessage.Visible = true;
        }
    }
}
