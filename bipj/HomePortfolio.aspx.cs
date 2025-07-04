using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace bipj
{
    public partial class HomePortfolio : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadPortfolios();
            }
        }
        private void LoadPortfolios()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT PortfolioId, PortfolioName, CreatedAt FROM Portfolio";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    gvPortfolios.DataSource = reader;
                    gvPortfolios.DataBind();

                    reader.Close();
                }
            }
        }


        protected void gvPortfolios_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewPortfolio")
            {
                // Use row index to get PortfolioId from the grid
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gvPortfolios.Rows[index];
                string portfolioId = row.Cells[0].Text; // Assuming PortfolioId is in the first cell

                Session["SelectedPortfolioId"] = portfolioId;
                Response.Redirect("PortfolioBuilder.aspx"); // Navigate to detail view
            }

            if (e.CommandName == "DeletePortfolio")
            {
                string portfolioId = e.CommandArgument.ToString();
                DeletePortfolioFromDatabase(portfolioId);
                LoadPortfolios(); // Refresh grid after deletion
            }
        }


        private void DeletePortfolioFromDatabase(string portfolioId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "DELETE FROM Portfolio WHERE PortfolioId = @PortfolioId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PortfolioId", portfolioId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


        protected void btnCreatePortfolio_Click(object sender, EventArgs e)
        {
            string portfolioName = txtPortfolioName.Text.Trim();

            if (string.IsNullOrEmpty(portfolioName))
            {
                litCreateMessage.Text = "<span style='color:red;'>Portfolio name cannot be empty.</span>";
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string insertQuery = "INSERT INTO Portfolio (UserId, PortfolioName, CreatedAt) VALUES (@UserId, @PortfolioName, GETDATE())";
                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@UserId", 1); // hardcoded for now; replace if you have a login system
                cmd.Parameters.AddWithValue("@PortfolioName", portfolioName);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    litCreateMessage.Text = "<span style='color:green;'>Portfolio created successfully!</span>";
                    txtPortfolioName.Text = "";
                    LoadPortfolios(); // Refresh the GridView
                }
                catch (Exception ex)
                {
                    litCreateMessage.Text = "<span style='color:red;'>Error: " + ex.Message + "</span>";
                }
            }
        }
    }
}