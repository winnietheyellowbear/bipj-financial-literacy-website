using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace bipj
{
    public partial class Profile : System.Web.UI.Page
    {
        protected int CurrentUserId => Convert.ToInt32(Session["UserId"]); // Assuming you store user ID in session
        protected int ProfileUserId => Request.QueryString["userId"] != null ?
            Convert.ToInt32(Request.QueryString["userId"]) : CurrentUserId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserId"] == null)
                {
                    Response.Redirect("Loginpage.aspx");
                    return;
                }

                LoadProfileData();
                LoadComments();
            }
        }

        private void LoadProfileData()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT Name, Email, Point, Profile, Bio, JoinDate FROM [User] WHERE Id = @UserId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", ProfileUserId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ltName.Text = reader["Name"].ToString();
                        ltEmail.Text = reader["Email"].ToString();
                        ltPoints.Text = reader["Point"].ToString();
                        ltBio.Text = string.IsNullOrEmpty(reader["Bio"].ToString()) ?
                            "This user hasn't written a bio yet." : reader["Bio"].ToString();

                        if (reader["JoinDate"] != DBNull.Value)
                        {
                            DateTime joinDate = Convert.ToDateTime(reader["JoinDate"]);
                            ltJoinDate.Text = joinDate.ToString("MMMM yyyy");
                        }

                        // Set profile picture (assuming Profile column stores image path)
                        string profilePic = reader["Profile"].ToString();
                        imgProfile.ImageUrl = string.IsNullOrEmpty(profilePic) ?
                            "/images/default-profile.png" : profilePic;
                    }
                }
            }
        }

        private void LoadComments()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"SELECT c.*, u.Name AS UserName 
                              FROM ProfileComments c
                              JOIN [User] u ON c.UserId = u.Id
                              WHERE c.ProfileUserId = @ProfileUserId
                              ORDER BY c.CommentDate DESC";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@ProfileUserId", ProfileUserId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptComments.DataSource = dt;
                    rptComments.DataBind();
                    lblNoComments.Visible = false;
                }
                else
                {
                    rptComments.Visible = false;
                    lblNoComments.Visible = true;
                }
            }
        }

        protected void btnPostComment_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtComment.Text))
            {
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"INSERT INTO ProfileComments 
                              (UserId, ProfileUserId, CommentText)
                              VALUES (@UserId, @ProfileUserId, @CommentText)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", CurrentUserId);
                cmd.Parameters.AddWithValue("@ProfileUserId", ProfileUserId);
                cmd.Parameters.AddWithValue("@CommentText", txtComment.Text.Trim());

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // Refresh comments and clear textbox
            LoadComments();
            txtComment.Text = string.Empty;
        }
    }
}