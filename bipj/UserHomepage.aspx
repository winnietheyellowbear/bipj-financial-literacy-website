<%@ Page Title="FinClarity Home" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master"
    AutoEventWireup="true" CodeBehind="UserHomepage.aspx.cs" Inherits="bipj.UserHomepage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Lottie Animation -->
    <script src="https://unpkg.com/@lottiefiles/dotlottie-wc@0.6.2/dist/dotlottie-wc.js" type="module"></script>
    <style>
        /* Global Styles */
        body {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .main-container {
            min-height: 100vh;
            padding: 2rem 0;
        }

        /* Welcome Header Section */
        .welcome-header {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 25px;
            padding: 2rem;
            margin-bottom: 2rem;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
            display: flex;
            align-items: center;
            justify-content: space-between;
            position: relative;
            overflow: hidden;
        }

        .welcome-header::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 4px;
            background: linear-gradient(90deg, #667eea, #764ba2, #f093fb);
        }

        .welcome-content h1 {
            font-size: 2.5rem;
            margin: 0;
            color: #2d3748;
            font-weight: 700;
        }

        .welcome-content .user-name {
            background: linear-gradient(45deg, #667eea, #764ba2);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            font-weight: 800;
            text-shadow: 0 0 30px rgba(102, 126, 234, 0.3);
        }

        .welcome-subtitle {
            color: #718096;
            font-size: 1.2rem;
            margin: 0.5rem 0 0 0;
            font-weight: 400;
        }

        .welcome-animation {
            flex-shrink: 0;
        }

        /* Dashboard Cards Grid */
        .dashboard-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 1.5rem;
            margin-bottom: 2rem;
        }

        .dashboard-card {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 1.5rem;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            border: 1px solid rgba(255, 255, 255, 0.2);
            position: relative;
            overflow: hidden;
        }

        .dashboard-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 3px;
            background: var(--card-gradient, linear-gradient(90deg, #667eea, #764ba2));
        }

        .dashboard-card:hover {
            transform: translateY(-10px);
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
        }

        .card-header {
            display: flex;
            align-items: center;
            margin-bottom: 1rem;
        }

        .card-icon {
            width: 48px;
            height: 48px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-right: 1rem;
            font-size: 1.5rem;
        }

        .card-title {
            font-size: 1.3rem;
            font-weight: 700;
            color: #2d3748;
            margin: 0;
        }

        .card-subtitle {
            color: #718096;
            font-size: 0.9rem;
            margin: 0.25rem 0 0 0;
        }

        /* Progress Cards Specific Styles */
        .progress-card {
            --card-gradient: linear-gradient(90deg, #48bb78, #38a169);
        }

        .progress-card .card-icon {
            background: linear-gradient(135deg, #48bb78, #38a169);
            color: white;
        }

        .lesson-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 0.75rem 0;
            border-bottom: 1px solid rgba(0, 0, 0, 0.05);
        }

        .lesson-item:last-child {
            border-bottom: none;
        }

        .lesson-name {
            font-weight: 600;
            color: #2d3748;
        }

        .progress-bar {
            flex-grow: 1;
            height: 6px;
            background: #e2e8f0;
            border-radius: 3px;
            margin: 0 1rem;
            overflow: hidden;
        }

        .progress-fill {
            height: 100%;
            background: linear-gradient(90deg, #48bb78, #68d391);
            border-radius: 3px;
            transition: width 0.3s ease;
        }

        .progress-percentage {
            font-size: 0.85rem;
            font-weight: 600;
            color: #48bb78;
        }

        .resume-btn {
            background: linear-gradient(135deg, #667eea, #764ba2);
            color: white;
            border: none;
            padding: 0.5rem 1rem;
            border-radius: 10px;
            font-size: 0.85rem;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .resume-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
        }

        /* Quick Actions Card */
        .actions-card {
            --card-gradient: linear-gradient(90deg, #4299e1, #3182ce);
        }

        .actions-card .card-icon {
            background: linear-gradient(135deg, #4299e1, #3182ce);
            color: white;
        }

        .action-buttons {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 0.75rem;
        }

        .action-btn {
            background: linear-gradient(135deg, #4299e1, #3182ce);
            color: white;
            border: none;
            padding: 0.75rem 1rem;
            border-radius: 12px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            text-decoration: none;
            text-align: center;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .action-btn:hover {
            transform: translateY(-3px);
            box-shadow: 0 8px 20px rgba(66, 153, 225, 0.4);
            color: white;
            text-decoration: none;
        }

        /* Stats Card */
        .stats-card {
            --card-gradient: linear-gradient(90deg, #ed8936, #dd6b20);
        }

        .stats-card .card-icon {
            background: linear-gradient(135deg, #ed8936, #dd6b20);
            color: white;
        }

        .stats-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1rem;
        }

        .stat-item {
            text-align: center;
            padding: 0.75rem;
            background: rgba(237, 137, 54, 0.1);
            border-radius: 12px;
        }

        .stat-value {
            font-size: 1.5rem;
            font-weight: 700;
            color: #ed8936;
            margin: 0;
        }

        .stat-label {
            font-size: 0.85rem;
            color: #718096;
            margin: 0.25rem 0 0 0;
        }

        /* Community Card */
        .community-card {
            --card-gradient: linear-gradient(90deg, #9f7aea, #805ad5);
        }

        .community-card .card-icon {
            background: linear-gradient(135deg, #9f7aea, #805ad5);
            color: white;
        }

        .community-btn {
            width: 100%;
            background: linear-gradient(135deg, #9f7aea, #805ad5);
            color: white;
            border: none;
            padding: 1rem;
            border-radius: 12px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            margin-top: 0.5rem;
        }

        .community-btn:hover {
            transform: translateY(-3px);
            box-shadow: 0 8px 20px rgba(159, 122, 234, 0.4);
        }

        /* Floating chatbot styles */
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

        /* Custom scrollbar */
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

        /* Responsive Design */
        @media (max-width: 768px) {
            .welcome-header {
                flex-direction: column;
                text-align: center;
                padding: 1.5rem;
            }

            .welcome-content h1 {
                font-size: 2rem;
            }

            .dashboard-grid {
                grid-template-columns: 1fr;
                gap: 1rem;
            }

            .action-buttons {
                grid-template-columns: 1fr;
            }

            .stats-grid {
                grid-template-columns: 1fr;
            }

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

    <!-- JavaScript code moved here to the head section -->
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

            // Initialize user stats from server-side data
            initializeUserData();
        });

        // Initialize user data from server-side variables
        function initializeUserData() {
            // Data is already populated from server-side code-behind
            // This function can be extended for dynamic updates if needed
            console.log('User data initialized from server');
        }

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

            // Make the API call using the correct page method
            fetch('<%= ResolveUrl("~/UserHomepage.aspx/ChatHandler") %>', {
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

        // Function to update lesson progress dynamically (for future enhancement)
        function updateLessonProgress(lessons) {
            lessons.forEach((lesson, index) => {
                const progressBar = document.querySelector(`.lesson-item:nth-child(${index + 1}) .progress-fill`);
                const progressText = document.querySelector(`.lesson-item:nth-child(${index + 1}) .progress-percentage`);
                const lessonName = document.querySelector(`.lesson-item:nth-child(${index + 1}) .lesson-name`);

                if (progressBar && progressText && lessonName) {
                    progressBar.style.width = lesson.Progress + '%';
                    progressText.textContent = lesson.Progress + '%';
                    lessonName.textContent = lesson.Name;
                }
            });
        }

        // Function to refresh user stats dynamically (for future enhancement)
        function refreshUserStats() {
            fetch('<%= ResolveUrl("~/UserHomepage.aspx/GetUserStats") %>', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify({})
            })
                .then(response => response.json())
                .then(data => {
                    if (data && data.d) {
                        updateStatsDisplay(data.d);
                    }
                })
                .catch(error => {
                    console.error('Error refreshing user stats:', error);
                });
        }

        // Function to update stats display
        function updateStatsDisplay(stats) {
            // Update points
            const pointsElement = document.querySelector('.stat-item:nth-child(1) .stat-value');
            if (pointsElement && stats.Points !== undefined) {
                pointsElement.textContent = stats.Points.toLocaleString();
            }

            // Update lessons completed
            const lessonsElement = document.querySelector('.stat-item:nth-child(2) .stat-value');
            if (lessonsElement && stats.CompletedLessons !== undefined) {
                lessonsElement.textContent = stats.CompletedLessons;
            }

            // Update advisor sessions
            const sessionsElement = document.querySelector('.stat-item:nth-child(3) .stat-value');
            if (sessionsElement && stats.AdvisorSessions !== undefined) {
                sessionsElement.textContent = stats.AdvisorSessions;
            }

            // Update days active
            const daysElement = document.querySelector('.stat-item:nth-child(4) .stat-value');
            if (daysElement && stats.DaysActive !== undefined) {
                daysElement.textContent = stats.DaysActive;
            }
        }

        // Prevent form submission on Enter key in chat input
        document.addEventListener('keydown', function (e) {
            if (e.target.id === 'userInput' && e.key === 'Enter') {
                e.preventDefault();
            }
        });

        // Handle server-side button click completion (if needed)
        function onDiscussionRedirect() {
            // This function can be called after server-side redirect completes
            // if you need to perform any client-side actions
            console.log('Redirecting to Discussion page...');
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="main-container">
        <div class="container">
            <!-- Welcome Header with Animation -->
            <div class="welcome-header">
                <div class="welcome-content">
                    <h1>Welcome back, <span class="user-name"><%= UserName %>!</span></h1>
                    <p class="welcome-subtitle">Ready to continue your financial literacy journey? 🚀</p>
                </div>
                <div class="welcome-animation">
                    <dotlottie-wc
                        src="https://lottie.host/6ed92f63-3227-4a01-a8f2-14fb3f25cce5/q4qO3pPHBl.lottie"
                        style="width: 200px; height: 200px"
                        speed="1"
                        autoplay
                        loop>
                    </dotlottie-wc>
                </div>
            </div>

            <!-- Dashboard Cards Grid -->
            <div class="dashboard-grid">
                <!-- Resume Lessons Card -->
                <div class="dashboard-card progress-card">
                    <div class="card-header">
                        <div class="card-icon">
                            📚
                        </div>
                        <div>
                            <h3 class="card-title">Resume Lessons</h3>
                            <p class="card-subtitle">Continue your learning journey</p>
                        </div>
                    </div>
                    
                    <!-- Lesson progress will be populated from database -->
                    <div class="lesson-item">
                        <span class="lesson-name">Debt Management</span>
                        <div class="progress-bar">
                            <div class="progress-fill" style="width: 70%"></div>
                        </div>
                        <span class="progress-percentage">70%</span>
                        <button class="resume-btn" onclick="location.href='Education.aspx'">Resume</button>
                    </div>
                    
                    <div class="lesson-item">
                        <span class="lesson-name">Investment Basics</span>
                        <div class="progress-bar">
                            <div class="progress-fill" style="width: 50%"></div>
                        </div>
                        <span class="progress-percentage">50%</span>
                        <button class="resume-btn" onclick="location.href='Education.aspx'">Resume</button>
                    </div>
                    
                    <div class="lesson-item">
                        <span class="lesson-name">Insurance Planning</span>
                        <div class="progress-bar">
                            <div class="progress-fill" style="width: 90%"></div>
                        </div>
                        <span class="progress-percentage">90%</span>
                        <button class="resume-btn" onclick="location.href='Education.aspx'">Resume</button>
                    </div>
                </div>

                <!-- Quick Actions Card -->
                <div class="dashboard-card actions-card">
                    <div class="card-header">
                        <div class="card-icon">
                            ⚡
                        </div>
                        <div>
                            <h3 class="card-title">Quick Actions</h3>
                            <p class="card-subtitle">Jump to your favorite features</p>
                        </div>
                    </div>
                    
                    <div class="action-buttons">
                        <a href="Dashboard.aspx" class="action-btn">📊 Dashboard</a>
                        <a href="AllProfile.aspx" class="action-btn">👤 Profile</a>
                        <a href="BookingForum.aspx" class="action-btn">📅 Book Advisor</a>
                        <a href="VoucherExchange.aspx" class="action-btn">🎁 Rewards</a>
                    </div>
                </div>

                <!-- Your Stats Card - Now using server-side data -->
                <div class="dashboard-card stats-card">
                    <div class="card-header">
                        <div class="card-icon">
                            📈
                        </div>
                        <div>
                            <h3 class="card-title">Your Stats</h3>
                            <p class="card-subtitle">Track your progress</p>
                        </div>
                    </div>
                    
                    <div class="stats-grid">
                        <div class="stat-item">
                            <p class="stat-value"><%= UserPoints.ToString("N0") %></p>
                            <p class="stat-label">Points Earned</p>
                        </div>
                        <div class="stat-item">
                            <p class="stat-value"><%= CompletedLessons %></p>
                            <p class="stat-label">Lessons Completed</p>
                        </div>
                        <div class="stat-item">
                            <p class="stat-value"><%= AdvisorSessions %></p>
                            <p class="stat-label">Advisor Sessions</p>
                        </div>
                        <div class="stat-item">
                            <p class="stat-value"><%= DaysActive %></p>
                            <p class="stat-label">Days Active</p>
                        </div>
                    </div>
                </div>

                <!-- Community Card - Now using server-side button -->
                <div class="dashboard-card community-card">
                    <div class="card-header">
                        <div class="card-icon">
                            💬
                        </div>
                        <div>
                            <h3 class="card-title">Join the Community</h3>
                            <p class="card-subtitle">Connect with other learners</p>
                        </div>
                    </div>
                    
                    <p style="margin-bottom: 1rem; color: #718096;">
                        Share your experiences, ask questions, and learn from others on their financial journey!
                    </p>
                    
                    <!-- Using server-side button instead of JavaScript -->
                    <asp:Button ID="btnExploreDiscussions" runat="server" 
                        Text="🌟 Explore Discussions" 
                        CssClass="community-btn" 
                        OnClick="btnExploreDiscussions_Click" 
                        UseSubmitBehavior="False" />
                </div>
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
                    👋 Hello <%= UserName %>! Welcome to FinClarity! I'm here to help you navigate our platform. You can ask me about:
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

    <!-- Hidden field for user ID (for JavaScript functionality) -->
    <asp:HiddenField ID="hdnUserId" runat="server" />
</asp:Content>