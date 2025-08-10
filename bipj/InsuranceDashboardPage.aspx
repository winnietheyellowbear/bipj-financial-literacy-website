<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="InsuranceDashboardPage.aspx.cs" Inherits="bipj.InsuranceDashboardPage" Async="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
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
                <!-- Section for the Pie Chart and Budget -->
                <div class="col-lg-5">
                    <div class="card h-100 shadow-sm">
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
                    <h5 class="mb-3">AI Recommended Insurance Strategy</h5>
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
                    <h5 class="card-title">AI Recommended Policies & Comparison</h5>
                     <hr />
                    <asp:Literal ID="litPolicyComparison" runat="server"></asp:Literal>
                     <div class="mt-3 text-end">
                         <asp:Button ID="btnViewComparison" runat="server" Text="View Full Comparison &rarr;" OnClick="btnViewComparison_Click"
                             CssClass="btn btn-success" />
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
    </script>
</asp:Content>
