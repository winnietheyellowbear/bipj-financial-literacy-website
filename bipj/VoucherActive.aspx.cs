using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
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

        protected void statusCheckTimer_Tick(object sender, EventArgs e)
        {
            string token = voucherToken.Text;

            // Get the latest voucher status from the database
            user_voucher = user_voucher.GetVoucherByToken(token);

            if (user_voucher != null && user_voucher.Status == "used")
            {
                // Stop the timer and close the modal (triggered by ScriptManager)

                CloseModalWithAlert();
                voucher_list = user_voucher.GetVoucherByUserID(user_id);
                Voucher.DataSource = voucher_list;
                Voucher.DataBind();
                UpdatePanel.Update();

                
            }
        }

     

        // Method to close the modal and show an alert from C#
        private void CloseModalWithAlert()
        {
            // JavaScript to close the modal and show the alert
            ScriptManager.RegisterStartupScript(
                this,
                this.GetType(),
                "closeModalAndAlert",
                "closeModal(); alert('Voucher used. 😊');",
                true
            );
        }

    }
}