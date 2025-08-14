<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InsurancePlanPage.aspx.cs" Inherits="bipj.InsurancePlanPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        <div class="d-flex justify-content-between align-items-center mb-4 p-3 bg-white rounded shadow-sm">
            <h1 class="h3 mb-0">My Insurance Plans</h1>
            <asp:Button ID="btnCreateNewPlan" runat="server" Text="+ Create New Plan" OnClick="btnCreateNewPlan_Click"
                CssClass="btn btn-primary" />
        </div>

        <asp:Panel ID="pnlNoPlans" runat="server" Visible="false" class="text-center bg-white p-5 rounded shadow-sm">
            <p class="text-muted">You haven't created any insurance plans yet.</p>
            <p class="mt-2 text-muted">Click the button above to get started!</p>
        </asp:Panel>

        <asp:Repeater ID="rptInsurancePlans" runat="server" OnItemCommand="rptInsurancePlans_ItemCommand">
            <HeaderTemplate>
                <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="col">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body d-flex flex-column">
                            <h5 class="card-title"><%# Eval("PlanName") %></h5>
                            <p class="card-text text-muted small">Created: <%# Eval("CreatedAt", "{0:MMMM d, yyyy}") %></p>
                            <div class="mt-auto text-end">
                                 <asp:Button ID="btnViewPlan" runat="server" Text="View Details" CommandName="View" CommandArgument='<%# Eval("PlanID") %>'
                                     CssClass="btn btn-success btn-sm me-2" />
                                <asp:Button ID="btnCompare" runat="server" Text="Compare" CommandName="Compare" CommandArgument='<%# Eval("PlanID") %>'
                                     CssClass="btn btn-info btn-sm me-1" />
                                <asp:Button ID="btnEditPlan" runat="server" Text="Edit" CommandName="Edit" CommandArgument='<%# Eval("PlanID") %>'
                                     CssClass="btn btn-secondary btn-sm me-2" />
                                 <asp:Button ID="btnDeletePlan" runat="server" Text="Delete" CommandName="Delete" CommandArgument='<%# Eval("PlanID") %>'
                                     OnClientClick="return confirm('Are you sure you want to delete this plan?');"
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
