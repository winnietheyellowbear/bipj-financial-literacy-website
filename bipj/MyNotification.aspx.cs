using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bipj
{
    public partial class MyNotification : System.Web.UI.Page
    {
        User_Notification user_notification = new User_Notification();
        public List<User_Notification> notification_list = new List<User_Notification>();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Loginpage.aspx");
            }
            else
            {
                string user_id = Session["UserId"].ToString();
                notification_list = user_notification.GetNotificationsByUserID(user_id);

                if (!IsPostBack)
                {
                    Notification.DataSource = notification_list;
                    Notification.DataBind();
                }
            }
          
        }
    }
}