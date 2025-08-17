<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="Booking2.aspx.cs" Inherits="bipj.Booking2" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
  <style>
    /* Fix for navbar overlap */
    #ContentPlaceHolder1 {
      margin-top: 200px; /* Adjust this value based on your navbar height */
      padding-top: 20px;
    }

    .content-wrapper {
      display: flex;
      align-items: flex-start;
      margin-top: 20px;
    }

    .card-grid {
      display: grid;
      grid-template-columns: repeat(4, 360px);
      grid-auto-rows: 500px;
      gap: 24px;
      padding: 24px;
      justify-content: center;
    }

    body {
  padding-top: 120px !important; 
}

    .card {
      width: 360px;
      height: 500px;
      display: flex;
      flex-direction: column;
      background: #fff;
      border-radius: 12px;
      box-shadow: 0 6px 16px rgba(0,0,0,0.08);
      overflow: hidden;
      transition: transform .2s;
    }

    .card:hover { transform: translateY(-4px); }

    .card-header {
      height: 140px;
      background: #5e4b8b;
      padding: 24px;
      text-align: center;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .card-header img {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      border: 4px solid #fff;
      object-fit: cover;
    }

    .card-body {
      flex: 1;
      padding: 20px;
      overflow: visible;
    }

    .card-body h4 {
      margin: 0 0 4px;
      color: #3b3350;
      font-size: 20px;
    }

    .card-body .category {
      font-size: 14px;
      color: #777;
      margin-bottom: 12px;
    }

    .card-body .bio,
    .card-body .specialties {
      margin-bottom: 16px;
    }

    .card-body .bio h5,
    .card-body .specialties h5 {
      margin: 0 0 6px;
      font-size: 14px;
      color: #5e4b8b;
      text-transform: uppercase;
      letter-spacing: .5px;
    }

    .card-body .bio p {
      margin: 0;
      font-size: 14px;
      color: #555;
      line-height: 1.4;
    }

    .card-body .specialties ul {
      list-style: disc inside;
      padding-left: 18px;
      margin: 0;
      color: #555;
      font-size: 14px;
    }

    .card-body .rating i {
      color: #f5a623;
      margin-right: 2px;
    }

    .card-footer {
      display: flex;
      gap: 8px;
      padding: 16px 20px 24px;
      background: #fafafa;
      margin-top: auto;
    }

    .card-footer .btn-select {
      flex: 1;
      padding: 10px 0;
      border: none;
      border-radius: 6px;
      font-size: 14px;
      cursor: pointer;
      background: #5e4b8b;
      color: #fff;
      transition: background .2s;
    }

    .card-footer .btn-select:hover {
      background: #473871;
    }

    .wizard {
      display: flex;
      justify-content: center;
      gap: 20px;
      margin: 30px 0;
    }

    .wizard .step {
      width: 40px; height: 40px; line-height: 40px;
      border-radius: 50%; background: #ccc;
      text-align: center; color: #fff; font-weight: bold;
    }

    .wizard .step.completed,
    .wizard .step.active {
      background: #5e4b8b;
      border: 3px solid #d3c8ff;
    }

    .advisor-search {
      display: flex;
      gap: 10px;
      margin-bottom: 10px;
      justify-content: center;
    }

    .advisor-search input {
      padding: 8px;
      width: 250px;
      border-radius: 6px;
      border: 1px solid #ccc;
      font-size: 14px;
    }

    .advisor-search .filter-btn {
      background: #5e4b8b;
      color: #fff;
      border: none;
      border-radius: 6px;
      padding: 8px 12px;
      cursor: pointer;
      font-size: 14px;
    }

    /* OVERLAY FILTER STYLES */
    .filter-overlay {
      display: none;
      position: fixed;
      top: 0; left: 0;
      width: 100%; height: 100%;
      background-color: rgba(0, 0, 0, 0.5);
      z-index: 1000;
      justify-content: center;
      align-items: center;
    }

    .filter-content {
      background: #fff;
      padding: 30px;
      border-radius: 10px;
      width: 400px;
      box-shadow: 0 6px 20px rgba(0, 0, 0, 0.3);
    }

    .filter-content h3 {
      margin-top: 0;
      color: #3b3350;
    }

    .filter-content .filter-item {
      margin-bottom: 20px;
    }

    /* Alternative fix - if the above doesn't work, try this instead */
    /*
    body {
      padding-top: 80px; 
    }
    */
  </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <h2 style="text-align:center; color:#3b3350; margin-top:24px;">Step 2 — Select Your Advisor</h2>

  <!-- Wizard -->
  <div class="wizard">
    <div class="step completed">1</div>
    <div class="step active">2</div>
    <div class="step">3</div>
    <div class="step">4</div>
    <div class="step">5</div>
  </div>

  <!-- Search Bar + Overlay Filter Button -->
  <div class="advisor-search">
    <asp:TextBox ID="txtSearch" runat="server" Placeholder="Search by name or category…" />
    <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
    <button type="button" class="filter-btn" onclick="toggleOverlay()">
      <i class="fas fa-filter"></i> Filter
    </button>
  </div>

  <!-- Filter Overlay -->
  <div id="filterOverlay" class="filter-overlay">
    <div class="filter-content">
      <h3>Filter Advisors</h3>

      <div class="filter-item">
        <label for="ddlMinRating">Minimum Star Rating:</label>
        <asp:DropDownList ID="ddlMinRating" runat="server">
          <asp:ListItem Text="All" Value="0" />
          <asp:ListItem Text="1 Star & up" Value="1" />
          <asp:ListItem Text="2 Stars & up" Value="2" />
          <asp:ListItem Text="3 Stars & up" Value="3" />
          <asp:ListItem Text="4 Stars & up" Value="4" />
          <asp:ListItem Text="5 Stars only" Value="5" />
        </asp:DropDownList>
      </div>

      <asp:Button ID="btnOverlayApply" runat="server" Text="Apply Filter" OnClick="FilterChanged" CssClass="filter-btn" />
      <button type="button" onclick="toggleOverlay()" class="filter-btn" style="background:#ccc; color:#333">Cancel</button>
    </div>
  </div>

  <!-- Advisor Card Grid -->
  <div class="card-grid">
    <asp:Repeater ID="rptAdvisors" runat="server">
      <ItemTemplate>
        <div class="card">
          <div class="card-header">
            <img src='<%# Eval("PhotoPath") %>' alt='<%# Eval("Name") %>' />
          </div>
          <div class="card-body">
            <h4><%# Eval("Name") %></h4>
            <div class="category"><%# Eval("Category") %></div>
            <div class="bio">
              <h5>Bio</h5>
              <p><%# Eval("Bio") %></p>
            </div>
            <div class="rating">
              <%# GenerateStars(Convert.ToDecimal(Eval("Rating"))) %>
            </div>
            <div class="specialties">
              <h5>Specialties</h5>
              <ul>
                <%# GetSpecialtiesList(Eval("Specialty1"), Eval("Specialty2"), Eval("Specialty3")) %>
              </ul>
            </div>
          </div>
          <div class="card-footer">
            <asp:Button
              ID="btnSelect"
              runat="server"
              CssClass="btn-select"
              Text="Select"
              CommandArgument='<%# Eval("AdvisorId") %>'
              OnClick="btnSelectAdvisor_Click" />
          </div>
        </div>
      </ItemTemplate>
    </asp:Repeater>
  </div>

  <script>
      function toggleOverlay() {
          const overlay = document.getElementById("filterOverlay");
          overlay.style.display = (overlay.style.display === "flex") ? "none" : "flex";
          overlay.style.display === "flex" && (overlay.style.alignItems = "center");
      }
  </script>
</asp:Content>