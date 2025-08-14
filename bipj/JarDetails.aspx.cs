using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using bipj.Models;

namespace bipj
{
    public partial class JarDetails : System.Web.UI.Page
    {
        private int _userId;
        private int _jarId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || string.IsNullOrEmpty(Request.QueryString["jarId"]))
            {
                Response.Redirect("Jars.aspx");
                return;
            }

            _userId = Convert.ToInt32(Session["UserId"]);
            _jarId = Convert.ToInt32(Request.QueryString["jarId"]);

            if (!IsPostBack)
            {
                txtExpenseAmount.Attributes["step"] = "0.01";
                txtIncomeAmount.Attributes["step"] = "0.01";

                hdnSelectedPeriod.Value = "month";
                hdnSelectedDate.Value = DateTime.Now.ToString("yyyy-MM");

                LoadJarSummary();
                LoadTransactions();
                UpdateSelectedDateLabel();
                LoadTargetJarDropdown();
            }

        }

        protected void btnPeriodChange_Click(object sender, EventArgs e)
        {
            UpdateSelectedDateLabel();
            LoadJarSummary();
            LoadTransactions();
        }

        protected void rptTransactions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var row = (HtmlGenericControl)e.Item.FindControl("rowDiv");
            var txn = (DisplayTxn)e.Item.DataItem;

            if (txn.TransactionType == "Transfer") row.Attributes["class"] += " disabled-transaction";
        }

        protected void btnSubmitEntry_Click(object sender, EventArgs e)
        {
            string txnTypeStr = hdnTransactionType.Value;
            string name = txnTypeStr == "Expense" ? txtExpenseName.Text.Trim() : txtIncomeName.Text.Trim();

            if (!decimal.TryParse(txnTypeStr == "Expense" ? txtExpenseAmount.Text.Trim() : txtIncomeAmount.Text.Trim(), out decimal amount) || amount <= 0) return;
            if (!DateTime.TryParse(txnTypeStr == "Expense" ? txtExpenseDate.Text.Trim() : txtIncomeDate.Text.Trim(), out DateTime date)) return;

            var txn = new JarTransaction
            {
                UserId = _userId,
                JarId = _jarId,
                Name = name,
                Amount = amount,
                Date = date,
                TransactionType = txnTypeStr == "Expense" ? TxnType.Expense : TxnType.Income,
                Category = null
            };
            txn.InsertTransaction();

            var jar = new Jar().GetJarById(_jarId, _userId);
            if (jar != null)
            {
                decimal net = txn.GetTransactionSum(_userId, _jarId);                
            }

            LoadJarSummary();
            LoadTransactions();

            Response.Redirect(Request.RawUrl);
        }

        protected void btnUpdateTxn_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hdnEditTxnId.Value, out int txnId)) return;

            var txn = new JarTransaction().GetTransactionById(txnId, _userId);
            if (txn == null || txn.TransactionType == TxnType.Transfer) return;

            if (!decimal.TryParse(txtTxnAmount.Text.Trim(), out decimal amt) || amt <= 0) return;
            if (!DateTime.TryParse(txtTxnDate.Text.Trim(), out DateTime dt)) return;

            txn.Name = txtTxnName.Text.Trim();
            txn.Amount = amt;
            txn.Date = dt;
            txn.UpdateTransaction();

            LoadJarSummary();
            LoadTransactions();

            Response.Redirect(Request.RawUrl);
        }

        protected void btnConfirmTxnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hdnDeleteTxnId.Value, out int txnId)) return;

            var txn = new JarTransaction().GetTransactionById(txnId, _userId);
            if (txn == null || txn.TransactionType == TxnType.Transfer) return;

            txn.DeleteTransaction();
            LoadJarSummary();
            LoadTransactions();

            Response.Redirect(Request.RawUrl);

        }

        protected void btnMoveFunds_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(ddlTargetJar.SelectedValue, out int targetJarId)) return;
            if (!decimal.TryParse(txtMoveAmount.Text.Trim(), out decimal amount)
                || amount <= 0
                || targetJarId == _jarId)
                return;

            var jarSvc = new Jar();
            var sourceJar = jarSvc.GetJarById(_jarId, _userId);
            var destJar = jarSvc.GetJarById(targetJarId, _userId);
            if (sourceJar == null || destJar == null) return;

            // balance guard using transaction-based balance
            decimal sourceLiveBal = jarSvc.GetCurrentBalance(_userId, _jarId);
            if (amount > sourceLiveBal)
            {
                ScriptManager.RegisterStartupScript(
                    this, GetType(), "insuff",
                    "showInsufficientFundsModal();",
                    true);
                return;
            }

            string transferName = $"Move {amount:C} from {sourceJar.JarName} to {destJar.JarName}";

            // perform transfer (records two transactions and invalidates cache)
            new JarTransaction().InsertTransfer(_userId, _jarId, targetJarId, amount, transferName);

            // refresh UI
            LoadJarSummary();
            LoadTransactions();
        }

        // ---------------------------------------------------------------------

        private sealed class DisplayTxn
        {
            public int TransactionId { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public string Category { get; set; }
            public string TransactionType { get; set; } // Income / Expense / Transfer (normalized below)
            public decimal Amount { get; set; }
            public string RowCss { get; set; }
        }

        private void LoadTransactions()
        {
            string period = hdnSelectedPeriod.Value ?? "month";
            string sel = hdnSelectedDate.Value;

            DateTime from, to;
            GetRange(period, sel, out from, out to);

            var raw = new JarTransaction().GetTransactionsByJar(_userId, _jarId);

            if (period != "all")
                raw = raw.Where(t => t.Date >= from && t.Date < to).ToList();

            var display = raw.Select(t =>
            {
                bool isTransfer = t.TransactionType == TxnType.Transfer;
                bool outflow = isTransfer && t.Amount < 0;

                return new DisplayTxn
                {
                    TransactionId = t.TransactionId,
                    Name = t.Name,
                    Date = t.Date,
                    Category = t.Category,
                    TransactionType = isTransfer ? "Transfer" : t.TransactionType.ToString(),
                    Amount = t.Amount,
                    RowCss = isTransfer ? "disabled-transaction" : "clickable-card"
                };
            })
             .OrderByDescending(d => d.Date)
             .ThenByDescending(d => d.TransactionId)
             .ToList();

            rptTransactions.DataSource = display;
            rptTransactions.DataBind();
        }

        protected string FormatAmount(object amountObj)
        {
            if (!decimal.TryParse(Convert.ToString(amountObj), out decimal amt)) amt = 0;
            string sign = amt < 0 ? "−" : "+";
            return $"{sign}{Math.Abs(amt):C2}";
        }

        protected string AmountCss(object amountObj)
        {
            if (!decimal.TryParse(Convert.ToString(amountObj), out decimal amt)) amt = 0;
            return amt < 0 ? "text-danger fs-5 fw-bold" : "text-success fs-5 fw-bold";
        }

        private void LoadJarSummary()
        {
            var jarSvc = new Jar();
            var jar = jarSvc.GetJarById(_jarId, _userId);
            if (jar == null || jar.UserId != _userId)
            {
                Response.Redirect("Jars.aspx");
                return;
            }

            lblJarName.Text = jar.JarName;

            string period = hdnSelectedPeriod.Value ?? "month";
            string sel = hdnSelectedDate.Value;
            DateTime from, to;
            GetRange(period, sel, out from, out to);

            var txnMgr = new JarTransaction();
            var txns = txnMgr.GetTransactionsByJar(_userId, _jarId)
                             .Where(t => t.Date >= from && t.Date < to)
                             .ToList();

            decimal income = txns
                .Where(t => t.TransactionType == TxnType.Income
                            || (t.TransactionType == TxnType.Transfer && t.Amount > 0))
                .Sum(t => t.Amount);

            decimal expense = txns
                .Where(t => t.TransactionType == TxnType.Expense
                            || (t.TransactionType == TxnType.Transfer && t.Amount < 0))
                .Sum(t => Math.Abs(t.Amount));

            decimal transferIn = txns
                .Where(t => t.TransactionType == TxnType.Transfer && t.Amount > 0)
                .Sum(t => t.Amount);

            decimal transferOut = txns
                .Where(t => t.TransactionType == TxnType.Transfer && t.Amount < 0)
                .Sum(t => -t.Amount);

            lblIncomeTotal.Text = $"${income:F2}";
            lblExpenseTotal.Text = $"${expense:F2}";
            lblTransferIn.Text = transferIn > 0 ? $"(Transferred ${transferIn:F2} in)" : "";
            lblTransferOut.Text = transferOut > 0 ? $"(Transferred ${transferOut:F2} out)" : "";

            decimal periodBalance = income - expense;
            lblBalance.Text = $"${periodBalance:F2}";
            lblBalance.CssClass = periodBalance >= 0
                                  ? "fw-bold fs-5 text-success"
                                  : "fw-bold fs-5 text-danger";

            // use the new GetCurrentBalance instead of the removed GetLiveBalance
            decimal liveBalance = jarSvc.GetCurrentBalance(_userId, _jarId);
            hdnCurrentJarBalance.Value = liveBalance.ToString("F2");
        }


        private void LoadTargetJarDropdown()
        {
            var jar = new Jar().GetJarById(_jarId, _userId);
            lblExpenseJarName.Text = jar.JarName;
            lblIncomeJarName.Text = jar.JarName;

            ddlTargetJar.DataSource = new Jar().GetJarsByUser(_userId).Where(j => j.JarId != _jarId).ToList();
            ddlTargetJar.DataTextField = "JarName";
            ddlTargetJar.DataValueField = "JarId";
            ddlTargetJar.DataBind();
        }

        private void UpdateSelectedDateLabel()
        {
            string period = hdnSelectedPeriod.Value;
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
