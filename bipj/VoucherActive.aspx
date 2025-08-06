<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="VoucherActive.aspx.cs" Inherits="bipj.VoucherActive" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap" rel="stylesheet">
    <link href="https://fonts.googleapis.com/css2?family=Titan+One&family=Inter:wght@400;500;600&display=swap" rel="stylesheet">
    
    <style>
       
        body {
            margin: 0;
            padding: 0;
        }

        /* Container */
        .container {
            max-width: 1200px;
            margin: auto;
            padding: 40px 20px;
        }

        /* Headings */
        h1 {
            font-family: 'Titan One', cursive;
            font-size: 28px;
            color: black;
            margin: 0;
        }

        /* Voucher Container */
        .voucher-container {
            display: grid;
            grid-template-columns: repeat(4, 1fr); /* Exactly 4 columns */
            gap: 24px;
            margin-top: 20px;
        }

        /* Media Query for Tablets */
        @media (max-width: 992px) {
            .voucher-container {
                grid-template-columns: repeat(2, 1fr); /* 2 per row on tablets */
            }
        }

        /* Voucher Box */
        .voucher-box {
            background-color: #fff;
            padding: 20px;
            display: flex;
            flex-direction: column;
            font-size: 14px;
            transition: transform 0.3s ease;
            border: 1px solid #E5E7EB;
        }

        /* Voucher Box Hover Effect */
        .voucher-box:hover {
            transform: translateY(-6px);
        }

        /* Voucher Company */
        .voucher-company {
            font-weight: 600;
            font-size: 18px;
            color: #111827;
            margin-bottom: 8px;
        }

        /* Voucher Description */
        .voucher-description {
            font-size: 13px;
            color: #6B7280;
            margin-bottom: 12px;
        }

        /* Voucher Meta */
        .voucher-meta {
            font-size: 12px;
            color: #9CA3AF;
            display: flex;
            flex-direction: column;
            gap: 6px;
            margin-bottom: 16px;
        }

        /* Empty State */
        .empty-state {
            text-align: center;
            padding: 60px 20px;
            background-color: var(--card-bg);
            border-radius: var(--border-radius);
            box-shadow: var(--shadow);
            margin-top: 60px;
        }

        /* Empty State Icon */
        .empty-state i {
            font-size: 80px;
            color: #D1D5DB;
            margin-bottom: 24px;
        }

        /* Empty State Heading */
        .empty-state h3 {
            font-size: 22px;
            font-weight: 600;
            color: #4B5563;
            margin-bottom: 12px;
        }

        /* Empty State Paragraph */
        .empty-state p {
            font-size: 14px;
            color: #6B7280;
            max-width: 500px;
            margin: 0 auto;
        }

        /* Modal styles */
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
            font-family: 'Titan One', cursive;
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

        .refresh-button {
            background-color: #3B387E;
            color: #fff;
            border: none;
            padding: 12px 20px;
            font-size: 14px;
            border-radius: 999px;
            cursor: pointer;
            transition: 0.3s ease;
        }

        .refresh-button:hover {
            background-color: #59569E;
        }

        .close-button {
            background-color: #fff;
            color: #3B387E;
            border: 1px solid #3B387E;
            padding: 12px 20px;
            font-size: 14px;
            border-radius: 999px;
            cursor: pointer;
            transition: 0.3s ease;
        }

        .close-button:hover {
            background-color: #f7f7f7;
        }

        /* QR Code and Button styles */
        .qrcode-container {
            margin: 20px 0;
            font-size: 13px;
        }

        #qrcode {
            display: block;
            margin: 0 auto;
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
            font-weight: 600;
        }

        /* Search and Filter Style */
        .search-filter-container {
            display: flex;
            gap: 15px;
            margin-bottom: 20px;
            justify-content: flex-end;
            width: 100%;
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

        /* Stat Pill Style */
        .stat-pill {
            display: flex;
            align-items: center;
            gap: 8px;
            background: #BFDBFE;
            color: #1E40AF;
            padding: 6px 14px;
            border-radius: 10px;
            font-size: 14px;
            font-weight: 600;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
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
            <div style="display: flex; justify-content: center; align-items: center; gap: 10px;">
                <button class="close-button" onclick="closeModal()" style="width: 180px;">Use another time</button>
                <asp:Button runat="server" Text="Refresh" CssClass="refresh-button" OnClick="Refresh" style="width: 100px;" />
            </div>

        </div>
    </div>


    <asp:UpdatePanel ID="UpdatePanel" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="container">
                <h1>My voucher</h1>

                <!-- Search and Filter Section -->
                <div class="search-filter-container">
                    <asp:TextBox ID="searchInput" runat="server" CssClass="search-bar" 
                        placeholder="Search by company name or description..." OnTextChanged="Search" AutoPostBack="true" />
                    
                    <asp:DropDownList ID="statusFilter" runat="server" CssClass="filter-dropdown" OnSelectedIndexChanged="Search" AutoPostBack="true">
                        <asp:ListItem>status</asp:ListItem>
                        <asp:ListItem>available</asp:ListItem>
                        <asp:ListItem>used</asp:ListItem>
                        <asp:ListItem>expired</asp:ListItem>
                    </asp:DropDownList>
                </div>

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
                                    Visible='<%# Eval("Status").ToString() == "available" %>'>
                                </asp:LinkButton>

                                <asp:LinkButton runat="server"
                                    Text="Used"
                                    CssClass="used-button redeem-button" style="background-color: #f5f2f3; color: #b0b0b0; cursor: not-allowed;"
                                    Visible='<%# Eval("Status").ToString() == "used" %>'>
                                </asp:LinkButton>
                                <asp:LinkButton runat="server"
                                    Text="Expired"
                                    CssClass="expired-button redeem-button" style="background-color: #febfc5; color: #b91032; cursor: not-allowed;"
                                    Visible='<%# Eval("Status").ToString() == "expired" %>'>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>

            <asp:TextBox ID="voucherToken" runat="server" style="display:none;"></asp:TextBox>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- QR Code Library -->
    <script src="https://cdn.jsdelivr.net/npm/qrcode@1.5.1/build/qrcode.min.js"></script>

    <script>
        
        document.addEventListener("DOMContentLoaded", function () {
            bindVoucherClickEvents(); 
        });

        function resetModal() {
            document.getElementById("modalDescription").innerText = "";
            document.getElementById("modalCompany").innerText = "";
            document.getElementById("modalExpiry").innerText = "";
            document.getElementById("qrcode").getContext('2d').clearRect(0, 0, 100, 100); 
            document.getElementById("voucherModal").style.display = "none";
        }

        function bindVoucherClickEvents() {
            document.querySelectorAll(".show-voucher").forEach(button => {
                button.addEventListener("click", function () {
                    resetModal();
                    const desc = this.getAttribute("data-description");
                    const company_name = this.getAttribute("data-company-name");
                    const expiry_date = this.getAttribute("data-expiry-date");
                    const token = this.getAttribute("data-token");
                    openModal(desc, company_name, expiry_date, token);
                });
            });
        }

        function openModal(desc, company_name, expiry_date, token) {
            document.getElementById("modalDescription").innerText = desc;
            document.getElementById("modalCompany").innerText = company_name;
            document.getElementById("modalExpiry").innerText = expiry_date;
            document.getElementById('<%= voucherToken.ClientID %>').value = token;

            QRCode.toCanvas(document.getElementById("qrcode"), token, function (error) {
                if (error) console.error(error);
            });

            document.getElementById("voucherModal").style.display = "flex";
        }

       
        function closeModal() {
            document.getElementById("voucherModal").style.display = "none";
        }

</script>
</asp:Content>
