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
      margin-top: 1rem; margin-bottom: 3rem;
    }
    .sidebar .nav-link {
      font-weight: 500; font-size: 1.1rem; padding: 0.75rem 1rem;
      display: flex; align-items: center; gap: 0.75rem;
      color: white !important; opacity: 0.9;
      transition: color .3s ease, background-color .3s ease;
      border-radius: 4px; text-decoration: none;
    }
    .sidebar .nav-link:hover { color: #a9bfff !important; background-color: rgba(169,191,255,.15); }
    .sidebar .nav-link img { height: 20px; width: 20px; object-fit: contain; margin-right: 6px; }

    .mainpage-content {
      margin-left: 220px; max-width: calc(100% - 220px);
      padding: 1.5rem; overflow-x: hidden; overflow-y: auto;
      display: flex; flex-direction: column; gap: 1rem;
    }
    .tools-header { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: .5rem; }
    .section-card { background: #fff; border-radius: 12px; padding: 1.5rem; box-shadow: 0 4px 12px rgba(0,0,0,.05); }
    .action-group { display: flex; gap: .5rem; align-items: center; }
    .btn-restart { border: 1px solid #dc3545; color: #dc3545; background: transparent; }
    .btn-restart:hover { background: rgba(220,53,69,.1); }
  </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <!-- Sidebar -->
  <div class="sidebar bg-dark">
    <h5 class="fw-bold">Budgeting Menu</h5>
    <ul class="nav flex-column mt-4">
      <li class="nav-item"><a class="nav-link" href="Dashboard.aspx"><img src="images/dashboard.png" alt="Dashboard" />Dashboard</a></li>
      <li class="nav-item"><a class="nav-link" href="Jars.aspx"><img src="images/jar.png" alt="Jars" />Jars</a></li>
      <li class="nav-item"><a class="nav-link" href="Goals.aspx"><img src="images/goal.png" alt="Goals" />Goals</a></li>
      <li class="nav-item"><a class="nav-link" href="Tools.aspx"><img src="images/tool.png" alt="Tools" />Tools</a></li>
    </ul>
  </div>

  <!-- Main content -->
  <div class="container-fluid mainpage-content">
    <!-- Header + global action -->
    <div class="tools-header">
      <div class="d-flex align-items-center gap-3 flex-wrap">
        <div><h1 class="fw-bold mb-0">TOOLS</h1></div>
        <div class="d-flex align-items-center gap-3 flex-wrap">
          <div><h3 class="mb-0">Import Bank Statement</h3></div>
          <asp:Panel runat="server" CssClass="mb-0">
            <div class="dropdown">
              <button class="btn btn-outline-secondary dropdown-toggle" type="button" id="downloadDropdown" data-bs-toggle="dropdown" aria-expanded="false">
                <i class="bi bi-download me-2"></i>Download Template
              </button>
              <ul class="dropdown-menu" aria-labelledby="downloadDropdown">
                <li><a class="dropdown-item d-flex align-items-center" href="Content/sample_import.xlsx" download><i class="bi bi-file-earmark-spreadsheet me-2"></i>XLSX</a></li>
                <li><a class="dropdown-item d-flex align-items-center" href="Content/sample_import.csv" download><i class="bi bi-file-earmark-spreadsheet me-2"></i>CSV</a></li>
              </ul>
            </div>
          </asp:Panel>
        </div>
      </div>

      <div class="action-group">
        <button type="button" class="btn btn-restart" data-bs-toggle="modal" data-bs-target="#confirmRestartModal">
          Restart All Jars
        </button>
      </div>
    </div>

    <!-- Upload & Preview card -->
    <div class="section-card">
      <!-- Upload -->
      <asp:Panel ID="pnlUpload" runat="server" CssClass="mb-3 d-flex flex-wrap gap-2 align-items-start">
        <asp:FileUpload ID="fuStatement" runat="server" CssClass="form-control me-2" />
        <asp:Button ID="btnParse" runat="server" Text="Upload & Preview" CssClass="btn btn-primary" OnClick="btnParse_Click" />
      </asp:Panel>

      <!-- Preview -->
      <asp:Panel ID="pnlPreview" runat="server" Visible="false">
        <div class="alert alert-info">Map each row to a Jar (or leave it – the Default Jar will be used).</div>

        <div class="row mb-3">
          <div class="col-md-6">
            <label class="form-label">Default Jar</label>
            <asp:DropDownList ID="ddlDefaultJar" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlDefaultJar_SelectedIndexChanged" />
          </div>
        </div>

        <asp:GridView ID="gvPreview" runat="server" CssClass="table table-striped" AutoGenerateColumns="False" OnRowDataBound="gvPreview_RowDataBound">
          <Columns>
            <asp:TemplateField HeaderText="Import">
              <ItemTemplate><asp:CheckBox ID="chkImport" runat="server" Checked='<%# Eval("Import") %>' /></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:BoundField DataField="Income" HeaderText="Income" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="Expense" HeaderText="Expense" DataFormatString="{0:N2}" />
            <asp:TemplateField HeaderText="Jar">
              <ItemTemplate><asp:DropDownList ID="ddlRowJar" runat="server" CssClass="form-select" /></ItemTemplate>
            </asp:TemplateField>
          </Columns>
        </asp:GridView>

        <div class="mt-3">
          <asp:Button ID="btnImport" runat="server" Text="Import Selected" CssClass="btn btn-success" OnClick="btnImport_Click" />
          <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary ms-2" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>
      </asp:Panel>

      <!-- Done / feedback -->
      <asp:Panel ID="pnlDone" runat="server" Visible="false" CssClass="alert mt-4">
        <asp:Literal ID="litDoneMessage" runat="server" />
        <span>&nbsp;<a href="Jars.aspx">View Jars</a></span>
      </asp:Panel>
    </div>
  </div>

  <!-- Confirm Restart Modal (inside server form via master page) -->
  <div class="modal fade" id="confirmRestartModal" tabindex="-1" aria-labelledby="confirmRestartLabel" aria-hidden="true">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title" id="confirmRestartLabel">Confirm Restart</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="modal-body">
          This will reset all jars (balances, snapshots, progress) for your account. It will not give you back the 6 default jars. 
            <br /> This action cannot be undone. Proceed?
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
