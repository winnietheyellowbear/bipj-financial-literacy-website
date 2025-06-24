using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class VoucherExchange : System.Web.UI.Page
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
    }
}