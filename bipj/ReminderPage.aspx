<%@ Page Title="" Language="C#"
    MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true"
    CodeBehind="ReminderPage.aspx.cs" Inherits="bipj.ReminderPage" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <style>
        .notification-container {
            max-width: 800px;
            margin: 40px auto;
            background: #fff;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
        }

        .notification {
            border-left: 5px solid #433e8e;
            padding: 16px;
            margin-bottom: 16px;
            background-color: #f9f8ff;
        }

        .notification h5 {
            margin: 0;
            color: #433e8e;
        }

        .notification p {
            margin: 6px 0 0 0;
        }

        .btn-review {
            display: inline-block;
            margin-top: 8px;
            padding: 6px 14px;
            background-color: #433e8e;
            color: #fff;
            border-radius: 6px;
            text-decoration: none;
            font-size: 14px;
        }

        .btn-review:hover {
            background-color: #322e6d;
        }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="notification-container">
        <h2 style="color:#3b3350; text-align:center; margin-bottom:30px">🔔 Your Notifications</h2>
        <asp:Repeater ID="rptNotifications" runat="server">
            <ItemTemplate>
                <div class="notification">
                    <h5><%# Eval("Title") %></h5>
                    <p><%# Eval("Message") %></p>
                    <%# Eval("BookingId") != DBNull.Value && Eval("BookingId") != null ? 
                        $"<a href='ReviewAdvisor.aspx?bookingId={Eval("BookingId")}' class='btn-review'>Review</a>" 
                        : "" %>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</asp:Content>
