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
            // load distinct categories from all advisors
            var cats = Advisor.GetAll()
                             .Select(a => a.Category)
                             .Distinct()
                             .OrderBy(c => c)
                             .ToList();

            ddlCategory.Items.Clear();
            ddlCategory.Items.Add(new ListItem("All Categories", ""));
            foreach (var c in cats)
                ddlCategory.Items.Add(new ListItem(c, c));
        }

        protected void FilterChanged(object sender, EventArgs e)
        {
            BindAdvisors();
        }

        private void BindAdvisors()
        {
            // fetch only approved advisors
            var list = Advisor.GetAll().Where(a => a.Status == 1);

            // apply category filter
            if (!string.IsNullOrEmpty(ddlCategory.SelectedValue))
                list = list.Where(a => a.Category == ddlCategory.SelectedValue);

            // apply rating filter
            switch (ddlRating.SelectedValue)
            {
                case "Below3":
                    list = list.Where(a => a.Rating < 3m);
                    break;
                case "3":
                    list = list.Where(a => a.Rating >= 3m && a.Rating < 4m);
                    break;
                case "4":
                    list = list.Where(a => a.Rating >= 4m && a.Rating < 5m);
                    break;
                case "5":
                    list = list.Where(a => a.Rating >= 5m);
                    break;
            }

            rptAll.DataSource = list.ToList();
            rptAll.DataBind();
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var id = (sender as Button).CommandArgument;
            Response.Redirect($"AdvisorProfile.aspx?advisorId={id}");
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int id = int.Parse(btn.CommandArgument);
            var adv = Advisor.GetById(id);
            if (adv != null)
                adv.Delete();

            // re-bind after deletion
            BindAdvisors();
        }

        /// <summary>
        /// Renders ★★½☆☆ etc.
        /// </summary>
        public string GenerateStars(decimal rating)
        {
            int full = (int)Math.Floor(rating);
            bool half = (rating - full) >= 0.5m;
            int empty = 5 - full - (half ? 1 : 0);

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < full; i++)
                sb.Append("<i class='fas fa-star'></i>");
            if (half)
                sb.Append("<i class='fas fa-star-half-alt'></i>");
            for (int i = 0; i < empty; i++)
                sb.Append("<i class='far fa-star'></i>");

            return sb.ToString();
        }
    }
}
