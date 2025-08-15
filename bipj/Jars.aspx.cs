using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using bipj.Models;

namespace bipj
{
    public partial class Jars : System.Web.UI.Page
    {
        private int _userId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || !int.TryParse(Session["UserId"].ToString(), out _userId))
            {
                Response.Redirect("Loginpage.aspx");
                return;
            }

            if (IsPostBack) return;

            var jarSvc = new Jar();
            if (!jarSvc.UserHasJars(_userId))
                jarSvc.CreateDefaultJars(_userId);

            LoadJars();
            BindDefaultJarDropdown();

            litDefaultJarName.Text = jarSvc.GetDefaultJarName(_userId);
        }

        private void LoadJars()
        {
            var jarSvc = new Jar();
            var jars = jarSvc.GetJarsByUser(_userId);

            rptJars.DataSource = jars;
            rptJars.DataBind();

            rptSettings.DataSource = jars;
            rptSettings.DataBind();

            lblTotalAmount.Text = $"${jars.Sum(j => j.Balance):F2}";

            var chartData = new
            {
                labels = jars.Where(j => j.Balance > 0).Select(j => j.JarName).ToList(),
                amounts = jars.Where(j => j.Balance > 0).Select(j => Math.Round(j.Balance, 2)).ToList(),
                colors = jars.Where(j => j.Balance > 0).Select(j => j.ColorHex ?? "#cccccc").ToList()
            };

            string json = new JavaScriptSerializer().Serialize(chartData);
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "chartData",
                $"window.chartData = {json}; renderPieChart();",
                true
            );
        }

        private void BindDefaultJarDropdown()
        {
            var jars = new Jar().GetJarsByUser(_userId);
            ddlDefaultJar.DataSource = jars;
            ddlDefaultJar.DataTextField = "JarName";
            ddlDefaultJar.DataValueField = "JarId";
            ddlDefaultJar.DataBind();

            var def = jars.FirstOrDefault(j => j.IsDefault);
            if (def != null) ddlDefaultJar.SelectedValue = def.JarId.ToString();
        }

        protected void btnSaveSettings_Click(object sender, EventArgs e)
        {
            decimal totalPct = 0m;
            var updated = new List<Jar>();

            foreach (RepeaterItem item in rptSettings.Items)
            {
                var tb = (TextBox)item.FindControl("percentInput");
                var hf = (HiddenField)item.FindControl("hiddenJarId");

                if (!decimal.TryParse(tb.Text.Trim(), out decimal pct)
                    || Math.Round(pct * 10) != pct * 10)
                {
                    ShowAlert("Percentages must be numbers with at most one decimal place.");
                    return;
                }

                totalPct += pct;

                updated.Add(new Jar
                {
                    JarId = int.Parse(hf.Value),
                    Percentage = pct,
                    IsDefault = ddlDefaultJar.SelectedValue == hf.Value,
                    UserId = _userId
                });
            }

            if (totalPct != 100m)
            {
                LoadJars();
                BindDefaultJarDropdown();

                const string script = @"
                    window.addEventListener('load', function() {
                      var s = document.getElementById('settingsModal');
                      if (s) bootstrap.Modal.getOrCreateInstance(s).hide();
                      var e = document.getElementById('percentErrorModal');
                      if (e) bootstrap.Modal.getOrCreateInstance(e).show();
                    });";
                ClientScript.RegisterStartupScript(GetType(), "pctError", script, true);
                return;
            }

            new Jar().UpdatePercentageAndDefault(updated);
            LoadJars();
            BindDefaultJarDropdown();

            const string closeScript = @"
                window.addEventListener('load', function() {
                  var s = document.getElementById('settingsModal');
                  if (s) bootstrap.Modal.getOrCreateInstance(s).hide();
                  renderPieChart();
                });";
            ClientScript.RegisterStartupScript(GetType(), "closeSettings", closeScript, true);
        }

        protected void btnAddJar_Click(object sender, EventArgs e)
        {
            string name = txtNewJarName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            string desc = txtNewJarDesc.Text.Trim();

            var jarSvc = new Jar();
            int pos = jarSvc.GetJarsByUser(_userId).Count;
            string color = jarSvc.GetNextAvailableColor(_userId);

            // match your Jar constructor: (jarId, userId, jarName, description, percentage, isDefault, position, colorHex)
            var jar = new Jar(0, _userId, name, desc, 0m, false, pos, color);
            jar.InsertJar();

            LoadJars();
            ScriptManager.RegisterStartupScript(this, GetType(), "closeAdd", "closeAddModal(); renderPieChart();", true);
            Response.Redirect(Request.RawUrl, true);
        }

        protected void rptJars_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Edit") return;

            LoadJars();

            int jarId = Convert.ToInt32(e.CommandArgument);
            var jar = new Jar().GetJarById(jarId, _userId);
            if (jar == null) return;

            hiddenEditJarId.Value = jar.JarId.ToString();
            txtEditName.Text = jar.JarName;
            txtEditDesc.Text = jar.Description;
            txtEditPercent.Text = jar.Percentage.ToString("0.0");

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "showEdit",
                "var m=new bootstrap.Modal(document.getElementById('editModal'));m.show();",
                true
            );
        }

        protected void btnUpdateJar_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hiddenEditJarId.Value, out int jarId)) return;

            var svc = new Jar();
            var jar = svc.GetJarById(jarId, _userId);
            if (jar == null) return;

            jar.JarName = txtEditName.Text.Trim();
            jar.Description = txtEditDesc.Text.Trim();
            jar.UpdateJar();

            LoadJars();
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "closeEdit",
                "closeEditModal(); renderPieChart();",
                true
            );
        }

        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hiddenDeleteJarId.Value, out int jarId)) return;

            var svc = new Jar();
            var toDelete = svc.GetJarById(jarId, _userId);
            if (toDelete == null || toDelete.IsDefault)
            {
                ShowAlert("Cannot delete default jar.");
                return;
            }

            toDelete.DeleteJar();

            LoadJars();
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "afterDelete",
                "renderPieChart();",
                true
            );
        }

        private void ShowAlert(string msg)
        {
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "alert",
                $"alert('{msg}');",
                true
            );
        }
    }
}
