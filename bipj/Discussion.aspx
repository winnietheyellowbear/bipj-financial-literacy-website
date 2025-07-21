<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" 
    MaintainScrollPositionOnPostBack="true" CodeBehind="Discussion.aspx.cs" Inherits="bipj.Discussion" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <head>
        <!-- Include external CSS files -->
        <link rel="stylesheet" href="Forum_Nav.css">
        <link rel="stylesheet" href="Forum_Post.css">

        <!-- Include ScriptManager for AJAX functionality -->
        <asp:ScriptManager ID="ScriptManager" runat="server" />
    </head>

    <!-- Main Styles -->
    <style>
        .content-wrapper {
            display: flex;
            align-items: flex-start;
            margin-top: 10px;
        }

        .main-content {
            flex: 1;
            background-color: #f8f9fa;
            padding: 30px;
            border-radius: 10px;
            margin-left: 20px;
            max-width: 1000px;
        }
    </style>

    <!-- Main Content Structure -->
    <div class="content-wrapper">
        
        <!-- Sidebar -->
        <div class="sidebar">
            <ul>
                <br />
                <br />
                <li class="active">
                    <a href="Discussion.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/Discussion_Icon.png") %>'/>
                        <span>Discussion</span>
                    </a>
                </li>
                <li>
                    <a href="SmartSearch.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/Magnifying_Glass_Icon.png") %>'/>
                        <span>Smart Search</span>
                    </a>
                </li>
                <li>
                    <a href="MyNotification.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/Notification_Icon.png") %>'/>
                        <span>Notification</span>
                    </a>
                </li>
                <li>
                    <a href="MyPost.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/MyPost_Icon.png") %>'/>
                        <span>My Post</span>
                    </a>
                </li>
                <li>
                    <a href="Post.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/Post_Icon.png") %>'/>
                        <span>Post</span>
                    </a>
                </li>
            </ul>
        </div>

        <!-- Main Content -->
        <div class="main-content">
            <h1>Welcome to the forum</h1>

            <!-- Search and Filter Section -->
            <div class="search-filter-container">
                <asp:TextBox ID="searchInput" runat="server" CssClass="search-bar" 
                             placeholder="Search by username or text..." OnTextChanged="Search" AutoPostBack="true" />
                
                <asp:DropDownList ID="categoryFilter" runat="server" CssClass="filter-dropdown" OnSelectedIndexChanged="Search" AutoPostBack="true">
                    <asp:ListItem>category</asp:ListItem>
                    <asp:ListItem>ask a question</asp:ListItem>
                    <asp:ListItem>share my journey</asp:ListItem>
                    <asp:ListItem>share tips and tools</asp:ListItem>
                </asp:DropDownList>
            </div>

            <!-- Posts Section -->
            <asp:UpdatePanel ID="UpdatePanel_Post" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Repeater ID="Post" runat="server">
                        <ItemTemplate>
                            <!-- Post Details -->
                            <div class="forum-post">
                                
                                <!-- Profile Section -->
                                <div class="post-header">
                                    <div class="profile-image">
                                        <asp:Image ID="imgProfile" runat="server" CssClass="profile-pic" 
                                            ImageUrl='<%# ResolveUrl("~/Images/" + Eval("Profile")) %>' />
                                    </div>
                                    <div class="user-info">
                                        <div><strong><%# Eval("Name") %></strong></div>
                                        <div><%# Eval("Post_DateTime") %> <%# Eval("Last_Update_DateTime") %></div>
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
                                            <asp:Image runat="server" style="width:100px; height: 100px"
                                                ImageUrl='<%# ResolveUrl((string)Container.DataItem) %>' 
                                                Visible='<%# !string.IsNullOrEmpty((string)Container.DataItem) %>' />
                                        </ItemTemplate>
                                    </asp:Repeater>

                                    <asp:Repeater ID="Video" runat="server" DataSource='<%# Eval("Videos_List") %>'>
                                        <ItemTemplate>
                                            <asp:Panel runat="server" Visible='<%# !string.IsNullOrEmpty((string)Container.DataItem) %>'>
                                                <video controls style="width:100px">
                                                    <source src='<%# ResolveUrl((string)Container.DataItem) %>' type="video/mp4" />
                                                </video>
                                            </asp:Panel>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>

                                <!-- Like Button -->
                                <div class="forum-actions">
                                            <asp:LinkButton ID="btn_like" runat="server" CommandArgument='<%# Eval("Post_ID") %>' 
                                                CssClass='<%# (bool)Eval("Like_Status") ? "btn-red" : "btn-blue" %>' OnClick="btn_like_Click">
                                                <%# (bool)Eval("Like_Status") ? "Liked" : "Like" %>
                                                (<%# GetLikeCount(Eval("Post_ID").ToString()) %>)
                                            </asp:LinkButton>
                                </div>

                                <!-- Comments Section -->
                                <div class="comments-section">
                                            <asp:Repeater ID="Comment" runat="server" DataSource='<%# Eval("Comments_List") %>'>
                                                <ItemTemplate>
                                                    <div class="comment">
                                                        <img src='<%# ResolveUrl("~/Images/" + Eval("User_Profile")) %>' 
                                                             class="profile-pic" />
                                                        <div class="comment-content">
                                                            <div class="comment-author"><%# Eval("User_Name") %></div>
                                                            <div class="comment-time"><%# Eval("Comment_DateTime", "{0:dd MMM yyyy, hh:mmtt}") %></div>
                                                            <div class="comment-text">
                                                                <%# Eval("Text") %>
                                                            </div>
            
                                                            <asp:LinkButton ID="btn_delete" runat="server" CssClass="btn-delete"
                                                                OnClientClick="return confirm('Are you sure you want to delete this comment?')"
                                                                Visible='<%# Eval("User_ID").ToString() == user_id %>' 
                                                                CommandArgument='<%# Eval("Comment_ID") %>' OnClick="btn_delete_comment_Click">Delete</asp:LinkButton>
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                </div>

                                <!-- AI Suggestion Section -->
                                <div class="ai-suggestion-section">
                                    <asp:Label ID="lbl_AISuggestion" runat="server" CssClass="ai-suggestion-label"/>
                                </div>

                                <!-- Comment Input Section -->
                                <div class="comment-input">
                                    <asp:TextBox ID="tb_text" runat="server" class="comment-textbox" placeholder="Write a comment..."
                                        onchange="validateComment(this)" oninput="validateComment(this)"></asp:TextBox>
                                    <asp:Button ID="btn_AI_suggestion" runat="server" Text="AI Suggestion" CssClass="ai-suggestion-button"
                                        CommandArgument='<%# Eval("Text") %>' OnClick="btn_comment_AI_suggestion_Click" Visible='<%# user_type == "Staff" %>' />
                                    <asp:Button ID="btn_publish" runat="server" Text="Comment" class="comment-button btn-submit btn-disabled"
                                        ToolTip="You cannot submit a blank comment." Disabled="true" CommandArgument='<%# Eval("Post_ID") %>'
                                        OnClick="btn_comment_Click" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <!-- Validation script for comment -->
    <script>
        function validateComment(textbox) {
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
</asp:Content>
