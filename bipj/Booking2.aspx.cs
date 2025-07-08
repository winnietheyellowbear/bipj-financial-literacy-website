using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class Booking2 : Page
    {
        // Always fetch fresh list of approved advisors
        private List<Advisor> AllAdvisors => Advisor.GetByStatus(1);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindSpecialties();
                BindAdvisors(AllAdvisors);
            }
        }

        private void BindSpecialties()
        {
            var specs = AllAdvisors
                .SelectMany(a => new[] { a.Specialty1, a.Specialty2, a.Specialty3 })
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .OrderBy(s => s);

            cblSpecialties.DataSource = specs;
            cblSpecialties.DataBind();
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

            // Text search
            var q = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(q))
                filtered = filtered
                    .Where(a => a.Name.ToLower().Contains(q)
                             || a.Category.ToLower().Contains(q))
                    .ToList();

            // Min rating
            int minR = int.Parse(ddlMinRating.SelectedValue);
            if (minR > 0)
                filtered = filtered.Where(a => a.Rating >= minR).ToList();

            // Specialties
            var chosen = cblSpecialties.Items
                .Cast<ListItem>()
                .Where(i => i.Selected)
                .Select(i => i.Value)
                .ToList();
            if (chosen.Any())
                filtered = filtered
                    .Where(a => chosen.Contains(a.Specialty1)
                             || chosen.Contains(a.Specialty2)
                             || chosen.Contains(a.Specialty3))
                    .ToList();

            BindAdvisors(filtered);
        }

        // Renders ★★½☆☆ etc.
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

        // Emits <li>Specialty</li> for each non-null specialty
        public string GetSpecialtiesList(object s1, object s2, object s3)
        {
            var list = new List<string>();
            if (s1 != null) list.Add($"<li>{s1}</li>");
            if (s2 != null) list.Add($"<li>{s2}</li>");
            if (s3 != null) list.Add($"<li>{s3}</li>");
            return string.Join("", list);
        }

        // When user clicks “Select”
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

            // store under the keys Booking5.aspx.cs expects:
            Session["BookingAdvisorName"] = adv.Name;
            Session["BookingAdvisorEmail"] = adv.Email;
            Session["BookingAdvisorCategory"] = adv.Category;
            // still keep the raw ID if you need it later
            Session["AdvisorId"] = adv.AdvisorId;

            Response.Redirect("Booking3.aspx");
        }
    }
}
