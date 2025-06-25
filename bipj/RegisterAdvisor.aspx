<%@ Page 
    Title="Apply as Advisor" 
    Language="C#" 
    MasterPageFile="~/Customer_Nav.Master" 
    AutoEventWireup="true" 
    CodeBehind="RegisterAdvisor.aspx.cs" 
    Inherits="bipj.RegisterAdvisor" 
%>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
  <style>
    .form-container {
      max-width: 600px;
      margin: 40px auto;
      background: #fff;
      padding: 24px;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    .form-container h2 {
      color: #2e266e;
      margin-bottom: 24px;
      text-align: center;
    }
    .form-group {
      margin-bottom: 16px;
    }
    .form-group label {
      font-weight: bold;
      display: block;
      margin-bottom: 6px;
    }
    .form-group input,
    .form-group select,
    .form-group textarea {
      width: 100%;
      padding: 8px;
      border: 1px solid #ccc;
      border-radius: 4px;
      box-sizing: border-box;
    }
    .btn-submit {
      display: block;
      width: auto;
      margin: 20px auto 0;
      background: #433e8e;
      color: #fff;
      border: none;
      padding: 10px 24px;
      border-radius: 6px;
      cursor: pointer;
      font-size: 1rem;
    }
    .btn-submit:hover {
      background: #2e266e;
    }
    .message {
      text-align: center;
      margin-top: 16px;
      font-weight: bold;
      color: green;
    }
  </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <div class="form-container">
    <h2>Apply as Financial Advisor</h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="message" />

    <div class="form-group">
      <label for="txtName">Full Name</label>
      <asp:TextBox ID="txtName" runat="server" />
    </div>

    <div class="form-group">
      <label for="txtEmail">Email Address</label>
      <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" />
    </div>

    <div class="form-group">
      <label for="ddlCategory">Primary Domain</label>
      <asp:DropDownList ID="ddlCategory" runat="server">
        <asp:ListItem>Investment</asp:ListItem>
        <asp:ListItem>Retirement Fund</asp:ListItem>
        <asp:ListItem>Budgeting</asp:ListItem>
        <asp:ListItem>Insurance</asp:ListItem>
      </asp:DropDownList>
    </div>

    <div class="form-group">
      <label>Specialties (up to 3)</label>
      <asp:TextBox ID="txtSpec1" runat="server" Placeholder="Specialty #1" /><br /><br />
      <asp:TextBox ID="txtSpec2" runat="server" Placeholder="Specialty #2" /><br /><br />
      <asp:TextBox ID="txtSpec3" runat="server" Placeholder="Specialty #3" />
    </div>

    <div class="form-group">
      <label for="txtBio">Short Bio (max 30 words)</label>
      <asp:TextBox ID="txtBio" runat="server" TextMode="MultiLine" Rows="3" />
    </div>

    <div class="form-group">
      <label for="fuPhoto">Profile Photo</label>
      <asp:FileUpload ID="fuPhoto" runat="server" />
    </div>

    <asp:Button ID="btnSubmit" runat="server" CssClass="btn-submit"
      Text="Submit Application" OnClick="btnSubmit_Click" />
  </div>
</asp:Content>
