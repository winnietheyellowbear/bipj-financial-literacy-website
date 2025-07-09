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
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="profile-container">
        <!-- Profile Header -->
        <div class="profile-header">
            <asp:Image ID="imgProfile" runat="server" CssClass="profile-picture" AlternateText="Profile Picture" />
            <h2><asp:Literal ID="ltName" runat="server" /></h2>
            <p class="text-muted">Member since <asp:Literal ID="ltJoinDate" runat="server" /></p>
            <p><asp:Literal ID="ltPoints" runat="server" /> Points</p>
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
            
            <!-- Comments List -->
            <asp:Repeater ID="rptComments" runat="server">
                <ItemTemplate>
                    <div class="comment">
                        <div class="d-flex justify-content-between mb-2">
                            <span class="comment-author"><%# Eval("UserName") %></span>
                            <span class="comment-date"><%# Eval("CommentDate", "{0:MMMM dd, yyyy}") %></span>
                        </div>
                        <p><%# Eval("CommentText") %></p>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            
            <asp:Label ID="lblNoComments" runat="server" Text="No comments yet." CssClass="text-muted" Visible="false"></asp:Label>
        </div>
    </div>
</asp:Content>