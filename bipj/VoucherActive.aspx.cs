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
        string user_id = "2";

        User_Voucher user_voucher = new User_Voucher();
        public List<User_Voucher> voucher_list = new List<User_Voucher>();

        protected void Page_Load(object sender, EventArgs e)
        {
            
            voucher_list = user_voucher.GetVoucherByUserID(user_id);

            if (!IsPostBack)
            {
                Voucher.DataSource = voucher_list;
                Voucher.DataBind();
            }

        }

        protected void Search(object sender, EventArgs e)
        {
            // Get search text and filter category
            string searchInput = this.searchInput.Text.Trim();
            string status = statusFilter.SelectedValue;

            // Call method to get filtered posts
            voucher_list = user_voucher.GetSearchVouchers(searchInput, status, user_id);

            Voucher.DataSource = voucher_list;
            Voucher.DataBind();

            // Update the UpdatePanel (to avoid full page reload)
            UpdatePanel.Update();
        }
    }
}