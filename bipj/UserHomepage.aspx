<%@ Page Title="FinClarity Home" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master"
    AutoEventWireup="true" CodeBehind="UserHomepage.aspx.cs" Inherits="bipj.UserHomepage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Lottie Animation -->
    <script src="https://unpkg.com/@lottiefiles/dotlottie-wc@0.6.2/dist/dotlottie-wc.js" type="module"></script>
    <style>
        /* Custom scrollbar for chat messages */
        .chat-container::-webkit-scrollbar {
            width: 8px;
        }
        .chat-container::-webkit-scrollbar-track {
            background: #f1f5f9;
        }
        .chat-container::-webkit-scrollbar-thumb {
            background: #cbd5e1;
            border-radius: 4px;
        }
        .chat-container::-webkit-scrollbar-thumb:hover {
            background: #94a3b8;
        }
        
        /* Floating button styles */
        .floating-chatbot-button {
            position: fixed;
            bottom: 30px;
            right: 30px;
            width: 80px;
            height: 80px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 50%;
            cursor: pointer;
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
            transition: all 0.3s ease;
            z-index: 10;
            display: flex;
            align-items: center;
            justify-content: center;
            overflow: hidden;
            border: none;
        }
        
        .floating-chatbot-button:hover {
            transform: scale(1.1);
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
        }
        
        .floating-chatbot-button.active {
            transform: scale(0.95);
        }
        
        /* Chat panel styles */
        .chat-panel {
            position: fixed;
            bottom: 120px;
            right: 30px;
            width: 400px;
            height: 600px;
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
            display: flex;
            flex-direction: column;
            z-index: 10;
            opacity: 0;
            transform: translateY(20px) scale(0.95);
            pointer-events: none;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        .chat-panel.open {
            opacity: 1;
            transform: translateY(0) scale(1);
            pointer-events: all;
        }
        
        /* Chat header */
        .chat-header {
            padding: 20px;
            background: linear-gradient(135deg, #3b82f6 0%, #4f46e5 100%);
            color: white;
            border-radius: 20px 20px 0 0;
            display: flex;
            align-items: center;
            justify-content: space-between;
            position: relative;
        }
        
        .chat-header-content {
            display: flex;
            align-items: center;
        }
        
        .chat-avatar {
            width: 40px;
            height: 40px;
            background: white;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-right: 12px;
        }
        
        .chat-avatar svg {
            width: 24px;
            height: 24px;
            color: #4f46e5;
        }
        
        .chat-title {
            font-size: 20px;
            font-weight: bold;
            margin: 0;
        }
        
        .chat-subtitle {
            font-size: 14px;
            color: rgba(255, 255, 255, 0.8);
            margin: 0;
        }
        
        /* Close button styles */
        .close-chat {
            position: absolute;
            top: 15px;
            right: 15px;
            width: 30px;
            height: 30px;
            background: rgba(255, 255, 255, 0.9);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
            transition: all 0.3s ease;
            z-index: 10;
            border: none;
        }
        
        .close-chat:hover {
            background: white;
            transform: rotate(90deg);
        }
        
        .close-chat svg {
            width: 16px;
            height: 16px;
            color: #6b7280;
        }
        
        /* Chat messages */
        .chat-messages {
            flex-grow: 1;
            padding: 20px;
            overflow-y: auto;
            background: #f9fafb;
        }
        
        .message-container {
            margin-bottom: 16px;
        }
        
        .message-container.user {
            display: flex;
            justify-content: flex-end;
        }
        
        .message-container.bot {
            display: flex;
            justify-content: flex-start;
        }
        
        .message-bubble {
            max-width: 280px;
            padding: 12px 16px;
            border-radius: 12px;
            font-size: 14px;
            line-height: 1.4;
        }
        
        .message-bubble.user {
            background: #3b82f6;
            color: white;
        }
        
        .message-bubble.bot {
            background: white;
            color: #374151;
            border: 1px solid #e5e7eb;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
        }
        
        .message-bubble ul {
            margin: 8px 0 0 0;
            padding-left: 16px;
        }
        
        .message-bubble li {
            font-size: 12px;
            margin-bottom: 2px;
        }
        
        /* Chat input */
        .chat-input {
            padding: 16px;
            background: white;
            border-top: 1px solid #e5e7eb;
            border-radius: 0 0 20px 20px;
        }
        
        .chat-input-container {
            display: flex;
            gap: 12px;
        }
        
        .chat-input-field {
            flex-grow: 1;
            padding: 12px 16px;
            border: 2px solid #d1d5db;
            border-radius: 12px;
            font-size: 14px;
            outline: none;
            transition: border-color 0.3s ease;
        }
        
        .chat-input-field:focus {
            border-color: #3b82f6;
        }
        
        .chat-send-btn {
            background: #3b82f6;
            color: white;
            border: none;
            padding: 12px 16px;
            border-radius: 12px;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.3s ease;
        }
        
        .chat-send-btn:hover {
            background: #2563eb;
        }
        
        .chat-send-btn:active {
            transform: scale(0.95);
        }
        
        .chat-send-btn svg {
            width: 20px;
            height: 20px;
        }
        
        /* Notification badge */
        .notification-badge {
            position: absolute;
            top: 5px;
            right: 5px;
            width: 20px;
            height: 20px;
            background: #ef4444;
            border-radius: 50%;
            display: none;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 12px;
            font-weight: bold;
            animation: pulse 2s infinite;
        }
        
        @keyframes pulse {
            0% {
                box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.7);
            }
            70% {
                box-shadow: 0 0 0 10px rgba(239, 68, 68, 0);
            }
            100% {
                box-shadow: 0 0 0 0 rgba(239, 68, 68, 0);
            }
        }
        
        /* Loading indicator */
        .loading-dots {
            display: flex;
            align-items: center;
        }
        
        .loading-dots .dot {
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: #9ca3af;
            margin: 0 2px;
            animation: loading-pulse 1.4s infinite ease-in-out both;
        }
        
        .loading-dots .dot:nth-child(1) { animation-delay: -0.32s; }
        .loading-dots .dot:nth-child(2) { animation-delay: -0.16s; }
        
        @keyframes loading-pulse {
            0%, 80%, 100% {
                transform: scale(0);
            }
            40% {
                transform: scale(1);
            }
        }
        
        /* Mobile responsiveness */
        @media (max-width: 768px) {
            .chat-panel {
                width: calc(100% - 20px);
                height: calc(100% - 100px);
                right: 10px;
                bottom: 90px;
                left: 10px;
            }
            
            .floating-chatbot-button {
                bottom: 20px;
                right: 20px;
                width: 70px;
                height: 70px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <!-- Main page content -->
    <div class="container mt-4">
        <div class="row">
            <div class="col-12">
                <h1 class="display-4 text-dark mb-3">Welcome to FinClarity</h1>
                <p class="lead text-muted">Your financial literacy journey starts here.</p>
            </div>
        </div>
    </div>

    <!-- Floating Chatbot Button -->
    <div class="floating-chatbot-button" id="chatbotButton">
        <dotlottie-wc 
            src="https://lottie.host/aeef10a6-b2b7-4a26-b660-6361d5bb4eb4/r5RVQOuBYE.lottie"
            style="width: 60px; height: 60px"
            speed="1"
            autoplay
            loop>
        </dotlottie-wc>
        <div class="notification-badge" id="notificationBadge">1</div>
    </div>

    <!-- Chat Panel -->
    <div class="chat-panel" id="chatPanel">
        <!-- Chat Header -->
        <div class="chat-header">
            <div class="chat-header-content">
                <div class="chat-avatar">
                    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"></path>
                    </svg>
                </div>
                <div>
                    <h2 class="chat-title">FinClarity Assistant</h2>
                    <p class="chat-subtitle">Here to help you navigate</p>
                </div>
            </div>
            <button class="close-chat" id="closeChat">
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                </svg>
            </button>
        </div>

        <!-- Chat Messages Container -->
        <div id="chatMessages" class="chat-messages chat-container">
            <!-- Initial welcome message from the bot -->
            <div class="message-container bot">
                <div class="message-bubble bot">
                    👋 Hello! Welcome to FinClarity! I'm here to help you navigate our platform. You can ask me about:
                    <ul>
                        <li>Dashboard</li>
                        <li>Profile</li>
                        <li>Bookings</li>
                        <li>Education</li>
                        <li>Discussions</li>
                        <li>Vouchers</li>
                    </ul>
                </div>
            </div>
        </div>

        <!-- Chat Input Form -->
        <div class="chat-input">
            <div class="chat-input-container">
                <input type="text" 
                       id="userInput" 
                       class="chat-input-field" 
                       placeholder="Type your question...">
                <button type="button" 
                        id="sendBtn" 
                        class="chat-send-btn">
                    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 5l7 7m0 0l-7 7m7-7H3"></path>
                    </svg>
                </button>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script type="text/javascript">
        // Chat state
        let isChatOpen = false;
        let unreadMessages = 0;

        // Ensure DOM is loaded before attaching events
        document.addEventListener('DOMContentLoaded', function () {
            // Get elements
            const chatbotButton = document.getElementById('chatbotButton');
            const chatPanel = document.getElementById('chatPanel');
            const closeChat = document.getElementById('closeChat');
            const sendBtn = document.getElementById('sendBtn');
            const userInput = document.getElementById('userInput');
            const notificationBadge = document.getElementById('notificationBadge');

            // Toggle chat panel
            function toggleChat() {
                isChatOpen = !isChatOpen;

                if (isChatOpen) {
                    chatPanel.classList.add('open');
                    chatbotButton.classList.add('active');
                    // Clear notification badge
                    unreadMessages = 0;
                    notificationBadge.style.display = 'none';
                    // Focus on input
                    setTimeout(() => userInput.focus(), 300);
                } else {
                    chatPanel.classList.remove('open');
                    chatbotButton.classList.remove('active');
                }
            }

            // Event listeners for opening/closing chat
            if (chatbotButton) {
                chatbotButton.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    toggleChat();
                });
            }

            if (closeChat) {
                closeChat.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    toggleChat();
                });
            }

            // Send message functionality
            if (sendBtn) {
                sendBtn.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    sendMessage();
                });
            }

            if (userInput) {
                userInput.addEventListener('keypress', function (e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        e.stopPropagation();
                        sendMessage();
                    }
                });
            }

            // Show notification badge after a delay (optional - for demo)
            setTimeout(() => {
                if (!isChatOpen) {
                    notificationBadge.style.display = 'flex';
                    unreadMessages = 1;
                }
            }, 5000);
        });

        // Helper function to append a message to the chat UI
        function appendMessage(sender, text) {
            const chatMessages = document.getElementById('chatMessages');
            if (!chatMessages) return;

            const messageDiv = document.createElement('div');
            messageDiv.className = 'message-container ' + sender;

            const messageBubble = document.createElement('div');
            messageBubble.className = 'message-bubble ' + sender;
            messageBubble.textContent = text;

            // If chat is closed and bot sends message, show notification
            if (!isChatOpen && sender === 'bot') {
                unreadMessages++;
                const notificationBadge = document.getElementById('notificationBadge');
                if (notificationBadge) {
                    notificationBadge.textContent = unreadMessages;
                    notificationBadge.style.display = 'flex';
                }
            }

            messageDiv.appendChild(messageBubble);
            chatMessages.appendChild(messageDiv);

            // Smooth scroll to bottom
            chatMessages.scrollTo({
                top: chatMessages.scrollHeight,
                behavior: 'smooth'
            });
        }

        // Main function to handle sending messages
        function sendMessage() {
            const userInput = document.getElementById('userInput');
            if (!userInput) return;

            const message = userInput.value.trim();

            if (message === '') {
                return;
            }

            // Add user message to chat
            appendMessage('user', message);
            userInput.value = ''; // Clear the input

            // Show loading indicator
            const loadingDiv = document.createElement('div');
            loadingDiv.className = 'message-container bot';
            loadingDiv.id = 'loadingMessage';
            const loadingBubble = document.createElement('div');
            loadingBubble.className = 'message-bubble bot';
            loadingBubble.innerHTML = '<div class="loading-dots"><div class="dot"></div><div class="dot"></div><div class="dot"></div></div>';
            loadingDiv.appendChild(loadingBubble);
            document.getElementById('chatMessages').appendChild(loadingDiv);

            // Scroll to show loading message
            document.getElementById('chatMessages').scrollTo({
                top: document.getElementById('chatMessages').scrollHeight,
                behavior: 'smooth'
            });

            // Make the API call
            fetch('UserHomepage.aspx/ChatHandler', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify({ message: message })
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    // Remove loading indicator
                    const loadingMsg = document.getElementById('loadingMessage');
                    if (loadingMsg) {
                        loadingMsg.remove();
                    }

                    // Add bot response
                    if (data && data.d) {
                        appendMessage('bot', data.d);
                    } else {
                        appendMessage('bot', 'Sorry, I am having trouble connecting right now. Please try again later.');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);

                    // Remove loading indicator
                    const loadingMsg = document.getElementById('loadingMessage');
                    if (loadingMsg) {
                        loadingMsg.remove();
                    }

                    appendMessage('bot', 'An error occurred while processing your request. Please try again.');
                });
        }
    </script>
</asp:Content>