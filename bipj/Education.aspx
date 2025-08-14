<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_LoggedIn.Master" AutoEventWireup="true" CodeBehind="Education.aspx.cs" Inherits="bipj.Education" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* [Insert your CSS from before here, or keep it short since you have a MasterPage] */
        .section { margin-bottom: 36px; }
        .recommend-box { background: #fff; border-radius: 12px; box-shadow: 0 2px 8px #f2f2f2; padding: 22px; width: 660px; }
        .recommend-header { display: flex; align-items: center; }
        .flame { font-size: 22px; color: #ff5e00; margin-right: 8px; }
        .show-all-btn { margin-left: auto; background: #433e8e; color: #fff; border: none; border-radius: 7px; padding: 7px 16px; cursor: pointer; }
        .topic-list {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;               /* replaces margin-right on cards */
  margin-top: 12px;
}

.topic-card {
  width: 160px;
  background: #e8f0fe;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px #ececec;
  display: flex;
  flex-direction: column;
}

.topic-img { 
  width: 100%; 
  height: 90px; 
  object-fit: cover; 
  display: block;        /* removes inline-img baseline gap */
}
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

    <asp:Repeater ID="rptRecommendedModules" runat="server">
         <HeaderTemplate><div class="topic-list"></HeaderTemplate>
      <ItemTemplate>
        <div class="topic-card">
          <img src='<%# Eval("ImageUrl") %>' alt='<%# Eval("Name") %>' class="topic-img" />
          <div class="topic-content">
            <div class="topic-title"><%# Eval("Name") %></div>
            <div class="topic-desc"><%# Eval("BriefDescription") %></div>
            <div class="topic-status-row" style="margin-top:8px;">
              <a class="details-btn" href='ViewSpecificEdu.aspx?moduleId=<%# Eval("Id") %>'>Start</a>
            </div>
          </div>
        </div>
      </ItemTemplate>
         <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>

    <asp:Panel ID="pnlNoRecommendations" runat="server" Visible="false">
      <span>No recommendations available right now.</span>
    </asp:Panel>
  </div>
</div>


    <!-- Recent Activities -->
    <div class="section activity-section">
        <div class="section-title">
            Recent Activities
            <asp:Button runat="server" ID="btnViewAllRecent" CssClass="show-all-btn" Text="View all" OnClick="btnViewAllRecent_Click" Style="margin-left:16px;" />
        </div>
        <asp:Repeater ID="rptRecentModules" runat="server">
              <HeaderTemplate><div class="topic-list"></HeaderTemplate>
    <ItemTemplate>
        <div class="topic-card">
            <img src='<%# Eval("ImageUrl") %>' alt='<%# Eval("Name") %>' class="topic-img" />
            <div class="topic-title"><%# Eval("Name") %></div>
            <div class="topic-desc"><%# Eval("BriefDescription") %></div>
            <div class="topic-status-row">
                <a class="details-btn" href='ViewSpecificEdu.aspx?moduleId=<%# Eval("Id") %>'>Continue</a>
            </div>
        </div>
    </ItemTemplate>
            <FooterTemplate></div></FooterTemplate>
</asp:Repeater>
        <asp:Panel ID="pnlNoRecent" runat="server" Visible="false" CssClass="activity-row">
    <img src="notfound.png" class="not-found-img" alt="Not Found" />
    <span class="activity-msg">Oops! Looks like you haven't started any modules yet.</span>
</asp:Panel>
    </div>

   <!-- Completed topics -->
<div class="section completed-section">
    <div class="section-title">
        Completed topics
        <asp:Button runat="server" ID="btnViewAllCompleted" CssClass="show-all-btn" Text="View all" OnClick="btnViewAllCompleted_Click" Style="margin-left:16px;" />
    </div>

    <asp:Repeater ID="rptCompletedModules" runat="server">
         <HeaderTemplate><div class="topic-list"></HeaderTemplate>
        <ItemTemplate>
            <div class="topic-card">
                <img src='<%# Eval("ImageUrl") %>' alt='<%# Eval("Name") %>' class="topic-img" />
                <div class="topic-content">
                    <div class="topic-title"><%# Eval("Name") %></div>
                    <div class="topic-desc"><%# Eval("BriefDescription") %></div>
                    <a href='ViewSpecificEdu.aspx?moduleId=<%# Eval("Id") %>' class="details-btn">Review</a>
                </div>
            </div>
        </ItemTemplate>
          <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>

    <asp:Panel ID="pnlNoCompleted" runat="server" Visible="false">
        <div class="activity-row">
            <img src="notfound.png" class="not-found-img" alt="Not Found" />
            <span class="activity-msg">Oops! Looks like you haven't completed any modules yet.</span>
        </div>
    </asp:Panel>
</div>


</asp:Content>
