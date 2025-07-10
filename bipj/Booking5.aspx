<%@ Page Title="Book a Session – Step 5" 
    Language="C#" 
    MasterPageFile="~/Customer_Nav.Master" 
    AutoEventWireup="true" 
    CodeBehind="Booking5.aspx.cs" 
    Inherits="bipj.Booking5" %>

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

    dl.summary { width:80%; margin:0 auto; }
    dl.summary dt { font-weight:bold; margin-top:12px; }
    dl.summary dd { margin:4px 0 12px; }
    .btn-prev, .btn-confirm {
      background:#433e8e; color:#fff; border:none; border-radius:6px;
      padding:10px 24px; cursor:pointer; font-size:1rem; margin-top:20px;
    }
    .btn-prev { background:#aaa; margin-right:10px; }
  </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <h2 style="text-align:center;color:#3b3350;margin-top:24px;">
    Book a Session — Step 5: Confirmation
  </h2>

  <div class="wizard">
    <div class="step completed" data-step="1">1</div>
    <div class="step completed" data-step="2">2</div>
    <div class="step completed" data-step="3">3</div>
    <div class="step completed" data-step="4">4</div>
    <div class="step active"    data-step="5">5</div>
  </div>

  <div id="step5" class="step-panel active">
    <dl class="summary">
      <dt>Session Type:</dt>      <dd><asp:Literal ID="litType" runat="server" /></dd>
      <dt>Advisor:</dt>           <dd><asp:Literal ID="litAdvisor" runat="server" /></dd>
      <dt>Date &amp; Time:</dt>   <dd><asp:Literal ID="litDateTime" runat="server" /></dd>
      <dt>Name:</dt>             <dd><asp:Literal ID="litName" runat="server" /></dd>
      <dt>Email:</dt>            <dd><asp:Literal ID="litEmail" runat="server" /></dd>
      <dt>Focus:</dt>            <dd><asp:Literal ID="litFocus" runat="server" /></dd>
    </dl>

    <asp:Button ID="btnBack" runat="server" CssClass="btn-prev"
        Text="Back" OnClick="btnBack_Click" />

    <asp:Button ID="btnConfirm" runat="server" CssClass="btn-confirm"
        Text="Confirm Booking" OnClick="btnConfirm_Click" />
  </div>
</asp:Content>
