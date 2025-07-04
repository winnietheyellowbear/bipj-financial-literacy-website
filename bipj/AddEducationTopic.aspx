<%@ Page Title="" Language="C#" MasterPageFile="~/Staff_Nav.Master" AutoEventWireup="true" CodeBehind="AddEducationTopic.aspx.cs" Inherits="bipj.AddEducationTopic" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css" />
    <style>
        .add-module-wrapper {
            max-width: 510px;
            margin: 35px auto 0 auto;
            background: #fff;
            border-radius: 18px;
            box-shadow: 0 4px 18px rgba(80,60,160,0.07);
            padding: 2.2rem 2.2rem 1.6rem 2.2rem;
            min-height: 630px;
        }
        .add-module-back {
            font-size: 2rem;
            margin-bottom: 15px;
            cursor: pointer;
        }
        .add-module-label {
            margin-top: 10px;
            margin-bottom: 2px;
            font-weight: 600;
            font-size: 1.04rem;
        }
        .add-module-form .form-control, .add-module-form textarea {
            border-radius: 9px;
            font-size: 1rem;
        }
        .add-module-form textarea {
            min-height: 100px;
        }
        .add-image-input-group {
            display: flex;
            gap: 10px;
        }
        .add-module-insert-btn, .add-module-topic-btn {
            display: flex;
            align-items: center;
            border: 1px solid #ccc;
            border-radius: 8px;
            background: #fff;
            color: #222;
            padding: 4px 16px;
            font-size: 1.07rem;
            font-weight: 500;
            cursor: pointer;
            transition: border 0.11s, background 0.11s;
        }
        .add-module-insert-btn:hover, .add-module-topic-btn:hover {
            background: #f5f5f5;
            border: 1.5px solid #5045a6;
            color: #5045a6;
        }
        .add-module-topic-btn {
            margin-top: 10px;
        }
        .add-module-create-btn {
            margin-top: 35px;
            width: 100%;
            background: #4337a6;
            color: #fff;
            font-size: 1.08rem;
            border: none;
            border-radius: 10px;
            padding: 10px 0;
            font-weight: 600;
            letter-spacing: .02em;
            transition: background 0.13s;
        }
        .add-module-create-btn:hover, .add-module-create-btn:focus {
            background: #2c246a;
        }
    </style>
</asp:Content>

<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="add-module-wrapper">
        <a href="ManageEducation.aspx" class="add-module-back" title="Back">
            <i class="bi bi-arrow-left-circle"></i>
        </a>
        <div class="add-module-form">
            <div class="add-module-label">Module name</div>
            <asp:TextBox ID="txtModuleName" runat="server" CssClass="form-control" placeholder="Name" />

            <div class="add-module-label">Brief description</div>
            <asp:TextBox ID="txtBriefDesc" runat="server" CssClass="form-control" placeholder="" />

            <div class="add-module-label">Image</div>
            <div class="add-image-input-group">
                <asp:TextBox ID="txtImage" runat="server" CssClass="form-control" placeholder="" />
                <asp:FileUpload ID="fileUploadImage" runat="server" CssClass="form-control" style="max-width:165px;" />
                <asp:Button ID="btnInsertImage" runat="server" CssClass="add-module-insert-btn" Text='<i class="bi bi-plus-circle"></i> Insert' UseSubmitBehavior="false" OnClick="btnInsertImage_Click" />
            </div>

            <div class="add-module-label">Indept Description</div>
            <asp:TextBox ID="txtIndeptDesc" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" placeholder="" />

            <asp:Button ID="btnAddTopic" runat="server" CssClass="add-module-topic-btn" Text='<i class="bi bi-plus-circle"></i> Topic' UseSubmitBehavior="false" OnClick="btnAddTopic_Click" />

            <!-- Here you can render dynamically added topic controls or a placeholder for topics/subtopics -->
            <asp:PlaceHolder ID="phTopics" runat="server"></asp:PlaceHolder>

            <asp:Button ID="btnCreate" runat="server" CssClass="add-module-create-btn" Text="Create" OnClick="btnCreate_Click" />
        </div>
    </div>
</asp:Content>
