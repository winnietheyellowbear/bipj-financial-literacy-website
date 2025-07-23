<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="EditMyPost.aspx.cs" Inherits="bipj.EditMyPost" %>

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
        margin-top: 10px;
    }

    .main-content {
        flex: 1;
        background-color: #f8f9fa;
        padding: 30px;
        margin-left: 20px;
        max-width: 1000px;
    }

    .form {
        padding: 30px;
        background-color: white;
        border-radius: 10px;
        box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.2);
        max-width: 500px;
        margin: 20px auto;
        padding: 20px;
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


    /* File Upload */
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


    /* Buttons */
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

    .back-button {
        background-color: #3B387E;
        color: white;
        padding: 10px 20px;
        border: none;
        border-radius: 20px;
        cursor: pointer;
        font-size: 16px;
        transition: background-color 0.3s ease;
        text-decoration: none;
    }

    .back-button:hover {
        background-color: #59569E;
    }

    /* File Preview */
    .image-preview, .video-preview {
        margin-top: 10px;
        display: flex;
        flex-wrap: wrap;
        gap: 10px;
    }

    .image-preview img {
        height: 80px;
        border-radius: 5px;
        object-fit: cover;
    }

    .video-preview video {
        height: 120px;
        border-radius: 5px;
    }

    /* Drop Zone */
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
            <li>
                <a href="Post.aspx">
                    <img src='<%= ResolveUrl("~/Forum/Icon/Post_Icon.png") %>'/>
                    <span>Post</span>
                </a>
            </li>
        </ul>
    </div>

    <div class="main-content">
        <h1>Edit post</h1>

        <!-- Back Button -->
            <asp:LinkButton class="back-button" OnClick="btn_back_Click" runat="server">
                <img src="<%= ResolveUrl("~/Images/back_icon.png") %>" style="width: 20px; height: 20px"/> back
            </asp:LinkButton>

        <div class="form">
            <!-- Image Upload -->
            <div class="form-group">
                <label>Upload Images:</label>
                <div id="drop_zone" class="drop-zone" ondrop="handleImageDrop(event)" ondragover="handleDragOver(event)">
                    Drag & Drop Images Here or Click to Upload
                    <input type="file" id="img_post" name="img_post[]" multiple accept="image/*" />
                </div>
                <div id="image_preview" class="image-preview"></div>
            </div>

            <!-- Image Repeater UpdatePanel -->
            <asp:UpdatePanel ID="UpdatePanel_Image" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Repeater ID="Image" runat="server">
                        <ItemTemplate>
                            <img src='<%# ResolveUrl((string)Container.DataItem) %>' class="post-img" style="width: 50px; height: 50px"/>
                            <asp:Button ID="btn_remove_image" runat="server" Text="remove" CommandArgument='<%# (string)Container.DataItem %>' OnClick="btn_remove_image_Click" />
                        </ItemTemplate>
                    </asp:Repeater>
                </ContentTemplate>
            </asp:UpdatePanel>

            <!-- Video Upload -->
            <div class="form-group">
                <label>Upload Videos:</label>
                <div id="video_drop_zone" class="drop-zone" ondrop="handleVideoDrop(event)" ondragover="handleDragOver(event)">
                    Drag & Drop Videos Here or Click to Upload
                    <input type="file" id="video_post" name="video_post[]" multiple accept="video/*" />
                </div>
                <div id="video_preview" class="video-preview"></div>
            </div>

            <!-- Video Repeater UpdatePanel -->
            <asp:UpdatePanel ID="UpdatePanel_Video" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Repeater ID="Video" runat="server">
                        <ItemTemplate>
                            <video controls class="post-video" style="width: 50px; height: 50px">
                                <source src='<%# ResolveUrl((string)Container.DataItem) %>' type="video/mp4" />
                            </video>
                            <asp:Button ID="btn_remove_video" runat="server" Text="remove" CommandArgument='<%# (string)Container.DataItem %>' OnClick="btn_remove_video_Click" />
                        </ItemTemplate>
                    </asp:Repeater>
                </ContentTemplate>
            </asp:UpdatePanel>

            <!-- Text Input -->
            <div class="form-group">
                <label for="tb_text">Text:</label>
                <asp:TextBox ID="tb_text" runat="server" CssClass="form-control" Height="120px"></asp:TextBox>
            </div>

            <!-- Category Selection -->
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

            <!-- Update Button -->
            <asp:Button ID="btn_update" runat="server" Text="Update" CssClass="btn-submit btn-enabled" OnClick="btn_update_Click"/>
        </div>
    </div>
</div>

<script>
    // Handle file drag-over (to allow file dropping)
    function handleDragOver(evt) {
        evt.preventDefault();
        evt.stopPropagation();
    }

    // Handle Image Drop Event
    function handleImageDrop(evt) {
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
        handleImageFiles(input.files);
    }

    // Handle Video Drop Event
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
    }

    // Handle Image Files
    function handleImageFiles(files) {
        const preview = document.getElementById("image_preview");
        preview.innerHTML = "";

        for (let i = 0; i < files.length; i++) {
            if (files[i].type.startsWith("image/")) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    const img = document.createElement("img");
                    img.src = e.target.result;
                    img.style.height = "80px";
                    img.style.marginRight = "10px";
                    preview.appendChild(img);
                };
                reader.readAsDataURL(files[i]);
            }
        }
    }

    // Handle Video Files
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
                    video.style.height = "120px";
                    video.style.marginRight = "10px";
                    preview.appendChild(video);
                };
                reader.readAsDataURL(files[i]);
            }
        }
    }

    // Trigger file preview on file input change
    document.getElementById("img_post").addEventListener("change", function () {
        handleImageFiles(this.files);
    });

    document.getElementById("video_post").addEventListener("change", function () {
        handleVideoFiles(this.files);
    });
</script>

</asp:Content>
