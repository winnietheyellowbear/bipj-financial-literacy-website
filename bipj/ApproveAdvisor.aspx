<%@ Page Title="" Language="C#" MasterPageFile="~/Staff_Nav.Master" 
    AutoEventWireup="true" CodeBehind="ApproveAdvisor.aspx.cs" 
    Inherits="bipj.ApproveAdvisor" %>


<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
  <!-- Font Awesome for stars -->
  <link 
    rel="stylesheet" 
    href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
  <style>
    /* FLEX LAYOUT */
    .content-wrapper {
      display: flex;
      align-items: flex-start;
      margin-top: 20px;
    }
    .sidebar {
      background: #333;
      color: #fff;
      width: 200px;
      padding: 20px;
      flex: 0 0 200px;
    }
    .sidebar ul {
      list-style: none;
      margin: 0;
      padding: 0;
    }
    .sidebar li {
      margin-bottom: 12px;
    }
    .sidebar li a {
      color: #fff;
      text-decoration: none;
      display: block;
      padding: 8px;
      border-radius: 4px;
      transition: background .2s;
    }
    .sidebar li.active a,
    .sidebar li a:hover {
      background: #575757;
    }

    .main-content {
      flex: 1;
      padding: 0 24px;
    }

    /* GRID CONTAINER: always 4 columns @360px each */
    .card-grid {
      display: grid;
      grid-template-columns: repeat(4, 360px);
      grid-auto-rows: 500px;
      gap: 24px;
      padding: 24px;
      justify-content: center;
    }

    /* CARD WRAPPER */
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
    .card:hover {
      transform: translateY(-4px);
    }

    /* HEADER WITH AVATAR */
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

    /* BODY SECTIONS */
    .card-body {
      flex: 1;
      padding: 20px;
      overflow: visible;  /* remove scrollbars */
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

    /* STAR RATING */
    .card-body .rating i {
      color: #f5a623;
      margin-right: 2px;
    }

    /* ACTION FOOTER */
    .card-footer {
      display: flex;
      gap: 8px;
      padding: 16px 20px 24px;
      background: #fafafa;
    }
    .card-footer .btn {
      flex: 1;
      padding: 10px 0;
      border: none;
      border-radius: 6px;
      font-size: 14px;
      cursor: pointer;
      transition: background .2s;
    }
    .btn-approve { background: #28a745; color: #fff; }
    .btn-delete  { background: #dc3545; color: #fff; }
    .btn-view    { background: #6c757d; color: #fff; }
    .btn-approve:hover { background: #218838; }
    .btn-delete:hover  { background: #c82333; }
    .btn-view:hover    { background: #5a6268; }
  </style>
</asp:Content>


<asp:Content ID="BodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
        <div class="content-wrapper">
            <!-- ◀︎ SIDEBAR NAV ▶︎ -->
            <div class="sidebar">
              <ul>
                <li class="active">
                  <a href="ApproveAdvisor.aspx">Approve Advisors</a>
                </li>
                <li>
                  <a href="ViewAdvisor.aspx">View Advisors</a>
                </li>
              </ul>
            </div>
        <!-- ▶︎ MAIN CONTENT ▶︎ -->
         <div class="main-content">
          <h2 style="text-align:center; color:#3b3350; margin-top:24px;">
            Pending Advisor Approvals
          </h2>

          <div class="card-grid">
            <asp:Repeater ID="rptPending" runat="server">
              <ItemTemplate>
                <div class="card">
                  <!-- Header / Avatar -->
                  <div class="card-header">
                    <img src='<%# Eval("PhotoPath") ?? "~/images/default-avatar.png" %>' 
                         alt='<%# Eval("Name") %>' />
                  </div>

                  <!-- Body -->
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
                        <%# GetSpecialtiesList(
                              Eval("Specialty1"), 
                              Eval("Specialty2"), 
                              Eval("Specialty3")) %>
                      </ul>
                    </div>
                  </div>

                  <!-- Footer / Actions -->
                  <div class="card-footer">
                    <asp:Button 
                        ID="btnApprove" runat="server" CssClass="btn btn-approve"
                        Text="Approve" CommandArgument='<%# Eval("AdvisorId") %>' 
                        OnClick="btnApprove_Click" />

                    <asp:Button 
                        ID="btnDelete" runat="server" CssClass="btn btn-delete"
                        Text="Delete" CommandArgument='<%# Eval("AdvisorId") %>' 
                        OnClick="btnDelete_Click" />

                    <asp:Button 
                        ID="btnView" runat="server" CssClass="btn btn-view"
                        Text="View" CommandArgument='<%# Eval("AdvisorId") %>' 
                        OnClick="btnView_Click" />
                  </div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
        </div>  <!-- .main-content -->
        </div>    <!-- .content-wrapper -->
</asp:Content>
