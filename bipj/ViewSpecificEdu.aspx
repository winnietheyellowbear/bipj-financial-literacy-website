<%@ Page Title="Education Module" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="ViewSpecificEdu.aspx.cs" Inherits="bipj.ViewSpecificEdu" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <!-- Add this to your head section -->
<script src="https://cdn.jsdelivr.net/npm/@editorjs/editorjs@2.26.5"></script>
<div class="edu-viewer-container" style="display:flex; min-height:80vh;">
    <!-- Side Navigation -->
    <div class="edu-sidenav" style="background:#f8f9fa; width:250px; padding:20px; border-right:1px solid #ddd;">
        <h4 class="mb-4"><asp:Literal ID="ltModuleTitle" runat="server" /></h4>
        
        <asp:Repeater ID="rptTopics" runat="server" OnItemDataBound="rptTopics_ItemDataBound">
            <ItemTemplate>
                <div class="mb-3">
                    <h5 style="color:#8576b1; font-weight:600;"><%# Eval("TopicName") %></h5>
                    <div class="pl-3">
                        <asp:Repeater ID="rptPages" runat="server">
                            <ItemTemplate>
                                <a href='ViewSpecificEdu.aspx?moduleId=<%# Eval("ModuleId") %>&pageId=<%# Eval("Id") %>'
                                   class='d-block mb-2 <%# (Eval("Id").ToString() == Request.QueryString["pageId"]) ? "text-primary font-weight-bold" : "text-dark" %>'>
                                   <i class='bi bi-file-text mr-2'></i><%# Eval("Title") %>
                                </a>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <!-- Main Content Area -->
    <div style="flex:1; padding:30px;">
        <asp:Panel ID="pnlNoPageSelected" runat="server" Visible="true" CssClass="alert alert-info">
            Please select a page from the navigation
        </asp:Panel>
        
        <asp:Panel ID="pnlPageContent" runat="server" Visible="false">
            <h2><asp:Literal ID="ltPageTitle" runat="server" /></h2>
            <hr />
            <div class="content-container mt-4">
    <asp:Literal ID="ltPageContent" runat="server" />
</div>
        </asp:Panel>
    </div>
</div>
        

<style>
    figure.media {
    max-width: 800px;
    margin: 20px auto; /* Center the video */
}

figure.media iframe {
    width: 100% !important;
    height: auto !important;
    aspect-ratio: 16 / 9;
    border-radius: 8px; /* Optional rounded corners */
    box-shadow: 0 0 10px rgba(0,0,0,0.1); /* Optional soft shadow */
}
    #editorjs-content {
        line-height: 1.6;
        font-size: 1.1rem;
    }
    #editorjs-content p {
        margin-bottom: 1rem;
    }
    #editorjs-content img {
        max-width: 100%;
        height: auto;
        border-radius: 8px;
        margin: 1rem 0;
    }
    #editorjs-content iframe {
        width: 100%;
        min-height: 400px;
        border: none;
        border-radius: 8px;
        margin: 1rem 0;
    }
    #editorjs-content ul, 
    #editorjs-content ol {
        margin-bottom: 1rem;
        padding-left: 2rem;
    }
      .content-container {
        font-size: 1.1rem;
        line-height: 1.6;
    }

    .content-container img {
        max-width: 100%;
        height: auto;
        border-radius: 8px;
        margin: 1rem 0;
    }

    .content-container iframe {
        width: 100%;
        height: 400px;
        border: none;
        border-radius: 8px;
        margin: 1rem 0;
    }

    .content-container p {
        margin-bottom: 1rem;
    }
    .chat-bubble {
    background: #ffffff;
    border: 2px solid #433e8e;
    color: #333;
    padding: 10px 15px;
    border-radius: 12px;
    max-width: 260px;
    font-size: 0.95rem;
    position: absolute;
    bottom: 140px;
    right: 10px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
    display: none;
    .ai-chat-wrapper {
    display: flex;
    align-items: center;
    position: relative;
}

.ai-dialogue-box {
    background-color: #fff;
    border: 2px solid #433e8e;
    color: #333;
    padding: 10px 15px;
    border-radius: 12px;
    max-width: 200px;
    font-size: 0.95rem;
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
    min-height: 50px;
    display: flex;
    align-items: center;
    justify-content: center;
}


</style>
    <script>
        const avatar = document.getElementById("aiAvatar");
        const dialogueBox = document.getElementById("aiDialogueBox");
        const responseBox = document.getElementById("ai-response");
        const textBox = document.getElementById("txtQuestion");

        const talkingFrames = ["Images/avatar_talk1.png", "Images/avatar_talk2.png", "Images/avatar_talk3.png"];
        const idleFrames = ["Images/avatar_idle.png"];
        let talkInterval, idleInterval;

        window.onload = function () {
            startIdleAnimation();
        };

        function startIdleAnimation() {
            let index = 0;
            idleInterval = setInterval(() => {
                avatar.src = idleFrames[index % idleFrames.length];
                index++;
            }, 800);
        }

        function stopIdleAnimation() {
            clearInterval(idleInterval);
        }

        function startTalkingAnimation() {
            let index = 0;
            stopIdleAnimation();
            talkInterval = setInterval(() => {
                avatar.src = talkingFrames[index % talkingFrames.length];
                index++;
            }, 150);
        }

        function stopTalkingAnimation() {
            clearInterval(talkInterval);
            avatar.src = "Images/avatar_idle.png";
        }

        function speakText(text) {
            dialogueBox.innerText = text;

            const utterance = new SpeechSynthesisUtterance(text);
            utterance.onstart = () => startTalkingAnimation();
            utterance.onend = () => {
                stopTalkingAnimation();
                startIdleAnimation();
            };
            speechSynthesis.speak(utterance);
        }

        function askAI() {
            const question = textBox.value.trim();
            const topic = "<%= ltModuleTitle.Text %>";

            if (question === "") return;

            responseBox.innerText = "Thinking...";
            dialogueBox.innerText = "Thinking...";

            fetch('ViewSpecificEdu.aspx/GetAIResponse', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ question: question, topic: topic })
            })
                .then(res => res.json())
                .then(data => {
                    const response = data.d;
                    responseBox.innerText = response;
                    speakText(response);
                })
                .catch(err => {
                    responseBox.innerText = "Sorry, something went wrong.";
                    dialogueBox.innerText = "Something went wrong.";
                    console.error(err);
                });
        }
    </script>

    <div id="ai-chatbot-container" style="position: fixed; bottom: 20px; right: 20px; width: 300px; z-index: 1000;">
    <div style="background: #fff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); padding: 15px; text-align: center;">
        <div class="ai-chat-wrapper">
    <div class="ai-dialogue-box" id="aiDialogueBox">Hi! I'm your assistant. Ask me anything about this topic.</div>
    <img src="Images/avatar_idle.png" id="aiAvatar" class="ai-avatar" />
