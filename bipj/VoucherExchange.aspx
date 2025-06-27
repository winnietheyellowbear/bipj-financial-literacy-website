<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" CodeBehind="VoucherExchange.aspx.cs" Inherits="bipj.VoucherExchange" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <head>
        <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap" rel="stylesheet">
        <link rel="stylesheet" href=" https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css ">
        <style>
            :root {
                --primary: #3B82F6;
                --secondary: #1075b9;
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
            }

            .stat-pill {
                display: flex;
                align-items: center;
                gap: 8px;
                background: #BFDBFE;
                color: #1E40AF;
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
                background-color: #1989d4;
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
    <div class="top-bar">
        <h2>Voucher Exchange</h2>
        <div style="display: flex; gap: 10px;">
            <div class="stat-pill">
                <i class="fas fa-ticket-alt"></i> Available Vouchers: <asp:Label ID="lbl_Voucher_Count" runat="server" Text='<%# Eval("TotalVouchers") %>'></asp:Label>
            </div>
            <div class="stat-pill">
                <i class="fas fa-coins"></i> Your Points: <asp:Label ID="lbl_Point" runat="server" Text='<%# Eval("UserPoints") %>'></asp:Label>
            </div>
        </div>
    </div>

    <div class="container">
        <!-- Vouchers Grid -->
        <div class="voucher-container">
            <asp:Repeater ID="Voucher" runat="server">
                <ItemTemplate>
                    <asp:Panel runat="server" Visible='<%# Eval("Status").ToString() == "Active" %>'>
                    <div class="voucher-box">
                        <div class="voucher-company"><%# Eval("Company_Name") %></div>
                        <div class="voucher-description"><%# Eval("Description") %></div>
                        <div class="voucher-meta">
                            <span><i class="fas fa-clock"></i> Validity: <%# Eval("Validity") %></span>
                            <span><i class="fas fa-coins"></i> Required Points: <%# Eval("Points_Required") %></span>
                        </div>
                        <asp:LinkButton runat="server"
                            Text="<i class='fas fa-gift'></i> Redeem"
                            CssClass="redeem-button"
                            CommandArgument='<%# Eval("Voucher_ID") %>'
                            OnClick="btn_redeem_Click"
                            CausesValidation="false">
                        </asp:LinkButton>
                    </div>
                    </asp:Panel>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <!-- Empty State -->
        <asp:Panel ID="pnlEmptyState" runat="server" Visible="false" CssClass="empty-state">
            <i class="far fa-folder-open"></i>
            <h3>No Vouchers Available</h3>
            <p>There are currently no vouchers available for exchange. Check back later or earn more points to unlock premium vouchers.</p>
        </asp:Panel>
    </div>
</asp:Content>