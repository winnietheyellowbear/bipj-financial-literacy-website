<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InsuranceComparisonPage.aspx.cs" Inherits="bipj.InsuranceComparisonPage" Async="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h1 class="h3 mb-0">
                AI Policy Analysis for <asp:Literal ID="litPlanName" runat="server"></asp:Literal>
            </h1>
            <asp:Button ID="btnBackToDashboard" runat="server" Text="&larr; Back to Dashboard" OnClick="btnBackToDashboard_Click"
                CssClass="btn btn-outline-secondary" />
        </div>

        <asp:Panel ID="pnlLoading" runat="server" CssClass="text-center py-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="text-muted mt-2">Performing detailed analysis... Please wait.</p>
        </asp:Panel>

        <asp:Panel ID="pnlResults" runat="server" Visible="false">
            <asp:Repeater ID="rptAnalysis" runat="server">
                <HeaderTemplate>
                    <div class="row row-cols-1 row-cols-md-2 g-4">
                </HeaderTemplate>
                <ItemTemplate>
                    <div class="col">
                        <div class="card h-100 shadow-sm">
                            <div class="card-header fw-bold">
                                <%# Eval("InsuranceType") %>
                            </div>
                            <div class="card-body">
                                <h5 class="card-title">Top Recommendation: <%# Eval("BestPolicyName") %></h5>
                                <p class="card-text"><%# Eval("Justification") %></p>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
                <FooterTemplate>
                    </div>
                </FooterTemplate>
            </asp:Repeater>
        </asp:Panel>

        <asp:Panel ID="pnlError" runat="server" Visible="false" class="mt-4 alert alert-danger">
             <h4 class="alert-heading">An Error Occurred</h4>
             <p>
                 <asp:Literal ID="litErrorMessage" runat="server"></asp:Literal>
             </p>
        </asp:Panel>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
