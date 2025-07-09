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
    <div id="editorjs-content"></div>
    <asp:HiddenField ID="hfPageContent" runat="server" />
</div>
        </asp:Panel>
    </div>
</div>
        <script>
document.addEventListener('DOMContentLoaded', function() {
    // Get the JSON content from the hidden field
    const contentJson = document.getElementById('<%= hfPageContent.ClientID %>').value;
    
    if (contentJson) {
        try {
            const contentData = JSON.parse(contentJson);
            renderEditorJsContent(contentData);
        } catch (e) {
            console.error("Error parsing content JSON:", e);
            document.getElementById('editorjs-content').innerHTML = 
                "<div class='alert alert-danger'>Error loading content</div>";
        }
    }
});

function renderEditorJsContent(data) {
    const holder = document.getElementById('editorjs-content');
    
    // Simple renderer for common block types
    data.blocks.forEach(block => {
        switch(block.type) {
            case 'paragraph':
                const p = document.createElement('p');
                p.innerHTML = block.data.text;
                holder.appendChild(p);
                break;
                
            case 'header':
                const header = document.createElement(`h${block.data.level}`);
                header.textContent = block.data.text;
                holder.appendChild(header);
                break;
                
            case 'list':
                const list = document.createElement(block.data.style === 'ordered' ? 'ol' : 'ul');
                block.data.items.forEach(item => {
                    const li = document.createElement('li');
                    li.textContent = item;
                    list.appendChild(li);
                });
                holder.appendChild(list);
                break;
                
            case 'image':
                const imgDiv = document.createElement('div');
                imgDiv.className = 'text-center my-3';
                const img = document.createElement('img');
                img.src = block.data.url;
                img.alt = block.data.caption || '';
                img.className = 'img-fluid';
                img.style.maxHeight = '500px';
                imgDiv.appendChild(img);
                if (block.data.caption) {
                    const caption = document.createElement('div');
                    caption.className = 'text-muted mt-2';
                    caption.textContent = block.data.caption;
                    imgDiv.appendChild(caption);
                }
                holder.appendChild(imgDiv);
                break;
                
            case 'embed':
                const embedDiv = document.createElement('div');
                embedDiv.className = 'embed-responsive embed-responsive-16by9 my-3';
                const iframe = document.createElement('iframe');
                iframe.className = 'embed-responsive-item';
                iframe.src = block.data.embed || block.data.source;
                iframe.allowFullscreen = true;
                embedDiv.appendChild(iframe);
                holder.appendChild(embedDiv);
                break;
                
            default:
                const unknown = document.createElement('div');
                unknown.className = 'alert alert-warning';
                unknown.textContent = `Unsupported block type: ${block.type}`;
                holder.appendChild(unknown);
        }
    });
}
        </script>

<style>
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
</style>

</asp:Content>