</div>
        <div class="chat-bubble" id="chatBubble">Hi! Ask me something.</div>

        <input type="text" id="txtQuestion" placeholder="Ask a question..." style="width: 100%; margin-top: 10px;" class="form-control" />
        <button class="btn btn-primary mt-2" onclick="askAI()">Ask</button>
        <div id="ai-response" style="margin-top: 10px; font-size: 0.9rem;"></div>
    </div>
</div>
</asp:Content>

<asp:Content ID="ScriptSection" ContentPlaceHolderID="scripts" runat="server">
<script>
    const avatar = document.getElementById("aiAvatar");
    const dialogueBox = document.getElementById("aiDialogueBox");
    const talkingFrames = ["Images/avatar_talk1.png", "Images/avatar_talk2.png", "Images/avatar_talk3.png"];
    const idleFrames = ["Images/avatar_idle.png"]; // Only 1 for now
    let talkInterval, idleInterval;
    window.onload = function () {
        const avatar = document.getElementById("aiAvatar");
        if (avatar) {
            startIdleAnimation();
        }
    };

    const responseBox = document.getElementById("ai-response");
    const textBox = document.getElementById("txtQuestion");

    const idleFrames = ["Images/avatar_idle.png"];
    const talkingFrames = ["Images/avatar_talk1.png", "Images/avatar_talk2.png", "Images/avatar_talk3.png"];
    let idleInterval, talkInterval;

    function startTalkingAnimation() {
        let index = 0;
        stopIdleAnimation();
        talkInterval = setInterval(() => {
            avatar.src = talkingFrames[index % talkingFrames.length];
            index++;
        }, 150);
    }

    function stopTalkingAnimation() {
        clearInterval(talkInterval);
        avatar.src = "Images/avatar_idle.png";
    }

    function startIdleAnimation() {
        let index = 0;
        idleInterval = setInterval(() => {
            avatar.src = idleFrames[index % idleFrames.length];
            index++;
        }, 800);
    }

    function stopIdleAnimation() {
        clearInterval(idleInterval);
    }

    function speakText(text) {
        dialogueBox.innerText = text;

        const utterance = new SpeechSynthesisUtterance(text);
        utterance.onstart = () => {
            startTalkingAnimation();
        };
        utterance.onend = () => {
            stopTalkingAnimation();
            startIdleAnimation();
        };
        speechSynthesis.speak(utterance);
    }

    function askAI() {
        const question = textBox.value.trim();
        const topic = "<%= ltModuleTitle.Text %>";

        if (question === "") return;

        responseBox.innerText = "Thinking...";
        fetch('ViewSpecificEdu.aspx/GetAIResponse', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ question: question, topic: topic })
        })
            .then(res => res.json())
            .then(data => {
                const response = data.d;
                responseBox.innerText = response;
                speakText(response);
            })
            .catch(err => {
                responseBox.innerText = "Sorry, something went wrong.";
                console.error(err);
            });
    }

    window.onload = startIdleAnimation;
</script>
</asp:Content>
