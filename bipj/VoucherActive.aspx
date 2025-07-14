<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" CodeBehind="VoucherActive.aspx.cs" Inherits="bipj.VoucherActive" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <div id="voucherModal" class="modal" style="display:none;">
        <div class="modal-content">
            <h2>Time to Enjoy Your Reward!</h2>
            <p><strong>🏢Sponsor:</strong> <span id="modalCompany"></span></p>
            <p><strong>📅Valid until:</strong> <span id="modalExpiry"></span></p>
            <p><strong>📃Description:</strong> <span id="modalDescription"></span></p>

            
            <div class="qrcode-container">
                Scan this at the counter to redeem the discount!🥳
                <canvas id="qrcode"></canvas>
            </div>

            <button class="close-button" onclick="closeModal()">Use another time</button>
        </div>
    </div>

     <head>
     <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap" rel="stylesheet">
     <link href="https://fonts.googleapis.com/css2?family=Titan+One&family=Inter:wght@400;500;600&display=swap" rel="stylesheet">
     <link rel="stylesheet" href="Voucher.css">
     <link rel="stylesheet" href=" https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css ">
     <style>
         :root {
             --primary: #10B981;
             --background: #F9FAFB;
             --card-bg: #FFFFFF;
             --text-color: #1f3720;
             --border-radius: 16px;
             --shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
             --section-gap: 40px;
             --transition: all 0.3s ease;
         }

         

     </style>
 </head>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

        <style>
        .modal {
            position: fixed;
            top: 0; left: 0; right: 0; bottom: 0;
            background: rgba(0, 0, 0, 0.5);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 10000;
        }

        .modal-content {
            background: #fff;
            width: 360px;
            padding: 30px 20px;
            border-radius: 16px;
            font-family: 'Inter', sans-serif;
            position: relative;
        }

        .modal-content h2 {
            font-family: 'Titan One', cursive; /* ✅ Use Titan One here */
            color: #3B387E;
            font-size: 20px;
            margin-bottom: 15px;
            text-align: center;
            font-weight: 100;
        }


        .modal-content p {
            margin: 6px 0;
            font-size: 13px;
            color: #333;
        }

        .modal-content strong {
            font-weight: 600;
        }



        .close-button {
            background-color: #3B387E;
            color: #fff;
            border: none;
            padding: 12px 20px;
            font-size: 14px;
            border-radius: 999px;
            cursor: pointer;
            transition: 0.3s ease;
        }

        .close-button:hover {
            background-color: #59569E;
        }

        .close {
            position: absolute;
            top: 12px;
            right: 16px;
            font-size: 20px;
            cursor: pointer;
        }

        .qrcode-container {
            margin: 20px 0;
            font-size: 13px;
            
        }

        #qrcode {
            display: block;      /* 👈 force it to behave like a block */
            margin: 0 auto;       /* 👈 horizontally center it */
        }


                    

        .search-filter-container {
            display: flex;
            gap: 15px;
            margin-bottom: 20px;
            justify-content: flex-end; /* Align to the right */
            width: 100%; /* Ensure it spans the entire width */
        }

        .search-bar {
            padding: 10px;
            font-size: 14px;
            width: 250px;
            border-radius: 8px;
            border: 1px solid #ddd;
            outline: none;
            transition: 0.3s;
        }

        .search-bar:focus {
            border-color: #10B981;
        }

        .filter-dropdown {
            padding: 10px;
            font-size: 14px;
            border-radius: 8px;
            border: 1px solid #ddd;
            background-color: #fff;
            cursor: pointer;
        }


        .redeem-button {
            padding: 6px 10px;
            font-size: 14px;
            border-radius: 10px;
            text-align: center;
            cursor: pointer;
            transition: 0.3s ease;
            border: none;
            width: 100%;
            text-decoration: none;
        }

    </style>
   

     <div class="top-bar">

     <h2>My voucher</h2>
     <div style="display: flex; gap: 10px;">
         <div class="stat-pill" style="background-color: #bffedf; color: #10B981">
             <i class="fas fa-ticket-alt"></i> Available vouchers: <asp:Label ID="lbl_Voucher_Count" runat="server" Text='<%# Eval("TotalVouchers") %>'></asp:Label>
         </div>
         <div class="stat-pill" style="background-color: #f5f2f3; color: #b0b0b0">
            <i class="fas fa-ticket-alt"></i> Used vouchers: <asp:Label ID="Label1" runat="server" Text='<%# Eval("TotalVouchers") %>'></asp:Label>
        </div>
         <div class="stat-pill" style="background-color: #febfc5; color: #b91032">
    <i class="fas fa-ticket-alt"></i> Expired vouchers: <asp:Label ID="Label2" runat="server" Text='<%# Eval("TotalVouchers") %>'></asp:Label>
</div>
     </div>
 </div>

    

 <div class="container">

       <!-- Search and Filter Section -->
<div class="search-filter-container">
    <!-- Search Bar -->
    <input type="text" id="searchInput" class="search-bar" placeholder="Search by company or description..." onkeyup="filterVouchers()" />
    
    <!-- Filter Dropdown -->
    <select id="statusFilter" class="filter-dropdown" onchange="filterVouchers()">
        <option value="">Filter by Status</option>
        <option value="Available">Available</option>
        <option value="Used">Used</option>
        <option value="Expired">Expired</option>
    </select>
</div>
     <!-- Vouchers Grid -->
     <div class="voucher-container">
         <asp:Repeater ID="Voucher" runat="server">
             <ItemTemplate>
                 <div class="voucher-box">
                     <div class="voucher-company"><%# Eval("Company_Name") %></div>
                     <div class="voucher-description"><%# Eval("Description") %></div>
                     <div class="voucher-meta">
                         <span><i class="fas fa-calendar-alt"></i> Expiry Date: <%# Eval("Expiry_Date") %></span>
                     </div>
                     <asp:LinkButton runat="server"
                         Text="Use"
                         CssClass="use-button redeem-button show-voucher"
                         style="background-color: #bffedf; color: #10B981"
                         OnClientClick="return false;" 
                        data-description='<%# Eval("Description") %>' 
                        data-company-name='<%# Eval("Company_Name") %>' 
                        data-expiry-date='<%# Eval("Expiry_Date") %>' 
                        data-token='<%# Eval("Token") %>'
                         CausesValidation="false"
                         Visible='<%# Eval("Status").ToString() == "Available" %>'>
                     </asp:LinkButton>

                     <asp:LinkButton runat="server"
                                    Text="Used"
                                    CssClass="used-button redeem-button" style="background-color: #f5f2f3; color: #b0b0b0; cursor: not-allowed;"
                                    Visible='<%# Eval("Status").ToString() == "Used" %>'>
                    </asp:LinkButton>
                    <asp:LinkButton runat="server"
                                    Text="Expired"
                                    CssClass="expired-button redeem-button" style="background-color: #febfc5; color: #b91032; cursor: not-allowed;"
                                    Visible='<%# Eval("Status").ToString() == "Expired" %>'>
                    </asp:LinkButton>

                 </div>
                
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
