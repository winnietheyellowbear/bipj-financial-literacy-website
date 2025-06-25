using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class CreateVoucher : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void btn_create_Click(object sender, EventArgs e)
        {
            int result = 0;

            string name = tb_Sponsor_Name.Text;
            string description = tb_Desc.Text;
            string validity = tb_Validity.Text + " " + ddl_Validity.SelectedValue;
            int points_required = int.Parse(tb_Points_Required.Text);

            Staff_Voucher staff_voucher = new Staff_Voucher(name, description, validity, points_required);
            result = staff_voucher.VoucherInsert();

            if (result > 0)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Voucher created. 😊'); window.location='CreateVoucher.aspx';", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Failed to create voucher. 😞');", true);
            }
        }
    }
}