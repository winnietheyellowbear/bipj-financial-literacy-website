using System;
using System.Data.SqlClient;
using System.Configuration; // <-- Required!

namespace bipj
{
    public partial class LoginPage : System.Web.UI.Page
    {
        // No need to hardcode! Get from Web.config:
        string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtName.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (username == "" || password == "")
            {
                lblMessage.Text = "Please enter both username and password.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT Id, type FROM Accounts WHERE name=@name AND password=@password";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = reader.GetInt32(0);
                            string userType = reader.GetString(1).Trim();

                            Session["username"] = username;
                            Session["userid"] = userId;
                            Session["usertype"] = userType;
                            if (Session["usertype"] == "Staff")
                            {
                                Response.Redirect("AdminPage.aspx");
                            }
                            else if(Session["usertype"] == "User")
                            {
                                Response.Redirect("Profile.aspx");
                            }
                            
                        }
                        else
                        {
                            lblMessage.Text = "Invalid username or password.";
                        }
                    }
                }
            }
        }
    }
}
