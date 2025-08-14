using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.UI;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;


namespace bipj
{
    public partial class Education : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                var returnUrl = HttpUtility.UrlEncode(Request.RawUrl);
                Response.Redirect("Loginpage.aspx?returnUrl=" + returnUrl + "&msg=login_required");
                return;
            }

            if (!IsPostBack)
            {
                LoadRecentModules();
                LoadCompletedModules();
                // Call the async loader synchronously on first load
                LoadRecommendedModulesAsync().GetAwaiter().GetResult();
            }
        }

        // ---------- NEW: Recommended Modules ----------
        private async Task LoadRecommendedModulesAsync()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            var seenIds = GetSeenModuleIds(userId);
            var candidates = GetCandidateModules(seenIds); // rows from EducationModules NOT IN seen

            // If we don't even have 3 candidates, just show what we have (or none)
            if (candidates.Count == 0)
            {
                pnlNoRecommendations.Visible = true;
                return;
            }
            if (candidates.Count <= 3)
            {
                BindRecommendations(candidates);
                return;
            }

            // Build a compact candidate list for the model (Id + Name + BriefDescription)
            var candidateSummaries = candidates.Select(c => new
            {
                c.Id,
                c.Name,
                c.BriefDescription
            }).ToList();

            // Optional: include a quick “history summary” to diversify (purely descriptive)
            string historySummary = GetHistorySummary(userId);

            string systemMsg = "You are a recommendation engine. Only return items from the provided candidate list by Id.";
            string userMsg = $@"
User history (for diversity/next-step learning only): 
{historySummary}

You are given a list of candidate modules from the database (Id, Name, BriefDescription). 
Pick EXACTLY 3 different Ids from this list that best fit next-step learning, avoiding overlap with the history.
Return ONLY valid JSON in this schema (no extra text):
[
  {{ ""Id"": 12, ""Reason"": ""short rationale"" }},
  {{ ""Id"": 5,  ""Reason"": ""short rationale"" }},
  {{ ""Id"": 19, ""Reason"": ""short rationale"" }}
]

