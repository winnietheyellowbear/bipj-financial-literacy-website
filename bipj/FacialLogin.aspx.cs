using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using Newtonsoft.Json;

namespace badpjProject
{
    public partial class FacialLogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Optionally, allow access without session check
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string descriptorJson = hfDescriptor.Value;
            string loginEmail = txtEmail.Text.Trim();

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
                string connString = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
                string storedDescriptorJson = null;
                int userId = 0;
                string userType = string.Empty;

                // Retrieve stored facial descriptor and user details
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = @"
                        SELECT u.Id, u.UserType, ufa.FaceDescriptor 
                        FROM [User] u
                        INNER JOIN UserFacialAuth ufa ON u.Id = ufa.UserId
                        WHERE u.Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", loginEmail);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userId = reader.GetInt32(reader.GetOrdinal("Id"));
                                userType = reader.GetString(reader.GetOrdinal("UserType"));
                                storedDescriptorJson = reader.GetString(reader.GetOrdinal("FaceDescriptor"));
                            }
                            else
                            {
                                lblResult.Text = "No facial data found for this user. Please enroll first.";
                                return;
                            }
                        }
                    }
                }

                // Compare face descriptors
                float[] storedDescriptor = JsonConvert.DeserializeObject<float[]>(storedDescriptorJson);
                float[] newDescriptor = JsonConvert.DeserializeObject<float[]>(descriptorJson);
                float distance = EuclideanDistance(newDescriptor, storedDescriptor);
                float threshold = 0.4f;

                if (distance < threshold)
                {
                    // Set session variables
                    Session["UserId"] = userId;
                    Session["UserEmail"] = loginEmail;
                    Session["Role"] = userType;

                    // Update last login date
                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        string updateSql = "UPDATE [User] SET LastLoginDate = GETDATE() WHERE Id = @UserId";
                        using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Redirect to PROFILE page instead of UserPage
                    Response.Redirect("Profile.aspx");
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
