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
        string user_id;

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
                user_id = Session["UserId"].ToString();
                notification_list = user_notification.GetNotificationsByUserID(user_id, "");

                if (!IsPostBack)
                {
                    Notification.DataSource = notification_list;
                    Notification.DataBind();
                }
            }
          
        }

        protected void Filter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string category = btn.CommandArgument;

            notification_list = user_notification.GetNotificationsByUserID(user_id, category);

            Notification.DataSource = notification_list;
            Notification.DataBind();

            UpdatePanel.Update();
        }
    }
}