Candidates:
{JsonConvert.SerializeObject(candidateSummaries)}
";

            var selectedIds = await AskOpenAIForIdsAsync(systemMsg, userMsg);

            // Fallback if OpenAI fails: just take 3 by a simple heuristic (e.g., random or most recently added)
            if (selectedIds == null || selectedIds.Count != 3)
            {
                // simple fallback: pick any 3
                var fallback = candidates.Take(3).ToList();
                BindRecommendations(fallback);
                return;
            }

            // Map chosen Ids back to our candidate rows
            var picked = candidates.Where(c => selectedIds.Contains(c.Id)).ToList();
            if (picked.Count < 3)
            {
                // Backfill to 3 if model missed one
                var extra = candidates.Where(c => !selectedIds.Contains(c.Id)).Take(3 - picked.Count);
                picked.AddRange(extra);
            }

            BindRecommendations(picked.Take(3).ToList());
        }

        private void BindRecommendations(List<ModuleRow> modules)
        {
            if (modules == null || modules.Count == 0)
            {
                pnlNoRecommendations.Visible = true;
                return;
            }
            rptRecommendedModules.DataSource = modules;
            rptRecommendedModules.DataBind();
            pnlNoRecommendations.Visible = false;
        }

        private List<int> GetSeenModuleIds(int userId)
        {
            var ids = new HashSet<int>();
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                // From progress table
                using (var cmd = new SqlCommand(@"
                    SELECT DISTINCT uep.ModuleId
                    FROM UserEducationProgress uep
                    WHERE uep.UserId = @UserId;", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) ids.Add((int)r["ModuleId"]);
                }

                // From viewed pages -> subtopics -> modules
                using (var cmd = new SqlCommand(@"
                    SELECT DISTINCT em.Id
                    FROM UserViewedPages uvp
                    INNER JOIN EducationPages ep ON uvp.PageId = ep.Id
                    INNER JOIN EducationSubTopics est ON ep.SubTopicId = est.Id
                    INNER JOIN EducationModules em ON est.ModuleId = em.Id
                    WHERE uvp.UserId = @UserId;", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) ids.Add((int)r["Id"]);
                }
            }

            return ids.ToList();
        }

        private List<ModuleRow> GetCandidateModules(List<int> seenIds)
        {
            var list = new List<ModuleRow>();
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                string sqlAll = @"SELECT Id, Name, BriefDescription, ImageUrl FROM EducationModules";
                string sqlNotIn = $@"SELECT Id, Name, BriefDescription, ImageUrl 
                                     FROM EducationModules 
                                     WHERE Id NOT IN ({string.Join(",", seenIds)})";

                var sql = (seenIds != null && seenIds.Count > 0) ? sqlNotIn : sqlAll;

                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ModuleRow
                        {
                            Id = (int)r["Id"],
                            Name = r["Name"]?.ToString(),
                            BriefDescription = r["BriefDescription"]?.ToString(),
                            ImageUrl = r["ImageUrl"]?.ToString()
                        });
                    }
                }
            }

            // Simple heuristic: stable order by Id desc (newer-looking first) — tweak as you like
            return list.OrderByDescending(m => m.Id).ToList();
        }

        private string GetHistorySummary(int userId)
        {
            // short text summary for the prompt (optional, helps diversity)
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            var names = new HashSet<string>();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT DISTINCT em.Name
                    FROM UserEducationProgress uep
                    INNER JOIN EducationModules em ON uep.ModuleId = em.Id
                    WHERE uep.UserId = @UserId

                    UNION

                    SELECT DISTINCT em.Name
                    FROM UserViewedPages uvp
                    INNER JOIN EducationPages ep ON uvp.PageId = ep.Id
                    INNER JOIN EducationSubTopics est ON ep.SubTopicId = est.Id
                    INNER JOIN EducationModules em ON est.ModuleId = em.Id
                    WHERE uvp.UserId = @UserId
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) names.Add(r["Name"].ToString());
                }
            }

            return (names.Count == 0) ? "(no prior modules)" : string.Join(", ", names);
        }

        private async Task<List<int>> AskOpenAIForIdsAsync(string systemMsg, string userMsg)
        {
            string apiKey = "";
            if (string.IsNullOrWhiteSpace(apiKey)) return null;

            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var body = new
                {
                    model = "gpt-3.5-turbo", // works well & cost-efficient; change if you prefer
                    messages = new object[]
                    {
                        new { role = "system", content = systemMsg },
                        new { role = "user", content = userMsg }
                    },
                    temperature = 0.2
                };

                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var resp = await http.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var json = await resp.Content.ReadAsStringAsync();

                // Extract the assistant content string
                dynamic parsed = JsonConvert.DeserializeObject(json);
                string text = parsed?.choices?[0]?.message?.content?.ToString();
                if (string.IsNullOrWhiteSpace(text)) return null;

                // Try to parse as an array of objects with Id
                try
                {
                    var picks = JsonConvert.DeserializeObject<List<Pick>>(text);
                    if (picks == null) return null;

                    // keep unique, valid ints, at most 3
                    return picks.Select(p => p.Id).Where(id => id > 0).Distinct().Take(3).ToList();
                }
                catch
                {
                    // Sometimes models add code fences; attempt to strip them
                    text = text.Trim().Trim('`');
                    int firstBracket = text.IndexOf('[');
                    int lastBracket = text.LastIndexOf(']');
                    if (firstBracket >= 0 && lastBracket >= firstBracket)
                    {
                        var inner = text.Substring(firstBracket, lastBracket - firstBracket + 1);
                        try
                        {
                            var picks = JsonConvert.DeserializeObject<List<Pick>>(inner);
                            return picks?.Select(p => p.Id).Where(id => id > 0).Distinct().Take(3).ToList();
                        }
                        catch { /* ignore */ }
                    }
                    return null;
                }
            }
        }
        private void LoadRecentModules()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            int userId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
            SELECT TOP 3 em.Id, em.Name, em.BriefDescription, em.ImageUrl
            FROM UserEducationProgress uep
            INNER JOIN EducationModules em ON uep.ModuleId = em.Id
            WHERE uep.UserId = @UserId
            ORDER BY uep.LastAccessed DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptRecentModules.DataSource = dt;
                rptRecentModules.DataBind();

                pnlNoRecent.Visible = dt.Rows.Count == 0;
                rptRecentModules.Visible = dt.Rows.Count > 0;
            }
        }
        private void LoadCompletedModules()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;
            int userId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
            SELECT em.Id, em.Name, em.BriefDescription, em.ImageUrl
            FROM UserEducationProgress uep
            INNER JOIN EducationModules em ON uep.ModuleId = em.Id
            WHERE uep.UserId = @UserId AND uep.CompletionPercentage >= 100";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptCompletedModules.DataSource = dt;
                rptCompletedModules.DataBind();
            }
        }
        protected void btnShowAllTopics_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllTopics.aspx");
        }

        protected void btnViewAllRecent_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllRecent.aspx");
        }

        protected void btnViewAllCompleted_Click(object sender, EventArgs e)
        {
            Response.Redirect("AllCompleted.aspx");
        }
        private class ModuleRow
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string BriefDescription { get; set; }
            public string ImageUrl { get; set; }
        }

        private class Pick
        {
            public int Id { get; set; }
            public string Reason { get; set; }
        }
    }
}
