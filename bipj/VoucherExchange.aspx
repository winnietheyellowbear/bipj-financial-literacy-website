<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" CodeBehind="VoucherExchange.aspx.cs" Inherits="bipj.VoucherExchange" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f9f9f9;
        }

        .container {
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
        }

        .nav-links {
            display: flex;
            justify-content: space-around;
            margin-bottom: 20px;
        }

        .nav-links a {
            text-decoration: none;
            color: #333;
            font-size: 14px;
            padding: 5px 10px;
            border-radius: 5px;
            background-color: #f9f9f9;
            transition: background-color 0.3s ease;
        }

        .nav-links a:hover {
            background-color: #eaeaea;
        }



         /* Voucher Cards */
        .voucher-card {
          background: #eef5fb;
          border-radius: 5px;
          box-shadow: 0px 0px 2px rgba(0, 0, 0, 0.2);
          padding: 20px;
          margin-bottom: 20px;
          display: flex;
          align-items: center;
          gap: 20px;
        }

        .voucher-logo {
          width: 60px;
          height: 60px;
          object-fit: contain;
        }

         .voucher-details {
          flex: 1;
        }

        .voucher-title {
          font-size: 1.2rem;
          font-weight: bold;
          margin-bottom: 5px;
        }

        .voucher-description {
          font-size: 0.9rem;
          color: #666;
          margin-bottom: 10px;
        }

        .voucher-info {
          display: flex;
          gap: 10px;
        }

         .voucher-info span {
          font-size: 0.9rem;
          color: #666;
        }

        .voucher-points {
          font-size: 1rem;
          color: red;
          font-weight: bold;
          margin-left: auto;
        }

        .voucher-redeem {
          background: white;
          border: 1px solid #ccc;
          border-radius: var(--border-radius);
          padding: 8px 16px;
          font-size: 0.9rem;
          cursor: pointer;
          transition: background 0.3s ease;
        }

        .voucher-redeem:hover {
          background: #f0f0f0;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <div class="nav-links">
            <a href="#">Voucher Exchange (2)</a>
            <a href="#">Active (0)</a>
            <a href="#">Used Voucher (0)</a>
            <a href="#">Expired Voucher (1)</a>
        </div>

          <!-- Voucher Cards -->
       <asp:Repeater ID="Voucher" runat="server" OnItemCommand="Voucher_ItemCommand">
       <ItemTemplate>
          <div class="voucher-card" data-voucher-id="1">
           <asp:Image 
            runat="server" 
            ImageUrl='<%# ResolveUrl(Eval("Company_Logo").ToString()) %>' 
            CssClass="voucher-logo" />
           <div class="voucher-details">
             <div class="voucher-description"><%# Eval("Description") %></div>
             <div class="voucher-info">
               <span>🏪 <%# Eval("Company_Name") %></span>
               <span>📅 <%# Eval("Validity") %></span>
               <span>🌟 <%# Eval("Points_Required") %> Points</span>
             </div>
           </div>
           <button class="voucher-redeem" CommandArgument="<%# Eval("Voucher_ID") %>">Redeem</button>
         </div>
       </ItemTemplate>
       </asp:Repeater>
    </div>

   
</asp:Content>
