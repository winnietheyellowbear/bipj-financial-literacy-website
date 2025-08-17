<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InsuranceFormPage.aspx.cs" Inherits="bipj.InsuranceFormPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
/* ============================================
   GLOBAL STYLES & ANIMATIONS
   ============================================ */
   
:root {
    --primary-color: #2563eb;
    --primary-dark: #1e40af;
    --success-color: #10b981;
    --danger-color: #ef4444;
    --warning-color: #f59e0b;
    --info-color: #06b6d4;
    --dark-color: #1f2937;
    --light-bg: #f8fafc;
    --card-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
    --card-hover-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

* {
    transition: all 0.3s ease;
}

body {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    min-height: 100vh;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.container {
    background: white;
    border-radius: 20px;
    padding: 2rem;
    margin-top: 2rem;
    margin-bottom: 2rem;
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
}

/* ============================================
   CARD STYLES WITH HOVER EFFECTS
   ============================================ */

.card {
    border: none;
    border-radius: 16px;
    transition: transform 0.3s ease, box-shadow 0.3s ease;
    overflow: hidden;
    background: white;
}

.card:hover {
    transform: translateY(-5px);
    box-shadow: var(--card-hover-shadow);
}

.card-header {
    background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-dark) 100%);
    color: white;
    font-weight: 600;
    padding: 1rem 1.5rem;
    border: none;
}

.card-body {
    padding: 1.5rem;
}

/* Insurance Plan Cards Grid */
.insurance-plan-card {
    position: relative;
    overflow: hidden;
}

.insurance-plan-card::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 4px;
    background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
}

.insurance-plan-card .card-title {
    color: var(--dark-color);
    font-size: 1.25rem;
    font-weight: 600;
    margin-bottom: 0.5rem;
}

.insurance-plan-card .btn-group {
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
}

/* ============================================
   FORM STYLES
   ============================================ */

.form-label {
    font-weight: 600;
    color: var(--dark-color);
    margin-bottom: 0.5rem;
    font-size: 0.95rem;
}

.form-control, .form-select {
    border-radius: 10px;
    border: 2px solid #e5e7eb;
    padding: 0.75rem 1rem;
    font-size: 1rem;
    transition: border-color 0.3s ease, box-shadow 0.3s ease;
}

.form-control:focus, .form-select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
    outline: none;
}

.form-check-input {
    width: 1.25rem;
    height: 1.25rem;
    border-radius: 4px;
    border: 2px solid #d1d5db;
}

.form-check-input:checked {
    background-color: var(--primary-color);
    border-color: var(--primary-color);
}

/* ============================================
   BUTTON STYLES
   ============================================ */

.btn {
    border-radius: 10px;
    padding: 0.625rem 1.25rem;
    font-weight: 600;
    text-transform: none;
    letter-spacing: 0.025em;
    transition: all 0.3s ease;
    border: none;
    position: relative;
    overflow: hidden;
}

.btn::before {
    content: '';
    position: absolute;
    top: 50%;
    left: 50%;
    width: 0;
    height: 0;
    border-radius: 50%;
    background: rgba(255, 255, 255, 0.3);
    transform: translate(-50%, -50%);
    transition: width 0.6s ease, height 0.6s ease;
}

.btn:hover::before {
    width: 300px;
    height: 300px;
}

.btn-primary {
    background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-dark) 100%);
    color: white;
}

.btn-primary:hover {
    transform: translateY(-2px);
    box-shadow: 0 10px 20px rgba(37, 99, 235, 0.3);
}

