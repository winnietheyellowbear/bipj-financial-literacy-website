<%@ Page Title="Admin Dashboard" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="AdminPage.aspx.cs" Inherits="bipj.AdminPage" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.googleapis.com/css2?family=Montserrat:wght@600&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css" />
    <style>
        .dashboard-container {
            min-height: 85vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .dashboard-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
            gap: 2rem;
        }
        .admin-card {
            background: #fff;
            border-radius: 2rem;
            box-shadow: 0 6px 20px rgba(80,60,160,0.09);
            padding: 2rem 1.6rem 1.5rem 1.6rem;
            display: flex;
            flex-direction: column;
            align-items: flex-start;
            min-height: 180px;
            position: relative;
            transition: transform .08s;
        }
        .admin-card:hover {
            transform: translateY(-6px) scale(1.025);
            box-shadow: 0 12px 32px rgba(80,60,160,0.13);
        }
        .admin-icon {
            font-size: 2.8rem;
            margin-bottom: 0.4rem;
            color: #353535;
        }
        .admin-title {
            font-size: 1.11rem;
            font-weight: 600;
            margin-bottom: 2.6rem;
            color: #222;
            min-height: 2.3rem;
        }
        .go-btn {
            position: absolute;
            right: 1.4rem;
            bottom: 1.2rem;
            background: #5045a6;
            color: #fff;
            border: none;
            border-radius: 1.1rem;
            padding: 4px 16px;
            font-weight: 600;
            font-size: 1rem;
            transition: background 0.15s;
            text-decoration: none;
            text-align: center;
        }
        .go-btn:hover {
            background: #362f72;
            color: #fff;
        }
    </style>
</asp:Content>

<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">
        <div class="dashboard-grid">
            <!-- Card 1 -->
            <div class="admin-card">
                <i class="bi bi-people-fill admin-icon"></i>
                <div class="admin-title">manage advisors</div>
                <a href="ApproveAdvisor.aspx" class="go-btn">Go</a>
            </div>
            <!-- Card 2 -->
            <div class="admin-card">
                <i class="bi bi-mortarboard-fill admin-icon"></i>
                <div class="admin-title">manage<br />education topics</div>
                <a href="ManageEducation.aspx" class="go-btn">Go</a>
            </div>
            <!-- Card 3 -->
            <div class="admin-card">
                <i class="bi bi-card-checklist admin-icon"></i>
                <div class="admin-title">manage tests</div>
                <a href="ManageTests.aspx" class="go-btn">Go</a>
            </div>
        </div>
    </div>
</asp:Content>
