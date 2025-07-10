<%@ Page Title="" Language="C#" MasterPageFile="~/Customer_Nav.Master" AutoEventWireup="true" CodeBehind="EditMyPost.aspx.cs" Inherits="bipj.EditMyPost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<head>
    <link rel="stylesheet" href="Forum_Nav.css">
    <link rel="stylesheet" href="Forum_Post.css">
    <asp:ScriptManager ID="ScriptManager" runat="server" />
</head>

<!-- Inline CSS Styling -->
<style>
    /* Main Content Styling */
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

    .main-content {
        flex: 1;
        background-color: #f8f9fa;
        padding: 30px;
        margin-left: 20px;
        max-width: 1000px;
    }

 /* Form Styling */
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


 /* Responsive */
 @media (max-width: 768px) {
     .main-content {
         width: 90%;
         padding: 20px;
     }
 }

 /* Styling for btn_post */
 .btn-post {
     background-color: green; /* Green background for the button */
     color: white;
     text-decoration: none;
     border: none;
     cursor: pointer;
     text-align: left;
     display: block;
     padding: 10px;
     width: 100%;
     border-radius: 5px; /* Optional: for rounded corners */
     font-size: 16px;
 }

 .btn-post:hover {
     background-color: #575757; /* Hover effect for both links and button */
 }

 /* Layout styling */
 .content-wrapper {
     display: flex;
     margin-top: 10px; /* Space below navbar */
 }
    
 .btn-disabled {
     background-color: gray;
     cursor: not-allowed;
     color: white; /* Optional: To ensure text remains visible */
 }

 .btn-enabled {
     background-color: #3B387E;
     cursor: pointer;
     color: white; /* Optional: To ensure text remains visible */
 }


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

/* Back Button Styles */
.back-button {
    
    background-color: #3B387E;
    color: white;
    padding: 10px 20px;
    border: none;
    border-radius: 20px;
    cursor: pointer;
    font-size: 16px;
    transition: background-color 0.3s ease;
    text-decoration: none
}

.back-button:hover {
    background-color: #59569E;
}
</style>