.btn-success {
    background: linear-gradient(135deg, var(--success-color) 0%, #059669 100%);
    color: white;
}

.btn-danger {
    background: linear-gradient(135deg, var(--danger-color) 0%, #dc2626 100%);
    color: white;
}

.btn-info {
    background: linear-gradient(135deg, var(--info-color) 0%, #0891b2 100%);
    color: white;
}

.btn-outline-secondary {
    background: transparent;
    border: 2px solid #d1d5db;
    color: var(--dark-color);
}

.btn-outline-secondary:hover {
    background: var(--light-bg);
    border-color: var(--primary-color);
    color: var(--primary-color);
}

/* ============================================
   DASHBOARD SPECIFIC STYLES
   ============================================ */

/* Loading Animation */
.spinner-border {
    width: 3rem;
    height: 3rem;
    border-width: 0.3rem;
}

.loading-container {
    min-height: 400px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
}

/* Budget Pie Chart Container */
.chart-container {
    position: relative;
    padding: 1.5rem;
    background: linear-gradient(135deg, #f8fafc 0%, #e5e7eb 100%);
    border-radius: 16px;
    margin-bottom: 1.5rem;
}

#budgetPieChart {
    max-height: 300px;
}

/* Recommendation Cards */
.recommendation-card {
    background: white;
    border-left: 4px solid var(--primary-color);
    padding: 1.5rem;
    margin-bottom: 1rem;
    border-radius: 12px;
    box-shadow: var(--card-shadow);
    transition: all 0.3s ease;
}

.recommendation-card:hover {
    transform: translateX(5px);
    box-shadow: var(--card-hover-shadow);
}

.recommendation-card h5 {
    color: var(--primary-color);
    font-weight: 600;
    margin-bottom: 0.5rem;
}

.recommendation-card h6 {
    color: #6b7280;
    font-size: 0.95rem;
    margin-bottom: 1rem;
}

/* Policy Cards */
.policy-card {
    background: white;
    border-radius: 12px;
    padding: 1rem;
    height: 100%;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    transition: all 0.3s ease;
    border: 2px solid transparent;
}

.policy-card:hover {
    border-color: var(--primary-color);
    transform: translateY(-3px);
    box-shadow: 0 8px 16px rgba(37, 99, 235, 0.15);
}

.policy-card .card-title {
    color: var(--dark-color);
    font-size: 0.95rem;
    font-weight: 600;
    margin-bottom: 0.5rem;
}

.policy-card .card-subtitle {
    color: var(--primary-color);
    font-size: 0.85rem;
    font-weight: 500;
}

.policy-card .card-text {
    color: #6b7280;
    font-size: 0.85rem;
    line-height: 1.4;
}

/* Filter Buttons */
.filter-btn {
    border-radius: 20px;
    padding: 0.5rem 1.25rem;
    margin: 0.25rem;
    font-size: 0.9rem;
    font-weight: 500;
    transition: all 0.3s ease;
}

.filter-btn.active {
    background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-dark) 100%);
    border-color: transparent;
    transform: scale(1.05);
}

.filter-btn:not(.active):hover {
    background: var(--light-bg);
    border-color: var(--primary-color);
    color: var(--primary-color);
}

/* Policy Column Animation */
.policy-column {
    transition: opacity 0.3s ease, transform 0.3s ease;
}

.policy-column[style*="visibility: hidden"] {
    opacity: 0;
    transform: scale(0.95);
}

.policy-column[style*="visibility: visible"] {
    opacity: 1;
    transform: scale(1);
}

/* ============================================
   COMPARISON PAGE STYLES
   ============================================ */

.analysis-card {
    background: white;
    border-radius: 16px;
    overflow: hidden;
    box-shadow: var(--card-shadow);
    transition: all 0.3s ease;
}

.analysis-card:hover {
    transform: translateY(-5px);
    box-shadow: var(--card-hover-shadow);
}

.analysis-card .card-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 1rem 1.5rem;
    font-weight: 600;
}

.analysis-card .card-title {
    color: var(--success-color);
    font-size: 1.1rem;
    font-weight: 600;
    margin-bottom: 1rem;
}

/* ============================================
   ALERT & ERROR STYLES
   ============================================ */

.alert {
    border-radius: 12px;
    padding: 1.25rem;
    border: none;
    box-shadow: var(--card-shadow);
}

.alert-danger {
    background: linear-gradient(135deg, #fee2e2 0%, #fecaca 100%);
    color: #991b1b;
}

.alert-success {
    background: linear-gradient(135deg, #d1fae5 0%, #a7f3d0 100%);
    color: #065f46;
}

.alert-info {
    background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
    color: #1e40af;
}

/* ============================================
   RESPONSIVE DESIGN
   ============================================ */

@media (max-width: 768px) {
    .container {
        padding: 1rem;
        margin-top: 1rem;
        margin-bottom: 1rem;
        border-radius: 12px;
    }
    
    .btn-group {
        flex-direction: column;
    }
    
    .filter-btn {
        width: 100%;
        margin: 0.25rem 0;
    }
    
    .policy-column {
        margin-bottom: 1.5rem;
    }
}

/* ============================================
   ANIMATION CLASSES
   ============================================ */

@keyframes fadeIn {
    from {
        opacity: 0;
        transform: translateY(20px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.fade-in {
    animation: fadeIn 0.5s ease;
}

@keyframes pulse {
    0% {
        box-shadow: 0 0 0 0 rgba(37, 99, 235, 0.4);
    }
    70% {
        box-shadow: 0 0 0 10px rgba(37, 99, 235, 0);
    }
    100% {
        box-shadow: 0 0 0 0 rgba(37, 99, 235, 0);
    }
}

.pulse {
    animation: pulse 2s infinite;
}

/* ============================================
   TOOLTIP STYLES (if using Bootstrap tooltips)
   ============================================ */

.tooltip {
    font-size: 0.875rem;
}

.tooltip-inner {
    background: var(--dark-color);
    padding: 0.5rem 0.75rem;
    border-radius: 8px;
}

/* ============================================
   PROGRESS INDICATORS
   ============================================ */

.progress {
    height: 8px;
    border-radius: 4px;
    background: #e5e7eb;
    overflow: hidden;
}

.progress-bar {
    background: linear-gradient(90deg, var(--primary-color) 0%, var(--primary-dark) 100%);
    animation: progressAnimation 2s ease;
}

@keyframes progressAnimation {
    from {
        width: 0%;
    }
}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container my-5">
        <div class="row justify-content-center">
            <div class="col-lg-8">
                <div class="card shadow-lg">
                    <div class="card-body p-5">
                        <asp:HiddenField ID="hfOriginalData" runat="server" />
                        <div class="text-center mb-4">
                            <h2 class="card-title h3">
                                <asp:Literal ID="litFormTitle" runat="server">Create Your Insurance Profile</asp:Literal>
                            </h2>
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
