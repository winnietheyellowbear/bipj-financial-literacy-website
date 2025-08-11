<%@ Page Title="Education Module" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="ViewSpecificEdu.aspx.cs" Inherits="bipj.ViewSpecificEdu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <style>
        .edu-viewer-container { display:flex; min-height:80vh; gap:0; }
        .edu-sidenav { background:#f8f9fa; width:280px; padding:20px; border-right:1px solid #ddd; }
        .topic-group { margin-bottom:18px; }
        .topic-title { color:#6a5fb4; font-weight:600; margin:0 0 8px 0; font-size:1.05rem; }
        .page-link { display:block; padding:6px 8px; border-radius:6px; text-decoration:none; color:#333; }
        .page-link:hover { background:#ecebfd; color:#2f2a6f; }
        .page-link.active { color:#2f2a6f; font-weight:700; background:#e6e4ff; }
        .edu-main { flex:1; padding:28px; }
        .edu-main h2 { margin-top:0; }
        .content-container { font-size:1.05rem; line-height:1.6; }
        .content-container img { max-width:100%; height:auto; border-radius:8px; margin:1rem 0; }
        .content-container iframe { width:100%; min-height:400px; border:none; border-radius:8px; margin:1rem 0; }
    </style>

    <div class="edu-viewer-container">
        <!-- Side Navigation -->
        <aside class="edu-sidenav">
            <h4 class="mb-3"><asp:Literal ID="ltModuleTitle" runat="server" /></h4>

            <asp:Repeater ID="rptTopics" runat="server" OnItemDataBound="rptTopics_ItemDataBound">
                <ItemTemplate>
                    <div class="topic-group">
                        <div class="topic-title"><%# Eval("TopicName") %></div>
                        <asp:Repeater ID="rptPages" runat="server" OnItemDataBound="rptPages_ItemDataBound">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkPage" runat="server" CssClass="page-link" />
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </aside>

        <!-- Main Content -->
        <main class="edu-main">
            <asp:Panel ID="pnlNoPageSelected" runat="server" Visible="true" CssClass="alert alert-info">
                Please select a page from the navigation.
            </asp:Panel>

            <asp:Panel ID="pnlPageContent" runat="server" Visible="false">
                <h2><asp:Literal ID="ltPageTitle" runat="server" /></h2>
                <hr />
                <div class="content-container">
                    <asp:Literal ID="ltPageContent" runat="server" />
                </div>
            </asp:Panel>
        </main>
    </div>
</asp:Content>