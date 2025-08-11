<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="Goals.aspx.cs" Inherits="bipj.Goals" %>

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
            overflow-x: hidden;
            overflow-y: auto;
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
                <a class="nav-link" href="Jars.aspx">
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

    <div class="mainpage-content" style="height: calc(100vh - 66px); overflow: hidden;">
        <div class="px-4 py-1 mb-2 d-flex justify-content-between align-items-center">
            <h1 class="fw-bold mb-0">MY GOALS</h1>
            <div class="d-flex gap-3 mb-1 align-items-end px-4" style="margin-top: -12px;">
                <div>
                    <label for="ddlGoalFilter" class="form-label fw-semibold">Filter by Status:</label>
                    <asp:DropDownList ID="ddlGoalFilter" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlGoalFilter_SelectedIndexChanged" CssClass="form-select" Width="200px">
                        <asp:ListItem Text="All Goals" Value="all" />
                        <asp:ListItem Text="Completed" Value="completed" />
                        <asp:ListItem Text="Overdue" Value="overdue" />
                        <asp:ListItem Text="Ongoing" Value="ongoing" />
                        <asp:ListItem Text="Archived" Value="archived" />
                    </asp:DropDownList>
                </div>

                <div>
                    <label for="ddlGoalSort" class="form-label fw-semibold">Sort by:</label>
                    <asp:DropDownList ID="ddlGoalSort" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlGoalSort_SelectedIndexChanged" CssClass="form-select" Width="200px">
                        <asp:ListItem Text="Newest First" Value="created_desc" />
                        <asp:ListItem Text="Oldest First" Value="created_asc" />
                        <asp:ListItem Text="Deadline Soonest" Value="deadline_asc" />
                        <asp:ListItem Text="Deadline Latest" Value="deadline_desc" />
                    </asp:DropDownList>
                </div>
            </div>
        </div>

        <div class="row g-0" style="height: calc(100vh - 155px);">
            <!-- Cards Group -->
            <div class="col-md-4 d-flex flex-column px-4 pb-4" style="height: 100%; min-height: 0;">
                <div class="flex-fill d-flex flex-column p-3 rounded-4 shadow-sm"
                    style="background: linear-gradient(180deg, #dbeafe, #ede9fe); min-height: 0;">
                    <div class="d-flex flex-column flex-fill justify-content-around gap-3">
                        <div class="bg-white rounded-4 text-center py-2 px-2 shadow-sm flex-fill d-flex flex-column justify-content-center">
                            <h6 class="text-muted">Total Goals</h6>
                            <h4 class="fw-bold">
                                <asp:Label ID="lblTotalGoals" runat="server" /></h4>
                        </div>
                        <div class="bg-white rounded-4 text-center py-2 px-2 shadow-sm flex-fill d-flex flex-column justify-content-center">
                            <h6 class="text-muted">Total Target</h6>
                            <h4 class="fw-bold">
                                <asp:Label ID="lblTotalTarget" runat="server" /></h4>
                        </div>
                        <div class="bg-white rounded-4 text-center py-2 px-2 shadow-sm flex-fill d-flex flex-column justify-content-center">
                            <h6 class="text-muted">Total Saved</h6>
                            <h4 class="fw-bold text-success">
                                <asp:Label ID="lblTotalSaved" runat="server" /></h4>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Right: Goals List -->
            <div class="col-md-8 d-flex flex-column px-4 pb-4" style="height: 100%;">
                <div class="flex-grow-1 overflow-auto pe-2">
                    <asp:Repeater ID="rptGoals" runat="server" OnItemCommand="rptGoals_ItemCommand">
                        <ItemTemplate>
                            <div class="col-md-12 mb-4">
                                <div
                                    class="position-relative border rounded p-3 shadow-sm bg-white goal-card clickable-card"
                                    data-url='GoalDetails.aspx?goalId=<%# Eval("GoalId") %>'>

                                    <!-- Goal info -->
                                    <h5 class="fw-bold mb-1"><%# Eval("GoalName") %></h5>
                                    <p class="text-muted small mb-1">
                                        $<%# Eval("TargetAmount", "{0:N2}") %> |
                                    <%# Eval("JarName") ?? "No Jar" %>
                                    </p>
                                    <div class="d-flex align-items-center gap-2">
                                        <div class="progress flex-grow-1" style="height: 6px; max-width: 88%">
                                            <div class="progress-bar bg-primary" role="progressbar"
                                                style='width: <%# (Convert.ToDecimal(Eval("SavedAmount")) / Convert.ToDecimal(Eval("TargetAmount")) * 100).ToString("0") %>%'>
                                            </div>
                                        </div>
                                        <span class="text-muted small" style="width: 30px;">
                                            <%# (Convert.ToDecimal(Eval("SavedAmount")) / Convert.ToDecimal(Eval("TargetAmount")) * 100).ToString("0") %>%
                                        </span>
                                    </div>
                                    <%# GetGoalStatus(Eval("SavedAmount"), Eval("TargetAmount"), Eval("Deadline")) %>

                                    <!-- Edit button -->
                                    <asp:LinkButton ID="lnkEditGoal" runat="server"
                                        CssClass="btn btn-outline-primary btn-sm rounded-circle position-absolute top-0 end-0 m-2"
                                        CommandName="Edit"
                                        CommandArgument='<%# Eval("GoalId") %>'
                                        OnClientClick="event.stopPropagation();">
                                        <i class="bi bi-pencil"></i>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
        <button type="button" class="btn ms-lg-3 add-btn" data-bs-toggle="modal" data-bs-target="#addGoalModal">
            + New Goal
        </button>
    </div>

    <!-- Add New Goal Modal -->
    <div class="modal fade" id="addGoalModal" tabindex="-1" aria-labelledby="addGoalModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-3 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="addGoalModalLabel">Add New Goal</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="modal-body">
                    <asp:Panel ID="pnlAddGoal" runat="server" DefaultButton="btnAddGoal">

                        <div class="mb-3">
                            <label for="txtGoalName" class="form-label fw-semibold">Name<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtGoalName" runat="server" CssClass="form-control" placeholder="e.g. Europe Trip" />
                        </div>

                        <div class="mb-3">
                            <label for="txtGoalAmount" class="form-label fw-semibold">Target Amount<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtGoalAmount" runat="server" CssClass="form-control" placeholder="e.g. 2000" Text="0" type="number" />
                        </div>

                        <div class="mb-3">
                            <label for="txtGoalTargetDate" class="form-label fw-semibold">Target Date<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtGoalTargetDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>

                        <div class="mb-4">
                            <label for="ddlGoalJar" class="form-label fw-semibold">Assign Jar (optional)</label>
                            <asp:DropDownList ID="ddlGoalJar" runat="server" CssClass="form-select">
                                <asp:ListItem Text="-- No Jar --" Value="" />
                            </asp:DropDownList>
                        </div>

                        <asp:Button ID="btnAddGoal" runat="server" Text="Save Goal"
                            CssClass="btn btn-primary w-100 fw-semibold"
                            OnClick="btnAddGoal_Click"
                            OnClientClick="return validateGoalForm();" />
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>

    <!-- Edit Goal Modal -->
    <div class="modal fade" id="editGoalModal" tabindex="-1" aria-labelledby="editGoalModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content p-3">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="editGoalModalLabel">Edit Goal</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:Panel ID="pnlEditGoal" runat="server" DefaultButton="btnUpdateGoal">

                        <asp:HiddenField ID="hdnEditGoalId" runat="server" />
                        <div class="mb-3">
                            <label for="txtGoalName" class="form-label fw-semibold">Name<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtEditGoalName" runat="server" CssClass="form-control" placeholder="Goal Name" />
                        </div>

                        <div class="mb-3">
                            <label for="txtGoalAmount" class="form-label fw-semibold">Target Amount<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtEditGoalAmount" runat="server" CssClass="form-control" TextMode="Number" placeholder="Target Amount" />
                        </div>

                        <div class="mb-3">
                            <label for="txtGoalTargetDate" class="form-label fw-semibold">Target Date<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtEditGoalTargetDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>

                        <div class="mb-4">
                            <label for="ddlGoalJar" class="form-label fw-semibold">Assign Jar (optional)</label>
                            <asp:DropDownList ID="ddlEditJar" runat="server" CssClass="form-select">
                                <asp:ListItem Text="-- No Jar --" Value="" />
                            </asp:DropDownList>
                        </div>

                        <asp:Button ID="btnUpdateGoal" runat="server" CssClass="btn btn-primary w-100 fw-semibold mb-2"
                            Text="Save Changes" OnClick="btnUpdateGoal_Click"
                            OnClientClick="return validateEditGoalForm();" />
                    </asp:Panel>

                    <button id="btnGoalDelete" type="button" class="btn btn-danger w-100 fw-semibold" onclick="openGoalDeleteModal(
                    document.getElementById('<%= hdnEditGoalId.ClientID %>').value,
                    document.getElementById('<%= txtEditGoalName.ClientID %>').value)">
                        Delete
                    </button>

                </div>
            </div>
        </div>
    </div>

    <!-- Delete Goal Confirmation Modal -->
    <asp:HiddenField ID="hdnDeleteGoalId" runat="server" />
    <asp:HiddenField ID="hdnDeleteGoalName" runat="server" />
    <asp:HiddenField ID="hdnDeleteIsCompleted" runat="server" />
    <asp:HiddenField ID="hdnDeleteDefaultJar" runat="server" />
    <div class="modal fade" id="deleteGoalConfirmModal" tabindex="-1" aria-labelledby="deleteGoalConfirmModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-4 shadow p-4 text-center">
                <div class="modal-body">
                    <h5 class="fw-bold">Are you sure you want to delete "<span id="goalNameToDelete"></span>"?</h5>
                    <p id="goalDeleteWarning" class="text-muted mt-2"></p>
                    <div class="d-flex justify-content-center gap-3 mt-4">
                        <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnConfirmDelete" runat="server" CssClass="btn btn-danger px-4" Text="Delete" OnClick="btnConfirmDelete_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            const addGoalModal = document.getElementById('addGoalModal');

            // 🟢 Reset fields and validation when modal is opened
            addGoalModal.addEventListener('show.bs.modal', function () {
                const form = addGoalModal.querySelector('.modal-body');
                resetFormInputs(form);
                resetValidation(form);
            });

            // 🟢 Also clear validation when modal is closed
            addGoalModal.addEventListener('hidden.bs.modal', function () {
                const form = addGoalModal.querySelector('.modal-body');
                resetValidation(form);
            });
        });

        function resetFormInputs(container) {
            const inputs = container.querySelectorAll('input, select, textarea');
            inputs.forEach(input => {
                if (input.type === 'submit' || input.type === 'button') {
                    // Skip resetting button text
                    return;
                } else if (input.type === 'checkbox' || input.type === 'radio') {
                    input.checked = false;
                } else if (input.tagName === 'SELECT') {
                    input.selectedIndex = 0;
                } else {
                    input.value = '';
                }
                input.classList.remove('is-invalid');
            });
        }

        function resetValidation(container) {
            const elements = container.querySelectorAll('input, select, textarea');
            elements.forEach(el => {
                el.classList.remove("is-invalid");
                const feedbacks = el.parentNode.querySelectorAll('.invalid-feedback');
                feedbacks.forEach(fb => fb.remove());
            });
        }

        function validateGoalForm() {
            var nameInput = document.getElementById('<%= txtGoalName.ClientID %>');
            var amountInput = document.getElementById('<%= txtGoalAmount.ClientID %>');
            var dateInput = document.getElementById('<%= txtGoalTargetDate.ClientID %>');

            resetValidation(nameInput.closest(".modal-body"));

            let isValid = true;

            if (!nameInput.value.trim()) {
                showInvalid(nameInput, "Please enter a goal name.");
                isValid = false;
            }

            const amount = parseFloat(amountInput.value);
            if (isNaN(amount) || amount <= 0) {
                showInvalid(amountInput, "Please enter a valid amount greater than 0.");
                isValid = false;
            }

            if (!dateInput.value) {
                showInvalid(dateInput, "Please select a target date.");
                isValid = false;
            } else {
                const selectedDate = new Date(dateInput.value);
                const today = new Date();
                today.setHours(0, 0, 0, 0);
                if (selectedDate < today) {
                    showInvalid(dateInput, "Target date cannot be in the past.");
                    isValid = false;
                }
            }

            return isValid;
        }

        function validateEditGoalForm() {
            var nameInput = document.getElementById('<%= txtEditGoalName.ClientID %>');
            var amountInput = document.getElementById('<%= txtEditGoalAmount.ClientID %>');
            var dateInput = document.getElementById('<%= txtEditGoalTargetDate.ClientID %>');

            resetValidation(nameInput.closest(".modal-body"));

            let isValid = true;

            if (!nameInput.value.trim()) {
                showInvalid(nameInput, "Please enter a goal name.");
                isValid = false;
            }

            const amount = parseFloat(amountInput.value);
            if (isNaN(amount) || amount <= 0) {
                showInvalid(amountInput, "Please enter a valid amount greater than 0.");
                isValid = false;
            }

            if (!dateInput.value) {
                showInvalid(dateInput, "Please select a target date.");
                isValid = false;
            } else {
                const selectedDate = new Date(dateInput.value);
                const today = new Date();
                today.setHours(0, 0, 0, 0);
                if (selectedDate < today) {
                    showInvalid(dateInput, "Target date cannot be in the past.");
                    isValid = false;
                }
            }

            return isValid;
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

        function openGoalDeleteModal(goalId, goalName) {
            const editModal = bootstrap.Modal.getInstance(document.getElementById('editGoalModal'));
            if (editModal) editModal.hide();

            document.getElementById('<%= hdnDeleteGoalId.ClientID %>').value = goalId;
            document.getElementById('<%= hdnDeleteGoalName.ClientID %>').value = goalName;

            // ✅ Let showDeleteGoalModal() handle the rest
            setTimeout(() => {
                showDeleteGoalModal();
            }, 50);
        }



        function cancelGoalDelete() {
            bootstrap.Modal.getInstance(document.getElementById('deleteGoalConfirmModal')).hide();
            const editModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('editGoalModal'));
            editModal.show();
        }
        function showDeleteGoalModal() {
            const isCompleted = document.getElementById('<%= hdnDeleteIsCompleted.ClientID %>').value === "true";
            const defaultJar = document.getElementById('<%= hdnDeleteDefaultJar.ClientID %>').value;
            const goalName = document.getElementById('<%= hdnDeleteGoalName.ClientID %>').value;

            document.getElementById("goalNameToDelete").innerText = goalName;

            let warningText = "";
            if (isCompleted) {
                warningText = `This action cannot be undone.<br>
     This goal is already <strong>completed</strong>.<br>
     Deleting it will <strong>not refund</strong> any saved amount.`;
            } else {
                warningText = `This action cannot be undone.<br>
     Current money in this goal will be <strong>moved to your default jar:</strong> ${defaultJar}.`;
            }

            document.getElementById("goalDeleteWarning").innerHTML = warningText;

            const modal = new bootstrap.Modal(document.getElementById('deleteGoalConfirmModal'));
            modal.show();
        }
        document.addEventListener("DOMContentLoaded", function () {
            document.querySelectorAll(".clickable-card").forEach(function (card) {
                card.addEventListener("click", function () {
                    var url = card.getAttribute("data-url");
                    if (url) window.location.href = url;
                });
            });
        });


    </script>
</asp:Content>
