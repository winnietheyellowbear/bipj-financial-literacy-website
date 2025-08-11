<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="Tools.aspx.cs" Inherits="bipj.Tools" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
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

        .mainpage-content {
            margin-left: 220px;
            max-width: calc(100% - 220px);
            padding: 1.5rem;
        }


        .tools-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 0.5rem;
        }

        .section-card {
            background: #fff;
            border-radius: 12px;
            padding: 1.5rem;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
        }

        .action-group {
            display: flex;
            gap: 0.5rem;
            align-items: center;
        }

        .btn-restart {
            border: 1px solid #dc3545;
            color: #dc3545;
            background: transparent;
            transition: background-color 0.2s, color 0.2s;
        }

            .btn-restart:hover {
                background: rgba(220, 53, 69, 0.1);
            }
    </style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <!-- Sidebar -->
    <div class="sidebar bg-dark">
        <h5 class="fw-bold">Budgeting Menu</h5>
        <ul class="nav flex-column mt-4">
            <li class="nav-item"><a class="nav-link" href="Dashboard.aspx">
                <img src="images/dashboard.png" alt="Dashboard" />Dashboard</a></li>
            <li class="nav-item"><a class="nav-link" href="Jars.aspx">
                <img src="images/jar.png" alt="Jars" />Jars</a></li>
            <li class="nav-item"><a class="nav-link" href="Goals.aspx">
                <img src="images/goal.png" alt="Goals" />Goals</a></li>
            <li class="nav-item"><a class="nav-link" href="Tools.aspx">
                <img src="images/tool.png" alt="Tools" />Tools</a></li>
        </ul>
    </div>

    <!-- Main content -->
    <div class="container-fluid mainpage-content d-flex flex-column">

        <!-- Page header -->
        <div class="d-flex justify-content-between align-items-center px-4 py-1 mb-2">
            <h1 class="fw-bold mb-0">TOOLS</h1>
            <button type="button" class="btn btn-outline-danger btn-sm" data-bs-toggle="modal" data-bs-target="#confirmRestartModal">
                Restart All Jars
            </button>
        </div>

        <!-- Import card -->
        <div class="card shadow-sm border-0 mb-3">
            <div class="card-header bg-white border-0 border-bottom d-flex align-items-center justify-content-between">
                <h3 class="h5 fw-semibold mb-0">Import Bank Statement</h3>

                <!-- Download Template -->
                <asp:Panel runat="server" CssClass="mb-0">
                    <div class="dropdown">
                        <button class="btn btn-outline-secondary btn-sm dropdown-toggle" type="button" id="downloadDropdown" data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="bi bi-download me-2"></i>Download Template
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="downloadDropdown">
                            <li>
                                <a class="dropdown-item d-flex align-items-center" href="Content/sample_import.xlsx" download>
                                    <i class="bi bi-file-earmark-spreadsheet me-2"></i>XLSX
                                </a>
                            </li>
                            <li>
                                <a class="dropdown-item d-flex align-items-center" href="Content/sample_import.csv" download>
                                    <i class="bi bi-file-earmark-spreadsheet me-2"></i>CSV
                                </a>
                            </li>
                        </ul>
                    </div>
                </asp:Panel>
            </div>

            <div class="card-body">
                <!-- Upload -->
                <asp:Panel ID="pnlUpload" runat="server" CssClass="d-flex flex-wrap align-items-center gap-2 mb-0">
                    <asp:FileUpload ID="fuStatement" runat="server" CssClass="form-control flex-grow-1" />
                    <asp:Button ID="btnParse" runat="server" Text="Upload &amp; Preview" CssClass="btn btn-primary" OnClick="btnParse_Click" />
                </asp:Panel>

                <!-- Done / feedback -->
                <asp:Panel ID="pnlDone" runat="server" Visible="false" CssClass="alert alert-success mt-3 mb-0">
                    <asp:Literal ID="litDoneMessage" runat="server" />
                    <span>&nbsp;<a href="Jars.aspx">View Jars</a></span>
                </asp:Panel>


                <!-- Preview card -->
                <asp:Panel ID="pnlPreview" runat="server" Visible="false">
                    <div class="card-header bg-white border-0">
                        <h3 class="h5 fw-semibold mb-0">Preview</h3>
                    </div>
                    <div class="card-body">
                        <div class="alert alert-info mb-3">
                            Map each row to a Jar (or leave it – the Chosen Jar will be used).
                        </div>

                        <div class="row mb-3">
                            <div class="col-md-6 col-lg-4">
                                <label class="form-label">Chosen Jar</label>
                                <asp:DropDownList ID="ddlDefaultJar" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlDefaultJar_SelectedIndexChanged" />
                            </div>
                        </div>

                        <div class="table-responsive">
                            <asp:GridView ID="gvPreview" runat="server"
                                CssClass="table table-striped table-hover align-middle mb-0"
                                AutoGenerateColumns="False"
                                OnRowDataBound="gvPreview_RowDataBound">
                                <Columns>
                                    <asp:TemplateField HeaderText="Import">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkImport" runat="server" Checked='<%# Eval("Import") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" />
                                    <asp:BoundField DataField="Description" HeaderText="Description" />
                                    <asp:BoundField DataField="Income" HeaderText="Income" DataFormatString="{0:N2}" />
                                    <asp:BoundField DataField="Expense" HeaderText="Expense" DataFormatString="{0:N2}" />
                                    <asp:TemplateField HeaderText="Jar">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlRowJar" runat="server" CssClass="form-select" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>

                        <div class="mt-3">
                            <asp:Button ID="btnImport" runat="server" Text="Import Selected" CssClass="btn btn-success" OnClick="btnImport_Click" />
                            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary ms-2" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>

    <!-- Confirm Restart Modal -->
    <div class="modal fade" id="confirmRestartModal" tabindex="-1" aria-labelledby="confirmRestartLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="confirmRestartLabel">Confirm Restart</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    This will reset all jars (balances, snapshots, progress) for your account. It will not give you back the 6 default jars.
        <br />
                    This action cannot be undone. Proceed?
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnConfirmRestart" runat="server"
                        CssClass="btn btn-danger"
                        Text="Yes, Restart All"
                        OnClick="btnRestartAllJars_Click"
                        CausesValidation="false"
                        UseSubmitBehavior="false"
                        OnClientClick="this.disabled=true; this.value='Restarting…';" />
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    <script>
</script>
</asp:Content>
