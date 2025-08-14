<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InvestmentPage.aspx.cs" Inherits="bipj.InvestmentPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        <div class="d-flex justify-content-between align-items-center mb-4 p-3 bg-white rounded shadow-sm">
            <h1 class="h3 mb-0">My Investment Portfolios</h1>
            <asp:Button ID="btnCreateNewPortfolio" runat="server" Text="+ Create New Portfolio" OnClick="btnCreateNewPortfolio_Click"
                CssClass="btn btn-primary" />
        </div>

        <asp:Panel ID="pnlEmptyData" runat="server" Visible="false" CssClass="text-center bg-white p-5 rounded shadow-sm">
            <p class="text-muted">You haven't created any investment portfolios yet.</p>
            <p class="mt-2 text-muted">Click the button above to get started!</p>
        </asp:Panel>

        <asp:Repeater ID="rptPortfolios" runat="server" OnItemCommand="rptPortfolios_ItemCommand">
            <HeaderTemplate>
                <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="col">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body d-flex flex-column">
                            <h5 class="card-title"><%# Eval("PortfolioName") %></h5>
                            <p class="card-text"><%# Eval("Description") %></p>
                            <p class="card-text text-muted small">Last Updated: <%# Eval("LastUpdatedAt", "{0:MMMM d, yyyy}") %></p>
                            
                            <div class="mt-auto text-end">
                                 <asp:Button ID="btnView" runat="server" Text="View & Add Assets" CommandName="View" CommandArgument='<%# Eval("PortfolioID") %>'
                                     CssClass="btn btn-success btn-sm me-2" />
                                <%-- ✅ MODIFIED: The "Edit" button is now an "Analyze" button that links to the dashboard. --%>
                                <asp:Button ID="btnAnalyze" runat="server" Text="Analyze" CommandName="Analyze" CommandArgument='<%# Eval("PortfolioID") %>'
                                     CssClass="btn btn-info btn-sm me-2" />
                                 <asp:Button ID="btnDelete" runat="server" Text="Delete" CommandName="Delete" CommandArgument='<%# Eval("PortfolioID") %>'
                                     OnClientClick="return confirm('Are you sure you want to delete this portfolio? This will remove all assets it contains.');"
                                     CssClass="btn btn-danger btn-sm" />
                            </div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:Repeater>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
