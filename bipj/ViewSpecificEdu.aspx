<%@ Page Title="Education Module" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="ViewSpecificEdu.aspx.cs" Inherits="bipj.ViewSpecificEdu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <style>
        .edu-viewer-container { display:flex; min-height:80vh; gap:0; }
        .edu-sidenav { background:#f8f9fa; width:280px; padding:20px; border-right:1px solid #ddd; }
        .topic-group { margin-bottom:18px; }
        .topic-title { color:#6a5fb4; font-weight:600; margin:0 0 8px 0; font-size:1.05rem; }
        .page-link { display:block; padding:6px 8px; border-radius:6px; text-decoration:none; color:#333; }
        .page-link:hover { background:#ecebfd; color:#2f2a6f; }
        .page-link.active { color:#2f2a6f; font-weight:700; background:#e6e4ff; }
        .edu-main { flex:1; padding:28px; }
        .edu-main h2 { margin-top:0; }
        .content-container { font-size:1.05rem; line-height:1.6; }
        .content-container img { max-width:100%; height:auto; border-radius:8px; margin:1rem 0; }
        .content-container iframe { width:100%; min-height:400px; border:none; border-radius:8px; margin:1rem 0; }
          .ai-assistant {
    position: fixed; right: 24px; bottom: 24px; z-index: 9999;
    width: 320px; background: #fff; border: 1px solid #e6e6e6;
    border-radius: 12px; box-shadow: 0 10px 30px rgba(0,0,0,.08); padding: 12px;
    font-family: system-ui, -apple-system, Segoe UI, Roboto, "Helvetica Neue", Arial, sans-serif;
  }
  .ai-header { display:flex; align-items:center; gap:10px; margin-bottom:10px; }
  .ai-header img { width:40px; height:40px; border-radius:50%; object-fit:cover; }
  .ai-title .ai-name { font-weight:600; }
  .ai-title .ai-topic { font-size:.85rem; color:#666; }
  .ai-response { min-height:60px; max-height:200px; overflow:auto; background:#fafafa; border-radius:8px; padding:10px; margin-bottom:10px; }
  .ai-input { display:flex; gap:8px; }
  .ai-input .form-control { flex:1; }
    </style>

    <div class="edu-viewer-container">
        <!-- Side Navigation -->
        <aside class="edu-sidenav">
            <h4 class="mb-3"><asp:Literal ID="ltModuleTitle" runat="server" /></h4>

            <asp:Repeater ID="rptTopics" runat="server" OnItemDataBound="rptTopics_ItemDataBound">
                <ItemTemplate>
                    <div class="topic-group">
                        <div class="topic-title"><%# Eval("TopicName") %></div>
                        <asp:Repeater ID="rptPages" runat="server" OnItemDataBound="rptPages_ItemDataBound">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkPage" runat="server" CssClass="page-link" />
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </aside>

        <!-- Main Content -->
        <main class="edu-main">
            <asp:Panel ID="pnlNoPageSelected" runat="server" Visible="true" CssClass="alert alert-info">
                Please select a page from the navigation.
            </asp:Panel>

            <asp:Panel ID="pnlPageContent" runat="server" Visible="false">
                <h2><asp:Literal ID="ltPageTitle" runat="server" /></h2>
                <hr />
                <div class="content-container">
                    <asp:Literal ID="ltPageContent" runat="server" />
                </div>
            </asp:Panel>
        </main>
    </div>
    <!-- Floating Study Assistant -->
<div id="ai-assistant" class="ai-assistant">
  <div class="ai-header">
    <img id="avatar" src="<%= ResolveUrl("~/Images/avatar_idle.png") %>" alt="AI" />
    <div class="ai-title">
      <div class="ai-name">Study Assistant</div>
      <div class="ai-topic">Topic: <span id="ai-topic"></span></div>
    </div>
  </div>

  <div id="ai-response" class="ai-response" aria-live="polite"></div>

  <div class="ai-input">
    <input id="ai-question" type="text" class="form-control" placeholder="Ask me about this module..." />
    <button id="ai-ask" type="button" class="btn btn-primary">Ask</button>
  </div>
</div>

    <script type="text/javascript">
  // --- dynamic topic (set by server; see code-behind) ---
  const TOPIC = "<%= TopicForAssistant %>";

  // DOM refs
  const avatar = document.getElementById("avatar");
  const responseBox = document.getElementById("ai-response");
  const inputBox = document.getElementById("ai-question");
  const askBtn = document.getElementById("ai-ask");
  document.getElementById("ai-topic").innerText = TOPIC;

  // Avatar animations
  const talkingFrames = [
    "<%= ResolveUrl("~/Images/avatar_talk1.png") %>",
    "<%= ResolveUrl("~/Images/avatar_talk2.png") %>",
    "<%= ResolveUrl("~/Images/avatar_talk3.png") %>"
  ];
  const idleFrames = ["<%= ResolveUrl("~/Images/avatar_idle.png") %>"];
  let talkInterval, idleInterval;

  function startTalkingAnimation() {
    let i = 0;
    clearInterval(talkInterval);
    talkInterval = setInterval(() => {
      avatar.src = talkingFrames[i % talkingFrames.length];
      i++;
    }, 100);
  }
  function stopTalkingAnimation() {
    clearInterval(talkInterval);
    avatar.src = idleFrames[0];
  }
  function startIdleAnimation() {
    let i = 0;
    clearInterval(idleInterval);
    idleInterval = setInterval(() => {
      avatar.src = idleFrames[i % idleFrames.length];
      i++;
    }, 800);
  }
  function stopIdleAnimation() { clearInterval(idleInterval); }

  function speakText(text) {
    stopIdleAnimation();
    const utter = new SpeechSynthesisUtterance(text);
    utter.onstart = startTalkingAnimation;
    utter.onend = () => { stopTalkingAnimation(); startIdleAnimation(); };
    speechSynthesis.speak(utter);
  }

  async function askAI() {
    const question = (inputBox.value || "").trim();
    if (!question) return;

    stopIdleAnimation();
    responseBox.textContent = "Thinking…";

    try {
      const res = await fetch('<%= ResolveUrl("~/StudyAssistant.aspx/GetAIResponse") %>', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ question: question, topic: TOPIC })
      });

      // PageMethods return JSON envelope { d: "answer" }
      const data = await res.json();
      const answer = data && (data.d || data.D) ? (data.d || data.D) : "Sorry, I couldn't get a response.";
      responseBox.textContent = answer;
      speakText(answer);
    } catch (e) {
      console.error(e);
      responseBox.textContent = "Sorry, something went wrong.";
    }
  }

  askBtn.addEventListener("click", askAI);
  inputBox.addEventListener("keydown", (e) => { if (e.key === "Enter") askAI(); });

  // boot
  window.addEventListener("load", startIdleAnimation);
    </script>

</asp:Content>