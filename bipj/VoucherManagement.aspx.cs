using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class VoucherManagement1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Retrieve the token from the query string
                string token = Request.QueryString["token"];
                Staff_Voucher staff_voucher = new Staff_Voucher();
                staff_voucher = staff_voucher.GetVoucherByToken(token);

                description.Text = staff_voucher.Description;
                companyName.Text = staff_voucher.Company_Name;
                validity.Text = staff_voucher.Validity;
                pointsRequired.Text = staff_voucher.Points_Required.ToString();
               
                if (staff_voucher.Status == "Active")
                {
                    btnEnable.Visible = false;
                }
                else if (staff_voucher.Status == "Inactive")
                {
                    btnDisable.Visible = false;
                }

            }
        }

        protected void btn_status_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string status = btn.CommandArgument;

            string token = Request.QueryString["token"];

            Staff_Voucher staff_voucher = new Staff_Voucher();
            int result = staff_voucher.StatusUpdate(token, status);

            if (result > 0)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    this.GetType(),
                    "alert",
                    "alert('Voucher is used successfully. 😊'); window.location='VoucherManagement.aspx?token=" + token + "';",
                    true
                );
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Failed to use voucher. 😞');", true);
            }

        }

    }
}