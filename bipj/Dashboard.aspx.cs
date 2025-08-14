using bipj.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private int _userId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) { Response.Redirect("Loginpage.aspx"); return; }
            _userId = Convert.ToInt32(Session["UserId"]);

            if (!IsPostBack)
            {
                new JarSnapshot().BackfillSnapshots(_userId);

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
            LoadJarSnapshotChart();

        }

        protected void btnPeriodChange_Click(object sender, EventArgs e)
        {
            new JarSnapshot().BackfillSnapshots(_userId);

            UpdatePeriodLabel();
            LoadTotals();
            LoadJarTotal();
            LoadGoals();
            LoadJarSnapshotChart();
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

            decimal income = 0m, expense = 0m;

            var txnMgr = new JarTransaction();
            var jars = new Jar().GetJarsByUser(_userId, includeDeleted: true);

            // Period P&L from jars; exclude transfers
            foreach (var jar in jars)
            {
                income += txnMgr.GetTransactionSumByType(_userId, jar.JarId, "Income", from, to, includeTransfers: false);
                expense += txnMgr.GetTransactionSumByType(_userId, jar.JarId, "Expense", from, to, includeTransfers: false);
            }

            // === ADD: manual goal top-ups (exclude jar->goal transfers) ===
            // GoalTransactions: UserId, Amount, SourceType ('topup' = manual), Date
            var connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            decimal manualGoalTopUps = 0m;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(@"
        SELECT ISNULL(SUM(gt.Amount), 0)
        FROM GoalTransactions gt
        WHERE gt.UserId = @uid
          AND gt.SourceType = 'topup'
          AND gt.[Date] >= @from AND gt.[Date] < @to;", conn))
            {
                cmd.Parameters.AddWithValue("@uid", _userId);
                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);
                conn.Open();
                var v = cmd.ExecuteScalar();
                manualGoalTopUps = (v == DBNull.Value) ? 0m : Convert.ToDecimal(v);
            }

            income += manualGoalTopUps;

            decimal balance = income - expense;

            lblIncome.Text = income.ToString("C2");
            lblExpense.Text = expense.ToString("C2");
            lblBalance.Text = balance.ToString("C2");
        }

        private void LoadJarTotal()
        {
            var (_, to) = GetRange();

           var jars = new Jar().GetJarsByUser(_userId, DateTime.MinValue, to, includeDeleted: true);

            decimal total = jars.Sum(j => j.Balance);

            lblJarTotal.Text = total.ToString("C2");
        }


        private void LoadGoals()
        {
            var (from, to) = GetRange();
            var goals = new Goal().GetGoalsByUser(_userId, from, to, includeArchived: false);

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
                decimal liveBal = jarSvc.GetCurrentBalance(_userId, jarId);
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
                }
                else
                {
                    foreach (var (jarId, pct) in jarSvc.GetJarsWithPercentages(_userId))
                    {
                        decimal share = Math.Floor(amount * pct / 100m * 100m) / 100m;
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
                    }
                }
            }

            // refresh UI
            UpdatePeriodLabel();
            LoadTotals();
            LoadJarTotal();
            LoadGoals();
        }

        protected string snapshotLabelsJson;
        protected string snapshotDatasetsJson;
        private void LoadJarSnapshotChart()
        {
            var snapshotSvc = new JarSnapshot();

            string periodType;

            switch (hdnSelectedPeriod.Value)
            {
                case "day":
                case "week":
                    periodType = "daily";
                    break;

                case "month":
                    periodType = "daily";
                    break;
                case "year":
                    periodType = "monthly";
                    break;
                case "all":
                    periodType = "monthly";
                    break;

                default:
                    periodType = "daily";
                    break;
            }

            var (fromValue, toValue) = GetRange();

            // cap the end to today so nothing after today appears
            DateTime effectiveEnd;
            if (periodType == "daily")
            {
                effectiveEnd = toValue.Date > DateTime.Today ? DateTime.Today : toValue.Date;
            }
            else // monthly
            {
                var requestedEndMonth = new DateTime(toValue.Year, toValue.Month, 1);
                var todayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                effectiveEnd = requestedEndMonth > todayMonth ? todayMonth : requestedEndMonth;
            }

            // clamp and normalize
            DateTime? from = fromValue;
            DateTime? to = toValue;
            bipj.Data.SqlDate.Clamp(ref from, ref to);
            var actualFrom = from ?? DateTime.MinValue;
            // use effectiveEnd for the upper bound of labels/generation
            var actualTo = effectiveEnd;

            // fetch snapshots in the original full range so we can forward-fill up to today
            var snapshots = snapshotSvc.GetSnapshots(_userId, periodType, actualFrom, toValue);
            if (snapshots == null || !snapshots.Any())
            {
                snapshotLabelsJson = "[]";
                snapshotDatasetsJson = "[]";
                return;
            }

            // build the full date series from period start to effectiveEnd (today capped)
            List<DateTime> allDates = new List<DateTime>();
            if (periodType == "daily")
            {
                for (var dt = actualFrom.Date; dt <= actualTo.Date; dt = dt.AddDays(1))
                    allDates.Add(dt);
            }
            else // monthly
            {
                DateTime currentMonth = new DateTime(actualFrom.Year, actualFrom.Month, 1);
                DateTime endMonth = new DateTime(actualTo.Year, actualTo.Month, 1);
                while (currentMonth <= endMonth)
                {
                    allDates.Add(currentMonth);
                    currentMonth = currentMonth.AddMonths(1);
                }
            }

            // group snapshots by normalized date
            var groupedData = snapshots
                .GroupBy(s =>
                    periodType == "daily"
                        ? s.SnapshotDate.Date
                        : new DateTime(s.SnapshotDate.Year, s.SnapshotDate.Month, 1))
                .ToDictionary(g => g.Key, g => g.ToList());

            // format labels
            var labels = allDates
                .Select(d => d.ToString(periodType == "daily" ? "dd MMM" : "MMM yyyy"))
                .ToList();

            var jars = new Jar().GetJarsByUser(_userId);
            var datasets = new List<object>();

            foreach (var jar in jars)
            {
                var dataPoints = new List<decimal>();
                decimal lastKnownBalance = 0m;

                foreach (var date in allDates)
                {
                    if (groupedData.TryGetValue(date, out var snapsForDate))
                    {
                        var snap = snapsForDate.FirstOrDefault(s => s.JarId == jar.JarId);
                        if (snap != null)
                        {
                            lastKnownBalance = snap.Balance;
                            dataPoints.Add(lastKnownBalance);
                            continue;
                        }
                    }
                    // forward-fill using last known balance (or zero if none yet)
                    dataPoints.Add(lastKnownBalance);
                }

                datasets.Add(new
                {
                    label = jar.JarName,
                    data = dataPoints,
                    borderWidth = 2,
                    fill = false,
                    spanGaps = false
                });
            }

            snapshotLabelsJson = JsonConvert.SerializeObject(labels);
            snapshotDatasetsJson = JsonConvert.SerializeObject(datasets);
        }

    }
}
