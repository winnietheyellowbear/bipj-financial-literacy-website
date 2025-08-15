<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InvestmentPage.aspx.cs" Inherits="bipj.InvestmentPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        <%-- ✅ REFACTORED: Added a card to contain the creation form. --%>
        <div class="card shadow-sm mb-4">
            <div class="card-body">
                <h5 class="card-title">Create a New Portfolio</h5>
                <div class="input-group">
                    <asp:TextBox ID="txtNewPortfolioName" runat="server" CssClass="form-control" placeholder="e.g., 'Retirement Fund', 'Tech Stocks'"></asp:TextBox>
                    <asp:Button ID="btnCreateNewPortfolio" runat="server" Text="Create Portfolio" OnClick="btnCreateNewPortfolio_Click"
                        CssClass="btn btn-primary" />
                </div>
                <asp:RequiredFieldValidator ID="rfvPortfolioName" runat="server" ControlToValidate="txtNewPortfolioName"
                    ErrorMessage="Portfolio name is required." ForeColor="Red" Display="Dynamic" CssClass="mt-1 d-block" />
            </div>
        </div>

        <h3 class="mb-3">My Existing Portfolios</h3>

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
