<%@ Page Title="" Language="C#" MasterPageFile="~/Staff_Nav.Master" AutoEventWireup="true" CodeBehind="CreateVoucher.aspx.cs" Inherits="bipj.CreateVoucher" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-container {
            max-width: 600px;
            margin: 20px auto;
            padding: 20px;
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
        }
        body {
            background-color: #F8F1FB;
        }

        .form-container h5 {
            text-align: center;
            margin-bottom: 20px;
            font-size: 20px;
            font-weight: bold;
            color: #333;
        }

        .form-group {
            margin-bottom: 15px;
        }

        .form-group label {
            display: block;
            font-weight: bold;
            margin-bottom: 5px;
            color: #555;
        }

        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 5px;
            font-size: 16px;
            box-sizing: border-box;
        }

        .btn-add {
            display: block;
            width: 100%;
            padding: 10px;
            font-size: 16px;
            color: white;
            background-color: #3B387E;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            text-align: center;
            text-decoration: none;
        }

        .btn-add:hover {
            background-color: #59569E;
        }

        .message {
            text-align: center;
            margin-top: 10px;
            font-size: 16px;
            color: red;
        }

        .inline-group {
            display: flex;
            gap: 10px;
        }

      

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
 
    <div class="form-container">
        <h5>Create Voucher</h5>
         <div class="form-group">
             <label for="tb_Sponsor_Company">Sponsor Name:</label>
             <asp:TextBox ID="tb_Sponsor_Name" runat="server" CssClass="form-control" />
         </div>

      
        <div class="form-group">
            <label for="tb_Desc">Description:</label>
            <asp:TextBox ID="tb_Desc" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group">
            <label>Validity:</label>
            <div class="inline-group">
                <asp:TextBox ID="tb_Validity" runat="server" CssClass="form-control" style="width: 10%"/>
                <asp:DropDownList ID="ddl_Validity" runat="server" CssClass="form-control" style="width: 20%">
                    <asp:ListItem Text="" Value=""></asp:ListItem>
                    <asp:ListItem Text="Days" Value="Days"></asp:ListItem>
                    <asp:ListItem Text="Months" Value="Months"></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>

        <div class="form-group">
            <label for="tb_Points_Required">Points Required:</label>
            <asp:TextBox ID="tb_Points_Required" runat="server" CssClass="form-control" style="width: 10%"/>
        </div>

        <div class="form-group">
            <asp:Button ID="btn_Submit" runat="server" Text="Create" CssClass="btn-add" OnClick="btn_create_Click"/>
        </div>

    </div>
</asp:Content>
