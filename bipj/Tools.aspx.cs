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
        // ============================================================
        // CORE FIELDS / CONSTANTS
        // ============================================================
        private int _userId;
        private static readonly HttpClient _httpClient = new HttpClient();

        // Ignore headers/footers/totals from bank PDFs
        private static readonly Regex RxIgnore = new Regex(
            @"^(CURRENCY:|Total\s+Balance|Balance\s+(Brought|Carried)\s+Forward|Account\s+No\.|Date\s*$|Description\s*$|Withdrawal|Deposit|Balance\s*$)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);


        // ============================================================
        // PAGE LIFECYCLE + UI WIRE-UP
        // (Controls: ddlDefaultJar, gvPreview, pnlUpload, pnlPreview, pnlDone, pnlAnalysis)
        // ============================================================
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
                // pnlAnalysis will be turned on by Analyze handler when needed
            }
        }

        // DropDown: change default jar for preview rows
        protected void ddlDefaultJar_SelectedIndexChanged(object sender, EventArgs e)
        {
            var rows = Session["importRows"] as List<RowVm>;
            if (rows == null) return;

            int newJar = int.Parse(ddlDefaultJar.SelectedValue);
            foreach (var r in rows) r.JarId = newJar;

            gvPreview.DataSource = rows;
            gvPreview.DataBind();
        }

        // Grid: bind row-level jar dropdown for each previewed transaction
        protected void gvPreview_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            var ddl = (DropDownList)e.Row.FindControl("ddlRowJar");
            BindJarDropDown(ddl);

            var vm = (RowVm)e.Row.DataItem;
            ddl.SelectedValue = vm.JarId.ToString();
        }


        // ============================================================
        // FEATURE: UPLOAD & PREVIEW
        // - btnParse_Click
        // - ParseWithGptAsync (for parsing raw table text -> rows)
        // - ParseCsv / ParseXlsx / ParsePdf / ExtractLinesFromPdf / MapRow / Get
        // - NormalizeRows / CleanDesc
        // ============================================================
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
                    // Try GPT parse first; fallback to internal PDF parser
                    var gpt = ParseWithGptAsync(rawText).GetAwaiter().GetResult();
                    rows = gpt ?? ParsePdf(new MemoryStream(fuStatement.FileBytes));
                }
                else if (ext == ".csv")
                {
                    var fallback = ParseCsv(new MemoryStream(fuStatement.FileBytes));
                    var lines = fallback.Select(r => $"{r.Date:yyyy-MM-dd} {r.Description} {(r.Income > 0 ? r.Income : -r.Expense)}");
                    var rawText = string.Join("\n", lines);
                    var gpt = ParseWithGptAsync(rawText).GetAwaiter().GetResult();
                    rows = gpt ?? fallback;
                }
                else if (ext == ".xlsx")
                {
                    var fallback = ParseXlsx(new MemoryStream(fuStatement.FileBytes));
                    var lines = fallback.Select(r => $"{r.Date:yyyy-MM-dd} {r.Description} {(r.Income > 0 ? r.Income : -r.Expense)}");
                    var rawText = string.Join("\n", lines);
                    var gpt = ParseWithGptAsync(rawText).GetAwaiter().GetResult();
                    rows = gpt ?? fallback;
                }
                else
                {
                    return;
                }
            }

            // Default jar assignment
            int defJarId = new Jar().GetDefaultJar(_userId)?.JarId ?? 0;
            foreach (var r in rows)
                if (r.JarId == 0) r.JarId = defJarId;

            // Normalize descriptions + enforce one-sided amount
            NormalizeRows(rows);

            // Bind preview
            Session["importRows"] = rows;
            gvPreview.DataSource = rows;
            gvPreview.DataBind();

            pnlUpload.Visible = false;
            pnlPreview.Visible = true;
            pnlAnalysis.Visible = false; // Clear previous analysis if any
        }

        // --- GPT PARSER (for parsing statement text -> structured rows) ---
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

                NormalizeRows(rows);
                return rows;
            }
            catch
            {
                return null;
            }
        }

        // --- CSV / XLSX / PDF parsers + helpers ---
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
            var rawLines = ExtractLinesFromPdf(s);

            // Stitch lines into records starting with dd/MM/yyyy
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

            // Parse records
            var list = new List<RowVm>();
            foreach (var rec in records)
            {
                if (rec.Length < 10) continue;

                var dateStr = rec.Substring(0, 10);
                if (!DateTime.TryParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
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
                    withdraw = decimal.Parse(withTok, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

                if (depTok != null)
                    deposit = decimal.Parse(depTok, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

                // Description extraction
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
                foreach (var n in names)
                    if (headers[i].Equals(n, StringComparison.OrdinalIgnoreCase))
                        return vals[i];
            return "";
        }

        // Normalize: clean desc + force one-sided amount
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


        // ============================================================
        // FEATURE: ANALYZE (HYBRID = LOCAL CRUNCH + GPT CATEGORIZATION/ADVICE)
        // - btnAnalyze_Click
        // - DTOs: SimpleTxn, GptHybridOut, RecurringVm
        // - Helpers: CanonicalMerchant, KeywordToCategory, LocalKeywordCategory
        // - Aggregations: AggregateByCategory, DetectRecurring
        // - GPT: CallGptHybridAsync + FallbackHybrid
        // ============================================================
        protected async void btnAnalyze_Click(object sender, EventArgs e)
        {
            var rows = Session["importRows"] as List<RowVm>;
            if (rows == null || rows.Count == 0)
            {
                pnlAnalysis.Visible = true;
                litAnalysisHtml.Text = "<div class='alert alert-warning mb-0'>No data to analyze. Upload a statement first.</div>";
                return;
            }

            // ✅ Apply current selections from the grid
            for (int i = 0; i < gvPreview.Rows.Count; i++)
            {
                var chk = (CheckBox)gvPreview.Rows[i].FindControl("chkImport");
                var ddl = (DropDownList)gvPreview.Rows[i].FindControl("ddlRowJar");
                rows[i].Import = chk?.Checked ?? false;
                if (ddl != null) rows[i].JarId = int.Parse(ddl.SelectedValue);
            }

            // ✅ Only analyze rows user ticked for import
            var working = rows.Where(r => r.Import).ToList();
            if (working.Count == 0)
            {
                pnlAnalysis.Visible = true;
                litAnalysisHtml.Text = "<div class='alert alert-warning mb-0'>No rows selected for analysis.</div>";
                return;
            }

            // ✅ Pick latest month in the selected data
            var latestMonth = working
                .Where(r => r.Expense > 0m)
                .OrderByDescending(r => r.Date)
                .Select(r => new DateTime(r.Date.Year, r.Date.Month, 1))
                .FirstOrDefault();

            if (latestMonth == default)
                latestMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var monthStart = latestMonth;
            var monthEnd = monthStart.AddMonths(1);

            // ✅ This-month subset for charts and category totals
            var thisMonthRows = working
                .Where(r => r.Date >= monthStart && r.Date < monthEnd && r.Expense > 0m)
                .ToList();

            // ✅ Compact summary for GPT (last 120 days from selected data)
            var cutoff = DateTime.Today.AddDays(-120);
            var sample = working
                .Where(r => r.Date >= cutoff)
                .Select(r => new SimpleTxn
                {
                    date = r.Date.ToString("yyyy-MM-dd"),
                    description = CanonicalMerchant(r.Description ?? ""),
                    amount = r.Expense > 0 ? Math.Round(r.Expense, 2) : -Math.Round(r.Income, 2) // +expense / -income
                })
                .Where(t => t.amount != 0m)
                .Take(400)
                .ToList();

            // ✅ GPT semantic categories + advice (fallback to local if API not set or fails)
            var gpt = await CallGptHybridAsync(sample);

            var sb = new StringBuilder();

            // Advice
            if (gpt != null && gpt.advice != null && gpt.advice.Count > 0)
            {
                sb.Append("<div class='mb-3'><h6 class='fw-semibold'>Top Suggestions</h6><ol class='mb-0'>");
                foreach (var tip in gpt.advice.Take(6))
                    sb.Append("<li>").Append(Server.HtmlEncode(tip)).Append("</li>");
                sb.Append("</ol></div>");
            }
            else
            {
                if (gpt != null && gpt.aiRan)
                    sb.Append("<div class='mb-3 text-muted fst-italic'>AI analyzed your data but had no suggestions to add.</div>");
                else
                    sb.Append("<div class='mb-3 text-muted fst-italic'>No AI-powered suggestions available (service unavailable).</div>");
            }


            // Category totals (prefer GPT merchant->category map)
            sb.Append($"<div class='mb-2'><h6 class='fw-semibold'>By Category — {monthStart:MMM yyyy}</h6>");
            var catTotals = AggregateByCategory(thisMonthRows, gpt?.categories);
            if (catTotals.Count == 0)
            {
                sb.Append("<div class='text-muted'>No expenses detected for this month.</div>");
            }
            else
            {
                sb.Append("<div class='small'>");
                foreach (var c in catTotals.OrderByDescending(c => c.Value))
                {
                    sb.Append("<div class='d-flex justify-content-between'><span>")
                      .Append(Server.HtmlEncode(c.Key))
                      .Append("</span><span>$")
                      .Append(c.Value.ToString("N2"))
                      .Append("</span></div>");
                }
                sb.Append("</div>");
            }
            sb.Append("</div>");

            // Recurring merchants (local)
            var recurring = DetectRecurring(thisMonthRows, gpt?.categories)
                .OrderByDescending(x => x.Total)
                .Take(6)
                .ToList();

            if (recurring.Count > 0)
            {
                sb.Append("<hr/><div class='mb-2'><h6 class='fw-semibold'>Recurring Charges (This Month)</h6><div class='small'>");
                foreach (var r in recurring)
                {
                    sb.Append("<div class='d-flex justify-content-between'><span>")
                      .Append(Server.HtmlEncode(r.Merchant))
                      .Append("</span><span>$")
                      .Append(r.Total.ToString("N2"))
                      .Append("</span></div>");
                }
                sb.Append("</div></div>");
            }

            pnlAnalysis.Visible = true;
            litAnalysisHtml.Text = sb.ToString();
        }

        // --- DTOs for Analyze ---
        private sealed class SimpleTxn
        {
            public string date { get; set; }        // yyyy-MM-dd
            public string description { get; set; } // canonical merchant-like
            public decimal amount { get; set; }     // +expense / -income
        }

        private sealed class GptHybridOut
        {
            public Dictionary<string, string> categories { get; set; } // merchant -> category
            public List<string> advice { get; set; }
            public bool aiRan { get; set; } // true when GPT succeeded
        }

        private sealed class RecurringVm
        {
            public string Merchant { get; set; }
            public decimal Total { get; set; }
        }

        // --- Categorization helpers ---
        private static string CanonicalMerchant(string desc)
        {
            var s = (desc ?? "").ToUpperInvariant();
            s = Regex.Replace(s, @"\s{2,}", " ").Trim();
            s = Regex.Replace(s, @"\b(REF|TXN|INV|ORDER|NO\.?)\s*[:#]?\s*\w+", "");
            s = Regex.Replace(s, @"\d{4,}", ""); // remove long numbers/codes
            s = Regex.Replace(s, @"\s{2,}", " ").Trim(' ', '-', ':');
            return string.IsNullOrWhiteSpace(s) ? "UNKNOWN" : s;
        }

        private static readonly Dictionary<string, string> KeywordToCategory =
    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "FAIRPRICE", "Groceries" },
        { "NTUC", "Groceries" },
        { "COLD STORAGE", "Groceries" },
        { "GRAB", "Transport" },
        { "GOJEK", "Transport" },
        { "MRT", "Transport" },
        { "EZLINK", "Transport" },
        { "STARHUB", "Subscriptions" },
        { "SINGTEL", "Subscriptions" },
        { "SPOTIFY", "Subscriptions" },
        { "NETFLIX", "Subscriptions" },
        { "AMAZON", "Shopping" },
        { "LAZADA", "Shopping" },
        { "SHOPEE", "Shopping" },
        { "STARBUCKS", "Dining" },
        { "MCDONALD", "Dining" },
        { "KOPITIAM", "Dining" },
        { "DIN TAI FUNG", "Dining" },
        { "CLINIC", "Health" },
        { "PHARMACY", "Health" },
        { "INTEREST", "Fees" },
        { "FEE", "Fees" }
    };

        private static readonly (Regex rx, string cat)[] CategoryPatterns = new (Regex, string)[]
            {
            (new Regex(@"\b(GROCERY|GROCER|SUPERMARKET|FAIRPRICE|COLD STORAGE|NTUC|MARKET)\b", RegexOptions.IgnoreCase), "Groceries"),
            (new Regex(@"\b(COFFEE|CAF[ÉE]|CAFE|TEA|BOBA|STARBUCKS|KOPITIAM|MCDONALD|DIN TAI FUNG)\b", RegexOptions.IgnoreCase), "Dining"),
            (new Regex(@"\b(GRAB|GOJEK|MRT|EZ[- ]?LINK|BUS|TAXI|RIDE[- ]?HAIL)\b", RegexOptions.IgnoreCase), "Transport"),
            (new Regex(@"\b(SPOTIFY|NETFLIX|DISNEY\+|YOUTUBE PREMIUM|APPLE MUSIC)\b", RegexOptions.IgnoreCase), "Subscriptions"),
            (new Regex(@"\b(STARHUB|SINGTEL|M1)\b", RegexOptions.IgnoreCase), "Utilities"),
            (new Regex(@"\b(FEE|FEES|CHARGE|INTEREST|ANNUAL|LATE)\b", RegexOptions.IgnoreCase), "Fees"),
            (new Regex(@"\b(CLINIC|PHARMACY|DENTAL)\b", RegexOptions.IgnoreCase), "Health"),
            (new Regex(@"\b(AMAZON|LAZADA|SHOPEE)\b", RegexOptions.IgnoreCase), "Shopping")
            };
        private static string LocalKeywordCategory(string desc)
        {
            if (string.IsNullOrWhiteSpace(desc)) return "Misc";
            foreach (var (rx, cat) in CategoryPatterns)
                if (rx.IsMatch(desc)) return cat;
            return "Misc";
        }

        // --- Aggregations for Analyze UI ---
        private static Dictionary<string, decimal> AggregateByCategory(
        List<RowVm> monthRows,
        Dictionary<string, string> gptMap)
        {
            var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in monthRows)
            {
                var merch = CanonicalMerchant(r.Description ?? "");

                // 1) Prefer GPT category if present and not Misc/Unknown
                string cat = null;
                if (gptMap != null &&
                    gptMap.TryGetValue(merch, out var gptCat) &&
                    !string.IsNullOrWhiteSpace(gptCat) &&
                    !gptCat.Equals("Misc", StringComparison.OrdinalIgnoreCase) &&
                    !gptCat.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                {
                    cat = gptCat;
                }
                else
                {
                    // 2) Lightweight local fallback (kept small on purpose)
                    cat = LocalKeywordCategory(merch); // returns "Misc" if nothing matches
                }

                if (!totals.ContainsKey(cat)) totals[cat] = 0m;
                totals[cat] += r.Expense;
            }

            return totals;
        }


        private static List<RecurringVm> DetectRecurring(List<RowVm> monthRows, Dictionary<string, string> gptMap)
        {
            return monthRows
                .GroupBy(r => CanonicalMerchant(r.Description ?? ""))
                .Select(g => new RecurringVm { Merchant = g.Key, Total = g.Sum(x => x.Expense) })
                .Where(x => x.Total > 0m && monthRows.Count(m => CanonicalMerchant(m.Description ?? "") == x.Merchant) >= 2)
                .OrderByDescending(x => x.Total)
                .ToList();
        }

        // --- GPT call for Analyze (semantic categories + advice) ---
        private async Task<GptHybridOut> CallGptHybridAsync(List<SimpleTxn> sample)
        {
            try
            {
                var apiKey = ConfigurationManager.AppSettings["OpenAI_API_Key"];
                if (string.IsNullOrWhiteSpace(apiKey)) return FallbackHybrid(sample);

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                var userContent = JsonConvert.SerializeObject(new { txns = sample });

                var payload = new
                {
                    model = "gpt-5",
                    response_format = new { type = "json_object" },
                    temperature = 0.1,
                    messages = new object[]
                    {
                new { role = "system", content =
@"You are a precise personal finance classifier and coach.

INPUT: Recent consumer transactions with merchant-like descriptions and signed amounts (+ = expense, - = income).

OUTPUT: ONLY JSON:
{
  ""categories"": { ""<merchant>"": ""<category>"" },
  ""advice"": [""..."", ""...""]
}

CATEGORIES (choose one per merchant): Groceries, Dining, Transport, Shopping, Entertainment, Subscriptions, Utilities, Fees, Health, Education, Rent, Travel, Income, Misc.

CLASSIFICATION RULES
- Prefer best-fit category; use Misc only if truly unclassifiable.
- Cafe/coffee/tea/boba terms → Dining.
- Grocery/supermarket/market terms → Groceries.
- Telcos/utilities providers → Utilities.
- Streaming/music services → Subscriptions.
- Ride-hail/public transport → Transport.
- Salary/Payroll/Paycheck/Client payment with negative amount → Income.
- Be consistent across similar merchants.

ADVICE RULES
- ALWAYS produce 3–6 concrete, actionable suggestions (never empty).
- Base tips on patterns you infer (spend spikes, recurring charges, category bias).
- Blend immediate actions with literacy concepts (e.g., 50/30/20, zero-based budget, sinking funds, emergency fund 3–6 months, automate savings, unit pricing, subscription audits).
- Make each tip specific and outcome-focused; no clichés; no scolding; avoid product endorsements."
                },

                // Few-shot with non-empty advice to prevent empty lists
                new { role = "user", content =
@"Recent transactions JSON:
{ ""txns"": [
  { ""date"": ""2025-06-01"", ""description"": ""GROCERY SHOPPING"", ""amount"": 120.50 },
  { ""date"": ""2025-06-03"", ""description"": ""SALARY"", ""amount"": -2500.00 },
  { ""date"": ""2025-06-05"", ""description"": ""COFFEE"", ""amount"": 5.75 },
  { ""date"": ""2025-06-07"", ""description"": ""GRAB"", ""amount"": 12.40 }
]}" },

                new { role = "assistant", content =
@"{
  ""categories"": {
    ""GROCERY SHOPPING"": ""Groceries"",
    ""SALARY"": ""Income"",
    ""COFFEE"": ""Dining"",
    ""GRAB"": ""Transport""
  },
  ""advice"": [
    ""Set a weekly grocery cap and use a list; compare unit pricing to cut 5–10%."",
    ""Adopt 50/30/20 this month: auto-transfer 20% of income on payday to savings."",
    ""Bundle trips or switch short rides to public transport to trim Transport by 20%.""
  ]
}" },

                // Your actual data
                new { role = "user", content = "Recent transactions JSON:\n" + userContent + "\nReturn JSON only." }
                    }
                };

                var body = JsonConvert.SerializeObject(payload);
                var resp = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    new StringContent(body, Encoding.UTF8, "application/json")
                );

                var raw = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"GPT HTTP {(int)resp.StatusCode}: {raw}");
                    return FallbackHybrid(sample);
                }

                var doc = JObject.Parse(raw);
                var content = doc["choices"]?[0]?["message"]?["content"]?.ToString();
                if (string.IsNullOrWhiteSpace(content))
                {
                    System.Diagnostics.Debug.WriteLine("GPT returned empty content.");
                    return FallbackHybrid(sample);
                }

                var parsed = JObject.Parse(content);

                var outObj = new GptHybridOut
                {
                    categories = parsed["categories"]?.ToObject<Dictionary<string, string>>()
                 ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    advice = parsed.TryGetValue("advice", out var adviceToken) && adviceToken is JArray
                 ? adviceToken.ToObject<List<string>>()
                 : new List<string>(),
                    aiRan = true
                };


                // Normalize merchant keys for lookup consistency
                if (outObj.categories.Count > 0)
                {
                    outObj.categories = outObj.categories
                        .GroupBy(kv => CanonicalMerchant(kv.Key))
                        .ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.OrdinalIgnoreCase);
                }

                // Guardrail: never allow empty advice if AI ran—re-ask once, strictly
                if (outObj.aiRan && (outObj.advice == null || outObj.advice.Count == 0))
                {
                    var nudgePayload = new
                    {
                        model = "gpt-5",
                        response_format = new { type = "json_object" },
                        temperature = 0.1,
                        messages = new object[]
                        {
                    new { role = "system", content = "Return ONLY JSON with key \"advice\" as a list of 3–6 concrete personal finance suggestions; no other keys." },
                    new { role = "user", content = "Generate 3–6 specific, actionable suggestions to improve spending habits and financial literacy for the prior dataset. Return JSON only." }
                        }
                    };
                    var nudgeResp = await _httpClient.PostAsync(
                        "https://api.openai.com/v1/chat/completions",
                        new StringContent(JsonConvert.SerializeObject(nudgePayload), Encoding.UTF8, "application/json")
                    );
                    var nudgeRaw = await nudgeResp.Content.ReadAsStringAsync();
                    if (nudgeResp.IsSuccessStatusCode)
                    {
                        try
                        {
                            var nudgeDoc = JObject.Parse(nudgeRaw);
                            var nudgeContent = nudgeDoc["choices"]?[0]?["message"]?["content"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(nudgeContent))
                            {
                                var nudgeParsed = JObject.Parse(nudgeContent);
                                var tips = nudgeParsed["advice"]?.ToObject<List<string>>() ?? new List<string>();
                                if (tips.Count > 0) outObj.advice = tips;
                            }
                        }
                        catch { /* best-effort */ }
                    }
                }

                return outObj;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GPT call failed: " + ex);
                return FallbackHybrid(sample);
            }
        }

        // --- Fallback if GPT unavailable ---
        private GptHybridOut FallbackHybrid(List<SimpleTxn> sample)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var t in sample.Where(t => t.amount > 0)) // expenses only
            {
                var merch = CanonicalMerchant(t.description ?? "");
                if (!map.ContainsKey(merch))
                    map[merch] = LocalKeywordCategory(merch);
            }

            // Honest fallback: no suggestions if AI unavailable
            return new GptHybridOut
            {
                categories = map,
                advice = new List<string>(),
                aiRan = false
            };
        }


        // ============================================================
        // FEATURE: IMPORT / CANCEL
        // - btnImport_Click
        // - btnCancel_Click
        // ============================================================
        protected void btnImport_Click(object sender, EventArgs e)
        {
            var rows = Session["importRows"] as List<RowVm>;
            if (rows == null) return;

            // Apply user choices from grid (import flag + jar per row)
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

            // Touch jars (forces any cache/balance refresh in your services)
            foreach (int jarId in touched)
            {
                decimal _ = txnSvc.GetTransactionSum(_userId, jarId);
                var jar = jarSvc.GetJarById(jarId, _userId);
                if (jar == null) continue;
            }

            Session.Remove("importRows");
            pnlPreview.Visible = false;
            pnlDone.Visible = true;
            pnlAnalysis.Visible = false;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Session.Remove("importRows");
            pnlPreview.Visible = false;
            pnlUpload.Visible = true;
            pnlAnalysis.Visible = false;
        }


        // ============================================================
        // FEATURE: RESTART ALL JARS (LITE)
        // - btnRestartAllJars_Click
        // ============================================================
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


        // ============================================================
        // INTERNAL VIEW MODEL (preview/import rows)
        // ============================================================
        private sealed class RowVm
        {
            public DateTime Date { get; set; }
            public string Description { get; set; }
            public decimal Income { get; set; }
            public decimal Expense { get; set; }
            public int JarId { get; set; }
            public bool Import { get; set; }
        }

        // ============================================================
        // SHARED UI HELPERS
        // ============================================================
        private void BindJarDropDown(DropDownList ddl)
        {
            var jars = new Jar().GetJarsByUser(_userId);
            ddl.DataSource = jars;
            ddl.DataTextField = "JarName";
            ddl.DataValueField = "JarId";
            ddl.DataBind();
        }
    }
}
