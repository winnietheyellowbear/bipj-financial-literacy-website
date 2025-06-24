using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class VoucherSponsor : System.Web.UI.Page
    {
        Sponsor_Voucher sponsor_voucher = new Sponsor_Voucher();
        public List<Sponsor_Voucher> sponsor_list = new List<Sponsor_Voucher>();

        protected void Page_Load(object sender, EventArgs e)
        {
            sponsor_list = sponsor_voucher.GetAllSponsors();

            if (!IsPostBack)
            {
                Sponsor.DataSource = sponsor_list;
                Sponsor.DataBind();
            }
        }
    }
}