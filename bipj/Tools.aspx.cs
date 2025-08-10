using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using bipj.Models;
using Microsoft.VisualBasic.FileIO;    // for CSV
using OfficeOpenXml;                   // for XLSX
using iText.Kernel.Pdf;                // for PDF
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bipj
{
    public partial class Tools : Page
    {
        private int _userId;
        private static readonly HttpClient _httpClient = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }
            _userId = Convert.ToInt32(Session["UserId"]);

            if (!IsPostBack)
            {
                BindJarDropDown(ddlDefaultJar);
                pnlPreview.Visible = false;
                pnlDone.Visible = false;
            }
        }

        // 1) UPLOAD & PREVIEW (synchronous handler)
        protected void btnParse_Click(object sender, EventArgs e)
        {
            if (!fuStatement.HasFile) return;

            List<RowVm> rows;
            string ext = Path.GetExtension(fuStatement.FileName).ToLowerInvariant();

            using (var msRaw = new MemoryStream(fuStatement.FileBytes))
            {
                if (ext == ".pdf")
                {
                    // PDF → raw lines → GPT → fallback parser
                    var lines = ExtractLinesFromPdf(msRaw);
                    var rawText = string.Join("\n", lines);
                    var gpt = ParseWithGptAsync(rawText).GetAwaiter().GetResult();
                    rows = gpt ?? ParsePdf(new MemoryStream(fuStatement.FileBytes));
                }
                else if (ext == ".csv")
                {
                    // CSV → fallback parse → to lines → GPT → fallback
                    var fallback = ParseCsv(new MemoryStream(fuStatement.FileBytes));
                    var lines = fallback.Select(r =>
                        $"{r.Date:yyyy-MM-dd} {r.Description} {(r.Income > 0 ? r.Income : -r.Expense)}");
                    var rawText = string.Join("\n", lines);
                    var gpt = ParseWithGptAsync(rawText).GetAwaiter().GetResult();
                    rows = gpt ?? fallback;
                }
                else if (ext == ".xlsx")
                {
                    var fallback = ParseXlsx(new MemoryStream(fuStatement.FileBytes));
                    var lines = fallback.Select(r =>
                        $"{r.Date:yyyy-MM-dd} {r.Description} {(r.Income > 0 ? r.Income : -r.Expense)}");
                    var rawText = string.Join("\n", lines);
                    var gpt = ParseWithGptAsync(rawText).GetAwaiter().GetResult();
                    rows = gpt ?? fallback;
                }
                else
                {
                    // unsupported file type
                    return;
                }
            }

            // Apply default‐jar fallback
            int defJarId = new Jar().GetDefaultJar(_userId)?.JarId ?? 0;
            foreach (var r in rows)
                if (r.JarId == 0) r.JarId = defJarId;

            // Bind to preview grid
            Session["importRows"] = rows;
            gvPreview.DataSource = rows;
            gvPreview.DataBind();

            pnlUpload.Visible = false;
            pnlPreview.Visible = true;
        }

        // 1a) User changed default‐jar dropdown
        protected void ddlDefaultJar_SelectedIndexChanged(object sender, EventArgs e)
        {
            var rows = Session["importRows"] as List<RowVm>;
            if (rows == null) return;

            int newJar = int.Parse(ddlDefaultJar.SelectedValue);
            foreach (var r in rows) r.JarId = newJar;

            gvPreview.DataSource = rows;
            gvPreview.DataBind();
        }

        // Bind per‐row jar dropdown in GridView
        protected void gvPreview_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            var ddl = (DropDownList)e.Row.FindControl("ddlRowJar");
            BindJarDropDown(ddl);
            var vm = (RowVm)e.Row.DataItem;
            ddl.SelectedValue = vm.JarId.ToString();
        }

        // 2) IMPORT SELECTED ROWS
        protected void btnImport_Click(object sender, EventArgs e)
        {
            var rows = Session["importRows"] as List<RowVm>;
            if (rows == null) return;

            // Capture per‐row checkbox & jar choice
            for (int i = 0; i < gvPreview.Rows.Count; i++)
            {
                var chk = (CheckBox)gvPreview.Rows[i].FindControl("chkImport");
                var ddl = (DropDownList)gvPreview.Rows[i].FindControl("ddlRowJar");
                rows[i].Import = chk?.Checked ?? false;
                rows[i].JarId = ddl != null ? int.Parse(ddl.SelectedValue) : rows[i].JarId;
            }

            var txnSvc = new JarTransaction();
            var jarSvc = new Jar();
            var touched = new HashSet<int>();

            foreach (var r in rows.Where(r => r.Import))
            {
                decimal amt = r.Income > 0 ? r.Income : r.Expense;
                var type = r.Income > 0 ? TxnType.Income : TxnType.Expense;

                new JarTransaction
                {
                    UserId = _userId,
                    JarId = r.JarId,
                    Name = r.Description,
                    Amount = amt,
                    Date = r.Date,
                    TransactionType = type,
                    Category = "Import"
                }.InsertTransaction();

                touched.Add(r.JarId);
            }

            // Recalculate each touched jar’s balance
            foreach (int jarId in touched)
            {
                decimal net = txnSvc.GetTransactionSum(_userId, jarId);
                var jar = jarSvc.GetJarById(jarId, _userId);
                if (jar == null) continue;                
            }

            Session.Remove("importRows");
            pnlPreview.Visible = false;
            pnlDone.Visible = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Session.Remove("importRows");
            pnlPreview.Visible = false;
            pnlUpload.Visible = true;
        }

        // === GPT CALL HELPER ===
        private async Task<List<RowVm>> ParseWithGptAsync(string rawText)
        {
            try
            {
                var apiKey = ConfigurationManager.AppSettings["OpenAI_API_Key"];
                if (string.IsNullOrEmpty(apiKey)) return null;

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                var payload = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a JSON parser for bank statements." },
                        new { role = "user", content = "Parse into JSON array of {date,description,income,expense}:\n" + rawText }
                    },
                    temperature = 0
                };

                string body = JsonConvert.SerializeObject(payload);
                var resp = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    new StringContent(body, Encoding.UTF8, "application/json")
                );
                if (!resp.IsSuccessStatusCode) return null;

                var json = await resp.Content.ReadAsStringAsync();
                var doc = JObject.Parse(json);
                var content = doc["choices"]?[0]?["message"]?["content"]?.ToString();
                return string.IsNullOrEmpty(content)
                    ? null
                    : JsonConvert.DeserializeObject<List<RowVm>>(content);
            }
            catch
            {
                return null;
            }
        }

        // === ORIGINAL PARSERS & HELPERS ===

        private void BindJarDropDown(DropDownList ddl)
        {
            var jars = new Jar().GetJarsByUser(_userId);
            ddl.DataSource = jars;
            ddl.DataTextField = "JarName";
            ddl.DataValueField = "JarId";
            ddl.DataBind();
        }

        private List<RowVm> ParseCsv(Stream s)
        {
            var list = new List<RowVm>();
            using (var parser = new TextFieldParser(s))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                string[] headers = null;
                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    if (headers == null) { headers = fields; continue; }
                    var row = MapRow(headers, fields);
                    if (row != null) list.Add(row);
                }
            }
            return list;
        }

        private List<RowVm> ParseXlsx(Stream s)
        {
            ExcelPackage.License.SetNonCommercialPersonal("FinClarity");

            var list = new List<RowVm>();
            using (var pkg = new ExcelPackage(s))
            {
                var ws = pkg.Workbook.Worksheets[0];
                int rMax = ws.Dimension.End.Row, cMax = ws.Dimension.End.Column;

                var headers = new string[cMax];
                for (int c = 1; c <= cMax; c++)
                    headers[c - 1] = ws.Cells[1, c].Text.Trim();

                for (int r = 2; r <= rMax; r++)
                {
                    var vals = new string[cMax];
                    for (int c = 1; c <= cMax; c++)
                        vals[c - 1] = ws.Cells[r, c].Text;
                    var row = MapRow(headers, vals);
                    if (row != null) list.Add(row);
                }
            }
            return list;
        }

        private List<RowVm> ParsePdf(Stream s)
        {
            // 1) grab raw lines
            var rawLines = ExtractLinesFromPdf(s);

            // 2) stitch multi‑line descriptions into single records
            var records = new List<string>();
            string current = null;
            var dateStart = new Regex(@"^\d{2}/\d{2}/\d{4}");
            foreach (var line in rawLines)
            {
                if (dateStart.IsMatch(line))
                {
                    if (current != null) records.Add(current);
                    current = line;
                }
                else if (current != null)
                {
                    current += " " + line;
                }
            }
            if (current != null) records.Add(current);

            // 3) parse each record by finding all decimal amounts
            var list = new List<RowVm>();
            foreach (var rec in records)
            {
                // pull date
                var dateStr = rec.Substring(0, 10);
                if (!DateTime.TryParseExact(dateStr,
                        "dd/MM/yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var date))
                    continue;

                // rest of the line after the date
                var rest = rec.Substring(10).Trim();

                // find all "123,456.78" tokens
                var matches = Regex.Matches(rest, @"\d{1,3}(?:,\d{3})*\.\d{2}")
                                   .Cast<Match>()
                                   .Select(m => m.Value)
                                   .ToList();
                if (matches.Count < 2)
                    continue;     // need at least [amt, balance]

                // last = balance (we won't use it here)
                // second‑to‑last = deposit
                // third‑to‑last (if >=3) = withdrawal
                decimal withdraw = 0m;
                decimal deposit = 0m;

                if (matches.Count >= 3)
                    withdraw = Decimal.Parse(matches[matches.Count - 3],
                                NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                                CultureInfo.InvariantCulture);

                deposit = Decimal.Parse(matches[matches.Count - 2],
                            NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                            CultureInfo.InvariantCulture);

                // everything before the first amount is the description
                var firstAmt = matches[0];
                int idx = rest.IndexOf(firstAmt, StringComparison.Ordinal);
                var desc = idx > 0
                    ? rest.Substring(0, idx).Trim()
                    : "(no description)";

                list.Add(new RowVm
                {
                    Date = date,
                    Description = desc,
                    Expense = withdraw,
                    Income = deposit,
                    JarId = 0,
                    Import = true
                });
            }

            return list;
        }

        private List<string> ExtractLinesFromPdf(Stream pdfStream)
        {
            var lines = new List<string>();
            var reader = new PdfReader(new MemoryStream(((MemoryStream)pdfStream).ToArray()));
            var pdf = new PdfDocument(reader);
            var strat = new SimpleTextExtractionStrategy();

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                var pageText = PdfTextExtractor.GetTextFromPage(pdf.GetPage(i), strat);

                foreach (var l in pageText
                    .Split('\n')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    // drop header row
                    .Where(x => !x.StartsWith("Date", StringComparison.OrdinalIgnoreCase))
                    // drop Balance Brought/Carried Forward lines
                    .Where(x => !Regex.IsMatch(x,
                        @"^Balance\s+(Brought|Carried)\s+Forward",
                        RegexOptions.IgnoreCase)))
                {
                    lines.Add(l);
                }
            }

            return lines;
        }

        private RowVm MapRow(string[] headers, string[] vals)
        {
            var dateStr = Get(headers, vals, "Date", "Transaction Date", "Posting Date");
            var desc = Get(headers, vals, "Description", "Details", "Memo");
            var inStr = Get(headers, vals, "Income", "Credit", "Amount In");
            var exStr = Get(headers, vals, "Expense", "Debit", "Amount Out");

            if (string.IsNullOrWhiteSpace(dateStr) ||
                string.IsNullOrWhiteSpace(desc))
                return null;

            if (!DateTime.TryParse(dateStr, out var date))
                date = DateTime.Today;

            decimal.TryParse(inStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var inc);
            decimal.TryParse(exStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var exp);

            if (inc == 0 && exp == 0)
            {
                var amtStr = Get(headers, vals, "Amount");
                if (decimal.TryParse(amtStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var a))
                {
                    inc = a > 0 ? a : 0;
                    exp = a < 0 ? Math.Abs(a) : 0;
                }
            }

            return new RowVm
            {
                Date = date,
                Description = desc,
                Income = inc,
                Expense = exp,
                JarId = 0,
                Import = true
            };
        }

        private string Get(string[] headers, string[] vals, params string[] names)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                foreach (var n in names)
                {
                    if (headers[i].Equals(n, StringComparison.OrdinalIgnoreCase))
                        return vals[i];
                }
            }
            return "";
        }

        // Simple VM for import
        private sealed class RowVm
        {
            public DateTime Date { get; set; }
            public string Description { get; set; }
            public decimal Income { get; set; }
            public decimal Expense { get; set; }
            public int JarId { get; set; }
            public bool Import { get; set; }
        }

        protected void btnRestartAllJars_Click(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) return;
            int userId = Convert.ToInt32(Session["UserId"]);

            pnlDone.Visible = true;
            pnlDone.CssClass = "alert alert-info mt-4";
            litDoneMessage.Text = "Starting reset…";

            try
            {
                Jar.ResetAllJarsForUser_Lite(userId);  // fast & FK-safe

                pnlDone.Visible = true;
                pnlDone.CssClass = "alert alert-success mt-4";
                litDoneMessage.Text = "All transactions and snapshots cleared. Jars kept.";

                // hide modal after success
                ScriptManager.RegisterStartupScript(this, GetType(), "hideModal",
                    "var m = bootstrap.Modal.getInstance(document.getElementById('confirmRestartModal')); if(m){m.hide();}", true);
            }
            catch (Exception ex)
            {
                pnlDone.Visible = true;
                pnlDone.CssClass = "alert alert-danger mt-4";
                litDoneMessage.Text = "Failed to reset: " + Server.HtmlEncode(ex.Message);
            }
        }

    }
}
