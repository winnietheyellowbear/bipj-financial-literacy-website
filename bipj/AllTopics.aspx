<%@ Page Title="All Topics" Language="C#" MasterPageFile="~/Customer_Nav_LoggedIn.Master" AutoEventWireup="true" CodeBehind="AllTopics.aspx.cs" Inherits="bipj.AllTopics" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .alltopics-container {
            background: #f9f8ff;
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            align-items: center;
            padding-bottom: 32px;
        }
        .topics-cardbox {
            background: #f4f2fd;
            border-radius: 18px;
            margin-top: 32px;
            padding: 32px 32px 16px 32px;
            width: 92vw;
            max-width: 960px;
        }
        .back-arrow {
            font-size: 2.2rem;
            font-weight: bold;
            color: #3b3350;
            margin-right: 18px;
            text-decoration: none;
            transition: color 0.1s;
        }
        .back-arrow:hover {
            color: #433e8e;
        }
        .topics-header-row {
            display: flex;
            align-items: center;
            font-size: 1.28rem;
            font-weight: bold;
            margin-bottom: 24px;
        }
        .topics-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 32px 56px;
        }
        @media (min-width: 900px) {
            .topics-grid {
                grid-template-columns: repeat(3, 1fr);
            }
        }
        .topic-card {
            background: #fff;
            border-radius: 16px;
            box-shadow: 0 1px 7px #e6e4f0;
            padding: 20px 18px 18px 18px;
            display: flex;
            flex-direction: column;
            align-items: center;
        }
        .topic-img {
            width: 80px;
            height: 80px;
            object-fit: contain;
            margin-bottom: 12px;
        }
        .topic-title {
            font-weight: bold;
            font-size: 1.09rem;
            color: #2e266e;
            margin-bottom: 5px;
        }
        .topic-desc {
            font-size: 0.95rem;
            color: #484848;
            text-align: center;
            margin-bottom: 15px;
            min-height: 48px;
        }
        .topic-status-row {
            width: 100%;
            display: flex;
            align-items: center;
            justify-content: space-between;
        }
        .topic-status {
            background: #fff;
            color: #3d3c3c;
            border-radius: 16px;
            border: 1.5px solid #c7c6c9;
            padding: 5px 14px;
            font-size: 0.92rem;
            margin-right: 0.5rem;
        }
        .details-btn {
            background: #433e8e;
            color: #fff;
            border: none;
            border-radius: 9px;
            padding: 7px 20px;
            font-size: 0.95rem;
            font-weight: 500;
            cursor: pointer;
            transition: background 0.15s;
        }
        .details-btn:hover {
            background: #2e266e;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="alltopics-container">
        <div class="topics-cardbox">
            <div class="topics-header-row">
                <a class="back-arrow" href="Education.aspx">&#8592;</a>
                Topics available
            </div>
            <div class="topics-grid">
<!-- Budgeting -->
<div class="topic-card">
    <img src="budgeting.png" alt="Budgeting" class="topic-img" />
    <div class="topic-title">Budgeting</div>
    <div class="topic-desc">Learn how to create a workable budget</div>
    <div class="topic-status-row">
        <div class="topic-status">Not started</div>
        <asp:Button runat="server" CssClass="details-btn" Text="View details" 
            OnClick="btnViewDetails_Click" CommandArgument="Budgeting" />
    </div>
</div>

<!-- Investing -->
<div class="topic-card">
    <img src="investing.png" alt="Investing" class="topic-img" />
    <div class="topic-title">Investing</div>
    <div class="topic-desc">Intro to investing and portfolio</div>
    <div class="topic-status-row">
        <div class="topic-status">Not started</div>
        <asp:Button runat="server" CssClass="details-btn" Text="View details" 
            OnClick="btnViewDetails_Click" CommandArgument="Investing" />
    </div>
</div>

<!-- Debt -->
<div class="topic-card">
    <img src="debt.png" alt="Debt" class="topic-img" />
    <div class="topic-title">Debt</div>
    <div class="topic-desc">Strategies for reducing debt</div>
    <div class="topic-status-row">
        <div class="topic-status">Not started</div>
        <asp:Button runat="server" CssClass="details-btn" Text="View details" 
            OnClick="btnViewDetails_Click" CommandArgument="Debt" />
    </div>
</div>

<!-- Tax -->
<div class="topic-card">
    <img src="tax.png" alt="Tax" class="topic-img" />
    <div class="topic-title">Tax</div>
    <div class="topic-desc">Smart tips to minimize your tax bill.</div>
    <div class="topic-status-row">
        <div class="topic-status">Not started</div>
        <asp:Button runat="server" CssClass="details-btn" Text="View details" 
            OnClick="btnViewDetails_Click" CommandArgument="Tax" />
    </div>
</div>

<!-- Credit management -->
<div class="topic-card">
    <img src="credit.png" alt="Credit management" class="topic-img" />
    <div class="topic-title">Credit management</div>
    <div class="topic-desc">Build, improve, and monitor your credit.</div>
    <div class="topic-status-row">
        <div class="topic-status">Not started</div>
        <asp:Button runat="server" CssClass="details-btn" Text="View details" 
            OnClick="btnViewDetails_Click" CommandArgument="Credit" />
    </div>
</div>

<!-- Risk management -->
<div class="topic-card">
    <img src="risk.png" alt="Risk management" class="topic-img" />
    <div class="topic-title">Risk management</div>
    <div class="topic-desc">Safeguard your finances against unexpected events.</div>
    <div class="topic-status-row">
        <div class="topic-status">Not started</div>
        <asp:Button runat="server" CssClass="details-btn" Text="View details" 
            OnClick="btnViewDetails_Click" CommandArgument="Risk" />
    </div>
</div>

<!-- Retirement -->
<div class="topic-card">
    <img src="retirement.png" alt="Retirement" class="topic-img" />
    <div class="topic-title">Retirement</div>
    <div class="topic-desc">Plan and invest for a secure retirement.</div>
    <div class="topic-status-row">
        <div class="topic-status">Not started</div>
        <asp:Button runat="server" CssClass="details-btn" Text="View details" 
            OnClick="btnViewDetails_Click" CommandArgument="Retirement" />
    </div>
</div>

<!-- Financial goals -->
<div class="topic-card">
    <img src="financialgoals.png" alt="Financial goals" class="topic-img" />
    <div class="topic-title">Financial goals</div>
    <div class="topic-desc">Set and track your money milestones.</div>
    <div class="topic-status-row">
        <div class="topic-status">Not started</div>
        <asp:Button runat="server" CssClass="details-btn" Text="View details" 
            OnClick="btnViewDetails_Click" CommandArgument="Goals" />
    </div>
</div>


            </div>
        </div>
    </div>
</asp:Content>
