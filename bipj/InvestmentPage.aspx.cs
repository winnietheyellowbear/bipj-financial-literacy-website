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

        // ✅ REFACTORED: This method now uses the input from the new textbox.
        protected void btnCreateNewPortfolio_Click(object sender, EventArgs e)
        {
            // First, check if the validator passed.
            if (!Page.IsValid)
            {
                return;
            }

            int userId = GetCurrentUserID();
            // Get the name from the textbox instead of generating a default one.
            string newPortfolioName = txtNewPortfolioName.Text.Trim();

            string constr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "INSERT INTO Portfolios (UserID, PortfolioName, CreatedAt, LastUpdatedAt) VALUES (@UserID, @PortfolioName, GETDATE(), GETDATE())";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@PortfolioName", newPortfolioName);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // ✅ MODIFIED: Instead of redirecting, we clear the textbox and refresh the portfolio list.
            txtNewPortfolioName.Text = "";
            BindPortfolios();
        }

        protected void rptPortfolios_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int portfolioId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "View")
            {
                Response.Redirect($"InvestmentPortfolioPage.aspx?id={portfolioId}");
            }
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
