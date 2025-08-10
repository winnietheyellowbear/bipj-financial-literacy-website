<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" CodeBehind="StudyAssistant.aspx.cs" Inherits="bipj.StudyAssistant" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="ai-avatar-container" style="text-align:center; margin-top:30px;">
    <img id="avatar" src="Images/avatar_idle.png" style="width:120px;" />
    <div style="margin-top:15px;">
        <asp:TextBox ID="txtQuestion" runat="server" CssClass="form-control" placeholder="Ask me something..." Width="400px" />
        <asp:Button ID="btnAsk" runat="server" Text="Ask" CssClass="btn btn-primary" OnClientClick="askAI(); return false;" OnClick="btnAsk_Click" />
    </div>
    <div id="ai-response" style="margin-top:20px; font-weight:bold;"></div>
</div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        window.onload = function () {
            startIdleAnimation();
            avatar.src = "Images/avatar_idle.png";
        };

        const avatar = document.getElementById("avatar");
        const responseBox = document.getElementById("ai-response");
        const textBox = document.getElementById("<%= txtQuestion.ClientID %>");

        const talkingFrames = ["Images/avatar_talk1.png", "Images/avatar_talk2.png", "Images/avatar_talk3.png"];
        let talkInterval;

        function startTalkingAnimation() {
            let index = 0;
            talkInterval = setInterval(() => {
                avatar.src = talkingFrames[index % talkingFrames.length];
                index++;
            }, 100);
        }

        function stopTalkingAnimation() {
            clearInterval(talkInterval);
            avatar.src = "Images/avatar_idle.png";
        }

        function speakText(text) {
            stopIdleAnimation(); // pause idle while speaking

            const utterance = new SpeechSynthesisUtterance(text);
            utterance.onstart = () => {
                startTalkingAnimation();
            };
            utterance.onend = () => {
                stopTalkingAnimation();
                startIdleAnimation(); // resume idle after speaking
            };
            speechSynthesis.speak(utterance);
        }


        function askAI() {
            const question = textBox.value.trim();
            const topic = "business"; // You can change this dynamically per page

            if (question === "") return;

            stopIdleAnimation();
            responseBox.innerText = "Thinking...";

            // AJAX call to WebMethod
            fetch('StudyAssistant.aspx/GetAIResponse', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
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


        function getDummyAnswer(q) {
            // Simple keyword-based mock response — replace with AI call if needed
            if (q.toLowerCase().includes("photosynthesis")) return "Photosynthesis is the process by which plants make food from sunlight.";
            if (q.toLowerCase().includes("gravity")) return "Gravity is a force that pulls objects toward the center of the Earth.";
            return "Sorry, I don't understand that yet!";
        }
        const idleFrames = ["Images/avatar_idle.png", "Images/avatar_idle.png", "Images/avatar_idle.png"];
        let idleInterval;

        function startIdleAnimation() {
            let index = 0;
            idleInterval = setInterval(() => {
                avatar.src = idleFrames[index % idleFrames.length];
                index++;
            }, 500); // Adjust speed if needed
        }

        function stopIdleAnimation() {
            clearInterval(idleInterval);
        }

    </script>

</asp:Content>
