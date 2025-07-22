<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="bipj.Dashboard" %>

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
            border: none;
            border-radius: 5px;
            padding: 6px 12px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            white-space: nowrap;
        }

            .add-btn:hover {
                background-color: #4e3cc7;
            }

        .info-icon-img {
            width: 12px;
            height: 12px;
            vertical-align: middle;
            position: relative;
            top: -12px;
        }

        .bg-white.rounded,
        .border.rounded {
            border-radius: 1rem !important;
            box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);
        }

        .p-3.flex-grow-1.bg-white.rounded {
            border-radius: 1rem !important;
            box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);
        }

        .mainpage-content {
            margin-left: 220px;
            max-width: calc(100% - 220px);
            padding: 1.5rem;
            overflow-x: hidden;
            overflow-y: auto;
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
    <div class="mainpage-content" style="height: calc(100vh - 66px); overflow: hidden;">
        <!-- Header Row -->
        <div class="d-flex justify-content-between align-items-center px-4 py-1 mb-2">
            <h1 class="fw-bold mb-0">OVERALL DASHBOARD</h1>
            <div class="col-auto">
                <div class="d-flex align-items-center gap-3">
                    <div class="custom-dropdown" id="customDropdown">

                        <!-- closed LinkButton -->
                        <asp:LinkButton
                            ID="btnPeriodChange"
                            runat="server"
                            OnClick="btnPeriodChange_Click"
                            Style="display: none">
                        </asp:LinkButton>

                        <div class="selected" id="selectedOption">
                            <asp:Literal ID="litPeriodIcon" runat="server" />
                            <span class="selected-label">
                                <asp:Literal ID="litPeriodLabel" runat="server" />
                            </span>
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

                    <input type="date" id="inputDay" class="form-control" style="min-width: 160px;" onchange="handleDateChange(this)" />
                    <input type="week" id="inputWeek" class="form-control" style="min-width: 160px; display: none;" onchange="handleDateChange(this)" />
                    <input type="month" id="inputMonth" class="form-control" style="min-width: 160px; display: none;" onchange="handleDateChange(this)" />
                    <input type="number" id="inputYear" class="form-control" style="min-width: 100px; display: none;" min="2000" max="2100" placeholder="Year" onchange="handleDateChange(this)" />
                </div>
            </div>
        </div>

        <!-- TOTAL BALANCE Section -->
        <div class="p-4 mb-3 d-flex flex-column"
            style="background: linear-gradient(90deg, #dfe4ff, #f4f2fd); border-radius: 16px; min-height: 200px;">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h3 class="fw-bold mb-0">TOTAL BALANCE</h3>
            </div>
            <div class="d-flex gap-4 align-items-stretch flex-grow-1">
                <!-- Expense Card -->
                <div class="bg-white text-center shadow-sm rounded p-3 flex-grow-1 d-flex flex-column justify-content-center">
                    <div class="fw-semibold fs-6 text-dark">Expense</div>
                    <div class="text-danger fw-bold fs-5">
                        <i class="bi bi-caret-down-fill"></i>
                        <asp:Label ID="lblExpense" runat="server"></asp:Label>
                    </div>
                </div>

                <!-- Income Card -->
                <div class="bg-white text-center shadow-sm rounded p-3 flex-grow-1 d-flex flex-column justify-content-center">
                    <div class="fw-semibold fs-6 text-dark">Income</div>
                    <div class="text-success fw-bold fs-5">
                        <i class="bi bi-caret-up-fill"></i>
                        <asp:Label ID="lblIncome" runat="server"></asp:Label>
                    </div>
                </div>


                <!-- Balance Card -->
                <div class="bg-white text-center shadow-sm rounded p-3 flex-grow-1 d-flex flex-column justify-content-center">
                    <div class="fw-semibold fs-6 text-dark">Balance</div>
                    <div id="balanceAmountDiv" runat="server" class="text-success fw-bold fs-5">
                        <i class="bi bi-equals"></i>
                        <asp:Label ID="lblBalance" runat="server"></asp:Label>
                    </div>
                </div>
            </div>
        </div>

        <!-- JARS + GOALS row -->
        <div class="d-flex gap-4">
            <!-- JARS -->
            <div class="flex-1 border p-4 rounded shadow-sm bg-white">
                <h3 class="fw-bold mb-3">MY JARS
                 <a href="#" data-bs-toggle="modal" data-bs-target="#info-popup" title="Learn more about Jars">
                     <img src="images/info-icon.png" alt="Info" class="info-icon-img" />
                 </a>
                </h3>
                <div class="d-flex align-items-center">
                    <img src="images/piggy-icon.png" alt="Jars" style="height: 60px;" class="me-3" />
                    <div>
                        <h6 class="mb-1">Total Money (All Jars)</h6>
                        <asp:Label ID="lblJarTotal" runat="server"
                            CssClass="fw-bold fs-5 text-dark"
                            Text="$0" />
                    </div>
                </div>
            </div>

            <!-- GOALS -->
            <div class="flex-fill border p-4 rounded shadow-sm bg-white position-relative">
                <h3 class="fw-bold mb-3">MY GOALS</h3>
                <asp:Panel ID="pnlOngoingGoals" runat="server">
                    <p class="mb-2">
                        <strong>
                            <asp:Label ID="lblOngoingCount" runat="server" /></strong> Ongoing,
                    <strong>
                        <asp:Label ID="lblCompletedCount" runat="server" /></strong> Completed
                    </p>
                    <div class="d-flex align-items-center gap-3">
                        <div class="d-flex align-items-center gap-2">
                            <canvas id="goalProgressRing" width="30" height="30" style="display: block;"></canvas>
                            <span class="fw-semibold" style="font-size: 14px;">
                                <asp:Label ID="lblOverallPercent" runat="server" />
                                Done
                            </span>
                        </div>
                        <span class="text-muted">|</span>
                        <div class="d-flex align-items-center gap-2">
                            <img src="images/piggy-icon.png" style="height: 30px;" />
                            <span class="fw-semibold" style="font-size: 14px;">
                                <asp:Label ID="lblSavedVsTarget" runat="server" />
                            </span>
                        </div>
                </asp:Panel>
                <asp:Label ID="lblNoOngoingGoals"
                    runat="server"
                    CssClass="text-muted"
                    Visible="false"
                    Text="No ongoing goals…" />
            </div>
        </div>
    </div>

    <!-- Fixed Action Buttons -->
    <div class="fixed-action-buttons d-flex gap-2">
        <button type="button"
            class="btn ms-lg-3 add-btn"
            data-bs-toggle="modal"
            data-bs-target="#addEntryModal">
            + New Entry
        </button>
    </div>

    <!-- Add New Entry Modal -->
    <div class="modal fade"
        id="addEntryModal"
        tabindex="-1"
        aria-labelledby="addEntryModalLabel"
        aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-3 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="addEntryModalLabel">Add New Entry</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:Panel ID="pnlAddEntry" runat="server" DefaultButton="btnSubmitEntry">
                        <asp:HiddenField ID="hdnTransactionType" runat="server" />

                        <div class="d-flex gap-2 mb-3">
                            <asp:Button ID="btnShowExpense" runat="server" Text="Expenses"
                                CssClass="btn btn-outline-primary w-50"
                                OnClientClick="toggleEntryForm('expense'); return false;"
                                UseSubmitBehavior="false" />
                            <asp:Button ID="btnShowIncome" runat="server" Text="Income"
                                CssClass="btn btn-outline-secondary w-50"
                                OnClientClick="toggleEntryForm('income'); return false;"
                                UseSubmitBehavior="false" />
                        </div>

                        <div id="expenseForm">
                            <div class="mb-3">
                                <label class="form-label">Name <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtExpenseName" runat="server"
                                    CssClass="form-control" placeholder="e.g. Subway" />
                            </div>
                            <div class="row mb-3">
                                <div class="col">
                                    <label class="form-label">Amount<span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtExpenseAmount" runat="server"
                                        CssClass="form-control" TextMode="Number" />
                                </div>
                                <div class="col">
                                    <label class="form-label">Date<span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtExpenseDate" runat="server"
                                        CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Jar<span class="text-danger">*</span></label>
                                <asp:DropDownList ID="ddlJars" runat="server"
                                    CssClass="form-select" />
                            </div>
                        </div>

                        <div id="incomeForm" style="display: none;">
                            <div class="mb-3">
                                <label class="form-label">Name<span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtIncomeName" runat="server"
                                    CssClass="form-control" placeholder="e.g. Monthly Allowance" />
                            </div>
                            <div class="row mb-3">
                                <div class="col">
                                    <label class="form-label">Amount<span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtIncomeAmount" runat="server"
                                        CssClass="form-control" TextMode="Number" />
                                </div>
                                <div class="col">
                                    <label class="form-label">Date<span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtIncomeDate" runat="server"
                                        CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                            <label class="form-label mb-1">How to allocate?</label>
                            <div class="form-check">
                                <input type="radio" name="incomeAllocation" id="autoDistribute" value="auto"
                                    checked onchange="toggleIncomeJarDropdown()" />
                                <label for="autoDistribute" class="form-check-label">
                                    Distribute automatically based on percentages
                                </label>
                            </div>
                            <div class="form-check mb-3">
                                <asp:HiddenField ID="hdnIncomeAllocation" runat="server" />
                                <input type="radio" name="incomeAllocation" id="manualDistribute"
                                    value="manual" onchange="toggleIncomeJarDropdown()" />
                                <label for="manualDistribute" class="form-check-label">
                                    Choose a specific jar
                                </label>
                                <div id="incomeJarDropdown" style="display: none; margin-top: .5rem;">
                                    <asp:DropDownList ID="ddlIncomeJars" runat="server"
                                        CssClass="form-select" />
                                </div>
                            </div>
                        </div>

                        <asp:Button ID="btnSubmitEntry" runat="server" Text="Save"
                            CssClass="btn btn-primary w-100"
                            OnClientClick="return validateAddEntryForm();"
                            OnClick="btnSubmitEntry_Click" />
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>

    <!-- Info Popup Modal -->
    <div class="modal fade" id="info-popup" tabindex="-1" role="dialog"
        aria-labelledby="info-popup-title" aria-modal="true" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg" role="document">
            <div class="modal-content rounded-3 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="info-popup-title">What are the 6 Jars?</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"
                        aria-label="Close">
                    </button>
                </div>
                <div class="modal-body">
                    <p>
                        The 6 Jars system was created by T. Harv Eker as part of his financial freedom strategy.<br />
                        It helps people manage money by assigning a fixed percentage of every dollar they earn to different life purposes.
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
                        <a href="https://harvekeronline.com/6-jars-exercise/"
                            target="_blank" rel="noopener noreferrer">https://harvekeronline.com/6-jars-exercise/
                        </a>
                    </p>
                </div>
            </div>
        </div>
    </div>

    <!-- Insufficient Funds Modal -->
    <asp:HiddenField ID="hdnInsufficientFunds" runat="server" Value="false" />
    <div class="modal fade" id="insufficientFundsModal" tabindex="-1"
        aria-labelledby="insufficientFundsLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-danger">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title" id="insufficientFundsLabel">Insufficient Funds</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"
                        aria-label="Close">
                    </button>
                </div>
                <div class="modal-body">
                    You don’t have enough balance in this jar to complete the transfer.
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-danger"
                        data-bs-dismiss="modal">
                        Okay</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script>
        // ====================== GLOBAL HELPERS ===================================
        function $(id) { return document.getElementById(id); }

        function resetValidation(ctx) {
            ctx.querySelectorAll('.is-invalid').forEach(i => i.classList.remove('is-invalid'));
            ctx.querySelectorAll('.invalid-feedback').forEach(d => d.remove());
        }

        function showInsufficientFundsModal() {
            const addEl = $('addEntryModal');
            const addInstance = bootstrap.Modal.getInstance(addEl) || new bootstrap.Modal(addEl);

            addEl.addEventListener('hidden.bs.modal', () => {
                const insuffEl = $('insufficientFundsModal');
                bootstrap.Modal.getOrCreateInstance(insuffEl).show();
            }, { once: true });

            addInstance.hide();
        }

        function selectedJarBalance() {
            const ddl = $('<%= ddlJars.ClientID %>');
            if (!ddl || ddl.selectedIndex < 0) return 0;
            const opt = ddl.options[ddl.selectedIndex];
            return parseFloat(opt.getAttribute('data-balance')) || 0;
        }

        // ====================== DOM READY ========================================
        document.addEventListener('DOMContentLoaded', function () {

            // 1) Goal doughnut
            (function initGoalChart() {
                const pctText = '<%= lblOverallPercent.Text.Replace("%", "") %>';
                const percentage = parseFloat(pctText) || 0;
                const canvas = $('goalProgressRing');
                if (!canvas) return;
                const ctx = canvas.getContext('2d');
                new Chart(ctx, {
                    type: 'doughnut',
                    data: {
                        datasets: [{
                            data: [percentage, 100 - percentage],
                            backgroundColor: ['#3C2C80', '#E5E5E5'],
                            borderWidth: 0,
                            cutout: '60%'
                        }]
                    },
                    options: {
                        responsive: false,
                        plugins: { legend: { display: false }, tooltip: { enabled: false } }
                    }
                });
            })();

            // 2) Period dropdown + date inputs
            const dropdown = $('customDropdown');
            const selected = dropdown.querySelector('.selected');
            const selectedLabel = selected.querySelector('.selected-label');
            const optionsContainer = dropdown.querySelector('.options');
            const optionsList = optionsContainer.querySelectorAll('.option');
            const periodField = $('<%= hdnSelectedPeriod.ClientID %>');
            const dateField = $('<%= hdnSelectedDate.ClientID %>');

            function updateVisibleInput() {
                ['inputDay', 'inputWeek', 'inputMonth', 'inputYear'].forEach(id => {
                    const el = $(id);
                    el.style.display = 'none';
                    el.value = '';
                });
                const map = { day: 'inputDay', week: 'inputWeek', month: 'inputMonth', year: 'inputYear' };
                const sId = map[periodField.value];
                if (sId) {
                    const el = $(sId);
                    el.style.display = 'block';
                    el.value = dateField.value;
                }
            }

            function handleOptionClick(opt) {
                const val = opt.getAttribute('data-value');
                const img = opt.querySelector('img');
                const text = opt.querySelector('span').innerText;

                if (img) {
                    let icon = selected.querySelector('img');
                    if (!icon) {
                        icon = document.createElement('img');
                        selected.insertBefore(icon, selectedLabel);
                    }
                    icon.src = img.src; icon.alt = img.alt || text;
                    icon.width = img.width; icon.height = img.height;
                } else {
                    const icon = selected.querySelector('img');
                    if (icon) icon.remove();
                }

                selectedLabel.innerText = text;
                periodField.value = val;

                const today = new Date();
                let def = '';
                switch (val) {
                    case 'day':
                        def = today.toLocaleDateString('en-CA');
                        break;
                    case 'week': {
                        const current = new Date();
                        const day = current.getDay();
                        const mondayOffset = (day === 0) ? -6 : 1 - day;
                        const monday = new Date(current.getFullYear(), current.getMonth(), current.getDate() + mondayOffset);
                        const jan1 = new Date(monday.getFullYear(), 0, 1);
                        const diffDays = Math.floor((monday - jan1) / 86400000);
                        const weekNum = Math.ceil((diffDays + jan1.getDay() + 1) / 7);
                        def = `${monday.getFullYear()}-W${weekNum.toString().padStart(2, '0')}`;
                        break;
                    }
                    case 'month':
                        def = today.toISOString().slice(0, 7);
                        break;
                    case 'year':
                        def = today.getFullYear();
                        break;
                    case 'all':
                        def = '';
                        break;
                }
                dateField.value = def;

                updateVisibleInput();
                optionsContainer.style.display = 'none';
                $('<%= btnPeriodChange.ClientID %>').click();
            }

            selected.addEventListener('click', e => {
                e.stopPropagation();
                const open = optionsContainer.style.display === 'block';
                document.querySelectorAll('.custom-dropdown .options').forEach(o => o.style.display = 'none');
                optionsContainer.style.display = open ? 'none' : 'block';
            });
            document.addEventListener('click', e => {
                if (!dropdown.contains(e.target)) optionsContainer.style.display = 'none';
            });
            optionsList.forEach(o => o.addEventListener('click', () => handleOptionClick(o)));

            window.handleDateChange = function (inp) {
                dateField.value = inp.value;
                $('<%= btnPeriodChange.ClientID %>').click();
            };
            updateVisibleInput();

            // 3) Form validation & toggles
            window.validateAddEntryForm = function () {
                const txnType = $('<%= hdnTransactionType.ClientID %>').value;
                let nameI, amtI, dateI;
                let valid = true;
                const currentBal = selectedJarBalance();

                if (txnType === 'Expense') {
                    nameI = $('<%= txtExpenseName.ClientID %>');
            amtI = $('<%= txtExpenseAmount.ClientID %>');
            dateI = $('<%= txtExpenseDate.ClientID %>');
            resetValidation(nameI.closest('form'));

            if (!nameI.value.trim()) { showInvalid(nameI, 'Please enter a name.'); valid = false; }
            const a = parseFloat(amtI.value);
            if (isNaN(a) || a <= 0) { showInvalid(amtI, 'Please enter a valid amount greater than 0.'); valid = false; }
            else if (a > currentBal) {
                showInsufficientFundsModal();
                return false;
            }
            if (!dateI.value) { showInvalid(dateI, 'Please select a date.'); valid = false; }
            const ddl = $('<%= ddlJars.ClientID %>');
            if (!ddl.value) { showInvalid(ddl, 'Please select a jar.'); valid = false; }
        }
        else if (txnType === 'Income') {
            nameI = $('<%= txtIncomeName.ClientID %>');
            amtI = $('<%= txtIncomeAmount.ClientID %>');
            dateI = $('<%= txtIncomeDate.ClientID %>');
            resetValidation(nameI.closest('form'));

            if (!nameI.value.trim()) { showInvalid(nameI, 'Please enter a name.'); valid = false; }
            const a = parseFloat(amtI.value);
            if (isNaN(a) || a <= 0) { showInvalid(amtI, 'Please enter a valid amount greater than 0.'); valid = false; }
            if (!dateI.value) { showInvalid(dateI, 'Please select a date.'); valid = false; }

            const alloc = $('hdnIncomeAllocation').value;
            if (alloc === 'manual') {
                const ddlInc = $('<%= ddlIncomeJars.ClientID %>');
                        if (!ddlInc.value) { showInvalid(ddlInc, 'Please select a jar for income allocation.'); valid = false; }
                    }
                } else {
                    valid = false;
                }

                return valid;

                function showInvalid(el, msg) {
                    el.classList.add('is-invalid');
                    const div = document.createElement('div');
                    div.className = 'invalid-feedback';
                    div.textContent = msg;
                    el.parentNode.insertBefore(div, el.nextSibling);
                }
            };

            window.toggleEntryForm = function (type) {
                const incForm = $('incomeForm');
                const expForm = $('expenseForm');
                const txnField = $('<%= hdnTransactionType.ClientID %>');
                const btnInc = $('<%= btnShowIncome.ClientID %>');
                const btnExp = $('<%= btnShowExpense.ClientID %>');

                if (type === 'income') {
                    incForm.style.display = 'block';
                    expForm.style.display = 'none';
                    txnField.value = 'Income';
                    btnInc.classList.add('btn-primary');
                    btnInc.classList.remove('btn-outline-secondary');
                    btnExp.classList.add('btn-outline-primary');
                    btnExp.classList.remove('btn-primary');
                    resetValidation(incForm);
                } else {
                    incForm.style.display = 'none';
                    expForm.style.display = 'block';
                    txnField.value = 'Expense';
                    btnExp.classList.add('btn-primary');
                    btnExp.classList.remove('btn-outline-primary');
                    btnInc.classList.add('btn-outline-secondary');
                    btnInc.classList.remove('btn-primary');
                    resetValidation(expForm);
                }
            };

            window.toggleIncomeJarDropdown = function () {
                const manual = $('manualDistribute');
                const dropdown = $('incomeJarDropdown');
                const hdn = $('hdnIncomeAllocation');
                if (manual.checked) {
                    dropdown.style.display = 'block';
                    hdn.value = 'manual';
                } else {
                    dropdown.style.display = 'none';
                    hdn.value = 'auto';
                    const ddl = $('<%= ddlIncomeJars.ClientID %>');
                    if (ddl) ddl.selectedIndex = 0;
                }
            };

            const addEntryModal = $('addEntryModal');
            if (addEntryModal) {
                addEntryModal.addEventListener('show.bs.modal', function () {
                    // reset fields
                    const today = new Date().toLocaleDateString('en-CA');
                    [
                '<%= txtExpenseName.ClientID %>',
                '<%= txtExpenseAmount.ClientID %>',
                '<%= txtExpenseDate.ClientID %>',
                '<%= txtIncomeName.ClientID %>',
                '<%= txtIncomeAmount.ClientID %>',
                '<%= txtIncomeDate.ClientID %>'
                    ].forEach(id => {
                        const el = $(id);
                        if (el) el.value = '';
                    });
                    $('<%= txtExpenseDate.ClientID %>').value = today;
                    $('<%= txtIncomeDate.ClientID %>').value = today;
                    toggleEntryForm('expense');
                    $('autoDistribute').checked = true;
                    $('hdnIncomeAllocation').value = 'auto';
                    toggleIncomeJarDropdown();
                    const ddlJ = $('<%= ddlJars.ClientID %>'); if (ddlJ) ddlJ.selectedIndex = 0;
                    const ddlI = $('<%= ddlIncomeJars.ClientID %>'); if (ddlI) ddlI.selectedIndex = 0;
                });
            }
        });
    </script>
</asp:Content>
