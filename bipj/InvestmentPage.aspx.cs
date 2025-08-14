using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class InvestmentPage : System.Web.UI.Page
    {
        private int GetCurrentUserID()
        {
            if (Session["UserID"] != null)
            {
                return Convert.ToInt32(Session["UserID"]);
            }
            Response.Redirect("LoginPage.aspx");
            return 0;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserID"] == null)
                {
                    Session["UserID"] = 1;
                }
                BindPortfolios();
            }
        }

        private void BindPortfolios()
        {
            int userId = GetCurrentUserID();
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT PortfolioID, PortfolioName, Description, LastUpdatedAt FROM Portfolios WHERE UserID = @UserID ORDER BY LastUpdatedAt DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        rptPortfolios.DataSource = dt;
                        rptPortfolios.DataBind();

                        pnlEmptyData.Visible = dt.Rows.Count == 0;
                        rptPortfolios.Visible = dt.Rows.Count > 0;
                    }
                }
            }
        }

        // ✅ MODIFIED: This button now creates a new portfolio in the database with a default name,
        // then immediately redirects to the page where you can add assets to it.
        protected void btnCreateNewPortfolio_Click(object sender, EventArgs e)
        {
            int userId = GetCurrentUserID();
            string newPortfolioName = $"My Portfolio - {DateTime.Now:yyyy-MM-dd HH:mm}";
            int newPortfolioId = 0;

            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                // Insert new portfolio and get its ID back using OUTPUT INSERTED.PortfolioID
                string query = "INSERT INTO Portfolios (UserID, PortfolioName, CreatedAt, LastUpdatedAt) OUTPUT INSERTED.PortfolioID VALUES (@UserID, @PortfolioName, GETDATE(), GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@PortfolioName", newPortfolioName);
                    con.Open();
                    // ExecuteScalar returns the first column of the first row, which is our new ID.
                    newPortfolioId = (int)cmd.ExecuteScalar();
                }
            }

            if (newPortfolioId > 0)
            {
                // Redirect to the page to add assets to the newly created portfolio.
                Response.Redirect($"InvestmentPortfolioPage.aspx?id={newPortfolioId}");
            }
            else
            {
                // Optionally, handle the case where the portfolio couldn't be created.
            }
        }

        // ✅ MODIFIED: The "Edit" command has been changed to "Analyze" to match the button.
        protected void rptPortfolios_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int portfolioId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "View")
            {
                Response.Redirect($"InvestmentPortfolioPage.aspx?id={portfolioId}");
            }
            // ✅ MODIFIED: This now handles the "Analyze" button and redirects to the dashboard.
            else if (e.CommandName == "Analyze")
            {
                Response.Redirect($"InvestmentDashboardPage.aspx?id={portfolioId}");
            }
            else if (e.CommandName == "Delete")
            {
                DeletePortfolio(portfolioId);
                BindPortfolios();
            }
        }

        private void DeletePortfolio(int portfolioId)
        {
            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Portfolios WHERE PortfolioID = @PortfolioID", con))
                {
                    cmd.Parameters.AddWithValue("@PortfolioID", portfolioId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}