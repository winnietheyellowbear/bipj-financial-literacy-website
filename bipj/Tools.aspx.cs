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
using Microsoft.VisualBasic.FileIO;    // CSV
using OfficeOpenXml;                   // XLSX
using iText.Kernel.Pdf;                // PDF
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

        // Ignore headers/footers/totals from bank PDFs
        private static readonly Regex RxIgnore = new Regex(
            @"^(CURRENCY:|Total\s+Balance|Balance\s+(Brought|Carried)\s+Forward|Account\s+No\.|Date\s*$|Description\s*$|Withdrawal|Deposit|Balance\s*$)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

        // 1) UPLOAD & PREVIEW
        protected void btnParse_Click(object sender, EventArgs e)
        {
            if (!fuStatement.HasFile) return;

            List<RowVm> rows;
            string ext = Path.GetExtension(fuStatement.FileName).ToLowerInvariant();

            using (var msRaw = new MemoryStream(fuStatement.FileBytes))
            {
                if (ext == ".pdf")
                {
                    var lines = ExtractLinesFromPdf(msRaw);
                    var rawText = string.Join("\n", lines);
                    var gpt = ParseWithGptAsync(rawText).GetAwaiter().GetResult();
                    rows = gpt ?? ParsePdf(new MemoryStream(fuStatement.FileBytes));
                }
                else if (ext == ".csv")
                {
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
                    return;
                }
            }

            // Default jar
            int defJarId = new Jar().GetDefaultJar(_userId)?.JarId ?? 0;
            foreach (var r in rows)
                if (r.JarId == 0) r.JarId = defJarId;

            // Normalize (guarantees clean desc + one-sided amount)
            NormalizeRows(rows);

            // Bind preview
            Session["importRows"] = rows;
            gvPreview.DataSource = rows;
            gvPreview.DataBind();

            pnlUpload.Visible = false;
            pnlPreview.Visible = true;
        }

        protected void ddlDefaultJar_SelectedIndexChanged(object sender, EventArgs e)
        {
            var rows = Session["importRows"] as List<RowVm>;
            if (rows == null) return;

            int newJar = int.Parse(ddlDefaultJar.SelectedValue);
            foreach (var r in rows) r.JarId = newJar;

            gvPreview.DataSource = rows;
            gvPreview.DataBind();
        }

        protected void gvPreview_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            var ddl = (DropDownList)e.Row.FindControl("ddlRowJar");
            BindJarDropDown(ddl);
            var vm = (RowVm)e.Row.DataItem;
            ddl.SelectedValue = vm.JarId.ToString();
        }

        // 2) IMPORT
        protected void btnImport_Click(object sender, EventArgs e)
        {
            var rows = Session["importRows"] as List<RowVm>;
            if (rows == null) return;

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

            foreach (int jarId in touched)
            {
                decimal _ = txnSvc.GetTransactionSum(_userId, jarId);
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

        // === GPT PARSER ===
        private async Task<List<RowVm>> ParseWithGptAsync(string rawTable)
        {
            try
            {
                var apiKey = ConfigurationManager.AppSettings["OpenAI_API_Key"];
                if (string.IsNullOrWhiteSpace(apiKey)) return null;

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                var payload = new
                {
                    model = "gpt-5",
                    response_format = new { type = "json_object" },
                    messages = new object[]
                    {
                        new { role = "system", content =
                            "Parse a bank statement table into strict JSON.\n" +
                            "Return ONLY JSON with shape: {\"rows\":[{date,description,income,expense}...]}\n" +
                            "Rules:\n" +
                            "- date format: yyyy-MM-dd\n" +
                            "- description: non-empty, concise merchant/payee text (remove 'DATE : dd/MM/yyyy' etc.)\n" +
                            "- Direction: credit>0 => income, debit>0 => expense; never set both.\n" +
                            "- If a single signed amount exists: + => income, - => expense.\n" +
                            "- Never return empty description; infer if unclear." },
                        new { role = "user", content =
                            "Here is the table (one line per transaction):\n" +
                            rawTable + "\nOutput JSON only." }
                    },
                    temperature = 0
                };

                var body = JsonConvert.SerializeObject(payload);
                var resp = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    new StringContent(body, Encoding.UTF8, "application/json")
                );
                if (!resp.IsSuccessStatusCode) return null;

                var json = await resp.Content.ReadAsStringAsync();
                var doc = JObject.Parse(json);
                var content = doc["choices"]?[0]?["message"]?["content"]?.ToString();
                if (string.IsNullOrWhiteSpace(content)) return null;

                var parsed = JObject.Parse(content);
                var rows = new List<RowVm>();
                foreach (var r in (JArray)parsed["rows"])
                {
                    rows.Add(new RowVm
                    {
                        Date = DateTime.ParseExact((string)r["date"], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Description = ((string)r["description"] ?? "").Trim(),
                        Income = (decimal?)r["income"] ?? 0m,
                        Expense = (decimal?)r["expense"] ?? 0m,
                        JarId = 0,
                        Import = true
                    });
                }

                // Last line of defense
                NormalizeRows(rows);
                return rows;
            }
            catch
            {
                return null;
            }
        }

        // === PARSERS & HELPERS ===

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
            NormalizeRows(list);
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
            NormalizeRows(list);
            return list;
        }

        private List<RowVm> ParsePdf(Stream s)
        {
            // 1) lines
            var rawLines = ExtractLinesFromPdf(s);

            // 2) stitch to dated records
            var records = new List<string>();
            string current = null;
            var dateStart = new Regex(@"^\d{2}/\d{2}/\d{4}");

            foreach (var line in rawLines)
            {
                if (dateStart.IsMatch(line))
                {
                    if (current != null) records.Add(current.Trim());
                    current = line.Trim();
                }
                else
                {
                    if (current != null && !RxIgnore.IsMatch(line))
                        current += " " + line.Trim();
                }
            }
            if (current != null) records.Add(current.Trim());

            // 3) parse records
            var list = new List<RowVm>();

            foreach (var rec in records)
            {
                if (rec.Length < 10) continue;

                var dateStr = rec.Substring(0, 10);
                if (!DateTime.TryParseExact(dateStr, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    continue;

                var rest = rec.Substring(10).Trim();
                if (RxIgnore.IsMatch(rest) || Regex.IsMatch(rest, @"\bTotal\s+Balance\b", RegexOptions.IgnoreCase))
                    continue;

                var matches = Regex.Matches(rest, @"\d{1,3}(?:,\d{3})*\.\d{2}")
                                   .Cast<Match>().Select(m => m.Value).ToList();
                if (matches.Count < 2) continue; // need amount + balance

                decimal withdraw = 0m, deposit = 0m;
                string lastBal = (matches.Count >= 1) ? matches[matches.Count - 1] : null;
                string depTok = (matches.Count >= 2) ? matches[matches.Count - 2] : null;
                string withTok = (matches.Count >= 3) ? matches[matches.Count - 3] : null;

                if (withTok != null)
                    withdraw = decimal.Parse(withTok,
                        NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture);

                if (depTok != null)
                    deposit = decimal.Parse(depTok,
                        NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture);


                // description
                string desc;
                int idxFirstAmt = rest.IndexOf(matches[0], StringComparison.Ordinal);
                desc = idxFirstAmt > 0 ? rest.Substring(0, idxFirstAmt).Trim() : string.Empty;

                if (string.IsNullOrWhiteSpace(desc))
                {
                    string tail = rest;

                    string RemoveLast(string txt, string token)
                    {
                        int i = txt.LastIndexOf(token, StringComparison.Ordinal);
                        return i >= 0 ? txt.Remove(i, token.Length).Trim() : txt;
                    }

                    tail = RemoveLast(tail, lastBal);
                    if (depTok != null) tail = RemoveLast(tail, depTok);
                    if (withTok != null) tail = RemoveLast(tail, withTok);

                    tail = Regex.Replace(tail, @"VALUE\s*DATE\s*:?\s*\d{2}/\d{2}/\d{4}", "", RegexOptions.IgnoreCase);
                    tail = Regex.Replace(tail, @"\bDATE\s*:?\s*\d{2}/\d{2}/\d{4}", "", RegexOptions.IgnoreCase);

                    desc = tail.Trim();
                }

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

            NormalizeRows(list);
            return list;
        }

        private List<string> ExtractLinesFromPdf(Stream pdfStream)
        {
            var lines = new List<string>();

            var ms = pdfStream as MemoryStream ?? new MemoryStream();
            if (!(pdfStream is MemoryStream))
                pdfStream.CopyTo(ms);
            else
                ms = (MemoryStream)pdfStream;

            using (var reader = new PdfReader(new MemoryStream(ms.ToArray())))
            using (var pdf = new PdfDocument(reader))
            {
                var strat = new SimpleTextExtractionStrategy();

                for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
                {
                    var pageText = PdfTextExtractor.GetTextFromPage(pdf.GetPage(i), strat);

                    foreach (var l in pageText
                        .Split('\n')
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Where(x => !RxIgnore.IsMatch(x)))
                    {
                        lines.Add(l);
                    }
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

            if (string.IsNullOrWhiteSpace(dateStr))
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
                Description = (desc ?? "").Trim(),
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

        // Normalizer: clean desc + force one-sided amount
        private static void NormalizeRows(List<RowVm> rows)
        {
            foreach (var r in rows)
            {
                r.Description = CleanDesc(r.Description);

                if (string.IsNullOrWhiteSpace(r.Description))
                    r.Description = r.Income > 0m ? "Deposit"
                                  : r.Expense > 0m ? "Payment"
                                  : "Transaction";

                if (r.Income > 0m && r.Expense > 0m)
                {
                    if (r.Income >= r.Expense)
                    {
                        r.Income = Math.Round(r.Income - r.Expense, 2);
                        r.Expense = 0m;
                    }
                    else
                    {
                        r.Expense = Math.Round(r.Expense - r.Income, 2);
                        r.Income = 0m;
                    }
                }
            }
        }

        private static string CleanDesc(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            s = Regex.Replace(s, @"VALUE\s*DATE\s*:?\s*\d{2}/\d{2}/\d{4}", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\bDATE\s*:?\s*\d{2}/\d{2}/\d{4}", "", RegexOptions.IgnoreCase);

            if (Regex.IsMatch(s, @"Advice\s+Funds\s+Transfer", RegexOptions.IgnoreCase))
                s = Regex.Replace(s, @"Advice\s+Funds\s+Transfer\s+\S+\s*:\s*", "Funds transfer ", RegexOptions.IgnoreCase);

            s = Regex.Replace(s, @"\s{2,}", " ").Trim(' ', '-', ':');
            return s;
        }

        // VM
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
                Jar.ResetAllJarsForUser_Lite(userId);

                pnlDone.Visible = true;
                pnlDone.CssClass = "alert alert-success mt-4";
                litDoneMessage.Text = "All transactions and snapshots cleared. Jars kept.";

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
