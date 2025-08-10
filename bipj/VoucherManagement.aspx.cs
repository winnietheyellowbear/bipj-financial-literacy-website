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

            if (status == "Active")
            {
                WhatsApp();
            }

            if (result > 0)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    this.GetType(),
                    "alert",
                    "alert('Voucher status is updated. 😊'); window.location='VoucherManagement.aspx?token=" + token + "';",
                    true
                );
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Failed to update voucher status. 😞');", true);
            }

        }

        private async void WhatsApp()
        {
            User user = new User();
            List<string> phone_number_list = new List<string>();
            phone_number_list = user.GetUsersPhoneNumber();

            Staff_Voucher staff_voucher = new Staff_Voucher();

            foreach (string phone_number in phone_number_list)
            {
                await staff_voucher.SendMessageAsync(phone_number);
            }
        }
    }
}