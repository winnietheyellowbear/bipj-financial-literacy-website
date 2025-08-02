using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class Booking2 : Page
    {
        // Fetch all approved advisors
        private List<Advisor> AllAdvisors => Advisor.GetByStatus(1);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindAdvisors(AllAdvisors);
            }
        }

        private void BindAdvisors(List<Advisor> advisors)
        {
            rptAdvisors.DataSource = advisors;
            rptAdvisors.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e) => FilterAndBind();
        protected void FilterChanged(object sender, EventArgs e) => FilterAndBind();

        private void FilterAndBind()
        {
            var filtered = AllAdvisors;

            // Keyword search: name or category
            var q = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(q))
            {
                filtered = filtered
                    .Where(a => a.Name.ToLower().Contains(q)
                             || a.Category.ToLower().Contains(q))
                    .ToList();
            }

            // Star rating filter
            if (int.TryParse(ddlMinRating.SelectedValue, out int minR) && minR > 0)
            {
                filtered = filtered.Where(a => a.Rating >= minR).ToList();
            }

            BindAdvisors(filtered);
        }

        // Generates star icons for rating
        public string GenerateStars(decimal rating)
        {
            int full = (int)Math.Floor(rating);
            bool half = rating - full >= 0.5m;
            int empty = 5 - full - (half ? 1 : 0);

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < full; i++) sb.Append("<i class='fas fa-star'></i>");
            if (half) sb.Append("<i class='fas fa-star-half-alt'></i>");
            for (int i = 0; i < empty; i++) sb.Append("<i class='far fa-star'></i>");
            return sb.ToString();
        }

        // Generates specialties as <li> items
        public string GetSpecialtiesList(object s1, object s2, object s3)
        {
            var list = new List<string>();
            if (s1 != null) list.Add($"<li>{s1}</li>");
            if (s2 != null) list.Add($"<li>{s2}</li>");
            if (s3 != null) list.Add($"<li>{s3}</li>");
            return string.Join("", list);
        }

        // When user clicks "Select"
        protected void btnSelectAdvisor_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int advisorId = int.Parse(btn.CommandArgument);
            var adv = Advisor.GetById(advisorId);

            if (adv == null)
            {
                Response.Redirect("Booking2.aspx");
                return;
            }

            Session["BookingAdvisorName"] = adv.Name;
            Session["BookingAdvisorEmail"] = adv.Email;
            Session["BookingAdvisorCategory"] = adv.Category;
            Session["AdvisorId"] = adv.AdvisorId;

            Response.Redirect("Booking3.aspx");
        }
    }
}
