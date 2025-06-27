<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" MaintainScrollPositionOnPostBack="true" CodeBehind="SmartSearch.aspx.cs" Inherits="bipj.SmartSearch" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

  <head>
    <link rel="stylesheet" href="Forum_Nav.css">
    <asp:ScriptManager ID="ScriptManager" runat="server" />
  </head>

  <style>

  body {
      font-family: 'Quicksand', sans-serif;
      background-color: #f4f6f9;
      margin: 0;
      padding: 0;
  }



  .content-wrapper {
      display: flex;
      min-height: 100vh;
  }

  .main-content {
      flex: 1;
      padding: 40px;
      background-color: #f4f6f9;
  }

  .search-container {
      max-width: 700px;
      margin: 0 auto;
      background-color: white;
      padding: 30px;
      border-radius: 12px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      text-align: center;
  }

  .search-header {
      font-size: 28px;
      font-weight: bold;
      color: #333;
      margin-bottom: 20px;
  }

  .search-subheader {
      font-size: 16px;
      color: #666;
      margin-bottom: 25px;
  }

  .search-bar {
      width: 100%;
      max-width: 600px;
      padding: 14px 20px;
      font-size: 16px;
      border: 2px solid #ccc;
      border-radius: 8px;
      outline: none;
      margin-bottom: 20px;
      transition: border-color 0.3s ease;
  }

  .search-bar:focus {
      border-color: #007BFF;
      box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.2);
  }

  .search-button {
      padding: 12px 30px;
      font-size: 16px;
      background-color: #007BFF;
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      transition: background-color 0.3s ease;
  }

  .search-button:hover {
      background-color: #0056b3;
  }

  @media (max-width: 768px) {
      
      .search-container {
          padding: 20px;
      }
  }

  </style>

  <!-- Sidebar and main content wrapper -->
  <div class="content-wrapper">
      <!-- Sidebar -->
      <div class="sidebar">
          <ul>
              <li class="active">
                  <a href="Discussion.aspx">
                      <img src='<%= ResolveUrl("~/Forum/Icon/Discussion_Icon.png") %>' alt="Discussion Icon"/>
                      <span>Discussion</span>
                  </a>
              </li>
              <li>
                <a href="SmartSearch.aspx">
                    <img src='<%= ResolveUrl("~/Forum/Icon/Notification_Icon.png") %>' alt="Notification Icon"/>
                    <span>Smart Search</span>
                </a>
            </li>
              <li>
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

      <!-- Main Content Area -->
      <div class="main-content">
          <div class="search-container">
              <h2 class="search-header">Tell Us What You're Interested In</h2>
              <p class="search-subheader">We'll find the best discussions for you based on your interests.</p>
              <input type="text" class="search-bar" id="searchInput" placeholder="e.g., Technology, Cooking, Travel..." />
              <br/>
              <button class="search-button">Search</button>
          </div>
      </div>
  </div>


</asp:Content>