<!-- Sidebar and main content wrapper -->
<div class="content-wrapper">
    <!-- Sidebar -->
    <div class="sidebar">
        <ul>
           <br />
           <br />
         <li>
             <a href="Discussion.aspx">
                 <img src='<%= ResolveUrl("~/Forum/Icon/Discussion_Icon.png") %>' alt="Discussion Icon"/>
                 <span>Discussion</span>
             </a>
         </li>
        <li>
            <a href="SmartSearch.aspx">
                <img src='<%= ResolveUrl("~/Forum/Icon/Magnifying_Glass_Icon.png") %>' alt="Notification Icon"/>
                <span>Smart Search</span>
            </a>
        </li>
         <li>
             <a href="MyNotification.aspx">
                 <img src='<%= ResolveUrl("~/Forum/Icon/Notification_Icon.png") %>' alt="Notification Icon"/>
                 <span>Notification</span>
             </a>
         </li>
         <li>
             <a href="MyPost.aspx">
                 <img src='<%= ResolveUrl("~/Forum/Icon/MyPost_Icon.png") %>' alt="My Post Icon"/>
                 <span>My Post</span>
             </a>
         </li>
         <li>
             <a href="Post.aspx">
                 <img src='<%= ResolveUrl("~/Forum/Icon/Post_Icon.png") %>' alt="Post Icon"/>
                 <span>Post</span>
             </a>
         </li>
     </ul>
    </div>

    <!-- Main content -->
    <div class="main-content">
        <h1>Edit post</h1>

        <div style="margin-top: 20px">
            <asp:LinkButton class="back-button" OnClick="btn_back_Click" runat="server">
                <img src="<%= ResolveUrl("~/Images/back_icon.png") %>" alt="Back" style="width: 20px; height: 20px"/> back
            </asp:LinkButton>
        </div>
        
        <div class="form">
            <div class="form-group">
                <label>Upload Images:</label>
                <div id="drop_zone" class="drop-zone" ondrop="handleImageDrop(event)" ondragover="handleDragOver(event)">
                    Drag & Drop Images Here or Click to Upload
                    <input type="file" id="img_post" name="img_post[]" multiple accept="image/*" />
                </div>
                <div id="image_preview" class="image-preview"></div>
            </div>
            
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

            <div class="form-group">
                <label>Upload Videos:</label>
                <div id="video_drop_zone" class="drop-zone" ondrop="handleVideoDrop(event)" ondragover="handleDragOver(event)">
                    Drag & Drop Videos Here or Click to Upload
                    <input type="file" id="video_post" name="video_post[]" multiple accept="video/*" />
                </div>
                <div id="video_preview" class="video-preview"></div>
            </div>

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

            <div class="form-group">
                <label for="tb_text">Text:</label>
                <asp:TextBox ID="tb_text" runat="server" CssClass="form-control" Height="120px"></asp:TextBox>
            </div>

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

            <asp:Button ID="btn_update" runat="server" Text="Update"
                CssClass="btn-submit btn-enabled" ToolTip="You can update now." OnClick="btn_update_Click"/>
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

        // Retrieve files from the event
        const files = evt.dataTransfer.files;

        const input = document.getElementById("img_post");

        // Prepare a DataTransfer object to collect valid files (image files)
        let dt = new DataTransfer();
        for (let i = 0; i < files.length; i++) {
            if (files[i].type.startsWith("image/")) {
                dt.items.add(files[i]);
            }
        }

        // Assign files to the input
        input.files = dt.files;

        // Call the function to handle files and show the preview
        handleImageFiles(input.files);
    }

    // Handle Video Drop Event
    function handleVideoDrop(evt) {
        evt.preventDefault();
        evt.stopPropagation();

        // Retrieve files from the event
        const files = evt.dataTransfer.files;

        const input = document.getElementById("video_post");

        // Prepare a DataTransfer object to collect valid files (video files)
        let dt = new DataTransfer();
        for (let i = 0; i < files.length; i++) {
            if (files[i].type.startsWith("video/")) {
                dt.items.add(files[i]);
            }
        }

        // Assign files to the input
        input.files = dt.files;

        // Call the function to handle files and show the preview
        handleVideoFiles(input.files);
    }

    // Handle and display image preview
    function handleImageFiles(files) {
        const preview = document.getElementById("image_preview");
        preview.innerHTML = ""; // Clear existing preview

        // Iterate over each file and display the preview
        for (let i = 0; i < files.length; i++) {
            if (files[i].type.startsWith("image/")) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    const img = document.createElement("img");
                    img.src = e.target.result; // Set image source to file data
                    img.style.height = "80px";
                    img.style.marginRight = "10px";
                    preview.appendChild(img); // Append the image to the preview container
                };
                reader.readAsDataURL(files[i]); // Read the file as Data URL
            }
        }
    }

    // Handle and display video preview
    function handleVideoFiles(files) {
        const preview = document.getElementById("video_preview");
        preview.innerHTML = ""; // Clear existing preview

        // Iterate over each file and display the preview
        for (let i = 0; i < files.length; i++) {
            if (files[i].type.startsWith("video/")) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    const video = document.createElement("video");
                    video.src = e.target.result; // Set video source to file data
                    video.controls = true; // Enable controls for the video
                    video.style.height = "120px";
                    video.style.marginRight = "10px";
                    preview.appendChild(video); // Append the video to the preview container
                };
                reader.readAsDataURL(files[i]); // Read the file as Data URL
            }
        }
    }

    // Ensure file input changes also trigger preview
    document.getElementById("img_post").addEventListener("change", function () {
        handleImageFiles(this.files);
    });

    document.getElementById("video_post").addEventListener("change", function () {
        handleVideoFiles(this.files);
    });
</script>

</asp:Content>
