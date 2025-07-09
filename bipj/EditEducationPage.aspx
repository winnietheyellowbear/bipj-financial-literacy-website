<%@ Page Title="Edit Education Page" Language="C#" MasterPageFile="~/Staff_Nav.Master" AutoEventWireup="true" CodeBehind="EditEducationPage.aspx.cs" Inherits="bipj.EditEducationPage" %>
<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<!-- Load EditorJS and all required tools -->
<script src="https://cdn.jsdelivr.net/npm/@editorjs/editorjs@2.26.5"></script>
<script src="https://cdn.jsdelivr.net/npm/@editorjs/header@2.6.2"></script>
<script src="https://cdn.jsdelivr.net/npm/@editorjs/paragraph@2.8.0"></script>
<script src="https://cdn.jsdelivr.net/npm/@editorjs/list@1.7.0"></script>
<script src="https://cdn.jsdelivr.net/npm/@editorjs/simple-image@1.4.1"></script>
<script src="https://cdn.jsdelivr.net/npm/@editorjs/embed@2.5.3"></script>


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
        
        <!-- Toolbar -->
        <div class="mb-3">
            <button type="button" class="btn btn-primary" onclick="insertParagraph()">
                <i class="bi bi-text-paragraph"></i> Add Text
            </button>
            <button type="button" class="btn btn-primary" onclick="insertImage()">
                <i class="bi bi-image"></i> Add Image
            </button>
            <button type="button" class="btn btn-primary" onclick="insertVideo()">
                <i class="bi bi-film"></i> Add Video
            </button>
            <button type="button" class="btn btn-primary" onclick="insertList()">
                <i class="bi bi-list-ul"></i> Add List
            </button>
            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-success" Text="Save Page" OnClick="btnSave_Click" />
        </div>

        <!-- Editor Container -->
        <div id="editorjs" style="border:1px solid #ddd; min-height:500px;"></div>
        
        <!-- Hidden field to store editor content -->
        <asp:HiddenField ID="hfEditorContent" runat="server" />
        
        <asp:Label ID="lblMessage" runat="server" CssClass="text-success mt-2" />
    </div>
</div>

<script>
    // Global editor reference
    let editor;

    // Initialize EditorJS when DOM is loaded
    document.addEventListener('DOMContentLoaded', function () {
        // Load saved data if exists
        let savedData = {};
        try {
            const savedJson = document.getElementById('<%= hfEditorContent.ClientID %>').value;
            if (savedJson) {
                savedData = JSON.parse(savedJson);
            }
        } catch (e) {
            console.error("Error parsing saved content:", e);
        }

        // Initialize the editor
        editor = new EditorJS({
            holder: 'editorjs',
            tools: {
                header: {
                    class: window.Header,
                    config: {
                        placeholder: 'Enter a header...',
                        levels: [2, 3, 4],
                        defaultLevel: 2
                    }
                },
                paragraph: {
                    class: window.Paragraph,
                    inlineToolbar: true
                },
                list: {
                    class: window.List,
                    inlineToolbar: true
                },
                image: window.SimpleImage,
                embed: window.Embed
            },
            data: savedData,
            onChange: function() {
                editor.save().then(output => {
                    document.getElementById('<%= hfEditorContent.ClientID %>').value = JSON.stringify(output);
                });
            }
        });
    });

    // Block insertion functions
    function insertParagraph() {
        if (editor) {
            editor.blocks.insert('paragraph', {
                text: 'Start typing your text here...'
            });
        }
    }

    function insertImage() {
        if (editor) {
            editor.blocks.insert('image', {
                url: '',
                caption: '',
                withBorder: false,
                stretched: false
            });
        }
    }

    function insertVideo() {
        if (editor) {
            editor.blocks.insert('embed', {
                service: 'youtube',
                source: '',
                width: 640,
                height: 360
            });
        }
    }

    function insertList() {
        if (editor) {
            editor.blocks.insert('list', {
                style: 'unordered',
                items: [
                    'First list item',
                    'Second list item'
                ]
            });
        }
    }
</script>

<style>
    /* Editor styling */
    #editorjs {
        background: white;
        padding: 20px;
        border-radius: 5px;
    }
    
    .ce-block--selected .ce-block__content {
        background: rgba(43, 227, 195, 0.1);
    }
    
    .ce-toolbar__plus {
        color: #8576b1;
    }
    
    .ce-toolbar__plus:hover {
        color: #6a5a9a;
    }
</style>

</asp:Content>