using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class VoucherRedemption : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                // Retrieve the token from the query string
                string token = Request.QueryString["token"];
                User_Voucher user_voucher = new User_Voucher();
                user_voucher = user_voucher.GetVoucherByToken(token);

                description.Text = user_voucher.Description;
                companyName.Text = user_voucher.Company_Name;
                expiryDate.Text = user_voucher.Expiry_Date;
                
                if (user_voucher.Status == "Available")
                {
                    btnUsed.Visible = false;
                }
                else if (user_voucher.Status == "Used")
                {
                    btnUse.Visible = false;
                }

            }
        }

        protected void btn_status_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string status = btn.CommandArgument;

            string token = Request.QueryString["token"];

            User_Voucher user_voucher = new User_Voucher();
            int result = user_voucher.StatusUpdate(token, status);

            if (result > 0)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    this.GetType(),
                    "alert",
                    "alert('Status updated successfully. 😊'); window.location='VoucherRedemption.aspx?token=" + token + "';",
                    true
                );
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Failed to update status. 😞');", true);
            }
        }
    }
}