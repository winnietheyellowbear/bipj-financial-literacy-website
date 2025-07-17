using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace badpjProject
{
    public partial class FacialLogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Optionally, allow access without session check if using facial login as an alternative.
        }

        // This event fires when the user clicks the "Login via Face" button.
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string descriptorJson = hfDescriptor.Value;
            string loginEmail = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(loginEmail))
            {
                lblResult.Text = "Please enter your email.";
                return;
            }
            if (string.IsNullOrEmpty(descriptorJson))
            {
                lblResult.Text = "No facial data captured. Please try again.";
                return;
            }

            try
            {
                string connString = ConfigurationManager.ConnectionStrings["MyDBConnectionString"].ConnectionString;
                string storedDescriptorJson = null;

                // Retrieve stored facial descriptor using Email
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = "SELECT FaceDescriptor FROM UserFacialAuth WHERE Login_Name = @Email";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", loginEmail);
                        conn.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            storedDescriptorJson = result.ToString();
                        }
                        else
                        {
                            lblResult.Text = "No facial data found for this user. Please enroll first.";
                            return;
                        }
                    }
                }

                float[] storedDescriptor = JsonConvert.DeserializeObject<float[]>(storedDescriptorJson);
                float[] newDescriptor = JsonConvert.DeserializeObject<float[]>(descriptorJson);
                float distance = EuclideanDistance(newDescriptor, storedDescriptor);
                float threshold = 0.6f;

                if (distance < threshold)
                {
                    int userId = 0;
                    string userType = string.Empty;

                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        string sql = "SELECT Id, UserType FROM [User] WHERE Email = @Email";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Email", loginEmail);
                            conn.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    userId = Convert.ToInt32(reader["Id"]);
                                    userType = reader["UserType"].ToString();
                                }
                                else
                                {
                                    lblResult.Text = "User not found in main table.";
                                    return;
                                }
                            }
                        }
                    }

                    Session["UserId"] = userId;
                    Session["Username"] = loginEmail;
                    Session["Role"] = userType;

                    Response.Redirect("UserPage.aspx");
                }
                else
                {
                    lblResult.Text = "Facial authentication failed. Face does not match.";
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "Error: " + ex.Message;
            }
        }


        private static float EuclideanDistance(float[] a, float[] b)
        {
            if (a.Length != b.Length) return float.MaxValue;
            float sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                float diff = a[i] - b[i];
                sum += diff * diff;
            }
            return (float)Math.Sqrt(sum);
        }
    }
}


