using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Security;

namespace bipj
{
    public partial class Loginpage : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                lblMessage.CssClass = "alert alert-danger";
                lblMessage.Visible = false;
            }

            // Redirect if already logged in
            if (Session["UserId"] != null)
            {
                RedirectBasedOnUserType();
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both email and password.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT Id, Name, UserType, Password, IsActive 
                                 FROM [User] 
                                 WHERE Email = @Email AND Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Check if account is active
                                if (!Convert.ToBoolean(reader["IsActive"]))
                                {
                                    ShowError("This account is inactive. Please contact support.");
                                    return;
                                }

                                // Successful login
                                int userId = Convert.ToInt32(reader["Id"]);
                                string userName = reader["Name"].ToString();
                                string userType = reader["UserType"].ToString();

                                // Update last login time
                                UpdateLastLogin(userId);

                                // Store user info in session
                                Session["UserId"] = userId;
                                Session["UserName"] = userName;
                                Session["UserType"] = userType;

                                // Set auth cookie if "Remember Me" is checked
                                if (chkRememberMe.Checked)
                                {
                                    FormsAuthentication.SetAuthCookie(email, true);
                                }

                                // Redirect based on user type
                                RedirectBasedOnUserType();
                            }
                            else
                            {
                                ShowError("Invalid email or password.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("An error occurred during login. Please try again.");
                // For debugging: ShowError(ex.Message);
            }
        }

        private void UpdateLastLogin(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = "UPDATE [User] SET LastLoginDate = GETDATE() WHERE Id = @UserId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }
        }

        private void RedirectBasedOnUserType()
        {
            if (Session["UserType"] == null) return;

            string userType = Session["UserType"].ToString();

            switch (userType.ToLower())
            {
                case "staff":
                case "admin":
                    Response.Redirect("AdminPage.aspx");
                    break;
                default: // Regular users
                    Response.Redirect("Profile.aspx");
                    break;
            }
        }

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "alert alert-danger";
            lblMessage.Visible = true;
        }

        protected void btnForgotPassword_Click(object sender, EventArgs e)
        {
            Response.Redirect("ForgotPassword.aspx");
        }
    }
}