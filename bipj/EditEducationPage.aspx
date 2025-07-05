<%@ Page Title="Edit Education Page" Language="C#" MasterPageFile="~/Staff_Nav.Master" AutoEventWireup="true" CodeBehind="EditEducationPage.aspx.cs" Inherits="bipj.EditEducationPage" %>
<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<div class="edu-admin-container" style="display:flex;min-height:600px;">
    <!-- Side Navigation -->
    <div class="edu-sidenav" style="background:#222;color:#fff;width:220px;padding:20px 10px 20px 10px;display:flex;flex-direction:column;">
        <a href="ManageEducation.aspx" class="btn btn-sm btn-outline-light mb-3">&larr; Save and go back</a>
        <asp:Repeater ID="rptTopics" runat="server">
            <ItemTemplate>
                <div>
                    <div style='margin-bottom:5px;font-weight:bold;background:#8576b1;color:white;padding:7px 10px;border-radius:5px;'>
                        <%# Eval("TopicName") %>
                        <span style="float:right;">
                            <i class='bi bi-caret-down-fill'></i>
                        </span>
                    </div>
                    <asp:Repeater ID="rptPages" runat="server" DataSource='<%# Eval("Pages") %>'>
                        <ItemTemplate>
                            <a href='EditEducationPage.aspx?moduleId=<%# Eval("ModuleId") %>&pageId=<%# Eval("Id") %>'
                               style='display:block;margin-left:12px;margin-bottom:6px;color:<%# (Eval("Id").ToString()==Eval("CurrentPageId").ToString())?"#2be3c3":"white" %>;'>
                               &bull; <%# Eval("Title") %>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <!-- Main Editing Panel -->
    <div style="flex:1;padding:40px;">
        <div>
            <!-- Toolbar -->
            <asp:Panel ID="pnlToolbar" runat="server">
                <asp:Button ID="btnAddTitle" runat="server" CssClass="btn btn-primary" Text="Add Title" />
                <asp:Button ID="btnAddText" runat="server" CssClass="btn btn-primary" Text="Add text" />
                <asp:Button ID="btnRemove" runat="server" CssClass="btn btn-primary" Text="Remove items" />
                <asp:Button ID="btnAddVideo" runat="server" CssClass="btn btn-primary" Text="Add video" />
            </asp:Panel>
        </div>
        <hr />
        <!-- Content Editor Area -->
        <asp:TextBox ID="txtPageTitle" runat="server" CssClass="form-control mb-2" placeholder="Page Title" />
        <asp:TextBox ID="txtContent" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="12" placeholder="Content here..." />
        <asp:Button ID="btnSave" runat="server" CssClass="btn btn-success mt-2" Text="Save Page" OnClick="btnSave_Click" />
        <asp:Label ID="lblMessage" runat="server" CssClass="text-success mt-2" />
    </div>
</div>

</asp:Content>
