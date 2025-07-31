<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="JarDetails.aspx.cs" Inherits="bipj.JarDetails" %>

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
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            white-space: nowrap;
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
            <a href="Jars.aspx" class="text-dark text-decoration-none">
                <h1 class="fw-bold mb-0"><i class="bi bi-arrow-left me-2"></i>MY JARS</h1>
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

        <!-- Content Row -->
        <div class="row g-0" style="height: calc(100vh - 155px);">
            <!-- Dashboard -->
            <div class="col-md-4 d-flex flex-column px-4 pb-4" style="height: 100%; min-height: 0;">
                <div class="flex-fill d-flex flex-column p-3 rounded-4 shadow-sm"
                    style="background: linear-gradient(180deg, #dbeafe, #ede9fe); min-height: 0;">

                    <!-- Jar Name -->
                    <h4 class="fw-bold text-center">
                        <asp:Label ID="lblJarName" runat="server"></asp:Label>
                    </h4>

                    <!-- Cards Group -->
                    <div class="d-flex flex-column flex-fill justify-content-around gap-3">

                        <!-- Expense Card -->
                        <div class="bg-white rounded-4 text-center py-2 px-2 shadow-sm flex-fill d-flex flex-column justify-content-center">
                            <div class="fw-semibold fs-6 text-dark">Expense</div>
                            <div class="text-danger fw-bold fs-5">
                                <i class="bi bi-caret-down-fill"></i>
                                <asp:Label ID="lblExpenseTotal" runat="server"></asp:Label>
                            </div>
                            <small class="text-muted" style="font-size: 0.8rem;">
                                <asp:Label ID="lblTransferOut" runat="server"></asp:Label>
                            </small>
                        </div>

                        <!-- Income Card -->
                        <div class="bg-white rounded-4 text-center py-2 px-2 shadow-sm flex-fill d-flex flex-column justify-content-center">
                            <div class="fw-semibold fs-6 text-dark">Income</div>
                            <div class="text-success fw-bold fs-5">
                                <i class="bi bi-caret-up-fill"></i>
                                <asp:Label ID="lblIncomeTotal" runat="server"></asp:Label>
                            </div>
                            <small class="text-muted" style="font-size: 0.8rem;">
                                <asp:Label ID="lblTransferIn" runat="server"></asp:Label>
                            </small>
                        </div>


                        <!-- Balance Card -->
                        <div class="bg-white rounded-4 text-center py-2 px-2 shadow-sm flex-fill d-flex flex-column justify-content-center">
                            <div class="fw-semibold fs-6 text-dark">Balance</div>
                            <div id="balanceAmountDiv" runat="server" class="text-success fw-bold fs-5">
                                <i class="bi bi-equals"></i>
                                <asp:Label ID="lblBalance" runat="server"></asp:Label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Transactions -->
            <div class="col-md-8 d-flex flex-column px-4 pb-4" style="height: 100%;">
                <h2 class="fw-bold mb-3">Transactions</h2>
                <div class="flex-grow-1 overflow-auto pe-2">
                    <asp:Repeater ID="rptTransactions" runat="server">
                        <ItemTemplate>
                            <div class='transaction-card d-flex align-items-center justify-content-between mb-3 p-3 border rounded shadow-sm bg-white 
                                <%# Eval("TransactionType").ToString() == "Transfer" ? "disabled-transaction" : "clickable-card" %>'
                                <%# Eval("TransactionType").ToString() == "Transfer" ? "" : "onclick=\"openEditModal(this)\"" %>
                                data-id='<%# Eval("TransactionId") %>'
                                data-name='<%# Eval("Name") %>'
                                data-amount='<%# Eval("Amount") %>'
                                data-type='<%# Eval("TransactionType") %>'
                                data-date='<%# Eval("Date", "{0:yyyy-MM-dd}") %>'
                                data-category='<%# Eval("Category") %>'>

                                <div>
                                    <strong><%# Eval("Name") %></strong><br />
                                    <small class="text-muted"><%# Eval("Date", "{0:ddd, MMM d, yyyy}") %></small>
                                </div>

                                <div>
                                    <span class='<%# AmountCss(Eval("Amount")) %>'>
                                        <%# FormatAmount(Eval("Amount")) %>
                                    </span>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </div>


    <!-- Buttons -->
    <div class="fixed-action-buttons d-flex gap-2">
        <button type="button" class="btn ms-lg-3 add-btn" data-bs-toggle="modal" data-bs-target="#moveFundsModal">
            <i class="bi bi-arrow-left-right me-1"></i>Move Funds
        </button>
        <button type="button" class="btn ms-lg-3 add-btn" data-bs-toggle="modal" data-bs-target="#addEntryModal">
            + New Entry
        </button>
    </div>


    <!-- Add New Entry Modal -->
    <div class="modal fade" id="addEntryModal" tabindex="-1" aria-labelledby="addEntryModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-3 shadow">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="addEntryModalLabel">Add New Entry</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:Panel ID="pnlAddEntry" runat="server" DefaultButton="btnSubmitEntry">
                        <asp:HiddenField ID="hdnTransactionType" runat="server" />
                        <!-- Toggle Buttons -->
                        <div class="d-flex gap-2 mb-3">
                            <asp:Button ID="btnShowExpense" runat="server" Text="Expenses" CssClass="btn btn-outline-primary w-50" OnClientClick="toggleEntryForm('expense'); return false;" UseSubmitBehavior="false" />
                            <asp:Button ID="btnShowIncome" runat="server" Text="Income" CssClass="btn btn-outline-secondary w-50" OnClientClick="toggleEntryForm('income'); return false;" UseSubmitBehavior="false" />
                        </div>

                        <!-- Expenses Form -->
                        <div id="expenseForm">
                            <div class="mb-3">
                                <label class="form-label">Name<span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtExpenseName" runat="server" CssClass="form-control" placeholder="e.g. Subway" />
                            </div>

                            <div class="row mb-3">
                                <div class="col">
                                    <label class="form-label">Amount<span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtExpenseAmount" runat="server" CssClass="form-control" TextMode="Number" />
                                </div>
                                <div class="col">
                                    <label class="form-label">Date<span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtExpenseDate" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>

                            <label class="form-label">Jar</label>
                            <div class="form-control mb-3 text-muted" style="background-color: #EAECEF;">
                                <asp:Label ID="lblExpenseJarName" runat="server" />
                            </div>
                        </div>

                        <!-- Income Form -->
                        <div id="incomeForm" style="display: none;">
                            <div class="mb-3">
                                <label class="form-label">Name<span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtIncomeName" runat="server" CssClass="form-control" placeholder="e.g. Monthly Allowance" />
                            </div>

                            <div class="row mb-3">
                                <div class="col">
                                    <label class="form-label">Amount<span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtIncomeAmount" runat="server" CssClass="form-control" TextMode="Number" />
                                </div>
                                <div class="col">
                                    <label class="form-label">Date<span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtIncomeDate" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>

                            <label class="form-label">Jar</label>
                            <div class="form-control mb-3 text-muted" style="background-color: #EAECEF;">
                                <asp:Label ID="lblIncomeJarName" runat="server" />
                            </div>
                        </div>

                        <asp:Button ID="btnSubmitEntry" runat="server" Text="Save" CssClass="btn btn-primary w-100"
                            OnClientClick="return validateAddEntryForm();" OnClick="btnSubmitEntry_Click" />
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
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>

                <div class="modal-body">
                    <asp:Panel ID="pnlEditTxn" runat="server" DefaultButton="btnUpdateTxn">
                        <asp:HiddenField ID="hdnEditTxnId" runat="server" />
                        <asp:HiddenField ID="hdnEditTxnCategory" runat="server" />

                        <div class="mb-3">
                            <label for="txtTxnName" class="form-label fw-semibold">Name<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtTxnName" runat="server" CssClass="form-control" placeholder="Transaction Name" />
                        </div>
                        <div class="mb-3">
                            <label for="txtTxnAmount" class="form-label fw-semibold">Amount<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtTxnAmount" runat="server" CssClass="form-control" placeholder="0.00" />
                        </div>
                        <div class="mb-3">
                            <label for="txtTxnDate" class="form-label fw-semibold">Date<span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtTxnDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>

                        <!-- Update button -->
                        <asp:Button ID="btnUpdateTxn" runat="server" Text="Update"
                            CssClass="btn btn-primary w-100 fw-semibold mb-2"
                            OnClientClick="return validateEditTxn();"
                            OnClick="btnUpdateTxn_Click" />
                    </asp:Panel>

                    <!-- Delete trigger -->
                    <button id="btnTxnDelete" type="button" class="btn btn-danger w-100 fw-semibold" onclick="openTxnDeleteModal(
                document.getElementById('<%= hdnEditTxnId.ClientID %>').value,
                document.getElementById('<%= txtTxnName.ClientID %>').value)">
                        Delete
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- Hidden field to store TransactionId for deletion -->
    <asp:HiddenField ID="hdnDeleteTxnId" runat="server" />

    <!-- Delete Transaction Confirmation Modal -->
    <div class="modal fade" id="deleteTxnConfirmModal" tabindex="-1" aria-labelledby="deleteTxnConfirmLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-4 shadow p-4 text-center">
                <div class="modal-body">
                    <h5 class="fw-bold" id="deleteTxnConfirmLabel">Are you sure you want to delete "<span id="txnNameToDelete"></span>"?
                    </h5>
                    <p class="text-muted mt-2">
                        This transaction will be permanently removed.
                    </p>

                    <div class="d-flex justify-content-center gap-3 mt-4">
                        <button type="button" class="btn btn-danger px-4" onclick="cancelTxnDelete()">Cancel</button>
                        <asp:Button ID="btnConfirmTxnDelete" runat="server" Text="Confirm"
                            CssClass="btn btn-primary px-4" OnClick="btnConfirmTxnDelete_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Move Funds Modal -->
    <div class="modal fade" id="moveFundsModal" tabindex="-1" aria-labelledby="moveFundsLabel" aria-hidden="true">
        <asp:HiddenField ID="hdnCurrentJarBalance" runat="server" />
        <asp:HiddenField ID="hdnCurrentJarId" runat="server" />
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="moveFundsLabel">Move Funds</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">To Jar <span class="text-danger">*</span></label>
                        <asp:DropDownList ID="ddlTargetJar" runat="server" CssClass="form-select" AppendDataBoundItems="true">
                            <asp:ListItem Text="-- Select Jar --" Value="" />
                        </asp:DropDownList>
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Amount <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtMoveAmount" runat="server" CssClass="form-control" TextMode="Number" />
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnMoveFunds" runat="server" CssClass="btn btn-primary"
                        Text="Confirm Move"
                        OnClick="btnMoveFunds_Click"
                        UseSubmitBehavior="true"
                        OnClientClick="return validateAndSwapModal();" />
                </div>
            </div>
        </div>
    </div>



    <!-- Insufficient Funds Modal -->
    <div class="modal fade" id="insufficientFundsModal" tabindex="-1" aria-labelledby="insufficientFundsLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-danger">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title" id="insufficientFundsLabel">Insufficient Funds</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    You don’t have enough balance in this jar to complete the transfer.
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-danger" data-bs-dismiss="modal">Okay</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>

    <script>
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

                // Set icon
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

                // Set default date
                const today = new Date();
                let defaultDate = "";

                switch (value) {
                    case "day":
                        defaultDate = today.toLocaleDateString('en-CA');
                        break;
                    case "week":
                        const current = new Date();
                        const day = current.getDay(); // 0 = Sun, 1 = Mon, ..., 6 = Sat
                        const mondayOffset = (day === 0) ? -6 : 1 - day; // shift Sunday back to previous Monday
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
                    default:
                        defaultDate = "";
                }

                dateField.value = defaultDate;

                updateVisibleInput();
                optionsContainer.style.display = 'none';

                document.getElementById('<%= btnPeriodChange.ClientID %>').click();
            }

            // Toggle dropdown
            selected.addEventListener('click', e => {
                e.stopPropagation();
                const isOpen = optionsContainer.style.display === 'block';
                document.querySelectorAll('.custom-dropdown .options').forEach(opt => opt.style.display = 'none');
                optionsContainer.style.display = isOpen ? 'none' : 'block';
            });

            // Click outside to close
            document.addEventListener('click', e => {
                if (!dropdown.contains(e.target)) {
                    optionsContainer.style.display = 'none';
                }
            });

            // Attach option logic
            optionsList.forEach(option => {
                option.addEventListener('click', () => handleOptionClick(option));
            });

            // Date change logic
            window.handleDateChange = function (input) {
                dateField.value = input.value;
                document.getElementById('<%= btnPeriodChange.ClientID %>').click();
            };

            // Initial state on page load
            updateVisibleInput();
        });

        // ===============================
        // 🔄 General Purpose Validation Reset
        // ===============================
        function resetValidation(container) {
            const elements = container.querySelectorAll('input, select');

            elements.forEach(el => {
                el.classList.remove("is-invalid");

                // Remove the error message if it exists
                const feedback = el.parentNode.querySelector('.invalid-feedback');
                if (feedback) {
                    feedback.remove();
                }
            });
        }


        function showInvalid(input, message) {
            input.classList.add("is-invalid");

            const errorDiv = document.createElement("div");
            errorDiv.className = "invalid-feedback";
            errorDiv.textContent = message;

            // For select (dropdown), append at end of parent
            if (input.tagName === "SELECT") {
                input.parentNode.appendChild(errorDiv);
            } else {
                input.parentNode.insertBefore(errorDiv, input.nextSibling);
            }
        }

        // ===============================
        // 🧾 Entry Form: Toggle, Reset, Validate
        // ===============================
        function toggleEntryForm(type) {
            const incomeForm = document.getElementById('incomeForm');
            const expenseForm = document.getElementById('expenseForm');
            const txnTypeField = document.getElementById('<%= hdnTransactionType.ClientID %>');
            const btnIncome = document.getElementById('<%= btnShowIncome.ClientID %>');
            const btnExpense = document.getElementById('<%= btnShowExpense.ClientID %>');

            if (type === 'income') {
                incomeForm.style.display = 'block';
                expenseForm.style.display = 'none';
                txnTypeField.value = 'Income';
                btnIncome.classList.add('btn-primary');
                btnIncome.classList.remove('btn-outline-secondary');
                btnExpense.classList.add('btn-outline-primary');
                btnExpense.classList.remove('btn-primary');
                resetValidation(incomeForm);
            } else {
                incomeForm.style.display = 'none';
                expenseForm.style.display = 'block';
                txnTypeField.value = 'Expense';
                btnExpense.classList.add('btn-primary');
                btnExpense.classList.remove('btn-outline-primary');
                btnIncome.classList.add('btn-outline-secondary');
                btnIncome.classList.remove('btn-primary');
                resetValidation(expenseForm);
            }
        }

        function resetEntryForm() {
            const today = new Date().toLocaleDateString('en-CA');
            [
         '<%= txtExpenseName.ClientID %>',
         '<%= txtExpenseAmount.ClientID %>',
         '<%= txtExpenseDate.ClientID %>',
         '<%= txtIncomeName.ClientID %>',
         '<%= txtIncomeAmount.ClientID %>',
         '<%= txtIncomeDate.ClientID %>'
            ].forEach(id => {
                const el = document.getElementById(id);
                if (el) el.value = "";
            });

            document.getElementById('<%= txtExpenseDate.ClientID %>').value = today;
            document.getElementById('<%= txtIncomeDate.ClientID %>').value = today;

            resetValidation(document.getElementById('expenseForm'));
            resetValidation(document.getElementById('incomeForm'));

            toggleEntryForm('expense');
        }

        function validateAddEntryForm() {
            const txnType = document.getElementById('<%= hdnTransactionType.ClientID %>').value;
            let nameInput, amountInput, dateInput;
            let isValid = true;

            if (txnType === "Expense") {
                nameInput = document.getElementById('<%= txtExpenseName.ClientID %>');
                amountInput = document.getElementById('<%= txtExpenseAmount.ClientID %>');
                dateInput = document.getElementById('<%= txtExpenseDate.ClientID %>');
            } else {
                nameInput = document.getElementById('<%= txtIncomeName.ClientID %>');
                amountInput = document.getElementById('<%= txtIncomeAmount.ClientID %>');
                dateInput = document.getElementById('<%= txtIncomeDate.ClientID %>');
            }

            resetValidation(nameInput.closest("form") || document);

            if (nameInput.value.trim() === "") {
                showInvalid(nameInput, "Please enter a name.");
                isValid = false;
            }

            const amount = parseFloat(amountInput.value);
            if (isNaN(amount) || amount <= 0) {
                showInvalid(amountInput, "Please enter a valid amount greater than 0.");
                isValid = false;
            }

            if (dateInput.value === "") {
                showInvalid(dateInput, "Please select a date.");
                isValid = false;
            }

            return isValid;
        }

        // ===============================
        // 🧾 Edit Form: Toggle, Reset, Validate
        // ===============================
        function openEditModal(el) {
            const id = el.dataset.id;
            const name = el.dataset.name;
            const amount = el.dataset.amount;
            const date = el.dataset.date;
            const type = el.dataset.type;
            if (type === "Transfer") return;
            const category = el.dataset.category;

            document.getElementById('<%= hdnEditTxnId.ClientID %>').value = id;
            document.getElementById('<%= txtTxnName.ClientID %>').value = name;
            document.getElementById('<%= txtTxnAmount.ClientID %>').value = amount;
            document.getElementById('<%= txtTxnDate.ClientID %>').value = date;
            document.getElementById('<%= hdnEditTxnCategory.ClientID %>').value = category;

            const deleteBtn = document.getElementById("btnTxnDelete");
            const updateBtn = document.getElementById("<%= btnUpdateTxn.ClientID %>");

            if ((type === "Expense" && category === "Transfer Out") ||
                (type === "Income" && category === "Transfer In")) {
                deleteBtn.style.display = "none";
                updateBtn.disabled = true;
                updateBtn.classList.remove("btn-primary");
                updateBtn.classList.add("btn-secondary");
            } else {
                deleteBtn.style.display = "block";
                updateBtn.disabled = false;
                updateBtn.classList.add("btn-primary");
                updateBtn.classList.remove("btn-secondary");
            }


            const modal = new bootstrap.Modal(document.getElementById("editTxnModal"));
            modal.show();
        }


        function validateEditTxn() {
            const name = document.getElementById('<%= txtTxnName.ClientID %>');
            const amount = document.getElementById('<%= txtTxnAmount.ClientID %>');
            const date = document.getElementById('<%= txtTxnDate.ClientID %>');

            resetValidation(document.getElementById('<%= pnlEditTxn.ClientID %>'));

            let isValid = true;

            if (name.value.trim() === "") {
                showInvalid(name, "Please enter a name.");
                isValid = false;
            }

            const amt = parseFloat(amount.value);
            if (isNaN(amt) || amt <= 0) {
                showInvalid(amount, "Please enter a valid amount greater than 0.");
                isValid = false;
            }

            if (date.value === "") {
                showInvalid(date, "Please select a date.");
                isValid = false;
            }

            return isValid;
        }


        // ===============================
        // ❌ Delete Modal Logic
        // ===============================
        function openTxnDeleteModal(txnId, txnName) {
            const editModal = bootstrap.Modal.getInstance(document.getElementById('editTxnModal'));
            if (editModal) {
                editModal.hide();
                editModal.dispose();
            }
            document.getElementById('<%= hdnDeleteTxnId.ClientID %>').value = txnId;
            document.getElementById('txnNameToDelete').textContent = txnName;
            new bootstrap.Modal(document.getElementById('deleteTxnConfirmModal')).show();
        }

        function cancelTxnDelete() {
            bootstrap.Modal.getInstance(document.getElementById('deleteTxnConfirmModal')).hide();
            new bootstrap.Modal(document.getElementById('editTxnModal')).show();
        }

        // ===============================
        // 🔁 Move Funds Modal Validation
        // ===============================
        function validateAndSwapModal() {
            const amountInput = document.getElementById("<%= txtMoveAmount.ClientID %>");
            const currentBalanceHidden = document.getElementById("<%= hdnCurrentJarBalance.ClientID %>");
            const jarSelect = document.getElementById("<%= ddlTargetJar.ClientID %>");

            resetValidation(document.getElementById("moveFundsModal"));

            let isValid = true;

            if (!jarSelect.value || jarSelect.selectedIndex === 0) {
                showInvalid(jarSelect, "Please select a target jar.");
                isValid = false;
            }

            const amount = parseFloat(amountInput.value);
            if (isNaN(amount) || amount <= 0) {
                showInvalid(amountInput, "Please enter a valid amount greater than 0.");
                isValid = false;
            } else {
                const currentBalance = parseFloat(currentBalanceHidden.value);
                if (!isNaN(currentBalance) && amount > currentBalance) {
                    // Show insufficient funds modal and prevent submission
                    var moveModalEl = document.getElementById("moveFundsModal");
                    var moveModal = bootstrap.Modal.getInstance(moveModalEl) || new bootstrap.Modal(moveModalEl);
                    moveModal.hide();

                    var insufficientModalEl = document.getElementById("insufficientFundsModal");
                    var insufficientModal = new bootstrap.Modal(insufficientModalEl);
                    insufficientModal.show();

                    return false; // Cancel submit
                }
            }

            return isValid; // If false, cancels form submit; if true, submits normally
        }

        // Reset inputs and validation on modal open
        // ✅ Robust reset of Move Funds modal on both open and close
        const moveFundsModalEl = document.getElementById('moveFundsModal');
        if (moveFundsModalEl) {
            // Reset validation when opened
            moveFundsModalEl.addEventListener('show.bs.modal', () => {
                resetValidation(moveFundsModalEl);
            });

            // Fully reset fields and validation when closed
            moveFundsModalEl.addEventListener('hidden.bs.modal', () => {
                document.getElementById("<%= txtMoveAmount.ClientID %>").value = '';
                const ddl = document.getElementById("<%= ddlTargetJar.ClientID %>");
                ddl.selectedIndex = 0;
                resetValidation(moveFundsModalEl);
            });
        }

        const addEntryModal = document.getElementById('addEntryModal');
        addEntryModal.addEventListener('show.bs.modal', resetEntryForm);
    </script>
</asp:Content>
