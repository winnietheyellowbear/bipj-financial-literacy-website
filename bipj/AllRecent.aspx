<%@ Page Title="All Recent" Language="C#" MasterPageFile="~/Customer_Nav_LoggedIn.Master" AutoEventWireup="true" CodeBehind="AllRecent.aspx.cs" Inherits="bipj.AllRecent" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .allrecent-container {
            background: #f9f8ff;
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            align-items: center;
            padding-bottom: 32px;
        }
        .recent-cardbox {
            background: #f4f2fd;
            border-radius: 18px;
            margin-top: 32px;
            padding: 32px 32px 32px 32px;
            width: 92vw;
            max-width: 700px;
        }
        .back-arrow {
            font-size: 2rem;
            font-weight: bold;
            color: #3b3350;
            margin-right: 18px;
            text-decoration: none;
            transition: color 0.1s;
        }
        .back-arrow:hover {
            color: #433e8e;
        }
        .recent-header-row {
            display: flex;
            align-items: center;
            font-size: 1.20rem;
            font-weight: bold;
            margin-bottom: 24px;
        }
        .recent-empty-row {
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 38px 0;
        }
        .recent-notfound-img {
            width: 110px;
            height: 110px;
            margin-right: 48px;
            object-fit: contain;
        }
        .recent-msg-box {
            display: flex;
            flex-direction: column;
            align-items: center;
        }
        .recent-msg {
            font-size: 1.08rem;
            color: #484848;
            margin-bottom: 20px;
        }
        .start-here-btn {
            background: #433e8e;
            color: #fff;
            border: none;
            border-radius: 9px;
            padding: 8px 30px;
            font-size: 1rem;
            font-weight: 500;
            cursor: pointer;
            transition: background 0.15s;
        }
        .start-here-btn:hover {
            background: #2e266e;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="allrecent-container">
        <div class="recent-cardbox">
            <div class="recent-header-row">
                <a class="back-arrow" href="Education.aspx">&#8592;</a>
                History
            </div>
            <div class="recent-empty-row">
                <img src="notfound.png" alt="Not Found" class="recent-notfound-img" />
                <div class="recent-msg-box">
                    <div class="recent-msg">No History of activity...</div>
                    <asp:Button runat="server" CssClass="start-here-btn" Text="Start here" OnClick="btnStartHere_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
