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

                User_Voucher user_voucher = new User_Voucher();
                string user_id = "2";
                int user_point = user_voucher.GetUserPoint(user_id);

                lbl_Point.Text = user_point.ToString();
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
                staff_voucher = staff_voucher.GetVoucherByVoucherID(voucher_id);

                // expiry date
                var parts = staff_voucher.Validity.Split(' ');
                int amount = int.Parse(parts[0]);
                string unit = parts[1].ToLower();

                DateTime expiryDate;

                if (unit.StartsWith("day"))
                {
                    expiryDate = DateTime.Now.AddDays(amount);
                }
                else
                {
                    expiryDate = DateTime.Now.AddMonths(amount);
                }

                string expiry_date = expiryDate.ToString("yyyy-MM-dd");


                User_Voucher user_voucher = new User_Voucher(staff_voucher.Voucher_ID, staff_voucher.Description, expiry_date, user_id);

                int user_point = userPoints - pointsRequired;
                user_voucher.PointUpdate(user_id, user_point);

                int result = user_voucher.VoucherInsert();

                if (result > 0)
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Voucher redeemed. 😊'); window.location='VoucherExchange.aspx';", true);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Failed to redeem voucher. 😞');", true);
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Not enough point. 😞');", true);
            }

        }


    }
}