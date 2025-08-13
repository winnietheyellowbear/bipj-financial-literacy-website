<%@ Page Title="Manage Education" Language="C#" MasterPageFile="~/Staff_Nav.Master" AutoEventWireup="true" CodeBehind="ManageEducation.aspx.cs" Inherits="bipj.ManageEducation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css" />
    <style>
        .edu-wrapper { min-height: 75vh; padding: 30px 5vw 10px 5vw; }
        .edu-header { font-size: 1.3rem; font-weight: 600; margin-bottom: 30px; display: flex; align-items: center; }
        .edu-card-list { display: flex; gap: 30px; flex-wrap: wrap; margin-top: 30px; }
        .edu-card {
            position: relative; /* for the X badge */
            overflow: visible;
            background: #fff; border-radius: 1.5rem; box-shadow: 0 4px 16px rgba(80,60,160,0.08);
            width: 220px; min-height: 250px; display: flex; flex-direction: column; align-items: center; justify-content: flex-start;
            transition: box-shadow 0.12s, border 0.12s; border: 3px solid transparent;
        }
        .edu-card.selected, .edu-card:focus-within { border: 3px solid #4e57c7; box-shadow: 0 0 0 3px #b8beee; }
        .edu-card img { margin-top: 1.2rem; border-radius: 1.1rem; width: 160px; height: 100px; object-fit: cover; background: #eef; }
        .edu-title { margin-top: 15px; font-size: 1.17rem; font-weight: 600; color: #333; text-align: center; }
        .edu-manage-btn { margin: 22px 0 12px 0; background: #4e57c7; color: #fff; border: none; border-radius: 10px; padding: 4px 32px; font-size: 1rem; transition: background 0.12s; }
        .edu-manage-btn:hover { background: #393e87; }

        .edu-actions { position: absolute; right: 7vw; top: 44px; display: flex; flex-direction: column; gap: 18px; }
        .edu-action-btn { display: flex; align-items: center; gap: 6px; background: #393e87; color: #fff; font-weight: 500; border: none; border-radius: 10px; padding: 7px 19px; margin-bottom: 3px; font-size: 1rem; transition: background 0.12s; }
        .edu-action-btn.add { background: #4ba675; }
        .edu-action-btn.add:hover { background: #319b64; }
        .edu-action-btn.del { background: #d24d53; }
        .edu-action-btn.del:hover { background: #a33237; }
        .edu-action-btn[disabled] { background: #bcbcbc; color: #fff; pointer-events: none; }

        .edu-no-topics { font-size: 1.1rem; color: #555; margin-top: 2.2rem; text-align: center; font-style: italic; }

        /* delete selector (red X) */
        .del-x {
            display: none; /* shown in delete mode */
            position: absolute; top: 8px; right: 8px; width: 36px; height: 36px; border-radius: 999px;
            background: #d24d53; box-shadow: 0 2px 8px rgba(0,0,0,.12); z-index: 5;
            cursor: pointer; align-items: center; justify-content: center; user-select: none; pointer-events: auto;
        }
        .del-x input[type="checkbox"] { display: none; }
        .del-x-icon { color: #fff; font-weight: 800; font-size: 20px; line-height: 1; transform: translateY(-1px); }

        /* show Xs & disable manage button in delete mode */
        .delete-mode .del-x { display: inline-flex; }
        .delete-mode .edu-card .edu-manage-btn { pointer-events: none; opacity: .5; }

        /* dim selected card (requires :has support) */
        .delete-mode .edu-card:has(.del-x input:checked) { opacity: .55; }

        @media (max-width: 850px) {
            .edu-wrapper { padding: 18px 2vw; }
            .edu-actions { right: 2vw; }
            .edu-card-list { gap: 18px; }
            .edu-card { width: 95vw; max-width: 330px; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="edu-wrapper position-relative">
        <div id="eduRoot" runat="server">
            <div class="edu-header">
                <a href="AdminPage.aspx" style="text-decoration:none; color:inherit; margin-right:12px;">
                    <i class="bi bi-arrow-left" style="font-size:1.7rem;"></i>
                </a>
                Current topics
            </div>

            <div class="edu-actions">
                <asp:LinkButton ID="btnAddTopic" runat="server" CssClass="edu-action-btn add" OnClick="btnAddTopic_Click">
                    <i class="bi bi-plus-circle"></i> add new topics
                </asp:LinkButton>

                <asp:LinkButton ID="btnEnterDelete" runat="server" CssClass="edu-action-btn del" OnClick="btnEnterDelete_Click">
                    <i class="bi bi-x-circle"></i> Delete topics
                </asp:LinkButton>

                <asp:LinkButton ID="btnConfirmDelete" runat="server" CssClass="edu-action-btn del"
                    OnClick="btnConfirmDelete_Click"
                    OnClientClick="return confirm('Delete selected topics? This cannot be undone.');"
                    Visible="false">
                    <i class="bi bi-check2-circle"></i> Confirm delete
                </asp:LinkButton>

                <asp:LinkButton ID="btnCancelDelete" runat="server" CssClass="edu-action-btn" OnClick="btnCancelDelete_Click" Visible="false">
                    <i class="bi bi-arrow-counterclockwise"></i> Cancel
                </asp:LinkButton>
            </div>

            <asp:Panel ID="pnlNoTopics" runat="server" Visible="false" CssClass="edu-no-topics">
                No topics created.
            </asp:Panel>

            <asp:Panel ID="Panel1" runat="server">
                <asp:Repeater ID="rptTopics" runat="server">
                    <HeaderTemplate>
                        <div class="edu-card-list" id="eduCardList">
                    </HeaderTemplate>

                    <ItemTemplate>
                        <div class="edu-card" tabindex="0">
                            <asp:HiddenField ID="hfModuleId" runat="server" Value='<%# Eval("Id") %>' />
                            <!-- red X selector -->
                            <label class="del-x">
                                <asp:CheckBox ID="chkSelect" runat="server" />
                                <span class="del-x-icon">×</span>
                            </label>

                            <img alt="Module image"
                                 src='<%# ResolveUrl(string.IsNullOrWhiteSpace(Eval("ImageUrl") as string) 
                                         ? "~/Images/edu-placeholder.jpg" 
                                         : Eval("ImageUrl").ToString()) %>' />

                            <div class="edu-title"><%# Eval("Name") %></div>
                            <div style="padding:0 14px; text-align:center; color:#666; font-size:0.95rem;">
                              <%# Eval("BriefDescription") %>
                            </div>

                            <a href='<%# "EditEducationPage.aspx?moduleId=" + Eval("Id") + "&pageId=0" %>' class="edu-manage-btn">
                                Manage Module
                            </a>
                        </div>
                    </ItemTemplate>

                    <FooterTemplate>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>
            </asp:Panel>
        </div> <!-- /#eduRoot -->
    </div>
</asp:Content>
