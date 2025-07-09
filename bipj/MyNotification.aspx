<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" CodeBehind="MyNotification.aspx.cs" Inherits="bipj.MyNotification" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<head>
  <link rel="stylesheet" href="Forum_Nav.css">
  <asp:ScriptManager ID="ScriptManager" runat="server" />
</head>

<style>

    /* General Styles */
    body {
        font-family: 'Quicksand', sans-serif;
    }

    .content-wrapper {
        display: flex;
        align-items: flex-start; /* Align items at the top */
        margin-top: 10px;
    }

    .main-content {
        flex: 1;
        background-color: #f8f9fa;
        padding: 30px;
        border-radius: 10px;
        margin-left: 20px; /* Add spacing so it doesn't overlap with the sidebar */
        max-width: 1000px;
    }


    /* Filter Buttons */
    .filters {
        display: flex;
        justify-content: flex-start;
        gap: 10px;
        margin-bottom: 25px;
        flex-wrap: wrap;
    }

    .filter-btn {
        padding: 8px 16px;
        font-size: 14px;
        font-weight: 500;
        border: 1px solid #ddd;
        border-radius: 4px;
        cursor: pointer;
        background-color: #fff;
        color: #333;
        transition: all 0.3s ease;
    }

    .filter-btn.active {
        background-color: #f5f5f5;
        color: #000;
    }

    /* Notification Styles */
    .notification-list {
        display: flex;
        flex-direction: column;
        gap: 15px;
    }

    .notification {
        background-color: #fff;
        border-radius: 6px;
        padding: 16px 20px;
        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.03);
        transition: transform 0.2s ease, box-shadow 0.2s ease;
        cursor: default;
        display: flex;
        gap: 10px;
        margin: 10px;
    }

    .notification:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.06);
    }

    .notification-profile-picture {
        width: 50px;
        height: 50px;
        border-radius: 50%;
        object-fit: cover;
    }

    .notification-content {
        flex: 1;
    }

    .notification-header {
        font-weight: bold;
        font-size: 16px;
        margin-bottom: 8px;
        color: #222;
    }

    .notification-comment {
        background-color: #f1f1f1;
        padding: 10px 15px;
        border-radius: 6px;
        font-size: 14px;
        color: #555;
        margin-top: 8px;
    }

    .notification-footer {
        margin-top: 10px;
        font-size: 12px;
        color: #bbb;
    }

    /* Responsive Design */
    @media (max-width: 768px) {
        .content-wrapper {
            flex-direction: column;
            align-items: center;
        }

        .main-content {
            width: 95%;
            padding: 20px;
            margin-left: 0;
        }
    }

</style>


<!-- Sidebar and main content wrapper -->
<div class="content-wrapper">
    <!-- Sidebar -->
    <div class="sidebar">
        <ul>
            <br />
            <br />
            <li>
                <a href="Discussion.aspx">
                    <img src='<%= ResolveUrl("~/Forum/Icon/Discussion_Icon.png") %>' alt="Discussion Icon"/>
                    <span>Discussion</span>
                </a>
            </li>
            <li>
                <a href="SmartSearch.aspx">
                    <img src='<%= ResolveUrl("~/Forum/Icon/Magnifying_Glass_Icon.png") %>' alt="Notification Icon"/>
                    <span>Smart Search</span>
                </a>
            </li>   
            <li class="active">
                <a href="MyNotification.aspx">
                    <img src='<%= ResolveUrl("~/Forum/Icon/Notification_Icon.png") %>' alt="Notification Icon"/>
                    <span>Notification</span>
                </a>
            </li>
            <li>
                <a href="MyPost.aspx">
                    <img src='<%= ResolveUrl("~/Forum/Icon/MyPost_Icon.png") %>' alt="My Post Icon"/>
                    <span>My Post</span>
                </a>
            </li>
            <li>
                <a href="Post.aspx">
                    <img src='<%= ResolveUrl("~/Forum/Icon/Post_Icon.png") %>' alt="Post Icon"/>
                    <span>Post</span>
                </a>
            </li>
        </ul>
    </div>

    <!-- Main content -->
    <div class="main-content">
        <h1>Notification</h1>

        <!-- Filter Buttons -->
        <div class="filters">
            <button class="filter-btn active" data-filter="all">All</button>
            <button class="filter-btn" data-filter="like">Likes</button>
            <button class="filter-btn" data-filter="comment">Comments</button>
        </div>

        <asp:Repeater ID="Notification" runat="server">
            <ItemTemplate>

                <!-- Notifications List -->
                <div class="notification-list" id="notifications">
                    <asp:Panel runat="server" Visible='<%# Eval("Action").ToString() == "Like" %>'>
                        <div class="notification like">
                            <img class="notification-profile-picture" 
                                     src='<%# ResolveUrl("~/Images/" + Eval("User_Profile")) %>' 
                                     alt="Profile Picture" />
                            <div class="notification-content">
                                <div class="notification-header">@<%# Eval("User_Name") %> liked your post</div>
                                <div class="notification-footer"><%# Eval("DateTime") %></div>
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel runat="server" Visible='<%# Eval("Action").ToString() == "Comment" %>'>
                        <div class="notification comment">
                            <img class="notification-profile-picture" 
                                     src='<%# ResolveUrl("~/Images/" + Eval("User_Profile")) %>' 
                                     alt="Profile Picture"/>
                            <div class="notification-content">
                                <div class="notification-header">@<%# Eval("User_Name") %> commented on your post</div>
                                <div class="notification-comment"><%# Eval("Text") %></div>
                                <div class="notification-footer"><%# Eval("DateTime") %></div>
                            </div>
                        </div>
                    </asp:Panel>
                </div>

            </ItemTemplate>
        </asp:Repeater>

    </div>
</div>

</asp:Content>