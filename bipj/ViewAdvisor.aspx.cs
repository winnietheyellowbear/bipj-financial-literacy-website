using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class ViewAdvisor : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateCategoryFilter();
                BindAdvisors();
            }
        }

        private void PopulateCategoryFilter()
        {
            // Load distinct advisor categories
            var categories = Advisor.GetAll()
                                    .Select(a => a.Category)
                                    .Distinct()
                                    .OrderBy(c => c)
                                    .ToList();

            ddlCategory.Items.Clear();
            ddlCategory.Items.Add(new ListItem("All Categories", ""));
            foreach (var category in categories)
            {
                ddlCategory.Items.Add(new ListItem(category, category));
            }
        }

        protected void FilterChanged(object sender, EventArgs e)
        {
            BindAdvisors();
        }

        private void BindAdvisors()
        {
            var advisors = Advisor.GetAll().Where(a => a.Status == 1); // Only approved advisors

            // Filter by category
            if (!string.IsNullOrEmpty(ddlCategory.SelectedValue))
            {
                advisors = advisors.Where(a => a.Category == ddlCategory.SelectedValue);
            }

            // Filter by rating
            switch (ddlRating.SelectedValue)
            {
                case "Below3":
                    advisors = advisors.Where(a => a.Rating < 3m);
                    break;
                case "3":
                    advisors = advisors.Where(a => a.Rating >= 3m && a.Rating < 4m);
                    break;
                case "4":
                    advisors = advisors.Where(a => a.Rating >= 4m && a.Rating < 5m);
                    break;
                case "5":
                    advisors = advisors.Where(a => a.Rating >= 5m);
                    break;
            }

            rptAll.DataSource = advisors.ToList();
            rptAll.DataBind();
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var advisorId = (sender as Button).CommandArgument;
            Response.Redirect($"AdvisorProfile.aspx?advisorId={advisorId}");
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int advisorId = int.Parse(btn.CommandArgument);
            var advisor = Advisor.GetById(advisorId);
            if (advisor != null)
            {
                advisor.Delete(); // Assumes you have a Delete() method
            }

            // Rebind grid after deletion
            BindAdvisors();
        }

        /// <summary>
        /// Converts decimal rating to star icons (e.g., ★★★★☆)
        /// </summary>
        public string GenerateStars(decimal rating)
        {
            int full = (int)Math.Floor(rating);
            bool hasHalf = (rating - full) >= 0.5m;
            int empty = 5 - full - (hasHalf ? 1 : 0);

            var stars = new System.Text.StringBuilder();
            for (int i = 0; i < full; i++)
                stars.Append("<i class='fas fa-star'></i>");
            if (hasHalf)
                stars.Append("<i class='fas fa-star-half-alt'></i>");
            for (int i = 0; i < empty; i++)
                stars.Append("<i class='far fa-star'></i>");

            return stars.ToString();
        }
    }
}
