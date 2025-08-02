<%@ Page Async="true" Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="Post.aspx.cs" Inherits="bipj.Post" MaintainScrollPositionOnPostBack="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <head>
        <link rel="stylesheet" href="Forum_Nav.css">
        <link rel="stylesheet" href="Forum_Post.css">
    </head>

    <style>
        .content-wrapper {
            display: flex;
            align-items: flex-start;
        }

        .main-content {
            flex: 1;
            background-color: #f8f9fa;
            border-radius: 10px;
            margin-left: 20px;
            max-width: 1000px;
        }

        /* Form Styling */
        .form {
            padding: 30px;
            background-color: white;
            border-radius: 10px;
            box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.2);
            max-width: 500px;
            margin: 20px auto;
            flex: 1;
        }

        .form-group {
            margin-bottom: 15px;
        }

        .form-group label {
            font-weight: bold;
            display: block;
            margin-bottom: 5px;
        }

        .form-group input,
        .form-group textarea {
            width: 95%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 5px;
            font-size: 14px;
            transition: border 0.3s ease;
        }

        .form-group input:focus,
        .form-group textarea:focus {
            border-color: #007bff;
            outline: none;
        }

        /* File Upload Styling */
        .file-upload {
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .file-upload input {
            width: auto;
        }

        /* Radio Button List */
        .radio-group {
            display: flex;
            gap: 30px;
            margin-top: 10px;
        }

        /* Button Styling */
        .btn-submit {
            width: 100%;
            padding: 12px;
            font-size: 16px;
            border-radius: 5px;
            cursor: pointer;
            margin-top: 10px;
            transition: all 0.3s ease;
            font-weight: bold;
        }

        /* Button and Layout Styling */
        .btn-post {
            background-color: green;
            color: white;
            text-decoration: none;
            border: none;
            cursor: pointer;
            text-align: left;
            display: block;
            padding: 10px;
            width: 100%;
            border-radius: 5px;
            font-size: 16px;
        }

        .btn-post:hover {
            background-color: #575757;
        }

        .btn-disabled {
            background-color: gray;
            cursor: not-allowed;
            color: white;
        }

        .btn-enabled {
            background-color: #3B387E;
            cursor: pointer;
            color: white;
        }

        /* Drop Zone Styling */
        .drop-zone {
            border: 2px dashed #ccc;
            padding: 20px;
            text-align: center;
            color: #aaa;
            border-radius: 5px;
            cursor: pointer;
            margin-top: 10px;
            position: relative;
        }

        .drop-zone input[type="file"] {
            opacity: 0;
            position: absolute;
            width: 100%;
            height: 100%;
            cursor: pointer;
            top: 0;
            left: 0;
        }

        .image-preview, .video-preview {
            margin-top: 10px;
        }

        .image-preview {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
        }

        .image-preview img {
            height: 100px;
            border-radius: 5px;
            object-fit: cover;
        }

        .video-preview {
            display: flex;
            flex-direction: column;
            gap: 15px;
        }

        .video-preview video {
            height: 200px;
            border-radius: 5px;
        }

        .error_msg {
            font-size: 16px;
            color: red;
        }
    </style>

    <div class="content-wrapper">
        <div class="sidebar">
            <ul>
                <br />
                <br />
                <li>
                    <a href="Discussion.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/Discussion_Icon.png") %>'/>
                        <span>Discussion</span>
                    </a>
                </li>
                <li>
                    <a href="SmartSearch.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/Magnifying_Glass_Icon.png") %>'/>
                        <span>Smart Search</span>
                    </a>
                </li>
                <li>
                    <a href="MyNotification.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/Notification_Icon.png") %>'/>
                        <span>Notification</span>
                    </a>
                </li>
                <li>
                    <a href="MyPost.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/MyPost_Icon.png") %>'/>
                        <span>My Post</span>
                    </a>
                </li>
                <li class="active">
                    <a href="Post.aspx">
                        <img src='<%= ResolveUrl("~/Forum/Icon/Post_Icon.png") %>'/>
                        <span>Post</span>
                    </a>
                </li>
            </ul>
        </div>

        <div class="main-content">
            <h1>Create post</h1>

            <div class="form">
                <!-- Image Upload Section -->
                <div class="form-group">
                    <label>Upload Images:</label>
                    <div id="drop_zone" class="drop-zone" ondrop="handleDrop(event)" ondragover="handleDragOver(event)">
                        Drag & Drop Images Here or Click to Upload
                        <input type="file" id="img_post" name="img_post[]" multiple accept="image/*" onchange="handleFiles(this.files); validatePost();" />
                    </div>
                    <div id="preview" class="image-preview"></div>
                </div>

                <!-- Video Upload Section -->
                <div class="form-group">
                    <label>Upload Videos:</label>
                    <div id="video_drop_zone" class="drop-zone" ondrop="handleVideoDrop(event)" ondragover="handleDragOver(event)">
                        Drag & Drop Videos Here or Click to Upload
                        <input type="file" id="video_post" name="video_post[]" multiple accept="video/*" onchange="handleVideoFiles(this.files); validatePost();" />
                    </div>
                    <div id="video_preview" class="video-preview"></div>
                </div>

                <!-- Text Section -->
                <div class="form-group">
                    <label for="tb_text">Text:</label>
                    <asp:TextBox ID="tb_text" runat="server" CssClass="form-control" Height="120px" onchange="validatePost()"></asp:TextBox>
                </div>

                <!-- Category Section -->
                <div class="form-group">
                    <label>Category:</label>
                    <div class="radio-group">
                        <asp:RadioButtonList ID="radiobtn_category" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem>ask a question</asp:ListItem>
                            <asp:ListItem>share my journey</asp:ListItem>
                            <asp:ListItem>share tips and tools</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                </div>

                <!-- Publish Button -->
                <asp:UpdatePanel ID="UpdatePanel" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Label ID="lbl_error_msg" runat="server" Text="" CssClass="error_msg"></asp:Label>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <asp:Button ID="btn_publish" runat="server" Text="Publish" CssClass="btn-submit btn-disabled" Disabled="true" ToolTip="You cannot submit a blank post." OnClick="btn_publish_Click" />
            </div>
        </div>
    </div>

    <script>
        // Validate Post before enabling the Submit Button
        function validatePost() {
            const text = document.getElementById("<%= tb_text.ClientID %>").value.trim();
            const imageUpload = document.getElementById("img_post").files.length;
            const videoUpload = document.getElementById("video_post").files.length;
            const submitButton = document.getElementById("<%= btn_publish.ClientID %>");

            if (text.length > 0 || imageUpload > 0 || videoUpload > 0) {
                submitButton.disabled = false;
                submitButton.classList.remove("btn-disabled");
                submitButton.classList.add("btn-enabled");
                submitButton.removeAttribute("title");
            } else {
                submitButton.disabled = true;
                submitButton.classList.remove("btn-enabled");
                submitButton.classList.add("btn-disabled");
                submitButton.title = "You cannot submit a blank post.";
            }
        }

        // Handle Drag Over for File Uploads
        function handleDragOver(evt) {
            evt.preventDefault();
            evt.stopPropagation();
        }

        // Handle Image Drop
        function handleDrop(evt) {
            evt.preventDefault();
            evt.stopPropagation();

            const files = evt.dataTransfer.files;
            const input = document.getElementById("img_post");

            let dt = new DataTransfer();
            for (let i = 0; i < files.length; i++) {
                if (files[i].type.startsWith("image/")) {
                    dt.items.add(files[i]);
                }
            }
            input.files = dt.files;

            handleFiles(input.files);
            validatePost();
        }

        // Handle Image Files and Preview
        function handleFiles(files) {
            const preview = document.getElementById("preview");
            preview.innerHTML = ""; // clear previous preview

            for (let i = 0; i < files.length; i++) {
                if (files[i].type.startsWith("image/")) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        const img = document.createElement("img");
                        img.src = e.target.result;
                        preview.appendChild(img);
                    };
                    reader.readAsDataURL(files[i]);
                }
            }
        }

        // Handle Video Drop
        function handleVideoDrop(evt) {
            evt.preventDefault();
            evt.stopPropagation();

            const files = evt.dataTransfer.files;
            const input = document.getElementById("video_post");

            let dt = new DataTransfer();
            for (let i = 0; i < files.length; i++) {
                if (files[i].type.startsWith("video/")) {
                    dt.items.add(files[i]);
                }
            }
            input.files = dt.files;

            handleVideoFiles(input.files);
            validatePost();
        }

        // Handle Video Files and Preview
        function handleVideoFiles(files) {
            const preview = document.getElementById("video_preview");
            preview.innerHTML = "";

            for (let i = 0; i < files.length; i++) {
                if (files[i].type.startsWith("video/")) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        const video = document.createElement("video");
                        video.src = e.target.result;
                        video.controls = true;
                        preview.appendChild(video);
                    };
                    reader.readAsDataURL(files[i]);
                }
            }
        }
    </script>
</asp:Content>
