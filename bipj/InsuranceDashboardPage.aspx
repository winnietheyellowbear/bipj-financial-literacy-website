<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InsuranceDashboardPage.aspx.cs" Inherits="bipj.InsuranceDashboardPage" Async="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h1 class="h3 mb-0">
                <asp:Literal ID="litPlanName" runat="server">Plan Dashboard</asp:Literal>
            </h1>
            <asp:Button ID="btnBackToPlans" runat="server" Text="&larr; Back to All Plans" OnClick="btnBackToPlans_Click"
                CssClass="btn btn-outline-secondary" />
        </div>

        <asp:Panel ID="pnlLoading" runat="server" CssClass="text-center py-5" Visible="false">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="text-muted mt-2">Generating your personalized recommendations... Please wait.</p>
        </asp:Panel>

        <asp:Panel ID="pnlResults" runat="server" Visible="true">
             <div class="row g-4">
                <div class="col-lg-6">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body">
                            <h5 class="card-title">AI Recommended Insurance Strategy</h5>
                            <hr />
                            <asp:Literal ID="litGeneralRecommendation" runat="server"></asp:Literal>
                        </div>
                    </div>
                </div>

                <div class="col-lg-6">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body d-flex flex-column">
                            <h5 class="card-title">AI Recommended Policies & Comparison</h5>
                            <hr />
                            <asp:Literal ID="litPolicyComparison" runat="server"></asp:Literal>
                            <div class="mt-auto text-end">
                                 <asp:Button ID="btnViewComparison" runat="server" Text="View Full Comparison &rarr;" OnClick="btnViewComparison_Click"
                                     CssClass="btn btn-success" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
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
