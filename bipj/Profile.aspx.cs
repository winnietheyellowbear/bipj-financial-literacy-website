using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace bipj
{
    public partial class Profile : System.Web.UI.Page
    {
        protected int CurrentUserId => Convert.ToInt32(Session["UserId"]);
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

        // Helper method to check if current user can delete a comment
        protected bool CanDeleteComment(int commentAuthorId)
        {
            // Users can delete their own comments or if they're viewing their own profile
            return commentAuthorId == CurrentUserId || ProfileUserId == CurrentUserId;
        }

        // Handle comment deletion
        protected void rptComments_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                int commentId = Convert.ToInt32(e.CommandArgument);
                DeleteComment(commentId);
                LoadComments(); // Refresh the comments list
            }
        }

        private void DeleteComment(int commentId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "DELETE FROM ProfileComments WHERE Id = @CommentId AND (UserId = @CurrentUserId OR @ProfileUserId = @CurrentUserId)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CommentId", commentId);
                cmd.Parameters.AddWithValue("@CurrentUserId", CurrentUserId);
                cmd.Parameters.AddWithValue("@ProfileUserId", ProfileUserId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Your existing LoadProfileData(), LoadComments(), and btnPostComment_Click() methods
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

                        string profilePic = reader["Profile"].ToString();
                        imgProfile.ImageUrl = string.IsNullOrEmpty(profilePic) ?
                            "/Profileuploads" : profilePic;
                    }
                }
            }
        }

        private void LoadComments()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"SELECT c.Id, c.UserId, c.CommentText, c.CommentDate, u.Name AS UserName 
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
        protected void btnEditProfile_Click(object sender, EventArgs e)
        {
            Response.Redirect("EditProfile.aspx");
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

            LoadComments();
            txtComment.Text = string.Empty;
        }
    }
}