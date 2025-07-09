<%@ Page Title="Education Modules" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.master" AutoEventWireup="true" CodeBehind="ViewEducationPage.aspx.cs" Inherits="bipj.ViewEducationPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<div class="container mt-4">
    <h2 class="mb-4">Education Modules</h2>
    
    <div class="row">
        <asp:Repeater ID="rptModules" runat="server">
            <ItemTemplate>
                <div class="col-md-4 mb-4">
                    <div class="card h-100">
                        <div class="card-img-top" style="height: 180px; background: #f5f5f5; display: flex; align-items: center; justify-content: center;">
                            <img src='<%# Eval("ImageUrl") ?? "/images/default-module.png" %>' 
                                 alt='<%# Eval("Name") %>' 
                                 style="max-height: 100%; max-width: 100%; object-fit: contain;" />
                        </div>
                        <div class="card-body">
                            <h5 class="card-title"><%# Eval("Name") %></h5>
                            <p class="card-text"><%# Eval("BriefDescription") %></p>
                            <a href='ViewSpecificEdu.aspx?moduleId=<%# Eval("Id") %>' 
                               class="btn btn-primary">View Module</a>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</div>
  
<style>
    .card {
        transition: transform 0.2s;
        border: 1px solid #e0e0e0;
        border-radius: 8px;
        overflow: hidden;
    }
    .card:hover {
        transform: translateY(-5px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }
    .card-img-top {
        background-color: #f8f9fa;
    }
</style>

</asp:Content>