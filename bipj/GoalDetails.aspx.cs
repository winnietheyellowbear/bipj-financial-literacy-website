using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using bipj.Models;

namespace bipj
{
    public partial class GoalDetails : System.Web.UI.Page
    {
        private int _goalId;
        private int _userId;

        private readonly Goal _goalModel = new Goal();
        private readonly GoalTransaction _goalTxnModel = new GoalTransaction();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || string.IsNullOrEmpty(Request.QueryString["goalId"]))
            {
                Response.Redirect("Goals.aspx");
                return;
            }

            _userId = Convert.ToInt32(Session["UserId"]);
            _goalId = Convert.ToInt32(Request.QueryString["goalId"]);

            if (IsPostBack) return;

            hdnSelectedPeriod.Value = "month";
            hdnSelectedDate.Value = DateTime.Now.ToString("yyyy-MM");

            txtTxnAmount.Attributes["step"] = "0.01";

            LoadJarDropdown();
            LoadGoalDetails();
            LoadTransactions();
            UpdateSelectedDateLabel();
        }

        protected void btnPeriodChange_Click(object sender, EventArgs e)
        {
            UpdateSelectedDateLabel();
            LoadTransactions();
        }

        protected void rptTransactions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var litTransferNote = (Literal)e.Item.FindControl("litTransferNote");
            var txn = (GoalTransaction)e.Item.DataItem;

            string txt = (txn.SourceType == "jar" && txn.SourceJarId.HasValue)
                ? $"<span class='text-muted'>Transferred from {GetJarName(txn.SourceJarId.Value)}</span>"
                : "<span class='text-muted'>Manual Top-up</span>";

            litTransferNote.Text = txt;
        }

        protected void btnSubmitEntry_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtTxnAmount.Text.Trim(), out decimal amount) || amount <= 0) return;
            if (!DateTime.TryParse(txtTxnDate.Text.Trim(), out DateTime date)) return;
            string name = txtTxnName.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;

            string sourceType = "topup";
            int? fromJarId = null;

            if (rdoTransferYes.Checked)
            {
                if (string.IsNullOrEmpty(ddlJars.SelectedValue)) return;
                fromJarId = int.Parse(ddlJars.SelectedValue);
                sourceType = "jar";

                var jar = new Jar().GetJarById(fromJarId.Value, _userId);
                if (jar == null || jar.Amount < amount)
                {
                    hdnInsufficientFunds.Value = "true";
                    return;
                }
            }

            bool ok = _goalTxnModel.InsertGoalTransaction(_goalId, _userId, name, amount, date, sourceType, fromJarId, lblGoalName.Text);
            if (!ok) return;

            hdnInsufficientFunds.Value = "false";
            LoadGoalDetails();
            LoadTransactions();

            txtTxnName.Text = "";
            txtTxnAmount.Text = "";
            txtTxnDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        protected void btnUpdateTxn_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hdnEditTxnId.Value, out int txnId)) return;
            if (!decimal.TryParse(txtEditTxnAmount.Text, out decimal amount) || amount <= 0) return;
            if (!DateTime.TryParse(txtEditTxnDate.Text, out DateTime date)) return;
            string name = txtEditTxnName.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;

            var existing = new GoalTransaction().GetTransactionById(txnId, _userId);
            if (existing == null || existing.SourceType == "jar") return;

            if (!_goalTxnModel.UpdateTransaction(txnId, _goalId, _userId, name, amount, date)) return;

            decimal newSaved = _goalTxnModel.GetTotalSavedAmount(_goalId, _userId);
            _goalModel.GoalId = _goalId;
            _goalModel.UpdateSavedAmount(newSaved);

            LoadGoalDetails();
            LoadTransactions();
        }

        protected void btnConfirmTxnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hdnDeleteTxnId.Value, out int txnId)) return;

            var txn = new GoalTransaction().GetTransactionById(txnId, _userId);
            if (txn == null || txn.SourceType == "jar") return;

            int rows = txn.DeleteTransaction();
            if (rows <= 0) return;

            decimal newSaved = _goalTxnModel.GetTotalSavedAmount(_goalId, _userId);
            _goalModel.GoalId = _goalId;
            _goalModel.UpdateSavedAmount(newSaved);

            LoadGoalDetails();
            LoadTransactions();
        }

        private void LoadJarDropdown()
        {
            var jars = new Jar().GetJarsByUser(_userId);
            ddlJars.DataSource = jars;
            ddlJars.DataTextField = "JarName";
            ddlJars.DataValueField = "JarId";
            ddlJars.DataBind();
            ddlJars.Items.Insert(0, new ListItem("-- Select Jar --", ""));
        }

        private void LoadGoalDetails()
        {
            var goal = _goalModel.GetGoalById(_goalId, _userId);
            if (goal == null)
            {
                Response.Redirect("Goals.aspx");
                return;
            }

            lblGoalName.Text = goal.GoalName;
            lblTargetAmount.Text = goal.TargetAmount.ToString("N2");
            lblTargetDate.Text = goal.Deadline.ToString("dd MMM yyyy");

            decimal saved = _goalTxnModel.GetTotalSavedAmount(_goalId, _userId);
            lblSavedAmount.Text = saved.ToString("N2");
            hdnSaved.Value = saved.ToString();
            hdnTarget.Value = goal.TargetAmount.ToString();

            int daysLeft = (goal.Deadline - DateTime.Today).Days;
            lblDaysLeft.Text = daysLeft > 0 ? $"{daysLeft} days left" : "Goal date passed";
        }

        private void LoadTransactions()
        {
            string period = hdnSelectedPeriod.Value ?? "month";
            string sel = hdnSelectedDate.Value;

            DateTime from, to;
            GetRange(period, sel, out from, out to);

            var txns = new GoalTransaction().GetTransactionsByGoal(_goalId, _userId);
            if (period != "all") txns = txns.Where(t => t.Date >= from && t.Date < to).ToList();

            rptTransactions.DataSource = txns
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.TransactionId)
                .ToList();
            rptTransactions.DataBind();
        }

        private void UpdateSelectedDateLabel()
        {
            string period = hdnSelectedPeriod.Value;
            string sel = hdnSelectedDate.Value;

            string label;
            string icon;

            switch (period)
            {
                case "day":
                    icon = "images/calendar/calendar-day.png";
                    label = "Day";
                    break;
                case "week":
                    icon = "images/calendar/calendar-week.png";
                    label = "Week";
                    break;
                case "month":
                    icon = "images/calendar/calendar-month.png";
                    label = "Month";
                    break;
                case "year":
                    icon = "images/calendar/calendar-year.png";
                    label = "Year";
                    break;
                case "all":
                    icon = "";
                    label = "∞ All Time";
                    break;
                default:
                    icon = "images/calendar/calendar-month.png";
                    label = "Month";
                    break;
            }

            litPeriodLabel.Text = label;
            litPeriodIcon.Text = string.IsNullOrEmpty(icon)
                ? ""
                : $"<img src='{icon}' width='20' height='20' alt='{label}' />";
        }

        private static void GetRange(string period, string selected, out DateTime from, out DateTime to)
        {
            from = DateTime.MinValue;
            to = DateTime.MaxValue;

            if (period == "all") return;

            if (period == "week" && selected.Contains("-W"))
            {
                var parts = selected.Split(new[] { "-W" }, StringSplitOptions.None);
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int y) &&
                    int.TryParse(parts[1], out int w))
                {
                    var jan4 = new DateTime(y, 1, 4);
                    int dow = jan4.DayOfWeek == 0 ? 7 : (int)jan4.DayOfWeek;
                    var mon = jan4.AddDays(1 - dow + (w - 1) * 7);
                    from = mon;
                    to = mon.AddDays(7);
                    return;
                }
            }

            if (!DateTime.TryParse(selected, out DateTime parsed))
            {
                if (period == "month" && DateTime.TryParse(selected + "-01", out parsed))
                {
                    from = new DateTime(parsed.Year, parsed.Month, 1);
                    to = from.AddMonths(1);
                    return;
                }
                if (period == "year" && int.TryParse(selected, out int yr))
                {
                    from = new DateTime(yr, 1, 1);
                    to = from.AddYears(1);
                    return;
                }
                return;
            }

            switch (period)
            {
                case "day":
                    from = parsed.Date;
                    to = from.AddDays(1);
                    break;
                case "week":
                    int diff = parsed.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)parsed.DayOfWeek - 1;
                    from = parsed.AddDays(-diff).Date;
                    to = from.AddDays(7);
                    break;
                case "month":
                    from = new DateTime(parsed.Year, parsed.Month, 1);
                    to = from.AddMonths(1);
                    break;
                case "year":
                    from = new DateTime(parsed.Year, 1, 1);
                    to = from.AddYears(1);
                    break;
            }
        }

        private string GetJarName(int jarId)
        {
            var jar = new Jar().GetJarById(jarId, _userId);
            return jar?.JarName ?? "Unknown";
        }
    }
}
