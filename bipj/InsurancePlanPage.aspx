<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InsurancePlanPage.aspx.cs" Inherits="bipj.InsurancePlanPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
/* ============================================
   GLOBAL STYLES & ANIMATIONS (Updated)
   ============================================ */

/* ✅ UPDATED: Copied variables from InvestmentPage.aspx for consistency */
:root {
    --primary-gradient: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    --success-gradient: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
    --danger-gradient: linear-gradient(135deg, #eb3349 0%, #f45c43 100%);
    --info-gradient: linear-gradient(135deg, #2193b0 0%, #6dd5ed 100%);
    
    --primary-color: #667eea;
    --success-color: #38ef7d;
    --danger-color: #f45c43;
    --info-color: #6dd5ed;
    --dark-color: #1f2937;
    --light-bg: #f8fafc;
    
    /* New shadow variables for a softer look */
    --shadow-sm: 0 2px 10px rgba(0,0,0,0.08);
    --shadow-md: 0 4px 20px rgba(0,0,0,0.1);
    --shadow-lg: 0 10px 40px rgba(0,0,0,0.15);

    /* New transition variable */
    --transition-fast: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

* {
    transition: all 0.3s ease;
}

/* ✅ UPDATED: Using the light background from InvestmentPage.aspx */
body {
    background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
    min-height: 100vh;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

/* ✅ MODIFIED: Removed background, padding, and shadow to eliminate the surrounding whitespace. */
.container {
    background: transparent;
    border-radius: 0;
    padding: 0;
    margin-top: 2rem;
    margin-bottom: 2rem;
    box-shadow: none;
}

/* ============================================
   CARD STYLES (New - Replicates InvestmentPage)
   ============================================ */

.card {
    background: white;
    border-radius: 1rem; /* Consistent 1rem radius */
    overflow: hidden; /* Important for the border effect */
    position: relative;
    box-shadow: var(--shadow-md); 
    transition: var(--transition-fast);
}

/* This is the magic for the animated top border */
.card::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 5px;
    background: var(--primary-gradient);
    transform: scaleX(0); /* Hidden by default */
    transform-origin: left; /* Animation starts from the left */
    transition: transform var(--transition-fast);
}

.card:hover {
    transform: translateY(-5px); /* Lifts the card up */
    box-shadow: var(--shadow-lg); /* Applies a stronger shadow */
}

/* The animated border appears on hover */
.card:hover::before {
    transform: scaleX(1); /* Scales to full width */
}

/* ✅ REMOVED: The old .card-header style is no longer needed for this design. */

.card-title {
    color: var(--dark-color);
    font-size: 1.25rem;
    font-weight: 600;
    margin-bottom: 0.5rem;
}

/* ============================================
   BUTTON STYLES (Updated)
   ============================================ */



/* ✅ REMOVED: Ripple effect is disabled for a cleaner look. */
.btn::before {
    display: none;
}

.btn:hover {
    transform: translateY(-2px);
    box-shadow: var(--shadow-md); /* Adds a subtle shadow on hover */
}

.btn-primary {
    background: var(--primary-gradient);
}

/* ✅ CHANGED: All action buttons now use the same brand color */
.btn-success,
.btn-info,
.btn-danger,
.btn-secondary {
    background: var(--brand-purple);
}

/* Keeping other styles like alerts and responsive design as they are compatible */

/* ============================================
   ALERT & RESPONSIVE STYLES (Unchanged)
   ============================================ */

.alert {
    border-radius: 12px;
    padding: 1.25rem;
    border: none;
    box-shadow: var(--shadow-sm); /* Using new shadow variable */
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
}
</style>
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
                    <div class="card h-100">
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
