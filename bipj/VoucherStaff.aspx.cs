using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class VoucherStaff : System.Web.UI.Page
    {
        Staff_Voucher staff_voucher = new Staff_Voucher();
        public List<Staff_Voucher> voucher_list = new List<Staff_Voucher>();

        protected void Page_Load(object sender, EventArgs e)
        {
            voucher_list = staff_voucher.GetAllVouchers();

            if (!IsPostBack)
            {
                Voucher.DataSource = voucher_list;
                Voucher.DataBind();
            }
        }

        protected void btn_update_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string voucher_id = btn.CommandArgument;

            Response.Redirect("UpdateVoucher.aspx?Voucher_ID=" + voucher_id);
        }
    }
}