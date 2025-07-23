using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class UpdateVoucher : System.Web.UI.Page
    {
        Staff_Voucher staff_voucher = new Staff_Voucher();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string voucher_id = Request.QueryString["Voucher_ID"];
                staff_voucher = staff_voucher.GetVoucherByVoucherID(voucher_id);

                tb_Sponsor_Name.Text = staff_voucher.Company_Name;
                tb_Desc.Text = staff_voucher.Description;
                
                string[] validityParts = staff_voucher.Validity.Split(' ');

                if (validityParts.Length == 2)
                {
                    string tbValidityText = validityParts[0];
                    string ddlValiditySelectedValue = validityParts[1];

                    tb_Validity.Text = tbValidityText;
                    ddl_Validity.SelectedValue = ddlValiditySelectedValue;
                }

                tb_Points_Required.Text = staff_voucher.Points_Required.ToString();

            }
        }
        protected void btn_update_Click(object sender, EventArgs e)
        {
            int result = 0;

            string name = tb_Sponsor_Name.Text;
            string description = tb_Desc.Text;
            string validity = tb_Validity.Text + " " + ddl_Validity.SelectedValue;
            int points_required = int.Parse(tb_Points_Required.Text);

            string voucher_id = Request.QueryString["Voucher_ID"];

            Staff_Voucher staff_voucher = new Staff_Voucher(name, description, validity, points_required);
            result = staff_voucher.VoucherUpdate(voucher_id);

            if (result > 0)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Voucher updated. 😊'); window.location='VoucherStaff.aspx';", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Failed to update voucher. 😞');", true);
            }
        }

        protected void btn_back_Click(object sender, EventArgs e)
        {
            Response.Redirect("VoucherStaff.aspx");
        }
    }
}