<%@ Page Title="" Language="C#" MasterPageFile="~/Staff_Nav.Master"
    AutoEventWireup="true" CodeBehind="ViewAdvisor.aspx.cs"
    Inherits="bipj.ViewAdvisor" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
  <!-- Font Awesome for star icons -->
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
      margin: 0; padding: 0;
    }
    .sidebar li {
      margin-bottom: 12px;
    }
    .sidebar a {
      color: #fff;
      text-decoration: none;
      display: block;
      padding: 8px;
      border-radius: 4px;
      transition: background .2s;
    }
    .sidebar li.active a,
    .sidebar a:hover {
      background: #575757;
    }

    .main-content {
      flex: 1;
      padding: 0 24px;
    }

    /* FILTERS BAR */
    .filters {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
      align-items: center;
    }
    .filters select {
      padding: 6px 12px;
      border-radius: 4px;
      border: 1px solid #ccc;
    }

    /* TABLE STYLING */
    .advisor-table {
      width: 100%;
      border-collapse: collapse;
      background: #fff;
      box-shadow: 0 2px 6px rgba(0,0,0,0.1);
    }
    .advisor-table th,
    .advisor-table td {
      padding: 12px 16px;
      border-bottom: 1px solid #eaeaea;
      text-align: left;
      vertical-align: middle;
    }
    .advisor-table th {
      background: #f8f9fa;
    }

    /* ACTION BUTTONS */
    .btn-view {
      background: #007bff;
      color: #fff;
      border: none;
      padding: 6px 12px;
      border-radius: 4px;
      cursor: pointer;
    }
    .btn-delete {
      background: #dc3545;
      color: #fff;
      border: none;
      padding: 6px 12px;
      border-radius: 4px;
      cursor: pointer;
    }
  </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <div class="content-wrapper">
    <!-- SIDEBAR NAV -->
    <div class="sidebar">
      <ul>
        <li><a href="ApproveAdvisor.aspx">Approve Advisors</a></li>
        <li class="active"><a href="ViewAdvisor.aspx">View Advisors</a></li>
      </ul>
    </div>

    <!-- MAIN CONTENT -->
    <div class="main-content">
      <h2>All Advisors</h2>

      <!-- FILTERS -->
      <div class="filters">
        <label>Category:
          <asp:DropDownList
            ID="ddlCategory"
            runat="server"
            AutoPostBack="true"
            OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Text="All Categories" Value="" />
          </asp:DropDownList>
        </label>

        <label>Rating:
          <asp:DropDownList
            ID="ddlRating"
            runat="server"
            AutoPostBack="true"
            OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Text="All Ratings" Value="" />
            <asp:ListItem Text="Below 3" Value="Below3" />
            <asp:ListItem Text="3 Stars" Value="3" />
            <asp:ListItem Text="4 Stars" Value="4" />
            <asp:ListItem Text="5 Stars" Value="5" />
          </asp:DropDownList>
        </label>
      </div>

      <!-- ADVISORS TABLE -->
      <table class="advisor-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Category</th>
            <th>Rating</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <asp:Repeater ID="rptAll" runat="server">
            <ItemTemplate>
              <tr>
                <td><%# Eval("Name") %></td>
                <td><%# Eval("Category") %></td>
                <td>
                  <%# GenerateStars(Convert.ToDecimal(Eval("Rating"))) %>
                </td>
                <td>
                  <asp:Button
                    ID="btnView2"
                    runat="server"
                    CssClass="btn-view"
                    Text="View"
                    CommandArgument='<%# Eval("AdvisorId") %>'
                    OnClick="btnView_Click" />

                  <asp:Button
                    ID="btnDelete2"
                    runat="server"
                    CssClass="btn-delete"
                    Text="Delete"
                    CommandArgument='<%# Eval("AdvisorId") %>'
                    OnClientClick="return confirm('Are you sure you want to delete this advisor?');"
                    OnClick="btnDelete_Click" />
                </td>
              </tr>
            </ItemTemplate>
          </asp:Repeater>
        </tbody>
      </table>
    </div>
  </div>
</asp:Content>
