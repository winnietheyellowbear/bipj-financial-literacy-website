using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class VoucherActive : System.Web.UI.Page
    {
        User_Voucher user_voucher = new User_Voucher();
        public List<User_Voucher> voucher_list = new List<User_Voucher>();

        protected void Page_Load(object sender, EventArgs e)
        {
            string user_id = "2";
            voucher_list = user_voucher.GetVoucherByUserID(user_id);

            if (!IsPostBack)
            {
                Voucher.DataSource = voucher_list;
                Voucher.DataBind();
            }

        }
    }
}