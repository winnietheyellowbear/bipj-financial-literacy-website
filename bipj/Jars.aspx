<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="Jars.aspx.cs" Inherits="bipj.Jars" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css" rel="stylesheet" />

    <style>
        .sidebar {
            width: 220px;
            padding: 2rem 1.5rem;
            background-color: #212529;
            height: calc(100vh - 66px);
            position: fixed;
        }

            .sidebar h5 {
                color: white;
                font-weight: 700;
                font-size: 1.25rem;
                margin-top: 1rem;
                margin-bottom: 3rem;
            }

            .sidebar .nav-link {
                font-weight: 500;
                font-size: 1.1rem;
                padding: 0.75rem 1rem;
                display: flex;
                align-items: center;
                gap: 0.75rem;
                color: white !important;
                opacity: 0.9;
                transition: color 0.3s ease, background-color 0.3s ease;
                border-radius: 4px;
                text-decoration: none;
            }

                .sidebar .nav-link:hover {
                    color: #a9bfff !important;
                    background-color: rgba(169, 191, 255, 0.15);
                }

                .sidebar .nav-link img {
                    height: 25px;
                    width: 25px;
                    object-fit: contain;
                }

        .add-btn {
            background-color: #5e4bd3;
            color: white;
            position: fixed;
            bottom: 30px;
            right: 40px;
            background-color: #5e4bd3;
            border: none;
            border-radius: 5px;
            padding: 6px 12px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 999;
        }

            .add-btn:hover {
                background-color: #4e3cc7;
            }

        .mainpage-content {
            margin-left: 220px;
            max-width: calc(100% - 220px);
            padding: 1.5rem;
        }

        .info-icon-img {
            width: 12px;
            height: 12px;
            vertical-align: middle;
            position: relative;
            top: -17px;
        }

        .settings-btn {
            background: none;
            border: none;
            font-size: 1.5rem;
        }

        .btn-outline-primary.btn-sm.rounded-circle {
            width: 32px;
            height: 32px;
            padding: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            /* Purple border and text */
            border-color: #5e4bd3;
            color: #5e4bd3;
            background-color: transparent;
            transition: background-color 0.2s, color 0.2s;
            z-index: 2;
        }

            .btn-outline-primary.btn-sm.rounded-circle:hover,
            .btn-outline-primary.btn-sm.rounded-circle:focus,
            .btn-outline-primary.btn-sm.rounded-circle:active {
                background-color: #5e4bd3; /* purple fill on hover */
                color: white;
                border-color: #5e4bd3;
            }

        .clickable-card {
            cursor: pointer;
            transition: box-shadow 0.2s ease, transform 0.2s ease;
        }

            .clickable-card:hover {
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                transform: translateY(-2px);
                background-color: #f8f9fa;
            }
    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <!-- Sidebar -->
    <div class="sidebar bg-dark p-4">
        <h5 class="fw-bold">Budgeting Menu</h5>
        <ul class="nav flex-column mt-4">
            <li class="nav-item">
                <a class="nav-link" href="Dashboard.aspx">
                    <img src="images/dashboard.png" alt="Dashboard" style="width: 20px; height: 20px; margin: 2.5px" />
                    Dashboard
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link active" href="Jars.aspx">
                    <img src="images/jar.png" alt="Jars" />
                    Jars
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" href="Goals.aspx">
                    <img src="images/goal.png" alt="Goals" />
                    Goals
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" href="Tools.aspx">
                    <img src="images/tool.png" alt="Tools" />
                    Tools
                </a>
            </li>
        </ul>
    </div>

    <!-- Main Content -->
    <div class="container-fluid mainpage-content d-flex flex-column" style="height: calc(100vh - 66px); overflow: hidden;">
        <!-- Header Row -->
        <div class="d-flex justify-content-between align-items-center px-4 py-1 mb-2">
            <h1 class="fw-bold mb-0">MY JARS
        <a href="#" data-bs-toggle="modal" data-bs-target="#info-popup" title="Learn more about Jars">
            <img src="images/info-icon.png" alt="Info" class="info-icon-img" />
        </a>
            </h1>
            <button type="button" class="settings-btn" data-bs-toggle="modal" data-bs-target="#settingsModal">
                <i class="bi bi-gear"></i>
            </button>
        </div>

        <!-- Content Row (Jars + Chart) -->
        <div class="row g-0" style="height: calc(100vh - 155px);">
            <!-- Jars List Repeater -->
            <div class="col-md-6 d-flex flex-column pb-4" style="height: 100%;">
                <div class="flex-grow-1 overflow-auto px-4 pb-4">
                    <asp:Repeater ID="rptJars" runat="server" OnItemCommand="rptJars_ItemCommand">
                        <ItemTemplate>
                            <div class="jar-row d-flex align-items-center justify-content-between mb-3 p-3 border rounded shadow-sm bg-white clickable-card"
                                data-url='JarDetails.aspx?JarId=<%# Eval("JarId") %>'
                                data-jar-id='<%# Eval("JarId") %>'
                                data-is-default='<%# Eval("IsDefault").ToString().ToLower() %>'>
                                <div class="d-flex align-items-center gap-3">
                                    <i class="bi bi-wallet2 fs-3"></i>
                                    <span class="fw-semibold fs-5"><%# Eval("JarName") %></span>
                                </div>
                                <div class="d-flex align-items-center gap-3">
                                    <span class='<%# ((decimal)Eval("Balance") >= 0 
                                          ? "fw-bold fs-5 text-success" 
                                          : "fw-bold fs-5 text-danger") %>'>$<%# ((decimal)Eval("Balance") % 1 == 0) 
                                    ? ((decimal)Eval("Balance")).ToString("F0") 
                                    : ((decimal)Eval("Balance")).ToString("F2") %>
                                    </span>
                                    <asp:LinkButton runat="server"
                                        CommandName="Edit"
                                        CommandArgument='<%# Eval("JarId") %>'
                                        CssClass="btn btn-outline-primary btn-sm rounded-circle"
                                        title="Edit Jar"
                                        OnClientClick="event.stopPropagation();">
                                    <i class="bi bi-pencil"></i>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>

            <!-- Pie Chart + Total -->
            <div class="col-md-6 d-flex flex-column px-4 pb-4" style="height: 100%;">
                <div class="bg-white rounded shadow-sm p-4 mb-3 d-flex flex-column justify-content-between" style="height: 100%;">
                    <h4 class="fw-bold mb-3">Jar Breakdown</h4>
                    <div class="d-flex flex-column flex-grow-1 align-items-center">
                        <div class="flex-grow-1 w-100" style="min-height: 200px; max-height: 45vh;">
                            <canvas id="jarChart" class="w-100 h-100" style="object-fit: contain;"></canvas>
                        </div>
                        <div class="text-center fw-bold fs-5 pt-3">
                            Total:
                    <asp:Label ID="lblTotalAmount" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Settings Modal -->
    <div class="modal fade" id="settingsModal" tabindex="-1" aria-labelledby="settingsModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-3 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="settingsModalLabel">Jar Settings</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="modal-body">
                    <asp:Panel ID="pnlSettings" runat="server" DefaultButton="btnSaveSettings">
                        <!-- Default Jar Dropdown -->
                        <div class="mb-4">
                            <label for="ddlDefaultJar" class="form-label fw-semibold">Default Jar:</label>
                            <asp:DropDownList ID="ddlDefaultJar" runat="server" CssClass="form-select"></asp:DropDownList>
                        </div>

                        <!-- Jar Percentage Inputs -->
                        <div class="row">
                            <asp:Repeater ID="rptSettings" runat="server">
                                <ItemTemplate>
                                    <div class="col-md-6 mb-3">
                                        <label class="fw-semibold"><%# Eval("JarName") %>:</label>
                                        <asp:HiddenField ID="hiddenJarId" runat="server" Value='<%# Eval("JarId") %>' />
                                        <asp:TextBox ID="percentInput" runat="server"
                                            Text='<%# Eval("Percentage") %>' CssClass="form-control" />
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                        <small class="text-muted d-block text-center mt-3">Note: Total must add up to 100%!</small>

                        <!-- Save Button -->
                        <asp:Button ID="btnSaveSettings" runat="server" Text="Save"
                            CssClass="btn btn-primary w-100 mt-3 fw-semibold" OnClick="btnSaveSettings_Click" />
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>

    <!-- Percentage Error Modal -->
    <div class="modal fade" id="percentErrorModal" tabindex="-1" aria-labelledby="percentErrorModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-3">
                <div class="modal-header">
                    <h5 class="modal-title text-danger" id="percentErrorModalLabel">Invalid Allocation</h5>
                </div>
                <div class="modal-body">
                    Total jar percentages must add up to exactly <strong>100%</strong>.
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary fw-semibold" data-bs-dismiss="modal" onclick="reopenSettingsModal()">OK</button>
                </div>
            </div>
        </div>
    </div>


    <!-- Add Jar Button -->
    <button type="button" class="btn ms-lg-3 add-btn" data-bs-toggle="modal" data-bs-target="#addModal">
        + New Jar
    </button>

    <!-- Add New Jar Modal -->
    <div class="modal fade" id="addModal" tabindex="-1" aria-labelledby="addModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-3 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="addModalLabel">Add New Jar</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:Panel ID="pnlAddJar" runat="server" DefaultButton="btnAddJar">
                        <div class="mb-3">
                            <label for="txtNewJarName" class="form-label fw-semibold">Name<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtNewJarName" runat="server" CssClass="form-control" placeholder="e.g. Emergency Fund" />
                        </div>
                        <div class="mb-3">
                            <label for="txtNewJarDesc" class="form-label fw-semibold">Description</label>
                            <asp:TextBox ID="txtNewJarDesc" runat="server" CssClass="form-control" placeholder="e.g. to use for emergencies like hospitalisation" TextMode="MultiLine" Rows="3" />
                        </div>
                        <small class="text-muted d-block mb-3">Note: Jar allocation is set to 0%. Adjust percentages in 
                    <i class="bi bi-gear"></i>on the jars page to split income automatically.
                        </small>

                        <asp:Button ID="btnAddJar" runat="server" Text="Save"
                            CssClass="btn btn-primary w-100 fw-semibold" OnClick="btnAddJar_Click" OnClientClick="return validateAddJarForm();" />
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>


    <!-- Edit Jar Modal -->
    <div class="modal fade" id="editModal" tabindex="-1" aria-labelledby="editModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-3 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="editModalLabel">Edit Jar</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="modal-body">
                    <asp:Panel ID="pnlEditJar" runat="server" DefaultButton="btnUpdateJar">
                        <asp:HiddenField ID="hiddenEditJarId" runat="server" />

                        <div class="mb-3">
                            <label for="txtEditName" class="form-label fw-semibold">Name<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtEditName" runat="server" CssClass="form-control" placeholder="Jar Name" />
                        </div>
                        <div class="mb-3">
                            <label for="txtEditDesc" class="form-label fw-semibold">Description</label>
                            <asp:TextBox ID="txtEditDesc" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Jar Description" />
                        </div>
                        <div class="mb-4">
                            <label for="txtEditPercent" class="form-label fw-semibold">Percentage</label>
                            <asp:TextBox ID="txtEditPercent" runat="server" CssClass="form-control" placeholder="%" ReadOnly="true" Enabled="false" />
                        </div>
                        <!-- Save -->
                        <asp:Button ID="btnUpdateJar" runat="server" Text="Save"
                            CssClass="btn btn-primary w-100 fw-semibold mb-2" OnClick="btnUpdateJar_Click" OnClientClick="return validateEditJarForm();" />
                    </asp:Panel>

                    <!-- Delete -->
                    <button type="button" class="btn btn-danger w-100 fw-semibold"
                        onclick="openDeleteModal('<%= hiddenEditJarId.Value %>', document.getElementById('<%= txtEditName.ClientID %>').value)">
                        Delete
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- Hidden field to store JarId for deletion -->
    <asp:HiddenField ID="hiddenDeleteJarId" runat="server" />

    <!-- Delete Confirmation Modal -->
    <div class="modal fade" id="deleteConfirmModal" tabindex="-1" aria-labelledby="deleteConfirmLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-4 shadow p-4 text-center">
                <div class="modal-body">
                    <h5 class="fw-bold" id="deleteConfirmLabel">Are you sure you want to delete "<span id="jarNameToDelete"></span>"?
                    </h5>
                    <p class="text-muted mt-2">
                        The leftover money will be transferred to "<asp:Literal ID="litDefaultJarName" runat="server" />"
                    </p>

                    <div class="d-flex justify-content-center gap-3 mt-4">
                        <button type="button" class="btn btn-danger px-4" onclick="cancelDeleteAndReturnToEdit()">Cancel</button>
                        <asp:Button ID="btnConfirmDelete" runat="server" Text="Confirm" CssClass="btn btn-primary px-4" OnClick="btnConfirmDelete_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal: Cannot Delete Default Jar -->
    <div class="modal fade" id="defaultJarWarningModal" tabindex="-1" aria-labelledby="defaultJarWarningLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-4 shadow text-center p-4">
                <div class="modal-body">
                    <h5 class="fw-bold" id="defaultJarWarningLabel">Cannot delete default jar</h5>
                    <p class="text-muted mt-2">This is your default jar. Please change the default jar before deleting it.</p>
                    <div class="d-flex justify-content-center mt-4">
                        <button type="button" class="btn btn-primary px-4" data-bs-dismiss="modal">OK</button>
                    </div>
                </div>
            </div>
        </div>
    </div>


    <!-- Info Popup -->
    <div class="modal fade" id="info-popup" tabindex="-1" role="dialog" aria-labelledby="info-popup-title" aria-modal="true" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg" role="document">
            <div class="modal-content rounded-3 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="info-popup-title">What are the 6 Jars?</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p>
                        The 6 Jars system was created by T. Harv Eker as part of his financial freedom strategy.<br />
                        It helps people manage money by assigning a fixed percentage of every dollar they earn different life purposes.
                    </p>

                    <h6>How to Use It</h6>
                    <ul>
                        <li><strong>NEC (Necessities – 55%)</strong>: Daily living expenses</li>
                        <li><strong>FFA (Financial Freedom – 10%)</strong>: Savings/Investments</li>
                        <li><strong>PLAY (Play – 10%)</strong>: Fun/spending guilt-free</li>
                        <li><strong>LTSS (Long Term Savings – 10%)</strong>: Big purchases like house/car</li>
                        <li><strong>EDU (Education – 10%)</strong>: Courses, books, learning</li>
                        <li><strong>GIVE (Give – 5%)</strong>: Donations, helping others</li>
                    </ul>

                    <p>💡 The idea is to split your income as soon as you receive it – consistently and automatically.</p>

                    <p>
                        Learn more:
  <a href="https://harvekeronline.com/6-jars-exercise/" target="_blank" rel="noopener noreferrer">https://harvekeronline.com/6-jars-exercise/
  </a>
                    </p>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <!-- Scripts -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chartjs-plugin-datalabels@2.2.0"></script>
    <script src="https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js"></script>

    <script type="text/javascript">
        // PieChart
        let lastJarId = null;
        let lastJarName = null;
        let currentPieChart = null;

        function toggleSettings() {
            const modal = document.getElementById("settingsModal");
            modal.style.display = modal.style.display === "flex" ? "none" : "flex";
        }

        function renderPieChart() {
            const ctx = document.getElementById('jarChart').getContext('2d');
            const data = window.chartData;

            if (!ctx || !data || !data.labels.length || !data.amounts.length) return;

            // Remove previous chart
            if (currentPieChart) currentPieChart.destroy();

            // Filter only data with amount > 0 (defensive check in case backend fails to filter)
            const filtered = data.labels.map((label, i) => ({
                label,
                value: data.amounts[i],
                color: data.colors[i]
            })).filter(j => j.value > 0);

            if (filtered.length === 0) return;

            currentPieChart = new Chart(ctx, {
                type: 'pie',
                data: {
                    labels: filtered.map(j => j.label),
                    datasets: [{
                        data: filtered.map(j => j.value),
                        backgroundColor: filtered.map(j => j.color),
                        borderWidth: 1,
                        borderColor: '#fff'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'bottom',
                            labels: {
                                color: '#000',
                                usePointStyle: true,
                                padding: 15,
                                font: {
                                    size: 14
                                }
                            }
                        },
                        datalabels: {
                            color: '#fff',
                            font: { weight: 'bold', size: 14 },
                            formatter: (value, context) => {
                                const total = context.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                                const percentage = (value / total) * 100;
                                return percentage < 1 ? '' : percentage.toFixed(1) + '%';
                            }
                        }
                    }
                },
                plugins: [ChartDataLabels]
            });
        }

        function validateAddJarForm() {
            const nameInput = document.getElementById('<%= txtNewJarName.ClientID %>');
            let isValid = true;

            resetValidation(nameInput.closest("form") || document);

            if (nameInput.value.trim() === "") {
                showInvalid(nameInput, "Please enter a jar name.");
                isValid = false;
            }

            const amount = parseFloat(amountInput.value);
            if (isNaN(amount) || amount < 0) {
                showInvalid(amountInput, "Please enter a valid starting amount (0 or more).");
                isValid = false;
            }

            return isValid;
        }

        function validateEditJarForm() {
            const nameInput = document.getElementById('<%= txtEditName.ClientID %>');
            let isValid = true;

            resetValidation(nameInput.closest("form") || document);

            if (nameInput.value.trim() === "") {
                showInvalid(nameInput, "Jar name cannot be empty.");
                isValid = false;
            }

            const amount = parseFloat(amountInput.value);
            if (isNaN(amount) || amount < 0) {
                showInvalid(amountInput, "Amount must be a valid number (0 or more).");
                isValid = false;
            }

            return isValid;
        }

        function resetValidation(container) {
            const elements = container.querySelectorAll('input, select');
            elements.forEach(el => {
                el.classList.remove("is-invalid");
                const feedback = el.parentNode.querySelector('.invalid-feedback');
                if (feedback) feedback.remove();
            });
        }

        function showInvalid(input, message) {
            input.classList.add("is-invalid");

            const errorDiv = document.createElement("div");
            errorDiv.className = "invalid-feedback";
            errorDiv.textContent = message;

            if (input.tagName === "SELECT") {
                input.parentNode.appendChild(errorDiv);
            } else {
                input.parentNode.insertBefore(errorDiv, input.nextSibling);
            }
        }

        function observeModalClose() {
            const modal = document.getElementById('editModal');
            if (!modal) return;

            const observer = new MutationObserver(() => {
                const isHidden = !modal.classList.contains('show');
                if (isHidden && window.chartData) {
                    renderPieChart();
                }
            });

            observer.observe(modal, { attributes: true, attributeFilter: ['class'] });
        }

        document.addEventListener("DOMContentLoaded", function () {
            const jarList = document.querySelector(".flex-grow-1.overflow-y-auto"); // container holding jar divs

            if (jarList) {
                new Sortable(jarList, {
                    animation: 150,
                    handle: ".jar-row", // makes entire jar row draggable
                    onEnd: function (evt) {
                        const jarOrder = [...jarList.children]
                            .filter(el => el.classList.contains("jar-row"))
                            .map(el => el.getAttribute("data-jar-id"));

                        fetch("Jars.aspx/ReorderJars", {
                            method: "POST",
                            headers: {
                                "Content-Type": "application/json"
                            },
                            body: JSON.stringify({ jarIdOrder: jarOrder.map(Number) })
                        });
                    }
                });
            }
            observeModalClose();
            document.querySelectorAll(".clickable-card").forEach(function (card) {
                card.addEventListener("click", function () {
                    const url = card.getAttribute("data-url");
                    if (url) {
                        window.location.href = url;
                    }
                });
            });
        });


        if (typeof Sys !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                observeModalClose();
                renderPieChart();
            });
        }

        function closeAddModal() {
            var modalEl = document.getElementById('addModal');
            var modal = bootstrap.Modal.getInstance(modalEl);
            if (modal) modal.hide();
        }

        function closeEditModal() {
            var modalEl = document.getElementById('editModal');
            var modal = bootstrap.Modal.getInstance(modalEl);
            if (modal) modal.hide();
        }

        function openDeleteModal(jarId, jarName) {
            lastJarId = jarId;
            lastJarName = jarName;

            const editModalEl = document.getElementById('editModal');
            const editModal = bootstrap.Modal.getInstance(editModalEl);
            if (editModal) editModal.hide();

            setTimeout(() => {
                const targetRow = document.querySelector(`.jar-row[data-jar-id='${jarId}']`);
                if (!targetRow) {
                    alert("Unexpected error: jar row not found.");
                    return;
                }

                const isDefault = targetRow.getAttribute("data-is-default") === "true";
                if (isDefault) {
                    const warningModal = new bootstrap.Modal(document.getElementById('defaultJarWarningModal'));
                    warningModal.show();
                    return;
                }

                document.getElementById('jarNameToDelete').textContent = jarName;
                document.getElementById('<%= hiddenDeleteJarId.ClientID %>').value = jarId;

                const deleteModal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'));
                deleteModal.show();
            }, 300);
        }

        function cancelDeleteAndReturnToEdit() {
            const deleteModalEl = document.getElementById('deleteConfirmModal');
            const deleteModal = bootstrap.Modal.getInstance(deleteModalEl);
            if (deleteModal) deleteModal.hide();

            setTimeout(() => {
                const editModalEl = document.getElementById('editModal');
                const editModal = new bootstrap.Modal(editModalEl);
                editModal.show();
            }, 300);
        }

        function reopenSettingsModal() {
            const settingsModalEl = document.getElementById('settingsModal');
            const modal = new bootstrap.Modal(settingsModalEl);
            modal.show();
        }
    </script>
    <asp:Literal ID="litChartData" runat="server" />
</asp:Content>
