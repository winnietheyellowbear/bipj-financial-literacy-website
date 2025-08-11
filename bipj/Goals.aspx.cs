using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using bipj.Models;

namespace bipj
{
    public partial class Goals : Page
    {
        private int _userId;

        protected int TotalGoals;
        protected decimal TotalTargetAmount;
        protected decimal TotalSavedAmount;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) { Response.Redirect("Loginpage.aspx"); return; }
            _userId = Convert.ToInt32(Session["UserId"]);

            if (IsPostBack) return;

            BindJarDropdowns();
            BindGoals();
        }

        private void BindJarDropdowns()
        {
            var jars = new Jar().GetJarsByUser(_userId);

            void Bind(DropDownList ddl)
            {
                ddl.DataSource = jars;
                ddl.DataTextField = "JarName";
                ddl.DataValueField = "JarId";
                ddl.DataBind();
                ddl.Items.Insert(0, new ListItem("-- No Jar --", ""));
            }

            Bind(ddlGoalJar);
            Bind(ddlEditJar);
        }

        private void BindGoals()
        {
            var goals = new Goal().GetGoalsByUser(_userId, DateTime.MinValue, DateTime.MaxValue).AsQueryable();

            switch (ddlGoalFilter.SelectedValue)
            {
                case "completed":
                    goals = goals.Where(g => g.SavedAmount >= g.TargetAmount);
                    break;
                case "overdue":
                    goals = goals.Where(g => g.SavedAmount < g.TargetAmount && g.Deadline.Date < DateTime.Today);
                    break;
                case "ongoing":
                    goals = goals.Where(g => g.SavedAmount < g.TargetAmount && g.Deadline.Date >= DateTime.Today);
                    break;
            }

            switch (ddlGoalSort.SelectedValue)
            {
                case "created_asc":
                    goals = goals.OrderBy(g => g.CreatedAt);
                    break;

                case "deadline_asc":
                    goals = goals.OrderBy(g => g.Deadline);
                    break;

                case "deadline_desc":
                    goals = goals.OrderByDescending(g => g.Deadline);
                    break;

                default: // created_desc
                    goals = goals.OrderByDescending(g => g.CreatedAt);
                    break;
            }


            var list = goals.ToList();

            rptGoals.DataSource = list;
            rptGoals.DataBind();

            TotalGoals = list.Count;
            TotalTargetAmount = list.Sum(g => g.TargetAmount);
            TotalSavedAmount = list.Sum(g => g.SavedAmount);

            lblTotalGoals.Text = TotalGoals.ToString();
            lblTotalTarget.Text = $"${TotalTargetAmount:N2}";
            lblTotalSaved.Text = $"${TotalSavedAmount:N2}";
        }

        protected void ddlGoalFilter_SelectedIndexChanged(object sender, EventArgs e) => BindGoals();
        protected void ddlGoalSort_SelectedIndexChanged(object sender, EventArgs e) => BindGoals();

        protected void rptGoals_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int goalId)) return;

            if (e.CommandName == "Edit") ShowEditModal(goalId);
            if (e.CommandName == "Delete") ShowDeleteModal(goalId);
        }

        private void ShowEditModal(int goalId)
        {
            var goal = new Goal().GetGoalById(goalId, _userId);
            if (goal == null) return;

            BindJarDropdowns();

            hdnEditGoalId.Value = goal.GoalId.ToString();
            txtEditGoalName.Text = goal.GoalName;
            txtEditGoalAmount.Text = goal.TargetAmount.ToString("0.##");
            txtEditGoalTargetDate.Text = goal.Deadline.ToString("yyyy-MM-dd");
            ddlEditJar.SelectedValue = goal.JarId?.ToString() ?? "";

            var defaultJar = new Jar().GetDefaultJar(_userId);
            hdnDeleteDefaultJar.Value = defaultJar?.JarName ?? "(Default Jar)";
            hdnDeleteIsCompleted.Value = (goal.SavedAmount >= goal.TargetAmount).ToString().ToLower();
            hdnDeleteGoalName.Value = goal.GoalName;

            ScriptManager.RegisterStartupScript(this, GetType(), "editGoalModal",
                "new bootstrap.Modal('#editGoalModal').show();", true);
        }


        private void ShowDeleteModal(int goalId)
        {
            var goal = new Goal().GetGoalById(goalId, _userId);
            if (goal == null) return;

            var defaultJar = new Jar().GetDefaultJar(_userId);

            hdnDeleteGoalId.Value = goalId.ToString();
            hdnDeleteGoalName.Value = goal.GoalName;
            hdnDeleteDefaultJar.Value = defaultJar?.JarName ?? "Default Jar";
            hdnDeleteIsCompleted.Value = (goal.SavedAmount >= goal.TargetAmount).ToString().ToLower();

            ScriptManager.RegisterStartupScript(this, GetType(), "deleteGoalModal", "showDeleteGoalModal();", true);
        }

        protected void btnAddGoal_Click(object sender, EventArgs e)
        {
            if (!ValidateGoalForm(txtGoalName, txtGoalAmount, txtGoalTargetDate,
                                  out var name, out var target, out var deadline)) return;

            int? jarId = string.IsNullOrEmpty(ddlGoalJar.SelectedValue) ? (int?)null : int.Parse(ddlGoalJar.SelectedValue);

            var goal = new Goal(_userId, jarId, name, target, deadline);
            if (goal.InsertGoal() > 0)
            {
                ClearAddForm();
                BindGoals();
            }
        }

        protected void btnUpdateGoal_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hdnEditGoalId.Value, out int goalId)) return;
            if (!ValidateGoalForm(txtEditGoalName, txtEditGoalAmount, txtEditGoalTargetDate,
                                  out var name, out var target, out var deadline)) return;

            int? jarId = string.IsNullOrEmpty(ddlEditJar.SelectedValue) ? (int?)null : int.Parse(ddlEditJar.SelectedValue);

            var goal = new Goal
            {
                GoalId = goalId,
                UserId = _userId,
                GoalName = name,
                TargetAmount = target,
                Deadline = deadline,
                JarId = jarId
            };

            if (goal.UpdateGoal() > 0)
            {
                BindGoals();
                BindJarDropdowns();
            }
        }

        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hdnDeleteGoalId.Value, out int goalId)) return;

            var goal = new Goal().GetGoalById(goalId, _userId);
            if (goal == null) return;

            // Goal.DeleteGoal() already handles refund + cleanup.
            if (goal.DeleteGoal() > 0) BindGoals();
        }

        private static bool ValidateGoalForm(TextBox txtName, TextBox txtAmt, TextBox txtDate,
                                             out string name, out decimal amount, out DateTime date)
        {
            name = txtName.Text.Trim();
            bool okAmt = decimal.TryParse(txtAmt.Text, out amount) && amount > 0;
            bool okDate = DateTime.TryParse(txtDate.Text, out date) && date >= DateTime.Today;
            return !string.IsNullOrEmpty(name) && okAmt && okDate;
        }

        private void ClearAddForm()
        {
            txtGoalName.Text = "";
            txtGoalAmount.Text = "";
            txtGoalTargetDate.Text = "";
            ddlGoalJar.SelectedIndex = 0;
        }

        public string GetGoalStatus(object savedObj, object targetObj, object deadlineObj)
        {
            decimal saved = Convert.ToDecimal(savedObj);
            decimal target = Convert.ToDecimal(targetObj);
            DateTime due = Convert.ToDateTime(deadlineObj);

            if (saved >= target)
                return "<p class='small mt-2 mb-0 text-success fw-bold'><i class='bi bi-check-circle me-1'></i>Goal Completed</p>";

            if (due.Date < DateTime.Today)
                return $"<p class='small mt-2 mb-0 text-danger fw-bold'><i class='bi bi-exclamation-circle me-1'></i>Overdue!!! – Savings Goal Date: {due:dd MMMM yyyy}</p>";

            return $"<p class='small mt-2 mb-0 text-muted'><i class='bi bi-calendar-event me-1'></i>Savings Goal Date: {due:dd MMMM yyyy}</p>";
        }
    }
}
