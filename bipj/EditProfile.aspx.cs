using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace bipj
{
    public partial class EditProfile : System.Web.UI.Page
    {
        protected int CurrentUserId => Convert.ToInt32(Session["UserId"]);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Loginpage.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadProfile();
            }
        }

        private void LoadProfile()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT Name, Bio, Profile FROM [User] WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", CurrentUserId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtName.Text = reader["Name"].ToString();
                        txtBio.Text = reader["Bio"].ToString();
                        string profilePic = reader["Profile"].ToString();
                        imgProfile.ImageUrl = string.IsNullOrEmpty(profilePic) ? "/images/default-profile.png" : profilePic;
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            lblError.Visible = false;

            string name = txtName.Text.Trim();
            string bio = txtBio.Text.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(name))
            {
                lblError.Text = "Name cannot be empty.";
                lblError.Visible = true;
                return;
            }
            if (name.Length > 50)
            {
                lblError.Text = "Name cannot exceed 50 characters.";
                lblError.Visible = true;
                return;
            }
            if (bio.Length > 500)
            {
                lblError.Text = "Bio cannot exceed 500 characters.";
                lblError.Visible = true;
                return;
            }

            string profilePicPath = null;
            // Handle image upload if there is a file
            if (fileProfileImage.HasFile)
            {
                try
                {
                    string extension = Path.GetExtension(fileProfileImage.FileName).ToLower();
                    if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".gif")
                    {
                        lblError.Text = "Please upload an image file (jpg, png, gif).";
                        lblError.Visible = true;
                        return;
                    }
                    string fileName = "profile_" + CurrentUserId + extension;
                    string folder = Server.MapPath("~/Profileuploads");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    string filePath = Path.Combine(folder, fileName);
                    fileProfileImage.SaveAs(filePath);
                    // Save path for DB (relative to site root)
                    profilePicPath = "/Profileuploads/" + fileName;

                }
                catch (Exception ex)
                {
                    lblError.Text = "Image upload failed: " + ex.Message;
                    lblError.Visible = true;
                    return;
                }
            }

            // Update user in DB
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "UPDATE [User] SET Name=@Name, Bio=@Bio"
                           + (profilePicPath != null ? ", Profile=@Profile" : "")
                           + " WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Bio", bio);
                if (profilePicPath != null)
                {
                    cmd.Parameters.AddWithValue("@Profile", profilePicPath);
                }
                cmd.Parameters.AddWithValue("@Id", CurrentUserId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            lblMessage.Text = "Profile updated successfully!";
            lblMessage.Visible = true;

            // Update image preview if changed
            if (profilePicPath != null)
                imgProfile.ImageUrl = profilePicPath;
        }
    }
}
