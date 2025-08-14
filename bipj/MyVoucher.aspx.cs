using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Twilio.Types;

namespace bipj
{
    public partial class VoucherActive : System.Web.UI.Page
    {
        string user_id;

        User_Voucher user_voucher = new User_Voucher();
        public List<User_Voucher> voucher_list = new List<User_Voucher>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Loginpage.aspx");
            }
            else
            {
                user_id = Session["UserId"].ToString();
            }

            voucher_list = user_voucher.GetVoucherByUserID(user_id);

            if (!IsPostBack)
            {
                Voucher.DataSource = voucher_list;
                Voucher.DataBind();
            }
        }

        protected void Search(object sender, EventArgs e)
        {
            string searchInput = this.searchInput.Text.Trim();
            string status = statusFilter.SelectedValue;
            voucher_list = user_voucher.GetSearchVouchers(searchInput, status, user_id);

            Voucher.DataSource = voucher_list;
            Voucher.DataBind();

            UpdatePanel.Update();
        }

        protected void Refresh(object sender, EventArgs e)
        {
            string token = voucherToken.Text;
            user_voucher = user_voucher.GetVoucherByToken(token);

            if (user_voucher != null && user_voucher.Status == "used")
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    this.GetType(),
                    "alertAndCloseModal", 
                    "alert('Voucher used. 😊'); closeModal();", 
                    true 
                );

                voucher_list = user_voucher.GetVoucherByUserID(user_id);
                Voucher.DataSource = voucher_list;
                Voucher.DataBind();

                UpdatePanel.Update();

            }
        }

      
    }
}