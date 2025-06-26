<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" CodeBehind="VoucherActive.aspx.cs" Inherits="bipj.VoucherActive" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
 <div id="voucherModal" class="modal" style="display:none;">
  <div class="modal-content">
    <span class="close" onclick="closeModal()">&times;</span>
    <h2>Voucher Details</h2>
    <p><strong>Description:</strong> <span id="modalDescription"></span></p>
    <p><strong>Company:</strong> <span id="modalCompany"></span></p>
    <p><strong>Expiry Date:</strong> <span id="modalExpiry"></span></p>
    <div>
      <h3>Scan QR Code</h3>
      <canvas id="qrcode"></canvas>
    </div>
  </div>
</div>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    
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
          background: #d4fade;
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

        .modal {
          position: fixed;
          top: 0; left: 0; right: 0; bottom: 0;
          background: rgba(0,0,0,0.5);
          display: flex;
          align-items: center;
          justify-content: center;
          z-index: 9999;
        }
        .modal-content {
          background: white;
          padding: 20px;
          border-radius: 5px;
          width: 300px;
          text-align: center;
        }
        .close {
          float: right;
          font-size: 1.5rem;
          cursor: pointer;
        }

    </style>

      <div class="container">
      <div class="nav-links">
          <a href="VoucherExchange.aspx">Voucher Exchange (<asp:Label ID="lbl_Voucher_Available" runat="server" Text=""></asp:Label>)</a>
          <a href="VoucherActive.aspx">Active (<asp:Label ID="lbl_Active_Voucher" runat="server" Text=""></asp:Label>)</a>
          <a href="#">Used Voucher (<asp:Label ID="lbl_Used_Voucher" runat="server" Text=""></asp:Label>)</a>
          <a href="#">Expired Voucher (<asp:Label ID="lbl_Expired_Voucher" runat="server" Text=""></asp:Label>)</a>
      </div>

        <!-- Voucher Cards -->
     <asp:Repeater ID="Voucher" runat="server">
     <ItemTemplate>
        <div class="voucher-card">
        <%-- <asp:Image 
          runat="server" 
          ImageUrl='<%# ResolveUrl(Eval("Company_Logo").ToString()) %>' 
          CssClass="voucher-logo" />--%>
         <div class="voucher-details">
           <div class="voucher-description"><%# Eval("Description") %></div>
           <div class="voucher-info">
             <span>🏪 <%# Eval("Company_Name") %></span>
             <span>📅 <%# Eval("Expiry_Date") %></span>
           </div>
         </div>
           <asp:Button 
            runat="server" 
            Text="Use" 
            CssClass="voucher-redeem show-voucher"
            OnClientClick="return false;" 
            data-description='<%# Eval("Description") %>' 
            data-company-name='<%# Eval("Company_Name") %>' 
            data-expiry-date='<%# Eval("Expiry_Date") %>' 
            data-token='<%# Eval("Token") %>'/>
       </div>
     </ItemTemplate>
     </asp:Repeater>
  </div>

<script src="https://cdn.jsdelivr.net/npm/qrcode@1.5.1/build/qrcode.min.js"></script>

<script>
    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".show-voucher").forEach(button => {
            button.addEventListener("click", function () {
                const desc = this.getAttribute("data-description");
                const company_name = this.getAttribute("data-company-name");
                const expiry_date = this.getAttribute("data-expiry-date");
                const token = this.getAttribute("data-token");

                // Populate modal
                document.getElementById("modalDescription").innerText = desc;
                document.getElementById("modalCompany").innerText = company_name;
                document.getElementById("modalExpiry").innerText = expiry_date;

                // Generate QR
                QRCode.toCanvas(document.getElementById("qrcode"), token, function (error) {
                    if (error) console.error(error);
                });

                // Show modal
                document.getElementById("voucherModal").style.display = "flex";
            });
        });
    });

    function closeModal() {
        document.getElementById("voucherModal").style.display = "none";
    }
</script>


</asp:Content>
