<%@ Page Title="Edit Education Page" Language="C#"
    MasterPageFile="~/Staff_Nav.Master"
    AutoEventWireup="true"
    CodeBehind="EditEducationPage.aspx.cs"
    Inherits="bipj.EditEducationPage"
    ValidateRequest="false" %>

<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Permissions API shim (prevents 'Illegal invocation' from third-party libs) -->
    <script>
        (function () {
            try {
                if (navigator.permissions && typeof navigator.permissions.query === 'function') {
                    var boundQuery = navigator.permissions.query.bind(navigator.permissions);
                    navigator.permissions.query = function () {
                        return boundQuery.apply(navigator.permissions, arguments);
                    };
                }
            } catch (e) { /* no-op */ }
        })();
    </script>

    <!-- CKEditor 5 Decoupled Document build -->
    <script src="https://cdn.ckeditor.com/ckeditor5/39.0.1/decoupled-document/ckeditor.js"></script>

    <style>
        .edu-admin-container { display:flex; min-height:600px; gap:0; }
        .edu-sidenav { background:#222; color:#fff; width:240px; padding:20px 10px; display:flex; flex-direction:column; }
        .topic-chip { margin-bottom:6px; font-weight:700; background:#8576b1; color:#fff; padding:7px 10px; border-radius:6px; }
        .page-link { display:block; margin-left:12px; margin-bottom:6px; color:#fff; text-decoration:none; }
        .page-link.active { color:#2be3c3; font-weight:700; }
        .editor-wrap { flex:1; padding:40px; }
        .document-toolbar { margin-bottom:10px; }
        #editorjs { min-height:500px; border:1px solid #ccc; border-radius:6px; }
        figure.media { max-width: 800px; margin: 20px auto; }
        figure.media iframe {
            width: 100% !important; height: auto !important; aspect-ratio: 16/9;
            border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }
    </style>

    <div class="edu-admin-container">
        <!-- Side Navigation -->
        <aside class="edu-sidenav">
            <a href="ManageEducation.aspx" class="btn btn-sm btn-outline-light mb-3">&larr; Back to Modules</a>

            <asp:Repeater ID="rptTopics" runat="server" OnItemDataBound="rptTopics_ItemDataBound">
                <ItemTemplate>
                    <div>
                        <div class="topic-chip">
    <%# Eval("Name") %>
    <span style="float:right;"><i class="bi bi-caret-down-fill"></i></span>
</div>
                        <asp:Repeater ID="rptPages" runat="server" OnItemDataBound="rptPages_ItemDataBound">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkPage" runat="server" CssClass="page-link" />
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </aside>

        <!-- Editor Panel -->
        <section class="editor-wrap">
            <asp:TextBox ID="txtPageTitle" runat="server" CssClass="form-control mb-3" placeholder="Page Title" />
            <div class="document-toolbar"></div>
            <div id="editorjs"></div>

            <!-- Hidden field to sync editor HTML -->
            <asp:HiddenField ID="hfEditorContent" runat="server" />

            <div class="mt-3">
                <asp:Button ID="btnSave" runat="server" CssClass="btn btn-success" Text="Save Page" OnClick="btnSave_Click" />
                <asp:Label ID="lblMessage" runat="server" CssClass="ms-2" />
            </div>
        </section>
    </div>

    <!-- CKEditor init -->
    <script>
    (function () {
        const hf = document.getElementById('<%= hfEditorContent.ClientID %>');

            DecoupledEditor.create(document.querySelector('#editorjs'), {
                mediaEmbed: { previewsInData: true },
                toolbar: {
                    items: [
                        'heading', '|',
                        'bold', 'italic', 'link', 'bulletedList', 'numberedList', '|',
                        'insertTable', 'mediaEmbed', '|',
                        'undo', 'redo'
                    ]
                },
                image: {
                    resizeUnit: '%',
                    toolbar: ['imageStyle:alignLeft', 'imageStyle:alignCenter', 'imageStyle:alignRight', '|', 'imageResize', '|', 'linkImage'],
                    styles: ['alignLeft', 'alignCenter', 'alignRight']
                }
            })
                .then(editor => {
                    window.editor = editor;

                    // Load existing HTML
                    if (hf.value) {
                        editor.setData(hf.value);
                    }
                    // Sync back to hidden field
                    editor.model.document.on('change:data', () => {
                        hf.value = editor.getData();
                    });
                    // Move toolbar into our container
                    document.querySelector('.document-toolbar').appendChild(editor.ui.view.toolbar.element);
                })
                .catch(err => console.error('CKEditor init error:', err));
        })();
    </script>
</asp:Content>
