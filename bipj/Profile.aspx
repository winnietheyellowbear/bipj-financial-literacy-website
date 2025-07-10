<%@ Page Title="Profile" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="bipj.Profile" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .profile-container {
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
        }
        .profile-header {
            text-align: center;
            margin-bottom: 30px;
        }
        .profile-picture {
            width: 150px;
            height: 150px;
            border-radius: 50%;
            object-fit: cover;
            border: 5px solid #fff;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .profile-info {
            background: #f8f9fa;
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 30px;
        }
        .comment-section {
            margin-top: 40px;
        }
        .comment-box {
            margin-bottom: 20px;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 15px;
            background: #fff;
        }
        .comment {
            border-bottom: 1px solid #eee;
            padding: 15px 0;
        }
        .comment:last-child {
            border-bottom: none;
        }
        .comment-author {
            font-weight: bold;
            margin-right: 10px;
        }
        .comment-date {
            color: #6c757d;
            font-size: 0.9em;
        }
        /* New styles for delete buttons */
        .comment-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 10px;
        }
        .comment-actions {
            display: flex;
            gap: 10px;
        }
        .btn-delete {
            color: #dc3545;
            background: none;
            border: none;
            padding: 0;
            cursor: pointer;
        }
        .btn-delete:hover {
            color: #bb2d3b;
            text-decoration: underline;
        }
.profile-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 30px;
    padding-top: 30px; /* Give space below navbar */
    position: relative;
}

.profile-header-left {
    display: flex;
    flex-direction: column;
    align-items: center;
    flex: 1;
}

.btn-edit-profile {
    background: #4f6ef7;
    color: #fff;
    border: none;
    padding: 8px 20px;
    border-radius: 22px;
    font-size: 1rem;
    font-weight: 500;
    cursor: pointer;
    transition: background 0.18s;
    z-index: 3;
    margin-left: 16px;
    margin-right: 0;
    margin-top: 0;
}
.btn-edit-profile:hover {
    background: #2b48c4;
}

    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="profile-container">



        <!-- Profile Header -->
      <div class="profile-header">
    <div class="profile-header-left">
        <asp:Image ID="imgProfile" runat="server" CssClass="profile-picture" AlternateText="Profile Picture" />
        <h2><asp:Literal ID="ltName" runat="server" /></h2>
        <p class="text-muted">Member since <asp:Literal ID="ltJoinDate" runat="server" /></p>
        <p><asp:Literal ID="ltPoints" runat="server" /> Points</p>
    </div>
    <asp:Button ID="btnEditProfile" runat="server" Text="Edit Profile"
        CssClass="btn-edit-profile"
        OnClick="btnEditProfile_Click" />
</div>


        <!-- Profile Info -->
        <div class="profile-info">
            <h4>About Me</h4>
            <asp:Literal ID="ltBio" runat="server" />
            <hr />
            <p><i class="bi bi-envelope"></i> <asp:Literal ID="ltEmail" runat="server" /></p>
        </div>

       <!-- Comment Section -->
    <div class="comment-section">
        <h4>Comments</h4>
        
        <!-- Comment Form -->
        <div class="comment-box">
            <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="3" 
                CssClass="form-control mb-2" placeholder="Write a comment..."></asp:TextBox>
            <asp:Button ID="btnPostComment" runat="server" Text="Post Comment" 
                CssClass="btn btn-primary" OnClick="btnPostComment_Click" />
        </div>
        
        <!-- Comments List with Delete Buttons -->
        <asp:Repeater ID="rptComments" runat="server" OnItemCommand="rptComments_ItemCommand">
            <ItemTemplate>
                <div class="comment">
                    <div class="comment-header">
                        <div>
                            <span class="comment-author"><%# Eval("UserName") %></span>
                            <span class="comment-date"><%# Eval("CommentDate", "{0:MMMM dd, yyyy}") %></span>
                        </div>
                        <div class="comment-actions">
                            <asp:LinkButton ID="btnDelete" runat="server" 
                                CommandName="Delete" 
                                CommandArgument='<%# Eval("Id") %>'
                                CssClass="btn-delete"
                                OnClientClick="return confirm('Are you sure you want to delete this comment?');"
                                Visible='<%# CanDeleteComment(Convert.ToInt32(Eval("UserId"))) %>'>
                                <i class="bi bi-trash"></i> Delete
                            </asp:LinkButton>
                        </div>
                    </div>
                    <p><%# Eval("CommentText") %></p>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        
        <asp:Label ID="lblNoComments" runat="server" Text="No comments yet." CssClass="text-muted" Visible="false"></asp:Label>
    </div>
</asp:Content>