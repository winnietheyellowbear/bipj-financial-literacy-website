<%@ Page Title="Edit Education Page" Language="C#" MasterPageFile="~/Staff_Nav.Master"
    AutoEventWireup="true" CodeBehind="EditEducationPage.aspx.cs"
    Inherits="bipj.EditEducationPage" ValidateRequest="false" %>

<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<!-- ✅ Load CKEditor 5 Classic Build -->
<script src="https://cdn.ckeditor.com/ckeditor5/39.0.1/decoupled-document/ckeditor.js"></script>


<div class="edu-admin-container" style="display:flex;min-height:600px;">
    <!-- Side Navigation (unchanged) -->
    <div class="edu-sidenav" style="background:#222;color:#fff;width:220px;padding:20px 10px 20px 10px;display:flex;flex-direction:column;">
        <a href="ManageEducation.aspx" class="btn btn-sm btn-outline-light mb-3">&larr; Back to Modules</a>
        <asp:Repeater ID="rptTopics" runat="server" OnItemDataBound="rptTopics_ItemDataBound">
            <ItemTemplate>
                <div>
                    <div style='margin-bottom:5px;font-weight:bold;background:#8576b1;color:white;padding:7px 10px;border-radius:5px;'>
                        <%# Eval("TopicName") %>
                        <span style="float:right;">
                            <i class='bi bi-caret-down-fill'></i>
                        </span>
                    </div>
                    <asp:Repeater ID="rptPages" runat="server">
                        <ItemTemplate>
                            <a href='EditEducationPage.aspx?moduleId=<%# Eval("ModuleId") %>&pageId=<%# Eval("Id") %>'
                               style='display:block;margin-left:12px;margin-bottom:6px;color:<%# (Eval("Id").ToString() == PageId.ToString()) ? "#2be3c3" : "white" %>;'>
                               &bull; <%# Eval("Title") %>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <!-- Main Editor Panel -->
    <div style="flex:1;padding:40px;">
        <!-- Page Title -->
        <asp:TextBox ID="txtPageTitle" runat="server" CssClass="form-control mb-3" placeholder="Page Title" />

     <!-- Editor Toolbar + CKEditor Container -->
<div class="document-toolbar" style="margin-bottom: 10px;"></div>
<div id="editorjs" style="min-height:500px; border:1px solid #ccc;"></div>

<!-- Hidden field to store editor content -->
<asp:HiddenField ID="hfEditorContent" runat="server" />
        <iframe width="560" height="315" src="https://www.youtube.com/embed/dQw4w9WgXcQ"
        title="YouTube video player" frameborder="0"
        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
        allowfullscreen>
</iframe>
<!-- Save Button -->
<asp:Button ID="btnSave" runat="server" CssClass="btn btn-success" Text="Save Page" OnClick="btnSave_Click" />
<asp:Label ID="lblMessage" runat="server" CssClass="text-success mt-2" />
    </div>
</div>

<!-- ✅ CKEditor Init Script -->
<script>
    let editorData = document.getElementById('<%= hfEditorContent.ClientID %>');

    DecoupledEditor
        .create(document.querySelector('#editorjs'), {
            mediaEmbed: {
                previewsInData: true
            },
            image: {
                resizeUnit: '%',
                toolbar: [
                    'imageStyle:alignLeft', 'imageStyle:alignCenter', 'imageStyle:alignRight',
                    '|', 'imageResize', '|', 'linkImage'
                ],
                styles: ['alignLeft', 'alignCenter', 'alignRight']
            },
            toolbar: {
                items: [
                    'heading', '|',
                    'bold', 'italic', 'link', 'bulletedList', 'numberedList', '|',
                    'insertTable', 'mediaEmbed', 'imageUpload', '|',
                    'undo', 'redo'
                ]
            }
        })
        .then(editor => {
            // Bind the editor instance globally
            window.editor = editor;

            // Load existing data
            if (editorData.value) {
                editor.setData(editorData.value);
            }

            // Sync data on change
            editor.model.document.on('change:data', () => {
                editorData.value = editor.getData();
            });

            // Move the toolbar to a separate area (optional)
            document.querySelector('.document-toolbar').appendChild(editor.ui.view.toolbar.element);
        })
        .catch(error => {
            console.error('CKEditor error:', error);
        });
</script>

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
</style>

</asp:Content>
