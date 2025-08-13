using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class ManageEducation : System.Web.UI.Page
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        protected bool DeleteMode
        {
            get { return (bool?)ViewState["DeleteMode"] ?? false; }
            set { ViewState["DeleteMode"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadModules();
                ApplyUiMode();
            }
        }

        private void Rebind()
        {
            LoadModules();
            ApplyUiMode();
        }

        private void ApplyUiMode()
        {
            // Toggle wrapper CSS class so CSS can show/hide X buttons
            eduRoot.Attributes["class"] = DeleteMode ? "delete-mode" : string.Empty;

            // Toggle action buttons
            btnAddTopic.Visible = !DeleteMode;
            btnEnterDelete.Visible = !DeleteMode;
            btnConfirmDelete.Visible = DeleteMode;
            btnCancelDelete.Visible = DeleteMode;
        }

        protected void btnEnterDelete_Click(object sender, EventArgs e)
        {
            DeleteMode = true;
            Rebind();
        }

        protected void btnCancelDelete_Click(object sender, EventArgs e)
        {
            DeleteMode = false;
            Rebind();
        }

        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            // collect selected module IDs from the repeater
            var idsToDelete = new System.Collections.Generic.List<int>();

            foreach (RepeaterItem item in rptTopics.Items)
            {
                var chk = (CheckBox)item.FindControl("chkSelect");
                var hf = (HiddenField)item.FindControl("hfModuleId");
                if (chk != null && hf != null && chk.Checked && int.TryParse(hf.Value, out int id))
                {
                    idsToDelete.Add(id);
                }
            }

            if (idsToDelete.Count == 0)
            {
                DeleteMode = false;
                Rebind();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;

                    try
                    {
                        cmd.CommandText = @"
    /* 1) Delete dependent page views */
    DELETE uvp
    FROM dbo.UserViewedPages uvp
    WHERE uvp.PageId IN (
        SELECT p.Id
        FROM dbo.EducationPages p
        INNER JOIN dbo.EducationSubTopics s ON s.Id = p.SubTopicId
        WHERE s.ModuleId = @mid
    );

    /* 2) Delete pages under the module's subtopics */
    DELETE p
    FROM dbo.EducationPages p
    WHERE p.SubTopicId IN (
        SELECT s.Id FROM dbo.EducationSubTopics s WHERE s.ModuleId = @mid
    );

    /* 3) Delete subtopics */
    DELETE FROM dbo.EducationSubTopics WHERE ModuleId = @mid;

    /* 4) Delete module-level progress rows */
    DELETE FROM dbo.UserEducationProgress WHERE ModuleId = @mid;

    /* 5) Finally, delete the module */
    DELETE FROM dbo.EducationModules WHERE Id = @mid;
";


                        cmd.Parameters.Add("@mid", SqlDbType.Int);

                        foreach (int mid in idsToDelete)
                        {
                            cmd.Parameters["@mid"].Value = mid;
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw; // or surface a friendly message label if you prefer
                    }
                }
            }

            DeleteMode = false;
            Rebind();
        }

        private void LoadModules()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT 
                        m.Id, 
                        m.Name, 
                        m.BriefDescription, 
                        m.ImageUrl,
                        m.IndeptDescription,
                        (SELECT COUNT(*) FROM EducationSubTopics s WHERE s.ModuleId = m.Id) AS SubTopicCount
                    FROM EducationModules m
                    ORDER BY m.Id DESC;
                ";

                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
            }

            if (dt.Rows.Count == 0)
            {
                pnlNoTopics.Visible = true;
                Panel1.Visible = false;
                btnEnterDelete.Enabled = false;
            }
            else
            {
                pnlNoTopics.Visible = false;
                Panel1.Visible = true;
                btnEnterDelete.Enabled = true;

                rptTopics.DataSource = dt;
                rptTopics.DataBind();
            }
        }

        protected void btnAddTopic_Click(object sender, EventArgs e)
        {
            Response.Redirect("AddEducationTopic.aspx");
        }
    }
}
