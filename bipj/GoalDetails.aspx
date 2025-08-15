<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="GoalDetails.aspx.cs" Inherits="bipj.GoalDetails" %>

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

        .fixed-action-buttons {
            position: fixed;
            bottom: 30px;
            right: 40px;
            z-index: 999;
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

        .custom-dropdown {
            position: relative;
            user-select: none;
            min-width: 140px;
        }

            .custom-dropdown .selected {
                position: relative;
                padding: 6px 36px 6px 12px;
                display: flex;
                align-items: center;
                gap: 8px;
                background-color: white;
                border: 1px solid #ced4da;
                border-radius: 0.375rem;
                cursor: pointer;
            }

            .custom-dropdown .dropdown-arrow {
                position: absolute;
                right: 12px;
                top: 50%;
                transform: translateY(-50%);
                font-size: 12px;
                color: #666;
                pointer-events: none;
            }

            .custom-dropdown .options {
                position: absolute;
                top: calc(100% + 2px);
                left: 0;
                right: 0;
                border: 1px solid #ced4da;
                background-color: white;
                border-radius: 0 0 0.375rem 0.375rem;
                max-height: 200px;
                overflow-y: auto;
                box-shadow: 0px 4px 8px rgba(0,0,0,0.1);
                z-index: 1000;
            }

            .custom-dropdown .option {
                padding: 6px 12px;
                display: flex;
                align-items: center;
                gap: 8px;
                cursor: pointer;
            }

                .custom-dropdown .option:hover {
                    background-color: #f8f9fa;
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

        .disabled-transaction {
            opacity: 0.6;
            cursor: not-allowed;
            pointer-events: none;
        }

        .form-check-input {
            box-shadow: none !important;
            border: none;
            position: relative;
            top: -2px;
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

    <!-- Main Content -->
    <div class="container-fluid mainpage-content d-flex flex-column" style="height: calc(100vh - 66px); overflow: hidden;">
        <!-- Header Row -->
        <div class="d-flex justify-content-between align-items-center px-4 py-1 mb-2">
            <a href="Goals.aspx" class="text-dark text-decoration-none">
                <h1 class="fw-bold mb-0"><i class="bi bi-arrow-left me-2"></i>MY GOALS</h1>

            </a>
            <div class="d-flex align-items-center gap-3">
                <div class="custom-dropdown" id="customDropdown">
                    <asp:LinkButton ID="btnPeriodChange" runat="server" OnClick="btnPeriodChange_Click" />
                    <div class="selected" id="selectedOption">
                        <asp:Literal ID="litPeriodIcon" runat="server" />
                        <span>
                            <asp:Literal ID="litPeriodLabel" runat="server" /></span>
                        <span class="dropdown-arrow"><i class="bi bi-caret-down-fill"></i></span>
                    </div>
                    <div class="options" style="display: none;">
                        <div class="option" data-value="day">
                            <img src="images/calendar/calendar-day.png" width="20" height="20" />
                            <span>Day</span>
                        </div>
                        <div class="option" data-value="week">
                            <img src="images/calendar/calendar-week.png" width="20" height="20" />
                            <span>Week</span>
                        </div>
                        <div class="option" data-value="month">
                            <img src="images/calendar/calendar-month.png" width="20" height="20" />
                            <span>Month</span>
                        </div>
                        <div class="option" data-value="year">
                            <img src="images/calendar/calendar-year.png" width="20" height="20" />
                            <span>Year</span>
                        </div>
                        <div class="option" data-value="all">
                            <span style="font-size: 1.1rem;">∞ All Time</span>
                        </div>
                    </div>
                </div>

                <asp:HiddenField ID="hdnSelectedPeriod" runat="server" />
                <asp:HiddenField ID="hdnSelectedDate" runat="server" />

                <!-- Input fields -->
                <input type="date" id="inputDay" class="form-control" style="min-width: 160px;" onchange="handleDateChange(this)" />
                <input type="week" id="inputWeek" class="form-control" style="min-width: 160px; display: none;" onchange="handleDateChange(this)" />
                <input type="month" id="inputMonth" class="form-control" style="min-width: 160px; display: none;" onchange="handleDateChange(this)" />
                <input type="number" id="inputYear" class="form-control" style="min-width: 100px; display: none;" min="2000" max="2100" placeholder="Year" onchange="handleDateChange(this)" />
            </div>
        </div>

        <!-- Hidden Fields -->
        <asp:HiddenField ID="hdnSaved" runat="server" />
        <asp:HiddenField ID="hdnTarget" runat="server" />

        <!-- Content Row (Transactions + Goal Progress) -->
        <div class="row g-0" style="height: calc(100vh - 155px);">

            <!-- Goal Progress + Chart -->
            <div class="col-md-4 d-flex flex-column px-4 pb-4" style="height: 100%;">
                <div class="bg-white rounded shadow-sm p-4 mb-3 d-flex flex-column justify-content-between" style="height: 100%;">
                    <h2 class="fw-bold mb-2 text-center">
                        <asp:Label ID="lblGoalName" runat="server" />
                    </h2>
                    <div class="d-flex flex-column flex-grow-1 align-items-center">
                        <!-- Circular Progress Chart -->
                        <div class="w-100 d-flex justify-content-center">
                            <div style="width: 200px; aspect-ratio: 1/1; position: relative;">
                                <canvas id="progressCircle" class="w-100 h-100" style="object-fit: contain;"></canvas>
                                <div id="progressTextOverlay" class="position-absolute top-50 start-50 translate-middle">
                                    <h1 class="fw-bold text-dark mb-0" id="progressTextOverlayValue">0%</h1>
                                </div>
                            </div>
                        </div>


                        <!-- Saved / Target -->
                        <div class="text-center fw-bold fs-5 pt-3">
                            $<asp:Label ID="lblSavedAmount" runat="server" />
                            / $<asp:Label ID="lblTargetAmount" runat="server" />
                        </div>

                        <!-- Date Info -->
                        <div class="text-center text-muted pt-1">
                            Goal Date:
                            <asp:Label ID="lblTargetDate" runat="server" /><br />
                            <asp:Label ID="lblDaysLeft" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
            <!-- Transactions List -->
            <div class="col-md-8 d-flex flex-column px-4 pb-4" style="height: 100%;">
                <h2 class="fw-bold mb-3">Transactions</h2>
                <div class="flex-grow-1 overflow-auto pe-2">
                    <asp:Repeater ID="rptTransactions" runat="server" OnItemDataBound="rptTransactions_ItemDataBound">
                        <ItemTemplate>
                            <div class='transaction-card d-flex align-items-center justify-content-between mb-3 p-3 border rounded shadow-sm bg-white <%# Eval("SourceType").ToString() == "jar" ? "disabled-transaction" : "clickable-card" %>'
                                <%# Eval("SourceType").ToString() == "jar" ? "" : "onclick=\"openEditModal(this)\"" %>
                                data-id='<%# Eval("TransactionId") %>'
                                data-name='<%# Eval("Name") %>'
                                data-amount='<%# Eval("Amount") %>'
                                data-type='Income'
                                data-date='<%# Eval("Date", "{0:yyyy-MM-dd}") %>'
                                data-category='<%# Eval("SourceType").ToString() == "jar" ? "Transfer In" : "Manual Top-up" %>'>

                                <div>
                                    <strong><%# Eval("Name") %></strong><br />
                                    <small class="text-muted"><%# Eval("Date", "{0:ddd, MMM d, yyyy}") %></small><br />
                                    <asp:Literal ID="litTransferNote" runat="server" />
                                </div>

                                <div>
                                    <span class='text-success fs-5 fw-bold'>+$<%# Eval("Amount", "{0:N2}") %>
                                    </span>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </div>

    <!-- Add Entry Button -->
    <button type="button" class="btn ms-lg-3 add-btn" data-bs-toggle="modal" data-bs-target="#addEntryModal">
        + New Entry
    </button>

    <!-- Add New Entry Modal -->
    <div class='modal fade' id='addEntryModal' tabindex='-1' aria-labelledby='addEntryModalLabel' aria-hidden='true'>
        <div class='modal-dialog modal-dialog-centered'>
            <div class='modal-content rounded-4 shadow'>
                <div class='modal-header'>
                    <h5 class='modal-title fw-bold' id='addEntryModalLabel'>Add New Entry</h5>
                    <button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button>
                </div>

                <div class='modal-body px-4'>
                    <asp:Panel ID='pnlAddEntry' runat='server' DefaultButton='btnSubmitEntry'>
                        <asp:HiddenField ID='hdnTransactionType' runat='server' />
                        <asp:HiddenField ID='hdnFixedJarId' runat='server' />


                        <!-- Common Inputs -->
                        <div class='mb-3'>
                            <label class='form-label fw-semibold'>Name <span class='text-danger'>*</span></label>
                            <asp:TextBox ID='txtTxnName' runat='server' CssClass='form-control' placeholder='e.g. Pocket Money' />
                        </div>

                        <div class='row mb-3'>
                            <div class='col'>
                                <label class='form-label fw-semibold'>Amount <span class='text-danger'>*</span></label>
                                <asp:TextBox ID='txtTxnAmount' runat='server' CssClass='form-control' TextMode='Number' />
                            </div>
                            <div class='col'>
                                <label class='form-label fw-semibold'>Date <span class='text-danger'>*</span></label>
                                <asp:TextBox ID='txtTxnDate' runat='server' CssClass='form-control' TextMode='Date' />
                            </div>
                        </div>

                        <!-- Toggle: Transfer from a Jar? -->
                        <div class="mb-3">
                            <label class="form-label fw-semibold">
                                Do you want to transfer this money from a Jar? <span class="text-danger">*</span>
                            </label>

                            <asp:HiddenField ID="hdnBoundJarId" runat="server" ClientIDMode="Static" />
                            <asp:HiddenField ID="hdnBoundJarName" runat="server" ClientIDMode="Static" />

                            <!-- Yes -->
                            <div class="form-check mb-2">
                                <asp:RadioButton ID="rdoTransferYes" runat="server" GroupName="TransferOption"
                                    CssClass="form-check-input" ClientIDMode="Static" />
                                <label class="form-check-label" for="rdoTransferYes">Yes</label>

                                <!-- When bound: show label -->
                                <asp:Panel ID="pnlBoundJar" runat="server" ClientIDMode="Static" CssClass="mt-2" Visible="false">
                                    <asp:TextBox ID="txtBoundJar" runat="server"
                                        CssClass="form-control"
                                        ReadOnly="true" Enabled="false" />
                                </asp:Panel>

                                <!-- When NOT bound: show picker -->
                                <asp:Panel ID="pnlJarPicker" runat="server" ClientIDMode="Static" CssClass="mt-2" Visible="true">
                                    <asp:DropDownList ID="ddlJars" runat="server" CssClass="form-select" />
                                </asp:Panel>
                            </div>

                            <!-- No -->
                            <div class="form-check">
                                <asp:RadioButton ID="rdoTransferNo" runat="server" GroupName="TransferOption"
                                    CssClass="form-check-input" ClientIDMode="Static" />
                                <label class="form-check-label" for="rdoTransferNo">No thanks</label>
                            </div>
                        </div>

                        <!-- Save Button -->
                        <asp:Button ID='btnSubmitEntry' runat='server' Text='Save'
                            CssClass='btn btn-primary w-100'
                            OnClientClick='return validateAddEntryForm();'
                            OnClick='btnSubmitEntry_Click' />
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>

    <!-- Edit Transaction Modal -->
    <div class="modal fade" id="editTxnModal" tabindex="-1" aria-labelledby="editTxnModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-3 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="editTxnModalLabel">Edit Transaction</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnEditTxnId" runat="server" />
                    <asp:Panel ID="pnlEditTxn" runat="server" DefaultButton="btnUpdateTxn">

                        <div class="mb-3">
                            <label for="txtEditTxnName" class="form-label fw-semibold">Transaction Name <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtEditTxnName" runat="server" CssClass="form-control" />
                        </div>

                        <div class="mb-3">
                            <label for="txtEditTxnAmount" class="form-label fw-semibold">Amount <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtEditTxnAmount" runat="server" CssClass="form-control" />
                        </div>

                        <div class="mb-3">
                            <label for="txtEditTxnDate" class="form-label fw-semibold">Date <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtEditTxnDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>


                        <asp:Button
                            ID="btnUpdateTxn"
                            runat="server"
                            Text="Save Changes"
                            CssClass="btn btn-primary w-100 fw-semibold mb-2"
                            OnClientClick="return validateEditTxn();"
                            OnClick="btnUpdateTxn_Click" />
                    </asp:Panel>

                    <button id="btnTxnDelete" type="button" class="btn btn-danger w-100 fw-semibold" onclick="openTxnDeleteModal(
                        document.getElementById('<%= hdnEditTxnId.ClientID %>').value,
                        document.getElementById('<%= txtEditTxnName.ClientID %>').value)">
                        Delete
                    </button>

                </div>
            </div>
        </div>
    </div>


    <!-- Hidden field to store TransactionId for deletion -->
    <asp:HiddenField ID="hdnDeleteTxnId" runat="server" />

    <!-- Delete Transaction Confirmation Modal -->
    <div class="modal fade" id="deleteTxnConfirmModal" tabindex="-1"
        aria-labelledby="deleteTxnConfirmLabel"
        aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-4 shadow p-4 text-center">
                <div class="modal-body">
                    <h5 class="fw-bold" id="deleteTxnConfirmLabel">Are you sure you want to delete "<span id="txnNameToDelete"></span>"?</h5>
                    <p class="text-muted mt-2">This transaction will be permanently removed.</p>

                    <div class="d-flex justify-content-center gap-3 mt-4">
                        <button type="button" class="btn btn-danger px-4"
                            onclick="cancelTxnDelete()">
                            Cancel</button>

                        <asp:Button ID="btnConfirmTxnDelete"
                            runat="server"
                            Text="Confirm"
                            CssClass="btn btn-primary px-4"
                            OnClick="btnConfirmTxnDelete_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Insufficient Funds Modal -->
    <asp:HiddenField ID="hdnInsufficientFunds" runat="server" Value="false" />
    <div class="modal fade"
        id="insufficientFundsModal"
        tabindex="-1"
        aria-labelledby="insufficientFundsLabel"
        aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-danger">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title"
                        id="insufficientFundsLabel">Insufficient Funds</h5>

                    <button type="button"
                        class="btn-close"
                        data-bs-dismiss="modal"
                        aria-label="Close">
                    </button>
                </div>

                <div class="modal-body">
                    You don’t have enough balance in this jar to complete the transfer.
                </div>

                <div class="modal-footer">
                    <button type="button"
                        class="btn btn-outline-danger"
                        data-bs-dismiss="modal">
                        Okay</button>
                </div>
            </div>
        </div>
    </div>

    <asp:HiddenField ID="hdnTargetExceeded" runat="server" Value="false" />

    <div class="modal fade"
        id="targetExceededModal"
        tabindex="-1"
        aria-labelledby="targetExceededLabel"
        aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-danger">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title" id="targetExceededLabel">Target Amount Exceeded</h5>
                    <button type="button"
                        class="btn-close"
                        data-bs-dismiss="modal"
                        aria-label="Close">
                    </button>
                </div>
                <div class="modal-body">
                    Adding this entry would exceed your goal’s target amount.
         Please enter a smaller amount.
                </div>
                <div class="modal-footer">
                    <button type="button"
                        class="btn btn-outline-warning"
                        data-bs-dismiss="modal">
                        Okay
                    </button>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>

    <!-- Chart.js import -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

    <script type="text/javascript">
        window.addEventListener('DOMContentLoaded', function () {
            const saved = parseFloat(document.getElementById('<%= hdnSaved.ClientID %>').value || 0);
            const target = parseFloat(document.getElementById('<%= hdnTarget.ClientID %>').value || 1);

            const percent = Math.min(100, ((saved / target) * 100).toFixed(2));
            const remainder = 100 - percent;

            console.log("✅ Chart Debug — Saved:", saved, "Target:", target, "Percent:", percent);

            document.getElementById('progressTextOverlayValue').innerText = percent + "%";

            const ctx = document.getElementById('progressCircle').getContext('2d');
            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    datasets: [{
                        data: [percent, remainder],
                        backgroundColor: ['#4e3cc7', '#e4e4f4'],
                        borderWidth: 0
                    }]
                },
                options: {
                    cutout: '80%',
                    responsive: false,
                    plugins: { legend: { display: false }, tooltip: { enabled: false } }
                }
            });

            const flag = document.getElementById('<%= hdnInsufficientFunds.ClientID %>').value;
            if (flag === "true") {
                new bootstrap.Modal(document.getElementById('insufficientFundsModal')).show();
            }

            const targetExceededFlag = document.getElementById('<%= hdnTargetExceeded.ClientID %>').value;
            if (targetExceededFlag === "true") {
                new bootstrap.Modal(document.getElementById('targetExceededModal')).show();
            }

        });

        document.addEventListener('DOMContentLoaded', function () {
            const dropdown = document.getElementById('customDropdown');
            const selected = dropdown.querySelector('.selected');
            const optionsContainer = dropdown.querySelector('.options');
            const optionsList = optionsContainer.querySelectorAll('.option');
            const periodField = document.getElementById('<%= hdnSelectedPeriod.ClientID %>');
            const dateField = document.getElementById('<%= hdnSelectedDate.ClientID %>');

            function updateVisibleInput() {
                const period = periodField.value;
                const selectedValue = dateField.value;

                ['inputDay', 'inputWeek', 'inputMonth', 'inputYear'].forEach(id => {
                    const input = document.getElementById(id);
                    input.style.display = 'none';
                    input.value = '';
                });

                let inputToShow = null;
                if (period === 'day') inputToShow = 'inputDay';
                else if (period === 'week') inputToShow = 'inputWeek';
                else if (period === 'month') inputToShow = 'inputMonth';
                else if (period === 'year') inputToShow = 'inputYear';

                if (inputToShow) {
                    const input = document.getElementById(inputToShow);
                    input.style.display = 'block';
                    input.value = selectedValue;
                }
            }

            function handleOptionClick(option) {
                const value = option.getAttribute('data-value');
                const img = option.querySelector('img');
                const text = option.querySelector('span').innerText;

                const selectedImg = selected.querySelector('img');
                const selectedSpan = selected.querySelector('span:not(.dropdown-arrow)');

                if (img) {
                    if (!selectedImg) {
                        const newImg = document.createElement('img');
                        selected.insertBefore(newImg, selectedSpan);
                    }
                    selected.querySelector('img').src = img.src;
                    selected.querySelector('img').alt = img.alt || text;
                    selected.querySelector('img').width = img.width;
                    selected.querySelector('img').height = img.height;
                } else if (selectedImg) {
                    selected.removeChild(selectedImg);
                }

                selectedSpan.innerText = text;
                periodField.value = value;

                const today = new Date();
                let defaultDate = "";

                switch (value) {
                    case "day":
                        defaultDate = today.toISOString().split('T')[0];
                        break;
                    case "week":
                        const current = new Date();
                        const day = current.getDay();
                        const mondayOffset = (day === 0) ? -6 : 1 - day;
                        const monday = new Date(current.getFullYear(), current.getMonth(), current.getDate() + mondayOffset);

                        const jan1 = new Date(monday.getFullYear(), 0, 1);
                        const diffDays = Math.floor((monday - jan1) / (24 * 60 * 60 * 1000));
                        const week = Math.ceil((diffDays + jan1.getDay() + 1) / 7);

                        const weekStr = week.toString().padStart(2, '0');
                        defaultDate = `${monday.getFullYear()}-W${weekStr}`;
                        break;
                    case "month":
                        defaultDate = today.toISOString().slice(0, 7);
                        break;
                    case "year":
                        defaultDate = today.getFullYear();
                        break;
                }

                dateField.value = defaultDate;
                updateVisibleInput();
                optionsContainer.style.display = 'none';
                document.getElementById('<%= btnPeriodChange.ClientID %>').click();
            }

            selected.addEventListener('click', e => {
                e.stopPropagation();
                const isOpen = optionsContainer.style.display === 'block';
                document.querySelectorAll('.custom-dropdown .options').forEach(opt => opt.style.display = 'none');
                optionsContainer.style.display = isOpen ? 'none' : 'block';
            });

            document.addEventListener('click', e => {
                if (!dropdown.contains(e.target)) optionsContainer.style.display = 'none';
            });

            optionsList.forEach(option => {
                option.addEventListener('click', () => handleOptionClick(option));
            });

            window.handleDateChange = function (input) {
                dateField.value = input.value;
                document.getElementById('<%= btnPeriodChange.ClientID %>').click();
            };

            updateVisibleInput();
        });

        // Validation helpers
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
            if (input.tagName.toLowerCase() === "select") {
                input.parentNode.appendChild(errorDiv);
            } else {
                input.parentNode.insertBefore(errorDiv, input.nextSibling);
            }
        }

        function resetEntryForm() {
            const todayISODate = new Date().toISOString().split('T')[0];
            const nameInput = document.getElementById('<%= txtTxnName.ClientID %>');
            const amountInput = document.getElementById('<%= txtTxnAmount.ClientID %>');
            const dateInput = document.getElementById('<%= txtTxnDate.ClientID %>');
            if (nameInput) nameInput.value = "";
            if (amountInput) amountInput.value = "";
            if (dateInput) dateInput.value = todayISODate;

            const yesRadio = document.getElementById('rdoTransferYes');
            const noRadio = document.getElementById('rdoTransferNo');
            if (yesRadio) yesRadio.checked = false;
            if (noRadio) noRadio.checked = false;

            const jarDropdown = document.getElementById('<%= ddlJars.ClientID %>');
            if (jarDropdown) jarDropdown.selectedIndex = 0;

            // Hide both dropdown & bound section
            ['pnlJarPicker', 'pnlBoundJar'].forEach(id => {
                const el = document.getElementById(id);
                if (el) el.style.display = 'none';
            });


            resetValidation(document.getElementById('<%= pnlAddEntry.ClientID %>'));
        }

        function validateAddEntryForm() {
            const nameInput = document.getElementById('<%= txtTxnName.ClientID %>');
            const amountInput = document.getElementById('<%= txtTxnAmount.ClientID %>');
            const dateInput = document.getElementById('<%= txtTxnDate.ClientID %>');
            const yesRadio = document.getElementById('rdoTransferYes');
            const noRadio = document.getElementById('rdoTransferNo');
            const jarDropdown = document.getElementById('<%= ddlJars.ClientID %>');

            resetValidation(document.getElementById('<%= pnlAddEntry.ClientID %>'));
            removeRadioGroupError();

            let isValid = true;

            if (!nameInput.value.trim()) { showInvalid(nameInput, "Please enter a name."); isValid = false; }
            let amt = parseFloat(amountInput.value);
            if (isNaN(amt) || amt <= 0) { showInvalid(amountInput, "Please enter a valid amount."); isValid = false; }
            if (!dateInput.value) { showInvalid(dateInput, "Please select a date."); isValid = false; }

            if (!yesRadio.checked && !noRadio.checked) {
                showRadioGroupError("Please select an option.");
                isValid = false;
            }

            // Only require dropdown if "Yes" and NOT bound
            if (yesRadio.checked) {
                const hasBound = (document.getElementById('hdnBoundJarId')?.value || '').trim() !== '';
                if (!hasBound) {
                    if (!jarDropdown.value || jarDropdown.value === "") {
                        showInvalid(jarDropdown, "Please select a target jar.");
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        function showRadioGroupError(message) {
            const container = document.querySelector('.form-check').parentNode;
            let existingError = container.querySelector('.invalid-feedback-radio');
            const yesRadio = document.getElementById('rdoTransferYes');
            const noRadio = document.getElementById('rdoTransferNo');
            yesRadio.classList.add('is-invalid-radio');
            noRadio.classList.add('is-invalid-radio');

            if (!existingError) {
                const errorDiv = document.createElement("div");
                errorDiv.className = "invalid-feedback invalid-feedback-radio";
                errorDiv.style.display = "block";
                errorDiv.style.color = "#dc3545";
                errorDiv.style.marginTop = "0.25rem";
                errorDiv.textContent = message;
                container.appendChild(errorDiv);
            }
        }

        function removeRadioGroupError() {
            const container = document.querySelector('.form-check').parentNode;
            const existingError = container.querySelector('.invalid-feedback-radio');
            if (existingError) existingError.remove();
            const yesRadio = document.getElementById('rdoTransferYes');
            const noRadio = document.getElementById('rdoTransferNo');
            yesRadio.classList.remove('is-invalid-radio');
            noRadio.classList.remove('is-invalid-radio');
        }

        function toggleTransferUI() {
            const yes = document.getElementById('rdoTransferYes')?.checked;
            const boundId = (document.getElementById('hdnBoundJarId')?.value || '').trim();
            const hasBound = boundId !== '';

            const boundSec = document.getElementById('pnlBoundJar');
            const boundName = document.getElementById('lblBoundJarName');
            const ddSec = document.getElementById('pnlJarPicker');

            if (!yes) {
                if (boundSec) boundSec.style.display = 'none';
                if (ddSec) ddSec.style.display = 'none';
                return;
            }

            if (hasBound) {
                if (boundName) boundName.textContent =
                    document.getElementById('hdnBoundJarName')?.value || ('Jar #' + boundId);
                if (boundSec) boundSec.style.display = '';
                if (ddSec) ddSec.style.display = 'none';
            } else {
                if (boundSec) boundSec.style.display = 'none';
                if (ddSec) ddSec.style.display = '';
            }
        }

        var addEntryModalEl = document.getElementById('addEntryModal');
        if (addEntryModalEl) {
            addEntryModalEl.addEventListener('show.bs.modal', function () {
                const yesRadio = document.getElementById('rdoTransferYes');
                const noRadio = document.getElementById('rdoTransferNo');
                if (!yesRadio || !noRadio) return;

                // Auto-select Yes if bound
                const hasBound = (document.getElementById('hdnBoundJarId')?.value || '').trim() !== '';
                if (hasBound) {
                    yesRadio.checked = true;
                    noRadio.checked = false;
                }

                toggleTransferUI();
                yesRadio.addEventListener('change', toggleTransferUI);
                noRadio.addEventListener('change', toggleTransferUI);
            });
        }

        function toggleEntryForm(type) {
            const yesRadio = document.getElementById('rdoTransferYes');
            if (!yesRadio) return;
            yesRadio.checked = (type === 'jar');
            const noRadio = document.getElementById('rdoTransferNo');
            if (noRadio) noRadio.checked = !yesRadio.checked;
            toggleTransferUI();
        }

        // Edit modal
        function openEditModal(el) {
            const type = el.dataset.type;
            if (type === "Transfer") return;
            document.getElementById('<%= hdnEditTxnId.ClientID %>').value = el.dataset.id || "";
            document.getElementById('<%= txtEditTxnName.ClientID %>').value = el.dataset.name || "";
            document.getElementById('<%= txtEditTxnAmount.ClientID %>').value = el.dataset.amount || "";
            document.getElementById('<%= txtEditTxnDate.ClientID %>').value = el.dataset.date || "";
            new bootstrap.Modal(document.getElementById("editTxnModal")).show();
        }

        function validateEditTxn() {
            const name = document.getElementById('<%= txtEditTxnName.ClientID %>');
            const amount = document.getElementById('<%= txtEditTxnAmount.ClientID %>');
            const date = document.getElementById('<%= txtEditTxnDate.ClientID %>');
            resetValidation(document.getElementById('<%= pnlEditTxn.ClientID %>'));

            let isValid = true;
            if (!name.value.trim()) { showInvalid(name, "Please enter a name."); isValid = false; }
            let amt = parseFloat(amount.value);
            if (isNaN(amt) || amt <= 0) { showInvalid(amount, "Please enter a valid amount greater than 0."); isValid = false; }
            if (!date.value) { showInvalid(date, "Please select a date."); isValid = false; }
            return isValid;
        }

        function openTxnDeleteModal(txnId, txnName) {
            let editModal = bootstrap.Modal.getInstance(document.getElementById('editTxnModal'));
            if (editModal) { editModal.hide(); editModal.dispose(); }
            document.getElementById('<%= hdnDeleteTxnId.ClientID %>').value = txnId || "";
            document.getElementById('txnNameToDelete').textContent = txnName || "";
            new bootstrap.Modal(document.getElementById('deleteTxnConfirmModal')).show();
        }

        function cancelTxnDelete() {
            bootstrap.Modal.getInstance(document.getElementById('deleteTxnConfirmModal')).hide();
            new bootstrap.Modal(document.getElementById('editTxnModal')).show();
        }

        var addEntryModal = document.getElementById('addEntryModal');
        if (addEntryModal) {
            addEntryModal.addEventListener('show.bs.modal', resetEntryForm);
        }
    </script>
</asp:Content>

