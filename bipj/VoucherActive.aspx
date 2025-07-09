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

     <head>
     <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap" rel="stylesheet">
     <link rel="stylesheet" href=" https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css ">
     <style>
         :root {
             --primary: #3B82F6;
             --secondary: #10B981;
             --background: #F9FAFB;
             --card-bg: #FFFFFF;
             --text-color: #1F2937;
             --border-radius: 16px;
             --shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
             --section-gap: 40px;
             --transition: all 0.3s ease;
         }

         body {
             font-family: 'Inter', sans-serif;
             background-color: var(--background);
             color: var(--text-color);
             margin: 0;
             padding: 0;
         }

         .top-bar {
             background: var(--card-bg);
             box-shadow: var(--shadow);
             padding: 20px 30px;
             display: flex;
             justify-content: space-between;
             align-items: center;
             border-bottom: 1px solid #E5E7EB;
             flex-wrap: wrap;
             gap: 10px;
             position: sticky;
         }

         .stat-pill {
             display: flex;
             align-items: center;
             gap: 8px;
             background: #bffeea;
             color: #1eaf6b;
             padding: 6px 14px;
             border-radius: 999px;
             font-size: 14px;
             font-weight: 600;
         }

         .container {
             max-width: 1200px;
             margin: auto;
             padding: 40px 20px;
         }

         h2 {
             font-size: 28px;
             font-weight: 600;
             color: black;
             margin: 0;
         }

         .voucher-container {
            display: grid;
            grid-template-columns: repeat(4, 1fr); /* Exactly 4 columns */
            gap: 24px;
            margin-top: 20px;
        }

        @media (max-width: 992px) {
            .voucher-container {
                grid-template-columns: repeat(2, 1fr); /* 2 per row on tablets */
            }
        }

        @media (max-width: 576px) {
            .voucher-container {
                grid-template-columns: 1fr; /* 1 per row on mobile */
            }
        }


         .voucher-box {
             background-color: var(--card-bg);
             border-radius: var(--border-radius);
             box-shadow: var(--shadow);
             padding: 20px;
             display: flex;
             flex-direction: column;
             font-size: 14px;
             transition: transform 0.3s ease;
             border: 1px solid #E5E7EB;
         }

         .voucher-box:hover {
             transform: translateY(-6px);
         }

         .voucher-company {
             font-weight: 600;
             font-size: 18px;
             color: #111827;
             margin-bottom: 8px;
         }

         .voucher-description {
             font-size: 13px;
             color: #6B7280;
             margin-bottom: 12px;
         }

         .voucher-meta {
             font-size: 12px;
             color: #9CA3AF;
             display: flex;
             flex-direction: column;
             gap: 6px;
             margin-bottom: 16px;
         }

         .redeem-button {
             margin-top: auto;
             padding: 10px 16px;
             font-size: 13px;
             font-weight: 600;
             background-color: var(--secondary);
             color: white;
             border: none;
             border-radius: var(--border-radius);
             cursor: pointer;
             display: inline-flex;
             align-items: center;
             gap: 8px;
             transition: background-color 0.3s ease;
             text-decoration: none;
         }

         .redeem-button:hover {
             background-color: #19d194;
         }

         .empty-state {
             text-align: center;
             padding: 60px 20px;
             background-color: var(--card-bg);
             border-radius: var(--border-radius);
             box-shadow: var(--shadow);
             margin-top: 60px;
         }

         .empty-state i {
             font-size: 80px;
             color: #D1D5DB;
             margin-bottom: 24px;
         }

         .empty-state h3 {
             font-size: 22px;
             font-weight: 600;
             color: #4B5563;
             margin-bottom: 12px;
         }

         .empty-state p {
             font-size: 14px;
             color: #6B7280;
             max-width: 500px;
             margin: 0 auto;
         }
     </style>
 </head>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <style>
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
    <br />
    <br />
     

     <div class="top-bar">

     <h2>Active Voucher</h2>
     <div style="display: flex; gap: 10px;">
         <div class="stat-pill">
             <i class="fas fa-ticket-alt"></i> Available Vouchers: <asp:Label ID="lbl_Voucher_Count" runat="server" Text='<%# Eval("TotalVouchers") %>'></asp:Label>
         </div>
     </div>
 </div>

 <div class="container">
     <!-- Vouchers Grid -->
     <div class="voucher-container">
         <asp:Repeater ID="Voucher" runat="server">
             <ItemTemplate>
                 <asp:Panel runat="server" Visible='<%# Eval("Status").ToString() == "Available" %>'>
                 <div class="voucher-box">
                     <div class="voucher-company"><%# Eval("Company_Name") %></div>
                     <div class="voucher-description"><%# Eval("Description") %></div>
                     <div class="voucher-meta">
                         <span><i class="fas fa-calendar-alt"></i> Expiry Date: <%# Eval("Expiry_Date") %></span>
                     </div>
                     <asp:LinkButton runat="server"
                         Text="<i class='fas fa-gift'></i> Use"
                         CssClass="redeem-button show-voucher"
                         OnClientClick="return false;" 
                        data-description='<%# Eval("Description") %>' 
                        data-company-name='<%# Eval("Company_Name") %>' 
                        data-expiry-date='<%# Eval("Expiry_Date") %>' 
                        data-token='<%# Eval("Token") %>'
                         CausesValidation="false">
                     </asp:LinkButton>
                 </div>
                 </asp:Panel>
             </ItemTemplate>
         </asp:Repeater>
     </div>

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
