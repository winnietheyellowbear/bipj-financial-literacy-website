<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" CodeBehind="VoucherExchange.aspx.cs" Inherits="bipj.VoucherExchange" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <head>
    <!-- Import Titan One Font -->
    <link href="https://fonts.googleapis.com/css2?family=Titan+One&display=swap" rel="stylesheet">
    </head>

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
          background: #e8f6fa;
          border-radius: 5px;
          box-shadow: 0 2px 2px rgba(0, 0, 0, 0.15);
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

      

        .voucher-description {
          font-size: 0.9rem;
          color: black;
          margin-bottom: 10px;
        }

        .voucher-info {
          display: flex;
          gap: 10px;
        }

         .voucher-info span {
          font-size: 0.9rem;
          color: black;
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


         /* Point Balance Container */
        .point-balance {
            text-align: center;
            width: 200px;
            margin: 0 auto 20px;
            background-color: #e8f6fa;
            box-shadow: 0 2px 2px rgba(0, 0, 0, 0.15);
            padding: 10px 20px;
            font-size: 16px;
            
        }

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <div class="nav-links">
            <a href="#">Voucher Exchange (<asp:Label ID="lbl_Voucher_Available" runat="server" Text=""></asp:Label>)</a>
            <a href="#">Active (<asp:Label ID="lbl_Active_Voucher" runat="server" Text=""></asp:Label>)</a>
            <a href="#">Used Voucher (<asp:Label ID="lbl_Used_Voucher" runat="server" Text=""></asp:Label>)</a>
            <a href="#">Expired Voucher (<asp:Label ID="lbl_Expired_Voucher" runat="server" Text=""></asp:Label>)</a>
        </div>

        <!-- Point Balance Container -->
        <div class="point-balance">
            <div style="font-family: 'Titan One'; display: block">
                Point Balance
            </div>
            <asp:Label ID="lbl_Point" runat="server" Text=""></asp:Label>
        </div>


          <!-- Voucher Cards -->
       <asp:Repeater ID="Voucher" runat="server">
       <ItemTemplate>
          <div class="voucher-card">
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
             <asp:Button 
                runat="server" 
                Text="Redeem"
                CssClass="voucher-redeem" 
                CommandArgument='<%# Eval("Voucher_ID") %>' 
                OnClick="btn_redeem_Click" />

         </div>
       </ItemTemplate>
       </asp:Repeater>
    </div>

   
</asp:Content>
