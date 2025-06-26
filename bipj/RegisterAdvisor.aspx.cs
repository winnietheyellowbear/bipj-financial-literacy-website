using System;
using System.IO;

namespace bipj
{
    public partial class RegisterAdvisor : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // you can put any initialization logic here if needed
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) Save uploaded photo (if any)
                string photoPath = null;
                if (fuPhoto.HasFile)
                {
                    string ext = Path.GetExtension(fuPhoto.FileName);
                    string fileName = Guid.NewGuid().ToString() + ext;
                    string folder = Server.MapPath("~/uploads/advisors/");
                    Directory.CreateDirectory(folder);
                    string fullPath = Path.Combine(folder, fileName);
                    fuPhoto.SaveAs(fullPath);
                    photoPath = "/uploads/advisors/" + fileName;
                }

                // 2) Build Advisor object
                var advisor = new Advisor
                {
                    Name = txtName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Category = ddlCategory.SelectedValue,
                    Specialty1 = txtSpec1.Text.Trim(),
                    Specialty2 = txtSpec2.Text.Trim(),
                    Specialty3 = txtSpec3.Text.Trim(),
                    Bio = txtBio.Text.Trim(),
                    PhotoPath = photoPath,
                    Status = 0,                // pending
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // 3) Insert into database
                int newId = advisor.Insert();

                // 4) Provide feedback
                lblMessage.Text = $"Thank you! Your application ID is {newId}. Pending approval.";
                lblMessage.CssClass = "message success";
                btnSubmit.Enabled = false;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error submitting application: " + ex.Message;
                lblMessage.CssClass = "message error";
            }
        }
    }
}