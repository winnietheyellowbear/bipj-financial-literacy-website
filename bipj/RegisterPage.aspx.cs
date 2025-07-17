using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Net;

namespace bipj
{
    public partial class RegisterPage : System.Web.UI.Page
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

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowError("Please fill in all fields.");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Passwords do not match.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Check if email already exists
                    string checkEmailSql = "SELECT COUNT(*) FROM [User] WHERE Email = @Email";
                    using (SqlCommand cmd = new SqlCommand(checkEmailSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            ShowError("Email is already registered.");
                            return;
                        }
                    }
                }

                // Generate OTP
                string otpCode = GenerateOTP();
                Session["OTP"] = otpCode;
                Session["Register_Name"] = name;
                Session["Register_Email"] = email;
                Session["Register_Password"] = password;

                // Send OTP
                if (SendOTP(email, otpCode))
                {
                    Response.Redirect("OtpConfirmation.aspx");
                }
                else
                {
                    ShowError("Failed to send OTP. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError("An error occurred: " + ex.Message);
            }
        }

        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private bool SendOTP(string email, string otp)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("ruihernh@gmail.com"); // Replace with your sender email
                mail.To.Add(email);
                mail.Subject = "Your OTP Code";
                mail.Body = $"Your OTP code is: {otp}";

                smtpServer.Port = 587;
                smtpServer.Credentials = new NetworkCredential("ruihernh@gmail.com", "yqqh pwcr byeq sseo"); // Use App Password
                smtpServer.EnableSsl = true;

                smtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                // Log or handle error as needed
                return false;
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
