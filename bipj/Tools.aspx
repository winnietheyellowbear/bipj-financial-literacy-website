<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="Tools.aspx.cs" Inherits="bipj.Tools" %>

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

    <div class="container-fluid mainpage-content d-flex flex-column">
        <div class="d-flex justify-content-between align-items-center px-4 py-1 mb-2">
            <h1 class="fw-bold">TOOLS</h1>
        </div>
        <div class="d-flex justify-content-between">
            <h3>Import Bank Statement</h3>
            <asp:Panel runat="server" CssClass="mb-4">
                <div class="dropdown">
                    <button class="btn btn-outline-secondary dropdown-toggle" type="button" id="downloadDropdown" data-bs-toggle="dropdown" aria-expanded="false">
                        <i class="bi bi-download me-2"></i>Download Template
                    </button>
                    <ul class="dropdown-menu" aria-labelledby="downloadDropdown">
                        <li>
                            <a class="dropdown-item" href="Content/sample_import.xlsx" download>
                                <i class="bi bi-file-earmark-spreadsheet"></i>XLSX
                            </a>
                        </li>
                        <li>
                            <a class="dropdown-item" href="Content/sample_import.csv" download>
                                <i class="bi bi-file-earmark-spreadsheet"></i>CSV
                            </a>
                        </li>
                    </ul>
                </div>
            </asp:Panel>
        </div>
        <!-- UPLOAD & PREVIEW PANEL -->
        <asp:Panel ID="pnlUpload" runat="server" CssClass="mb-4">
            <asp:FileUpload ID="fuStatement" runat="server" CssClass="form-control mb-2" />
            <asp:Button ID="btnParse" runat="server" Text="Upload & Preview"
                CssClass="btn btn-primary" OnClick="btnParse_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlPreview" runat="server" Visible="false">
            <div class="alert alert-info">
                Map each row to a Jar (or leave it – the Default Jar will be used).
            </div>
            <div class="mb-3">
                <label class="form-label">Default Jar</label>
                <asp:DropDownList ID="ddlDefaultJar" runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlDefaultJar_SelectedIndexChanged" />
            </div>

            <asp:GridView ID="gvPreview" runat="server"
                CssClass="table table-striped"
                AutoGenerateColumns="False"
                OnRowDataBound="gvPreview_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="Import">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkImport" runat="server" Checked='<%# Eval("Import") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Date" HeaderText="Date"
                        DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="Description" HeaderText="Description" />
                    <asp:BoundField DataField="Income" HeaderText="Income"
                        DataFormatString="{0:N2}" />
                    <asp:BoundField DataField="Expense" HeaderText="Expense"
                        DataFormatString="{0:N2}" />
                    <asp:TemplateField HeaderText="Jar">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlRowJar" runat="server"
                                CssClass="form-select" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <asp:Button ID="btnImport" runat="server" Text="Import Selected"
                CssClass="btn btn-success mt-3"
                OnClick="btnImport_Click" />
            <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                CssClass="btn btn-secondary ms-2 mt-3"
                CausesValidation="false"
                OnClick="btnCancel_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlDone" runat="server" Visible="false"
            CssClass="alert alert-success mt-4">
            Import complete! <a href="Jars.aspx">View Jars</a>
        </asp:Panel>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>
