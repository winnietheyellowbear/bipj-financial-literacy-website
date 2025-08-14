<%@ Page Title="Education Modules" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.master" AutoEventWireup="true" CodeBehind="ViewEducationPage.aspx.cs" Inherits="bipj.ViewEducationPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<div class="container mt-4">
    <h2 class="mb-4">Education Modules</h2>
    
    <div class="row">
        <asp:Repeater ID="rptModules" runat="server">
            <ItemTemplate>
                <div class="col-md-4 mb-4">
                    <div class="card h-100">
                        <div class="card-img-top" style="height: 180px; background: #f5f5f5; display: flex; align-items: center; justify-content: center;">
                            <img src='<%# Eval("ImageUrl") ?? "/images/default-module.png" %>' 
                                 alt='<%# Eval("Name") %>' 
                                 style="max-height: 100%; max-width: 100%; object-fit: contain;" />
                        </div>
                        <div class="card-body">
                            <h5 class="card-title"><%# Eval("Name") %></h5>
                            <p class="card-text"><%# Eval("BriefDescription") %></p>
                            <a href='ViewSpecificEdu.aspx?moduleId=<%# Eval("Id") %>' 
                               class="btn btn-primary">View Module</a>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</div>
  
<style>
    .card {
        transition: transform 0.2s;
        border: 1px solid #e0e0e0;
        border-radius: 8px;
        overflow: hidden;
    }
    .card:hover {
        transform: translateY(-5px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }
    .card-img-top {
        background-color: #f8f9fa;
    }
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