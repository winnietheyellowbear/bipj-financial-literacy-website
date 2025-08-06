<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="VoucherExchange.aspx.cs" Inherits="bipj.VoucherExchange" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <head>
        <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap" rel="stylesheet">
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
        <link href="https://fonts.googleapis.com/css2?family=Titan+One&display=swap" rel="stylesheet">
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

        </style>
    </head>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
   
    <asp:UpdatePanel ID="UpdatePanel" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="container">
                <h1>Exchange Voucher</h1>

                <div>
                    <asp:Label ID="lbl_Point" runat="server" Text='<%# Eval("UserPoints") %>'></asp:Label>
                </div>

                <!-- Search and Filter Section -->
                <div class="search-filter-container">
                    <!-- Search Bar -->
                    <asp:TextBox ID="searchInput" runat="server" CssClass="search-bar"
                        placeholder="Search by company name or description..." OnTextChanged="Search" AutoPostBack="true" />

                    <!-- Filter Dropdown -->
                    <asp:DropDownList ID="statusFilter" runat="server" CssClass="filter-dropdown" OnSelectedIndexChanged="Search" AutoPostBack="true">
                        <asp:ListItem Value="order">Order</asp:ListItem>
                        <asp:ListItem Value="ORDER BY Voucher_ID ASC">Voucher ID Ascending</asp:ListItem>
                        <asp:ListItem Value="ORDER BY Voucher_ID DESC">Voucher ID Descending</asp:ListItem>
                        <asp:ListItem Value="ORDER BY Validity ASC">Validity Ascending</asp:ListItem>
                        <asp:ListItem Value="ORDER BY Validity DESC">Validity Descending</asp:ListItem>
                        <asp:ListItem Value="ORDER BY Points_Required ASC">Points Required Ascending</asp:ListItem>
                        <asp:ListItem Value="ORDER BY Points_Required DESC">Points Required Descending</asp:ListItem>
                    </asp:DropDownList>
                </div>

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
                                        Text="Redeem"
                                        CssClass="redeem-button"
                                        style="background-color: #BFDBFE; color: #1075b9"
                                        CommandArgument='<%# Eval("Voucher_ID") %>'
                                        OnClick="btn_redeem_Click"
                                        CausesValidation="false">
                                    </asp:LinkButton>
                                </div>
                            </asp:Panel>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:Panel ID="pnlEmptyState" runat="server" Visible="false" CssClass="empty-state">
                    <i class="far fa-folder-open"></i>
                    <h3>No Vouchers Available.</h3>
                    <p>There are currently no vouchers available for exchange. Check back later.</p>
                </asp:Panel>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
