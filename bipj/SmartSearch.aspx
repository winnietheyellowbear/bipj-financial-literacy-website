<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" MaintainScrollPositionOnPostBack="true" CodeBehind="SmartSearch.aspx.cs" Inherits="bipj.SmartSearch" Async="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

  <head>
    <link rel="stylesheet" href="Forum_Nav.css">
    <link rel="stylesheet" href="Forum_Post.css">
  </head>

  <style>
  .search-container {
      max-width: 800px;
      background-color: white;
      padding: 30px;
      border-radius: 12px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      text-align: center;
      margin-bottom: 20px;
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


    <head>
      <link rel="stylesheet" href="Forum_Nav.css">
      <asp:ScriptManager ID="ScriptManager1" runat="server" />
    </head>

    <style>
    /* Ensure the sidebar and main content are aligned properly */
    .content-wrapper {
        display: flex;
        align-items: flex-start; /* Align items at the top */
        margin-top: 10px;
    }

  
    /* Main content takes the remaining space */
    .main-content {
        flex: 1;
        background-color: #f8f9fa;
        padding: 30px;
        border-radius: 10px;
        margin-left: 20px; /* Add spacing so it doesn't overlap with the sidebar */
        max-width: 1000px;
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
              <li class="active">
                <a href="SmartSearch.aspx">
                    <img src='<%= ResolveUrl("~/Forum/Icon/Magnifying_Glass_Icon.png") %>' alt="Notification Icon"/>
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
              <asp:TextBox ID="txtSearch" runat="server" CssClass="search-bar" placeholder="e.g., Technology, Cooking, Travel..."></asp:TextBox>
            <br />
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="search-button" OnClick="btnSearch_Click" />

          </div>



 <asp:UpdatePanel ID="UpdatePanel" runat="server" UpdateMode="Conditional">
 <ContentTemplate>     
          
    <asp:Repeater ID="Post" runat="server">
    <ItemTemplate>
        <div class="forum-post">
         <!-- Profile Section -->
            <div class="post-header">
                <div class="profile-image">
                    <asp:Image ID="imgProfile" runat="server" CssClass="profile-pic" 
                        ImageUrl='<%# ResolveUrl("~/Images/" + Eval("Profile")) %>'/>
                </div>
                <div class="user-info">
                    <strong><%# Eval("Name") %></strong><br/>
                    <%# Eval("Post_DateTime") %> <%# Eval("Last_Update_DateTime") %>
                </div>
            </div>


            <!-- Post Content -->
            <div class="post-content">
                <%# Eval("Text") %>
            </div>

            <!-- Image & Video Section -->
           <div class="post-media">
                <asp:Repeater ID="Image" runat="server" DataSource='<%# Eval("Images_List") %>'>
                <ItemTemplate>
                    <asp:Image runat="server" CssClass="post-img" style="width:100px; height: 100px"
                        ImageUrl='<%# ResolveUrl((string)Container.DataItem) %>' Visible='<%# !string.IsNullOrEmpty((string)Container.DataItem) %>'/>
                </ItemTemplate>
                </asp:Repeater>

                <asp:Repeater ID="Video" runat="server" DataSource='<%# Eval("Videos_List") %>'>
                <ItemTemplate>
                    <asp:Panel runat="server" Visible='<%# !string.IsNullOrEmpty((string)Container.DataItem) %>'>
                    <video controls class="post-video" style="max-width:100px; border-radius:8px;">
                        <source src='<%# ResolveUrl((string)Container.DataItem) %>' type="video/mp4" />
                    </video>
                    </asp:Panel>
                </ItemTemplate>
                </asp:Repeater>
              
           </div>

            
            <!-- Like & Comment Buttons -->
            <div class="forum-actions">
               
                <asp:LinkButton ID="btn_like" runat="server" CommandArgument='<%# Eval("Post_ID") %>' 
                    CssClass='<%# (bool)Eval("Like_Status") ? "btn-red" : "btn-blue" %>'>
                    <%# (bool)Eval("Like_Status") ? "Liked" : "Like" %>
                    (<asp:Label ID="lbl_Like_Count" runat="server" Text=""></asp:Label>)
                </asp:LinkButton>
               
            </div>

              <!-- Comments Section -->
            <div class="comments-section" style="overflow:scroll; overflow-x: hidden; min-height: 0px; max-height: 100px">
                
            <asp:Repeater ID="Comment" runat="server" DataSource='<%# Eval("Comments_List") %>'>
                <ItemTemplate>
                    <div class="comment">
                        <img src='<%# ResolveUrl("~/Images/" + Eval("User_Profile")) ?? "https://via.placeholder.com/32"  %>' 
                             alt='<%# Eval("User_Name") %> Profile' 
                             class="profile-pic" />
                        <div class="comment-content">
                            <div class="comment-author"><%# Eval("User_Name") %></div>
                            <div class="comment-time"><%# Eval("Comment_DateTime", "{0:dd MMM yyyy, hh:mmtt}") %></div>
                            <div class="comment-text">
                                <%# Eval("Text") %>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
               
        </div>
                

             <!-- Comment Input Section -->
            <div class="comment-input">
                <asp:TextBox ID="tb_text" runat="server" class="comment-textbox" placeholder="Write a comment..."
                    onchange="validatePost(this)" oninput="validatePost(this)"></asp:TextBox>
                <asp:Button ID="btn_publish" runat="server" Text="Comment" class="comment-button btn-submit btn-disabled"
                    ToolTip="You cannot submit a blank comment." Disabled="true" CommandArgument='<%# Eval("Post_ID") %>'
                    />
            </div>
        </div>

        <script>
            function validatePost(textbox) {
                const text = textbox.value.trim();
                const container = textbox.closest(".forum-post");
                const submitButton = container.querySelector(".comment-button");

                if (text.length > 0) {
                    submitButton.disabled = false;
                    submitButton.classList.remove("btn-disabled");
                    submitButton.classList.add("btn-enabled");
                    submitButton.removeAttribute("title");
                } else {
                    submitButton.disabled = true;
                    submitButton.classList.remove("btn-enabled");
                    submitButton.classList.add("btn-disabled");
                    submitButton.title = "You cannot submit a blank comment.";
                }
            }
        </script>

    </ItemTemplate>
    </asp:Repeater>

    </ContentTemplate>
    </asp:UpdatePanel>

      </div>
  </div>


</asp:Content>