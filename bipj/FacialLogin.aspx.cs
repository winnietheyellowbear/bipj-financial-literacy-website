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
            string loginName = txtUsername.Text.Trim(); // User-entered login name

            if (string.IsNullOrEmpty(loginName))
            {
                lblResult.Text = "Please enter your username.";
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

                // First, retrieve the stored facial descriptor for this login name from your facial authentication table.
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = "SELECT FaceDescriptor FROM UserFacialAuth WHERE Login_Name = @Login_Name";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", loginName);
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

                // Deserialize both descriptors
                float[] storedDescriptor = JsonConvert.DeserializeObject<float[]>(storedDescriptorJson);
                float[] newDescriptor = JsonConvert.DeserializeObject<float[]>(descriptorJson);

                // Compute Euclidean distance between the new and stored descriptors
                float distance = EuclideanDistance(newDescriptor, storedDescriptor);
                float threshold = 0.6f; // Typical threshold for 128D face descriptors

                if (distance < threshold)
                {
                    // If the facial comparison is successful, retrieve user info from the main user table.
                    int userId = 0;
                    string role = string.Empty;

                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        // Assuming your main user table is named [Table]
                        string sql = "SELECT Id, Role FROM [Table] WHERE Login_Name = @Login_Name";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Login_Name", loginName);
                            conn.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    userId = Convert.ToInt32(reader["Id"]);
                                    role = reader["Role"].ToString();
                                }
                                else
                                {
                                    lblResult.Text = "User not found in main table.";
                                    return;
                                }
                            }
                        }
                    }

                    // Set session variables as in your standard login code.
                    Session["UserId"] = userId;
                    Session["Username"] = loginName;
                    Session["Role"] = role;

                    // Redirect to the user profile page
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
