<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InsuranceFormPage.aspx.cs" Inherits="bipj.InsuranceFormPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container my-5">
        <div class="row justify-content-center">
            <div class="col-lg-8">
                <div class="card shadow-lg">
                    <div class="card-body p-5">
                        <div class="text-center mb-4">
                            <h2 class="card-title h3">Create Your Insurance Profile</h2>
                            <p class="text-muted">
                                Fill out the details below for a personalized recommendation.
                            </p>
                        </div>

                        <div class="mb-3">
                            <label for="<%=txtPlanName.ClientID%>" class="form-label">Plan Name</label>
                            <asp:TextBox ID="txtPlanName" runat="server" CssClass="form-control" placeholder="e.g., My Family Protection Plan"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvPlanName" runat="server" ControlToValidate="txtPlanName" ErrorMessage="Plan Name is required." CssClass="text-danger small mt-1" Display="Dynamic"></asp:RequiredFieldValidator>
                        </div>

                        <div class="row g-3 mb-3">
                            <div class="col-md-6">
                                <label for="<%=txtAge.ClientID%>" class="form-label">Your Age</label>
                                <asp:TextBox ID="txtAge" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                            </div>
                            <div class="col-md-6">
                                <label for="<%=ddlGender.ClientID%>" class="form-label">Gender</label>
                                <asp:DropDownList ID="ddlGender" runat="server" CssClass="form-select">
                                    <asp:ListItem Text="Select..." Value=""></asp:ListItem>
                                    <asp:ListItem>Male</asp:ListItem>
                                    <asp:ListItem>Female</asp:ListItem>
                                    <asp:ListItem>Prefer not to say</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="mb-3">
                            <label for="<%=txtOccupation.ClientID%>" class="form-label">Occupation</label>
                            <asp:TextBox ID="txtOccupation" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>

                        <div class="mb-3">
                            <label for="<%=txtAnnualIncome.ClientID%>" class="form-label">Annual Income (USD)</label>
                            <asp:TextBox ID="txtAnnualIncome" runat="server" CssClass="form-control" TextMode="Number" step="1000"></asp:TextBox>
                        </div>

                        <div class="row g-3 mb-3">
                            <div class="col-md-6">
                                <label for="<%=ddlMaritalStatus.ClientID%>" class="form-label">Marital Status</label>
                                <asp:DropDownList ID="ddlMaritalStatus" runat="server" CssClass="form-select">
                                     <asp:ListItem Text="Select..." Value=""></asp:ListItem>
                                     <asp:ListItem>Single</asp:ListItem>
                                     <asp:ListItem>Married</asp:ListItem>
                                     <asp:ListItem>Divorced</asp:ListItem>
                                     <asp:ListItem>Widowed</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                             <div class="col-md-6">
                                <label for="<%=ddlRiskTolerance.ClientID%>" class="form-label">Risk Tolerance</label>
                                <asp:DropDownList ID="ddlRiskTolerance" runat="server" CssClass="form-select">
                                    <asp:ListItem Text="Select..." Value=""></asp:ListItem>
                                    <asp:ListItem>Low</asp:ListItem>
                                    <asp:ListItem>Medium</asp:ListItem>
                                    <asp:ListItem>High</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="mb-3">
                            <div class="form-check">
                                <asp:CheckBox ID="chkHasDependents" runat="server" Text=" I have dependents" AutoPostBack="true" OnCheckedChanged="chkHasDependents_CheckedChanged" CssClass="form-check-input" />
                            </div>
                            <asp:Panel ID="pnlDependents" runat="server" Visible="false" class="mt-2">
                                 <label for="<%=txtNumberOfDependents.ClientID%>" class="form-label">Number of Dependents</label>
                                 <asp:TextBox ID="txtNumberOfDependents" runat="server" CssClass="form-control" TextMode="Number" Text="0"></asp:TextBox>
                            </asp:Panel>
                        </div>
                        
                        <div class="mb-3">
                            <label for="<%=txtHealthStatus.ClientID%>" class="form-label">Briefly describe your general health status (e.g., excellent, any chronic conditions).</label>
                            <asp:TextBox ID="txtHealthStatus" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </div>

                        <div class="mb-3">
                            <label for="<%=txtLifestyle.ClientID%>" class="form-label">Describe your lifestyle (e.g., active, sedentary, hobbies, travel habits).</label>
                            <asp:TextBox ID="txtLifestyle" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </div>

                        <div class="mb-3">
                            <label for="<%=txtFinancialGoals.ClientID%>" class="form-label">What are your long-term financial goals (e.g., retirement, buying a house)?</label>
                            <asp:TextBox ID="txtFinancialGoals" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </div>

                        <div class="mb-3">
                            <label for="<%=txtExistingCoverage.ClientID%>" class="form-label">Do you have any existing insurance coverage? If so, please provide details.</label>
                            <asp:TextBox ID="txtExistingCoverage" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                        </div>

                        <div class="d-grid mt-4">
                            <asp:Button ID="btnSubmit" runat="server" Text="Get My Recommendations" OnClick="btnSubmit_Click"
                                CssClass="btn btn-primary btn-lg" />
                        </div>
                         <asp:Literal ID="litError" runat="server" Visible="false"></asp:Literal>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
