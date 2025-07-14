<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" CodeBehind="VoucherExchange.aspx.cs" Inherits="bipj.VoucherExchange" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <head>
        <link rel="stylesheet" href="Voucher.css">
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
    </head>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="top-bar">
        <h2>Exchange voucher</h2>
        <div style="display: flex; gap: 10px;">
            <div class="stat-pill">
                <i class="fas fa-ticket-alt"></i> Available vouchers: <asp:Label ID="lbl_Voucher_Count" runat="server" Text='<%# Eval("TotalVouchers") %>'></asp:Label>
            </div>
            <div class="stat-pill">
                <i class="fas fa-coins"></i> My points: <asp:Label ID="lbl_Point" runat="server" Text='<%# Eval("UserPoints") %>'></asp:Label>
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

        <!-- Empty State -->
        <asp:Panel ID="pnlEmptyState" runat="server" Visible="false" CssClass="empty-state">
            <i class="far fa-folder-open"></i>
            <h3>No Vouchers Available</h3>
            <p>There are currently no vouchers available for exchange. Check back later or earn more points to unlock premium vouchers.</p>
        </asp:Panel>
    </div>
</asp:Content>