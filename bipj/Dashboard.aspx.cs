using System;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using bipj.Models;

namespace bipj
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private int _userId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) { Response.Redirect("Loginpage.aspx"); return; }
            _userId = Convert.ToInt32(Session["UserId"]);

            if (IsPostBack) return;

            txtExpenseAmount.Attributes["step"] = "0.01";
            txtIncomeAmount.Attributes["step"] = "0.01";

            hdnSelectedPeriod.Value = string.IsNullOrEmpty(hdnSelectedPeriod.Value) ? "month" : hdnSelectedPeriod.Value;
            hdnSelectedDate.Value = string.IsNullOrEmpty(hdnSelectedDate.Value) ? DateTime.Today.ToString("yyyy-MM", CultureInfo.InvariantCulture) : hdnSelectedDate.Value;

            UpdatePeriodLabel();
            LoadTotals();
            LoadJarTotal();
            LoadGoals();
            LoadJarsDropdowns();
        }

        protected void btnPeriodChange_Click(object sender, EventArgs e)
        {
            UpdatePeriodLabel();
            LoadTotals();
            LoadJarTotal();
            LoadGoals();
        }

        private (DateTime From, DateTime To) GetRange()
        {
            var period = hdnSelectedPeriod.Value;
            var sel = hdnSelectedDate.Value;

            if (period == "all") return (DateTime.MinValue, DateTime.MaxValue);

            if (period == "week" && sel.Contains("-W"))
            {
                var p = sel.Split(new[] { "-W" }, StringSplitOptions.None);
                int y = int.Parse(p[0]), w = int.Parse(p[1]);
                var jan4 = new DateTime(y, 1, 4);
                int dow = jan4.DayOfWeek == 0 ? 7 : (int)jan4.DayOfWeek;
                var mon = jan4.AddDays(1 - dow + (w - 1) * 7);
                return (mon, mon.AddDays(7));
            }

            if (period == "year" && int.TryParse(sel, out int yr))
                return (new DateTime(yr, 1, 1), new DateTime(yr + 1, 1, 1));

            if (period == "month" && DateTime.TryParseExact(sel + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var m))
                return (new DateTime(m.Year, m.Month, 1), new DateTime(m.Year, m.Month, 1).AddMonths(1));

            if (period == "day" && DateTime.TryParseExact(sel, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return (d.Date, d.Date.AddDays(1));

            return (DateTime.MinValue, DateTime.MaxValue);
        }

        private void UpdatePeriodLabel()
        {
            var period = hdnSelectedPeriod.Value;

            string label;
            string icon;

            switch (period)
            {
                case "day": label = "Day"; icon = "images/calendar/calendar-day.png"; break;
                case "week": label = "Week"; icon = "images/calendar/calendar-week.png"; break;
                case "month": label = "Month"; icon = "images/calendar/calendar-month.png"; break;
                case "year": label = "Year"; icon = "images/calendar/calendar-year.png"; break;
                case "all": label = "∞ All Time"; icon = ""; break;
                default: label = "Month"; icon = "images/calendar/calendar-month.png"; break;
            }

            litPeriodLabel.Text = label;
            litPeriodIcon.Text = string.IsNullOrEmpty(icon) ? "" : $"<img src='{icon}' width='20' height='20' alt='{label}' />";
        }

        private void LoadTotals()
        {
            var (from, to) = GetRange();

            decimal income = 0m;
            decimal expense = 0m;

            var txnMgr = new JarTransaction();
            var jars = new Jar().GetJarsByUser(_userId, includeDeleted: true);

            foreach (var jar in jars)
            {
                income += txnMgr.GetTransactionSumByType(_userId, jar.JarId, "Income", from, to, includeTransfers: false);
                expense += txnMgr.GetTransactionSumByType(_userId, jar.JarId, "Expense", from, to, includeTransfers: false);

                if (jar.InitialAmount > 0)
                {
                    if (hdnSelectedPeriod.Value == "all" ||
                        (jar.CreatedAt >= from && jar.CreatedAt < to))
                        income += jar.InitialAmount;
                }
            }

            decimal balance = income + expense;

            lblIncome.Text = income.ToString("C2");
            lblExpense.Text = Math.Abs(expense).ToString("C2");
            lblBalance.Text = balance.ToString("C2");
        }

        private void LoadJarTotal()
        {
            var (_, to) = GetRange();

            var jars = new Jar().GetJarsByUser(_userId);
            var txnMgr = new JarTransaction();

            decimal total = 0m;
            foreach (var jar in jars)
            {
                decimal net = txnMgr.GetTransactionSum(_userId, jar.JarId, null, to);
                total += jar.InitialAmount + net;
            }

            lblJarTotal.Text = total.ToString("C2");
        }

        private void LoadGoals()
        {
            var (from, to) = GetRange();

            var goals = new Goal().GetGoalsByUser(_userId, from, to);

            int completed = goals.Count(g => g.SavedAmount >= g.TargetAmount);
            int ongoing = goals.Count - completed;

            decimal totalSaved = goals.Sum(g => g.SavedAmount);
            decimal totalTarget = goals.Sum(g => g.TargetAmount);

            int pct = totalTarget > 0 ? (int)Math.Floor(totalSaved / totalTarget * 100m) : 0;

            lblOngoingCount.Text = ongoing.ToString();
            lblCompletedCount.Text = completed.ToString();
            lblOverallPercent.Text = pct.ToString();
            lblSavedVsTarget.Text = $"{totalSaved:C2} / {totalTarget:C2}";

            pnlOngoingGoals.Visible = ongoing > 0;
            lblNoOngoingGoals.Visible = ongoing == 0;
        }

        private void LoadJarsDropdowns()
        {
            var jars = new Jar().GetJarsByUser(_userId);

            ddlJars.DataSource = jars;
            ddlJars.DataTextField = "JarName";
            ddlJars.DataValueField = "JarId";
            ddlJars.DataBind();

            var jarSvc = new Jar();
            foreach (ListItem item in ddlJars.Items)
            {
                int jarId = int.Parse(item.Value);
                decimal bal = jarSvc.GetCurrentBalance(_userId, jarId);
                item.Attributes["data-balance"] = bal.ToString("0.##", CultureInfo.InvariantCulture);
            }

            ddlIncomeJars.DataSource = jars;
            ddlIncomeJars.DataTextField = "JarName";
            ddlIncomeJars.DataValueField = "JarId";
            ddlIncomeJars.DataBind();
        }

        protected void btnSubmitEntry_Click(object sender, EventArgs e)
        {
            bool isExpense = string.Equals(hdnTransactionType.Value, "Expense", StringComparison.OrdinalIgnoreCase);
            string name = isExpense ? txtExpenseName.Text.Trim() : txtIncomeName.Text.Trim();

            if (!decimal.TryParse(isExpense ? txtExpenseAmount.Text.Trim() : txtIncomeAmount.Text.Trim(),
                                  NumberStyles.AllowDecimalPoint,
                                  CultureInfo.InvariantCulture,
                                  out decimal amount) || amount <= 0) return;

            if (!DateTime.TryParse(isExpense ? txtExpenseDate.Text.Trim() : txtIncomeDate.Text.Trim(),
                                   out DateTime date)) return;

            var txnMgr = new JarTransaction { UserId = _userId, Name = name, Amount = amount, Date = date };
            var jarSvc = new Jar();

            if (isExpense)
            {
                int jarId = int.Parse(ddlJars.SelectedValue);

                // ---- balance guard ----
                var jar = new Jar().GetJarById(jarId, _userId);
                decimal liveBal = jar.InitialAmount + new JarTransaction().GetTransactionSum(_userId, jarId);
                if (amount > liveBal)
                {
                    ScriptManager.RegisterStartupScript(
                        this, GetType(), "insuff",
                        "showInsufficientFundsModal();",
                        true);
                    return;
                }
                hdnInsufficientFunds.Value = "false";

                txnMgr.JarId = jarId;
                txnMgr.TransactionType = TxnType.Expense;
                txnMgr.InsertTransaction();

                jarSvc.DeductAmount(jarId, amount);
            }
            else
            {
                string allocMode = (Request["incomeAllocation"] ?? "auto").ToLowerInvariant();

                if (allocMode == "manual")
                {
                    int jarId = int.Parse(ddlIncomeJars.SelectedValue);
                    txnMgr.JarId = jarId;
                    txnMgr.TransactionType = TxnType.Income;
                    txnMgr.InsertTransaction();

                    jarSvc.JarId = jarId;
                    jarSvc.UserId = _userId;
                    jarSvc.Amount = amount;
                    jarSvc.UpdateJarAmount();
                }
                else // auto distribute
                {
                    foreach (var (jarId, pct) in jarSvc.GetJarsWithPercentages(_userId))
                    {
                        decimal share = Math.Floor(amount * pct / 100m * 100m) / 100m; // 2dp floor
                        if (share <= 0) continue;

                        new JarTransaction
                        {
                            UserId = _userId,
                            JarId = jarId,
                            Name = name,
                            Amount = share,
                            Date = date,
                            TransactionType = TxnType.Income
                        }.InsertTransaction();

                        jarSvc.JarId = jarId;
                        jarSvc.UserId = _userId;
                        jarSvc.Amount = share;
                        jarSvc.UpdateJarAmount();
                    }
                }
            }

            // refresh UI
            UpdatePeriodLabel();
            LoadTotals();
            LoadJarTotal();
            LoadGoals();
        }
    }
}
