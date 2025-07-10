<%@ Page Title="Book a Session – Step 4" 
    Language="C#" 
    MasterPageFile="~/Customer_Nav.Master" 
    AutoEventWireup="true" 
    CodeBehind="Booking4.aspx.cs" 
    Inherits="bipj.Booking4"
     UnobtrusiveValidationMode="None" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
  <style>
    /* reuse your wizard & panel styles */
    .wizard { text-align:center; margin:40px 0; }
    .wizard .step { display:inline-block; width:40px; height:40px; line-height:40px; border-radius:50%; background:#eee; color:#777; margin:0 10px; font-weight:bold; position:relative; }
    .wizard .step.completed,
    .wizard .step.active { background:#433e8e; color:#fff; }
    .wizard .step:after { content:''; position:absolute; top:50%; right:-30px; width:60px; height:4px; background:#eee; transform:translateY(-50%); z-index:-1; }
    .wizard .step.completed:after,
    .wizard .step.active:after { background:#433e8e; }
    .wizard .step:last-child:after { display:none; }

    .step-panel { display:none; background:#f5f3ff; border-radius:12px; padding:32px; margin:0 auto 40px; max-width:600px; }
    .step-panel.active { display:block; }

    .field-group { margin-bottom:16px; }
    .field-group label { display:block; margin-bottom:4px; font-weight:bold; }
    .field-group input, .field-group textarea {
      width:100%; padding:8px; border:1px solid #ccc; border-radius:6px;
      font-size:1rem;
    }

    .btn-prev, .btn-next {
      background:#433e8e; color:#fff; border:none; border-radius:6px;
      padding:10px 24px; cursor:pointer; font-size:1rem; margin-top:20px;
    }
    .btn-prev { background:#aaa; margin-right:10px; }
  </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <h2 style="text-align:center;color:#3b3350;margin-top:24px;">
    Book a Session — Step 4: Additional Information
  </h2>

  <div class="wizard">
    <div class="step completed" data-step="1">1</div>
    <div class="step completed" data-step="2">2</div>
    <div class="step completed" data-step="3">3</div>
    <div class="step active" data-step="4">4</div>
    <div class="step" data-step="5">5</div>
  </div>

  <div id="step4" class="step-panel active">
    <asp:ValidationSummary ID="vs" runat="server" CssClass="text-danger" />

    <div class="field-group">
      <label for="txtName">Name:</label>
      <asp:TextBox ID="txtName" runat="server" />
      <asp:RequiredFieldValidator ID="rfvName" runat="server"
          ControlToValidate="txtName" ErrorMessage="Name is required"
          Display="Dynamic" CssClass="text-danger" />
    </div>

    <div class="field-group">
      <label for="txtEmail">Email:</label>
      <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" />
      <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
          ControlToValidate="txtEmail" ErrorMessage="Email is required"
          Display="Dynamic" CssClass="text-danger" />
      <asp:RegularExpressionValidator ID="revEmail" runat="server"
          ControlToValidate="txtEmail"
          ValidationExpression="\S+@\S+\.\S+"
          ErrorMessage="Invalid email"
          Display="Dynamic" CssClass="text-danger" />
    </div>

    <div class="field-group">
      <label for="txtFocus">What would you like to focus on?</label>
      <asp:TextBox ID="txtFocus" runat="server" TextMode="MultiLine" Rows="4" />
    </div>

    <asp:Button ID="btnBack" runat="server" CssClass="btn-prev"
        Text="Back" OnClick="btnBack_Click" />
    <asp:Button ID="btnNext" runat="server" CssClass="btn-next"
        Text="Next" OnClick="btnNext_Click" />
  </div>
</asp:Content>

