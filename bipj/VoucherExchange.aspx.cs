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

        protected void btn_redeem_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string voucher_id = btn.CommandArgument;

            string user_id = "2";

            var variables = staff_voucher.IsPointEnough(voucher_id, user_id);
            bool isPointEnough = variables.isPointEnough;
            int userPoints = variables.userPoints;
            int pointsRequired = variables.pointsRequired;

            if (isPointEnough)
            {
                userPoints = userPoints - pointsRequired;
                User_Voucher user_voucher = new User_Voucher();

                user_voucher.PointUpdate(user_id, userPoints);
                int result = user_voucher.VoucherInsert();
            }

        }


    }
}