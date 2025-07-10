<%@ Page Title="Edit Profile" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master" AutoEventWireup="true" CodeBehind="EditProfile.aspx.cs" Inherits="bipj.EditProfile" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .edit-profile-container {
            max-width: 500px;
            margin: 40px auto 0 auto;
            padding: 32px 30px;
            background: #fff;
            border-radius: 16px;
            box-shadow: 0 6px 28px 0 rgba(0,0,0,0.10);
            text-align: center;
        }
        .edit-profile-image {
            display: block;
            margin: 0 auto 18px auto;
            width: 140px;
            height: 140px;
            border-radius: 50%;
            object-fit: cover;
            border: 4px solid #e8e9fc;
            box-shadow: 0 1px 5px rgba(30,30,60,0.04);
            cursor: pointer;
            transition: border-color 0.2s;
        }
        .edit-profile-image:hover {
            border-color: #4f6ef7;
        }
        .edit-profile-form {
            margin-top: 16px;
        }
        .edit-profile-form .form-label {
            font-weight: 500;
            margin-bottom: 3px;
            display: block;
            text-align: left;
        }
        .edit-profile-form .form-control {
            width: 100%;
            padding: 8px 12px;
            margin-bottom: 18px;
            border: 1px solid #cfd8dc;
            border-radius: 7px;
            font-size: 1rem;
        }
        .edit-profile-form .btn-save {
            background: #4f6ef7;
            color: #fff;
            border: none;
            padding: 10px 36px;
            border-radius: 22px;
            font-size: 1.1rem;
            font-weight: 500;
            cursor: pointer;
            transition: background 0.16s;
        }
        .edit-profile-form .btn-save:hover {
            background: #3241d9;
        }
        .edit-profile-form .form-text {
            font-size: 0.92rem;
            color: #76819b;
            margin-top: -10px;
            margin-bottom: 15px;
            display: block;
            text-align: left;
        }
        .edit-profile-success, .edit-profile-error {
            margin-bottom: 15px;
            font-weight: 500;
            font-size: 1.08rem;
        }
        .edit-profile-success { color: #28b23f; }
        .edit-profile-error { color: #bb2d3b; }
        /* Hide the actual file input */
        #fileProfileImage {
            display: none;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="edit-profile-container">
        <h2>Edit Profile</h2>
        <!-- Profile Image, Click to Upload -->
        <asp:Image ID="imgProfile" runat="server" CssClass="edit-profile-image" AlternateText="Profile Picture" onclick="document.getElementById('fileProfileImage').click(); return false;" />

        <asp:FileUpload ID="fileProfileImage" runat="server" Accept="image/*" onchange="document.getElementById('imgProfile').src = window.URL.createObjectURL(this.files[0]);" />

        <asp:Label ID="lblMessage" runat="server" CssClass="edit-profile-success" Visible="false"></asp:Label>
        <asp:Label ID="lblError" runat="server" CssClass="edit-profile-error" Visible="false"></asp:Label>

        <div class="edit-profile-form">
            <label class="form-label" for="txtName">Display Name</label>
            <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="50" />

            <label class="form-label" for="txtBio">Bio</label>
            <asp:TextBox ID="txtBio" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="500" />
            <span class="form-text">Max 500 characters.</span>

            <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn-save" OnClick="btnSave_Click" />
        </div>
    </div>
    <script>
        // Instantly update preview after file selection
        document.addEventListener('DOMContentLoaded', function () {
            var fileInput = document.getElementById('fileProfileImage');
            if (fileInput) {
                fileInput.onchange = function (evt) {
                    if (fileInput.files && fileInput.files[0]) {
                        document.getElementById('<%= imgProfile.ClientID %>').src = window.URL.createObjectURL(fileInput.files[0]);
                    }
                }
            }
        });
    </script>
</asp:Content>
