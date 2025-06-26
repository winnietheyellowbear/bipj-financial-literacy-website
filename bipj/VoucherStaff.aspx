<%@ Page Title="" Language="C#" MasterPageFile="~/Staff_Nav.Master" AutoEventWireup="true" CodeBehind="VoucherStaff.aspx.cs" Inherits="bipj.VoucherStaff" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


    <head>
      <asp:ScriptManager ID="ScriptManager" runat="server" />
    </head>


    <style>
    body {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        background-color: #f7f7f7;
        color: #333;
    }

    .voucher-table {
        width: 85%;
        margin: 30px auto; 
        border: 1px solid #ddd;
        background: linear-gradient(135deg, #ffffff, #f1f1f1);
        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
        overflow: hidden;
    }

    .voucher-table th, .voucher-table td {
        padding: 16px 24px;
        text-align: center;
        border-bottom: 1px solid #ddd;
        font-size: 14px;
    }

    .voucher-table th {
        background-color: #EBE3F7;
        color: black;
        font-weight: bold;
        text-transform: uppercase;
        letter-spacing: 1px;
    }

    .voucher-table tr:hover {
        background-color: #f3f0f7;
    }

    .voucher-table img {
        width: 50px;
        height: 50px;
    }

    .voucher-table .btn {
        padding: 10px 16px;
        margin: 5px;
        border: none;
        border-radius: 6px;
        font-size: 14px;
        cursor: pointer;
        transition: transform 0.2s, background-color 0.3s ease;
        font-weight: bold;
    }

  
    .voucher-table .btn-update {
        background-color: #3B387E;
        color: white;
    }

    .voucher-table .btn:hover {
        transform: translateY(-2px);
    }

    .voucher-table .btn-update:hover {
        background-color: #59569E;
    }




    .voucher-table .search-box {
        width: 300px;
        padding: 10px;
        margin: 20px auto;
        font-size: 14px;
        border-radius: 6px;
        border: 1px solid #ddd;
        background-color: #fff;
    }

    @media screen and (max-width: 768px) {
        .voucher-table {
            width: 95%;
        }

        .voucher-table th, .voucher-table td {
            padding: 12px 16px;
            font-size: 12px;
        }

        .voucher-table .search-box {
            width: 80%;
        }
    }

     .custom {
         padding: 5px;
         border: 1px solid #ccc;
         border-radius: 10px;
         box-sizing: border-box; 
     }

</style>

   <br />
   <br />

<asp:UpdatePanel ID="UpdatePanel" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        

        <div class="text-center">
            <asp:TextBox ID="txtSearch" runat="server" AutoPostBack="true" placeholder=" 🔍 " class="custom"/>

          
            <asp:DropDownList ID="filterType" runat="server" AutoPostBack="true" class="custom">
                <asp:ListItem Text="Select Category Type" Value="" />
                <asp:ListItem Text="Grocery" Value="Grocery" />
                <asp:ListItem Text="Recipe" Value="Recipe" />
            </asp:DropDownList>

            <asp:DropDownList ID="filterOrder" runat="server" AutoPostBack="true" class="custom">
                <asp:ListItem Text="Select Order" Value="" />
                <asp:ListItem Text="Latest" Value="latest" />
                <asp:ListItem Text="Earliest" Value="earliest" />
            </asp:DropDownList>

        </div>

        <table class="voucher-table">
            <tr>
                <th>ID</th>
                <th>Company Name</th>
                <th>Description</th>
                <th>Validity</th>
                <th>Points Required</th>
                <th>Number Of Exchange</th>
                <th></th>
            </tr>

            <asp:Repeater ID="Voucher" runat="server">
                <ItemTemplate>
                    <tr>
                        <td>
                            <%# Eval("Voucher_ID") %>
                        </td>
                        <td>
                            <%# Eval("Company_Name") %>
                        </td>
                        <td>
                            <%# Eval("Description") %>
                        </td>
                        <td>
                            <%# Eval("Validity") %>
                        </td>
                        <td>
                            <%# Eval("Points_Required") %>
                        </td>
                        <td>
                           <%-- <%# Eval("Number_Of_Exchange") %>--%>
                        </td>
                        <td>
                            <asp:Button runat="server" Text="Update" CommandArgument='<%# Eval("Voucher_ID") %>' CssClass="btn btn-update"/>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </table>


    </ContentTemplate>
</asp:UpdatePanel>

   <br />
   <br />


</asp:Content>
