<%@ Page 
    Title="Book a Session – Step 1" 
    Language="C#" 
    MasterPageFile="~/Customer_Nav.Master" 
    AutoEventWireup="true" 
    CodeBehind="Booking1.aspx.cs" 
    Inherits="bipj.Booking1" 
%>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
  <style>
  /* STEP INDICATOR (Wizard) */
  .wizard {
    display: flex;
    justify-content: center;
    margin: 30px 0;
    gap: 20px;
  }

  .wizard .step {
    width: 40px;
    height: 40px;
    line-height: 40px;
    border-radius: 50%;
    background-color: #ccc;
    text-align: center;
    color: #fff;
    font-weight: bold;
    font-size: 18px;
  }

  .wizard .step.active {
    background-color: #5e4b8b;
    border: 3px solid #d3c8ff;
    color: white;
  }

  /* CARD GRID */
  .card-grid {
    display: flex;
    justify-content: center;
    gap: 40px;
    margin: 40px auto;
    max-width: 900px;
    flex-wrap: wrap;
  }

  .card {
    background: #ffffff;
    border-radius: 10px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    padding: 30px 20px;
    text-align: center;
    width: 250px;
    transition: transform 0.3s ease;
  }

  .card:hover {
    transform: translateY(-5px);
  }

  .card i {
    font-size: 48px;
    margin-bottom: 15px;
    color: #3b3350;
  }

  .card h4 {
    font-size: 20px;
    margin-bottom: 10px;
    color: #3b3350;
  }

  .card p {
    font-size: 14px;
    color: #555;
    margin-bottom: 20px;
  }

  /* BUTTON STYLE */
  .btn-next {
    background-color: #5e4b8b;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 6px;
    font-size: 14px;
    cursor: pointer;
    transition: background-color 0.3s ease;
  }

  .btn-next:hover {
    background-color: #473871;
  }
</style>

</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <h2 style="text-align:center; margin-top:24px; color:#3b3350;">
    Step 1 — Choose Your Session Type
  </h2>

  <!-- PROGRESS HEADER (optional) -->
  <div class="wizard">
    <div class="step active" data-step="1">1</div>
    <div class="step"        data-step="2">2</div>
    <div class="step"        data-step="3">3</div>
    <div class="step"        data-step="4">4</div>
    <div class="step"        data-step="5">5</div>
  </div>

  <!-- CARD GRID -->
  <div class="card-grid">
    <!-- Individual Card -->
    <div class="card">
      <i class="fas fa-user"></i>
      <h4>Individual</h4>
      <p>1-on-1 personal guidance tailored to your needs</p>
      <asp:Button 
          ID="btnIndividual" 
          runat="server" 
          CssClass="btn-next" 
          Text="Select" 
          CommandArgument="Individual" 
          OnClick="btnSelect_Click" />
    </div>

    <!-- Group Card -->
    <div class="card">
      <i class="fas fa-users"></i>
      <h4>Group</h4>
      <p>Shared learning and discussion. Max 10 students</p>
      <asp:Button 
          ID="btnGroup" 
          runat="server" 
          CssClass="btn-next" 
          Text="Select" 
          CommandArgument="Group" 
          OnClick="btnSelect_Click" />
    </div>
  </div>
</asp:Content>
