<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InsuranceDashboardPage.aspx.cs" Inherits="bipj.InsuranceDashboardPage" Async="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        .policy-column {
            display: flex;
            flex-direction: column;
        }
        /* ✅ This ensures the card itself will stretch to the full height of the column */
        .policy-card {
            flex-grow: 1;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        

        <asp:Panel ID="pnlLoading" runat="server" CssClass="text-center py-5" Visible="false">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="text-muted mt-2">Generating your personalized recommendations... Please wait.</p>
        </asp:Panel>

        <asp:Panel ID="pnlResults" runat="server" Visible="true">
            <div class="row g-4">
                <!-- Section for the Pie Chart and Budget -->
                <div class="col-lg-5">
                    <div class="card h-100 shadow-sm my-4">
                        <div class="card-body">
                            <h5 class="card-title">Recommended Budget Allocation</h5>
                            <hr />
                            <canvas id="budgetPieChart"></canvas>
                            <div class="mt-3 text-center">
                                <asp:Literal ID="litBudgetNumbers" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Section for the Recommendation Cards -->
                <div class="col-lg-7">
                    <h5 class="mb-3 my-4">Recommended Insurance Strategy</h5>
                    <asp:Repeater ID="rptRecommendations" runat="server">
                        <ItemTemplate>
                            <div class="card shadow-sm mb-3">
                                <div class="card-body">
                                    <h5 class="card-title"><%# Eval("Type") %></h5>
                                    <h6 class="card-subtitle mb-2 text-muted"><%# Eval("Coverage") %></h6>
                                    <p class="card-text"><%# Eval("Explanation") %></p>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
             <hr class="my-4"/>
            <!-- Section for Policy Comparison -->
            <div class="card shadow-sm">
                 <div class="card-body">
                    <h5 class="card-title">Recommended Policies</h5>
                     <hr />
                     <%-- ✅ NEW: Filter Buttons --%>
                     <div class="text-center mb-4">
                         <div class="btn-group flex-wrap" role="group" aria-label="Insurance Type Filter">
                             <button type="button" class="btn btn-primary filter-btn active" data-filter="all">All</button>
                             <asp:Repeater ID="rptFilterButtons" runat="server">
                                 <ItemTemplate>
                                     <button type="button" class="btn btn-outline-primary filter-btn" data-filter="<%# Container.DataItem %>"><%# Container.DataItem %></button>
                                 </ItemTemplate>
                             </asp:Repeater>
                         </div>
                     </div>

                     <div class="row row-cols-1 row-cols-md-3 row-cols-lg-5 g-4">
                         <asp:Repeater ID="rptPolicyCategories" runat="server">
                             <ItemTemplate>
                                 <%-- ✅ NEW: Added data-category attribute --%>
                                 <div class="col policy-column" data-category="<%# Eval("InsuranceType") %>">
                                     <h6 class="text-center fw-bold mb-3"><%# Eval("InsuranceType") %></h6>
                                     <asp:Repeater ID="rptPolicies" runat="server" DataSource='<%# Eval("RecommendedPolicies") %>'>
                                         <ItemTemplate>
                                             <div class="card shadow-sm mb-3 policy-card d-flex flex-column">
                                                 <div class="card-body flex-grow-1">
                                                     <h6 class="card-title small fw-bold"><%# Eval("PolicyName") %></h6>
                                                     <p class="card-subtitle mb-2 text-muted small"><%# Eval("Provider") %></p>
                                                     <p class="card-text small"><%# Eval("Details") %></p>
                                                 </div>
                                             </div>
                                         </ItemTemplate>
                                     </asp:Repeater>
                                 </div>
                             </ItemTemplate>
                         </asp:Repeater>
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
        <div class="d-flex justify-content-between align-items-center my-4">
            <asp:Button ID="btnBackToPlans" runat="server" Text="&larr; View All Plans" OnClick="btnBackToPlans_Click"
                    CssClass="btn btn-outline-secondary" />
            <asp:Button ID="btnViewComparison" runat="server" Text="View Policy Analysis" OnClick="btnViewComparison_Click"
                    CssClass="btn btn-info" />
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script type="text/javascript">
        // This function will be called from the C# code-behind to draw the chart
        function drawBudgetChart(chartData) {
            const ctx = document.getElementById('budgetPieChart');
            if (!ctx) return;

            new Chart(ctx, {
                type: 'pie',
                data: {
                    labels: chartData.map(d => d.label),
                    datasets: [{
                        label: 'Budget %',
                        data: chartData.map(d => d.value),
                        backgroundColor: [
                            'rgba(255, 99, 132, 0.7)',
                            'rgba(54, 162, 235, 0.7)',
                            'rgba(255, 206, 86, 0.7)',
                            'rgba(75, 192, 192, 0.7)',
                            'rgba(153, 102, 255, 0.7)',
                            'rgba(255, 159, 64, 0.7)'
                        ],
                        borderColor: [
                            'rgba(255, 99, 132, 1)',
                            'rgba(54, 162, 235, 1)',
                            'rgba(255, 206, 86, 1)',
                            'rgba(75, 192, 192, 1)',
                            'rgba(153, 102, 255, 1)',
                            'rgba(255, 159, 64, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'top',
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    let label = context.label || '';
                                    if (label) {
                                        label += ': ';
                                    }
                                    if (context.parsed !== null) {
                                        label += context.parsed + '%';
                                    }
                                    return label;
                                }
                            }
                        }
                    }
                }
            });
        }

        document.addEventListener("DOMContentLoaded", function () {
            const filterButtons = document.querySelectorAll('.filter-btn');
            const policyColumns = document.querySelectorAll('.policy-column');

            filterButtons.forEach(button => {
                button.addEventListener('click', function () {
                    // Handle active button styling
                    filterButtons.forEach(btn => btn.classList.remove('active', 'btn-primary'));
                    filterButtons.forEach(btn => btn.classList.add('btn-outline-primary'));

                    this.classList.add('active', 'btn-primary');
                    this.classList.remove('btn-outline-primary');

                    const filter = this.getAttribute('data-filter');

                    policyColumns.forEach(column => {
                        if (filter === 'all' || column.getAttribute('data-category') === filter) {
                            column.style.visibility = 'visible'; // Use flex to maintain column layout
                        } else {
                            column.style.visibility = 'hidden';
                        }
                    });
                });
            });
        });
    </script>
</asp:Content>
