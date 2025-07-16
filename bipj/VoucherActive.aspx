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
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
        
        
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
            font-weight: 600;
        }

        
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

body {
    font-family: 'Inter', sans-serif;
    background-color: white;
    color: var(--text-color);
    margin: 0;
    padding: 0;
}

    </style>
   
    </head>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <head>
        <asp:ScriptManager ID="ScriptManager" runat="server" />
    </head>

    <style>
        /* Modal and other styles as per your original code */
    </style>

    <br /><br />

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
            <asp:Timer ID="statusCheckTimer" runat="server" OnTick="statusCheckTimer_Tick" />
        </ContentTemplate>
    </asp:UpdatePanel>

    <script src="https://cdn.jsdelivr.net/npm/qrcode@1.5.1/build/qrcode.min.js"></script>

    <script>
        let statusCheckInterval;

        document.addEventListener("DOMContentLoaded", function () {
            bindVoucherClickEvents(); // Bind events after page load
        });

        // Function to reset modal
        function resetModal() {
            // Clear QR code and other modal data
            document.getElementById("modalDescription").innerText = "";
            document.getElementById("modalCompany").innerText = "";
            document.getElementById("modalExpiry").innerText = "";
            document.getElementById("qrcode").getContext('2d').clearRect(0, 0, 100, 100);  // Clear previous QR code

            // Close and hide the modal before opening it again
            document.getElementById("voucherModal").style.display = "none";
        }

        // Function to bind events to voucher buttons
        function bindVoucherClickEvents() {
            document.querySelectorAll(".show-voucher").forEach(button => {
                button.addEventListener("click", function () {
                    resetModal(); // Reset the modal before opening it again

                    const desc = this.getAttribute("data-description");
                    const company_name = this.getAttribute("data-company-name");
                    const expiry_date = this.getAttribute("data-expiry-date");
                    const token = this.getAttribute("data-token");

                    // Open the modal with new content
                    openModal(desc, company_name, expiry_date, token);
                });
            });
        }

        // Function to open modal
        function openModal(desc, company_name, expiry_date, token) {
            // Populate modal with data
            document.getElementById("modalDescription").innerText = desc;
            document.getElementById("modalCompany").innerText = company_name;
            document.getElementById("modalExpiry").innerText = expiry_date;

            // Set the token to the hidden input field
            document.getElementById('<%= voucherToken.ClientID %>').value = token;

            // Generate QR code
            QRCode.toCanvas(document.getElementById("qrcode"), token, function (error) {
                if (error) console.error(error);
            });

            // Show the modal
            document.getElementById("voucherModal").style.display = "flex";

            // Start the status check timer
            startStatusCheckTimer(token);
        }

        // Function to start the timer for checking voucher status
        function startStatusCheckTimer(token) {
            // Clear any existing interval before starting a new one
            if (statusCheckInterval) {
                clearInterval(statusCheckInterval);
            }

            // Set the interval to check the voucher status every 1 second
            statusCheckInterval = setInterval(function () {
                checkVoucherStatus(token);
            }, 100);
        }

        // Function to stop the status check timer
        function stopStatusCheckTimer() {
            if (statusCheckInterval) {
                clearInterval(statusCheckInterval);
                statusCheckInterval = null;
            }
        }

        // Function to check voucher status
        function checkVoucherStatus(token) {
            fetch('/VoucherActive.aspx/GetVoucherStatus', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ token: token })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.status === "used") {
                        stopStatusCheckTimer();
                        closeModal();
                        alert("Voucher has been used. 😊");
                    }
                })
                .catch(error => console.error("Error checking voucher status:", error));
        }

        // Close modal function
        function closeModal() {
            document.getElementById("voucherModal").style.display = "none";
            stopStatusCheckTimer();  // Stop the timer when the modal is closed
        }

        // Rebind events after data is reloaded (such as after UpdatePanel postback)
        function rebindVoucherEvents() {
            bindVoucherClickEvents();
        }

        // Call after UpdatePanel data reload
        <% ScriptManager.RegisterStartupScript(this, this.GetType(), "rebindVoucherEvents", "rebindVoucherEvents();", true); %>
</script>
</asp:Content>
