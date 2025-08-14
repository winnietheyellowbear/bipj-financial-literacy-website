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
    grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
    gap: 32px 40px;
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
        .topics-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
    gap: 32px 40px;
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
  <asp:Repeater ID="rptModules" runat="server">
        <ItemTemplate>
    <div class="topic-card">
        <img src='<%# GetModuleImageUrl(Eval("ImageUrl")) %>'
     alt='<%# Eval("Name") %>'
     class="topic-img" />

        <div class="topic-title"><%# Eval("Name") %></div>
        <div class="topic-desc"><%# Eval("BriefDescription") %></div>

        <div class="topic-status-row">
            <span class="topic-status">Available</span>
            <a href='ViewSpecificEdu.aspx?moduleId=<%# Eval("Id") %>' class="details-btn">View</a>
        </div>
    </div>
</ItemTemplate>

    </asp:Repeater>





            </div>
        </div>
    </div>
</asp:Content>
