<%@ Page Title="All Profiles" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="AllProfile.aspx.cs" Inherits="bipj.AllProfile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .allprofile-container {
            max-width: 900px;
            margin: 0 auto;
            padding: 24px 0;
        }
        .search-bar {
            width: 100%;
            margin-bottom: 20px;
        }
        .profile-list-table {
            width: 100%;
            border-collapse: collapse;
        }
        .profile-list-table th, .profile-list-table td {
            padding: 12px 8px;
            border-bottom: 1px solid #e0e0e0;
        }
        .profile-list-table th {
            background: #f8f9fa;
        }
        .profile-view-btn {
            background: #007bff;
            color: #fff;
            border: none;
            border-radius: 6px;
            padding: 6px 16px;
            cursor: pointer;
            transition: background 0.2s;
        }
        .profile-view-btn:hover {
            background: #0056b3;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="allprofile-container">
        <h2>All Users</h2>
        <!-- Search Bar -->
        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control search-bar" placeholder="Search by name or email..." AutoPostBack="true" OnTextChanged="txtSearch_TextChanged" />
        
        <!-- User List Table -->
        <asp:GridView ID="gvProfiles" runat="server" AutoGenerateColumns="False" CssClass="profile-list-table" GridLines="None" ShowHeader="true" OnRowCommand="gvProfiles_RowCommand" EmptyDataText="No profiles found." AllowPaging="true" PageSize="10" OnPageIndexChanging="gvProfiles_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="ID" ItemStyle-Width="60px" />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:BoundField DataField="Point" HeaderText="Points" ItemStyle-Width="80px" />
                <asp:BoundField DataField="JoinDate" HeaderText="Join Date" DataFormatString="{0:MMMM yyyy}" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="btnViewProfile" runat="server" Text="View Profile" CssClass="profile-view-btn"
                            CommandName="ViewProfile" CommandArgument='<%# Eval("Id") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
