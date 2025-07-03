<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" MaintainScrollPositionOnPostBack="true" CodeBehind="MyPost.aspx.cs" Inherits="bipj.MyPost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <head>
      <link rel="stylesheet" href="Forum_Nav.css">
      <asp:ScriptManager ID="ScriptManager" runat="server" />
    </head>

    <style>

    .btn-delete {
        background-color: #fccaca;
        color: white;
    }

    .btn-blue {
        background-color: #007BFF; /* Not liked */
        color: white;
        padding: 8px 14px;
        border: none;
        border-radius: 8px;
        font-size: 14px;
        font-weight: bold;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        text-decoration: none;
    }

    .btn-red {
        background-color: #E0245E; /* Liked */
        color: white;
        padding: 8px 14px;
        border: none;
        border-radius: 8px;
        font-size: 14px;
        font-weight: bold;
        cursor: pointer;
        transition: all 0.3s ease-in-out;
        text-decoration: none;
    }



    body {
        font-family: 'Quicksand', sans-serif;
    }

    /* Styling for btn_post */
    .btn-post {
        background-color: green;
        color: white;
        text-decoration: none;
        border: none;
        cursor: pointer;
        text-align: left;
        display: block;
        padding: 10px;
        width: 100%;
        border-radius: 5px;
        font-size: 16px;
    }

   

    .btn-post:hover {
        background-color: #575757;
    }

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

    /* Ensure the forum header is properly aligned */
    .forum-header {
        text-align: left;
        font-size: 26px;
        font-weight: bold;
        color: #333;
        margin-bottom: 20px;
        padding-bottom: 10px;
        border-bottom: 2px solid #ddd;
    }

    /* Forum Post Styling */
    .forum-post {
        background-color: #fff;
        border: 1px solid #ddd;
        border-radius: 12px;
        padding: 20px;
        margin-bottom: 20px;
        box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);
        transition: all 0.3s ease-in-out;
        max-width: 800px;
    }

    .forum-post:hover {
        box-shadow: 0px 6px 12px rgba(0, 0, 0, 0.2);
    }

    /* Profile Section */
    .post-header {
        display: flex;
        align-items: center;
        margin-bottom: 10px;
    }

    /* Profile Picture Placeholder */
    .profile-picture {
        width: 45px;
        height: 45px;
        border-radius: 50%;
        background-color: #007bff;
        display: flex;
        justify-content: center;
        align-items: center;
        font-size: 18px;
        font-weight: bold;
        color: white;
        margin-right: 12px;
    }

    .profile-image {
        width: 50px;
        height: 50px;
        border-radius: 50%;
        overflow: hidden;
        margin-right: 10px;
    }

    /* Profile Picture */
    .profile-pic {
        width: 50px; /* Main post profile picture size */
        height: 50px;
        border-radius: 50%; /* Makes the image circular */
        overflow: hidden; /* Hides any overflowing parts */
        object-fit: cover; /* Ensures the image fills the container */
        background-color: #ddd; /* Fallback color if image fails to load */
    }

    /* Comment Profile Picture */
    .comment .profile-pic {
        width: 32px; /* Smaller size for comments */
        height: 32px;
        border-radius: 50%;
        overflow: hidden;
        object-fit: cover;
        background-color: #ddd; /* Fallback color */
    }


    /* User Info */
    .user-info {
        font-size: 14px;
        color: #555;
    }

    /* Post Content */
    .post-content {
        font-size: 16px;
        color: #333;
        line-height: 1.6;
        margin-bottom: 15px;
    }

    /* Media Display */
    .post-media {
        display: flex;
        flex-wrap: wrap;
        gap: 15px;
        margin-bottom: 15px;
    }

    .forum-post img {
        max-width: 100%;
        height: auto;
        border-radius: 8px;
        max-height: 250px;
        object-fit: cover;
    }

    .forum-post video {
        max-width: 100%;
        border-radius: 8px;
    }

    /* Forum Actions (Like, Comment, etc.) */
    .forum-actions {
        display: flex;
    }


     /* Like & Comment Buttons */
     .btn-edit, .btn-delete {
         padding: 8px 14px;
         border: none;
         border-radius: 8px;
         font-size: 14px;
         font-weight: bold;
         cursor: pointer;
         transition: all 0.3s ease-in-out;
     }

     .btn-comment {
        background-color: #28a745;
        color: white;
     }

    .btn-comment:hover {
        background-color: #1c7430;
    }

    /* Post Media Container */
    .post-media {
        display: flex;
        align-items: center; /* Aligns media properly */
        gap: 15px; /* Adds spacing between items */
        margin-top: 15px;
        flex-wrap: wrap; /* Ensures responsiveness */
    }

    /* Image Styling */
    .forum-post img {
        max-width: 100%;
        width: 250px; /* Fixed width for consistency */
        height: auto;
        border-radius: 8px;
        object-fit: cover;
    }

    /* Video Styling */
    .forum-post video {
        max-width: 100%;
        width: 350px; /* Slightly larger for better visibility */
        border-radius: 8px;
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

     /* Comments Section */
    .comments-section {
      max-height: 160px;
      overflow-y: auto;
      border-top: 1px solid #eee;
      margin-top: 10px;
      padding-top: 10px;
    }

    .comment {
      display: flex;
      align-items: flex-start;
      gap: 10px;
      margin-bottom: 12px;
    }

    
    .comment-content {
      background: #f1f3f5;
      padding: 8px 12px;
      border-radius: 8px;
      flex: 1;
    }

    .comment-author {
      font-size: 13px;
      font-weight: bold;
      color: #333;
    }

    .comment-time {
      font-size: 11px;
      color: #888;
    }

    .comment-text {
      font-size: 13px;
      color: #555;
      margin-top: 4px;
    }

    /* Comment Input */
    .comment-input {
      margin-top: 15px;
      display: flex;
      gap: 10px;
      align-items: center;
    }

    .comment-textbox {
      flex: 1;
      padding: 10px 12px;
      border: 1px solid #ccc;
      border-radius: 8px;
      font-size: 14px;
    }

   .comment-button {
      padding: 10px 12px;
      border: 1px solid #ccc;
      border-radius: 8px;
      font-size: 14px;
    }
 

   .btn-disabled {
      background-color: gray;
      cursor: not-allowed;
      color: white; /* Optional: To ensure text remains visible */
   }

   .btn-enabled {
      background-color: #3B387E;
      cursor: pointer;
      color: white; /* Optional: To ensure text remains visible */
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
                <li>
                    <a href="MyNotification.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/Notification_Icon.png") %>' alt="Notification Icon"/>
                        <span>Notification</span>
                    </a>
                </li>
                <li class="active">
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
           <h1>My post</h1>

   
   <asp:UpdatePanel ID="UpdatePanel_Post" runat="server" UpdateMode="Conditional">
   <ContentTemplate>         
    <asp:Repeater ID="Post" runat="server" OnItemDataBound="post_ItemDataBound">
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
                <asp:UpdatePanel ID="UpdatePanel_Like" runat="server" UpdateMode="Conditional">
                <ContentTemplate>     
                <asp:LinkButton ID="btn_like" runat="server" CommandArgument='<%# Eval("Post_ID") %>' 
                    CssClass='<%# (bool)Eval("Like_Status") ? "btn-red" : "btn-blue" %>' OnClick="btn_like_Click">
                    <%# (bool)Eval("Like_Status") ? "Liked" : "Like" %>
                    (<asp:Label ID="lbl_Like_Count" runat="server" Text=""></asp:Label>)
                </asp:LinkButton>
                <asp:Button ID="btn_edit" runat="server" CssClass="btn-edit"
                    Text="Edit" CommandArgument='<%# Eval("Post_ID") %>' OnClick="btn_edit_Click"/>
                <asp:Button ID="btn_delete" runat="server" CssClass="btn-delete"
                    Text="Delete" CommandArgument='<%# Eval("Post_ID") %>' OnClick="btn_delete_Click"/>
                </ContentTemplate>
                </asp:UpdatePanel>
            </div>

              <!-- Comments Section -->
            <div class="comments-section" style="overflow:scroll; overflow-x: hidden; min-height: 0px; max-height: 100px">
                <asp:UpdatePanel ID="UpdatePanel_Comment" runat="server" UpdateMode="Conditional">
                <ContentTemplate>     
            <asp:Repeater ID="Comment" runat="server">
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
                         
                            <br />
                            <asp:Button ID="btn_delete" runat="server" CssClass="btn-delete" Text="Delete"
                                Visible='<%# Eval("User_ID").ToString() == user_id %>' CommandArgument='<%# Eval("Comment_ID") %>' OnClick="btn_delete_comment_Click"/>

                        </div>
                        
                    </div>
                </ItemTemplate>
            </asp:Repeater>
                </ContentTemplate>
                </asp:UpdatePanel>
        </div>
                

             <!-- Comment Input Section -->
            <div class="comment-input">
                <asp:TextBox ID="tb_text" runat="server" class="comment-textbox" placeholder="Write a comment..."
                    onchange="validatePost(this)" oninput="validatePost(this)"></asp:TextBox>
                <asp:Button ID="btn_publish" runat="server" Text="Comment" class="comment-button btn-submit btn-disabled"
                    ToolTip="You cannot submit a blank comment." Disabled="true" CommandArgument='<%# Eval("Post_ID") %>'
                    OnClick="btn_comment_Click"/>
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
