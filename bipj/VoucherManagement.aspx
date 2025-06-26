<%@ Page Title="" Language="C#" MasterPageFile="~/Voucher.Master" AutoEventWireup="true" CodeBehind="VoucherManagement.aspx.cs" Inherits="bipj.VoucherManagement1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <style>
     * {
         box-sizing: border-box;
     }

     body {
         font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
         background-color: #f4f6f8;
         margin: 0;
         padding: 20px;
     }

     label {
            display: block;
            font-weight: bold;
            margin-bottom: 5px;
            color: #555;
     }


     .container {
         max-width: 600px;
         margin: auto;
         background: white;
         border-radius: 12px;
         box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
         padding: 30px;
     }

     h2 {
         text-align: center;
         color: #333;
         margin-bottom: 20px;
     }

     .voucher-field {
         margin-bottom: 20px;
     }


     .voucher-text {
        background-color: #f9f9f9;
        padding: 10px;
        border-radius: 4px;
        color: #333;
        font-size: 1rem;
        margin: 0;
        display: block; /* so that the label behaves like a block element */
    }

     .btn {
         width: 100%;
         padding: 14px;
         border: none;
         border-radius: 6px;
         cursor: pointer;
         font-size: 1.1rem;
         font-weight: bold;
         color: white;
         transition: background-color 0.3s ease;
         margin: 8px 0;
     }

      .enable-btn {
            background-color: #28a745;
        }

        .enable-btn:hover {
            background-color: #218838;
        }

        .disable-btn {
            background-color: #dc3545;
        }

        .disable-btn:hover {
            background-color: #c82333;
        }


 </style>

    <div class="container" id="voucherContainer">
    <div class="voucher-field">
        <label for="description">Description:</label>
        <asp:Label ID="description" runat="server" CssClass="voucher-text" Text=""></asp:Label>
    </div>

    <div class="voucher-field">
        <label for="companyName">Company Name:</label>
        <asp:Label ID="companyName" runat="server" CssClass="voucher-text" Text=""></asp:Label>
    </div>

    <div class="voucher-field">
        <label for="validity">Validity:</label>
        <asp:Label ID="validity" runat="server" CssClass="voucher-text" Text=""></asp:Label>
    </div>

    <div class="voucher-field">
        <label for="pointsRequired">Points Required:</label>
        <asp:Label ID="pointsRequired" runat="server" CssClass="voucher-text" Text=""></asp:Label>
    </div>


    <div class="buttons">
        <asp:Button ID="btnEnable" runat="server" Text="Enable" CssClass="enable-btn btn" CommandArgument="Active" OnClick="btn_status_Click"/>
        <asp:Button ID="btnDisable" runat="server" Text="Disable" CssClass="disable-btn btn" CommandArgument="Inactive" OnClick="btn_status_Click"/>
    </div>

    </div>
   

</asp:Content>
