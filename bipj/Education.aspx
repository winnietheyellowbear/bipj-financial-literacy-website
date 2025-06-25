<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_LoggedIn.Master" AutoEventWireup="true" CodeBehind="Education.aspx.cs" Inherits="bipj.Education" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* [Insert your CSS from before here, or keep it short since you have a MasterPage] */
        .section { margin-bottom: 36px; }
        .recommend-box { background: #fff; border-radius: 12px; box-shadow: 0 2px 8px #f2f2f2; padding: 22px; width: 660px; }
        .recommend-header { display: flex; align-items: center; }
        .flame { font-size: 22px; color: #ff5e00; margin-right: 8px; }
        .show-all-btn { margin-left: auto; background: #433e8e; color: #fff; border: none; border-radius: 7px; padding: 7px 16px; cursor: pointer; }
        .topic-list { display: flex; margin-top: 12px; }
        .topic-card { width: 160px; margin-right: 16px; background: #e8f0fe; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px #ececec; }
        .topic-card:last-child { margin-right: 0; }
        .topic-img { width: 100%; height: 90px; object-fit: cover; }
        .topic-content { padding: 8px 12px 13px 12px; }
        .topic-title { font-weight: bold; font-size: 1rem; color: #333; }
        .topic-desc { font-size: 0.9rem; color: #444; margin-top: 5px; }
        .activity-section { background: #f6f5ff; padding: 32px 0; border-radius: 16px; margin-bottom: 22px; }
        .section-title { font-weight: bold; font-size: 1.12rem; margin-bottom: 18px; display: flex; align-items: center; }
        .not-found-img { width: 85px; margin-right: 20px; }
        .activity-row { display: flex; align-items: center; }
        .activity-msg { color: #383838; }
        .see-btn { margin-left: 18px; background: #433e8e; color: #fff; border: none; border-radius: 6px; padding: 7px 16px; cursor: pointer; }
        .completed-section { background: #fcfcfc; padding: 24px 0; border-radius: 16px; }
        .container {
    /* Centers the container horizontally */
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
    padding: 24px 0;
    width: 100%;
    box-sizing: border-box;
}

/* Make recommended topics box always center */
.recommend-box {
    margin: 0 auto;
}

/* Center activity and completed sections as well */
.section,
.activity-section,
.completed-section {
    width: 80%;
    min-width: 320px;
    max-width: 900px;
    margin: 0 auto 36px auto;  /* Center and add space below */
}

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Recommended topics -->
    <div class="section">
        <div class="recommend-box">
            <div class="recommend-header">
                <span class="flame">&#128293;</span>
                <span style="font-weight:bold;font-size:1.04rem;">Recommended topics</span>
                <asp:Button runat="server" ID="btnShowAllTopics" CssClass="show-all-btn" Text="Show All Topics" OnClick="btnShowAllTopics_Click" />
            </div>
            <div class="topic-list">
                <div class="topic-card">
                    <img src="budgeting.png" alt="Budgeting" class="topic-img" />
                    <div class="topic-content">
                        <div class="topic-title">Budgeting</div>
                        <div class="topic-desc">Learn how to create a workable budget</div>
                    </div>
                </div>
                <div class="topic-card">
                    <img src="debt.png" alt="Debt" class="topic-img" />
                    <div class="topic-content">
                        <div class="topic-title">Debt</div>
                        <div class="topic-desc">Strategies for reducing debt</div>
                    </div>
                </div>
                <div class="topic-card">
                    <img src="investing.png" alt="Investing" class="topic-img" />
                    <div class="topic-content">
                        <div class="topic-title">Investing</div>
                        <div class="topic-desc">Intro to investing and portfolio</div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Recent Activities -->
    <div class="section activity-section">
        <div class="section-title">
            Recent Activities
            <asp:Button runat="server" ID="btnViewAllRecent" CssClass="show-all-btn" Text="View all" OnClick="btnViewAllRecent_Click" Style="margin-left:16px;" />
        </div>
        <div class="activity-row">
            <img src="notfound.png" class="not-found-img" alt="Not Found" />
            <span class="activity-msg">Oops seems you haven't started on any topics yet</span>
            <asp:Button runat="server" ID="btnSeeTopics" CssClass="see-btn" Text="See topics" OnClick="btnShowAllTopics_Click" />
        </div>
    </div>

    <!-- Completed topics -->
    <div class="section completed-section">
        <div class="section-title">
            Completed topics
            <asp:Button runat="server" ID="btnViewAllCompleted" CssClass="show-all-btn" Text="View all" OnClick="btnViewAllCompleted_Click" Style="margin-left:16px;" />
        </div>
        <img src="notfound.png" class="not-found-img" alt="Not Found" />
    </div>

</asp:Content>
