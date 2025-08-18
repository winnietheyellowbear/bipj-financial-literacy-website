<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InsuranceComparisonPage.aspx.cs" Inherits="bipj.InsuranceComparisonPage" Async="true" %>
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
    <div class="container mt-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h1 class="h3 mb-0 my-4">
                Policy Analysis
            </h1>
            <asp:Button ID="btnBackToDashboard" runat="server" Text="&larr; Back to Dashboard" OnClick="btnBackToDashboard_Click"
                CssClass="btn btn-outline-secondary my-4" />
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
