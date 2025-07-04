<%@ Page Title="" Language="C#" MasterPageFile="~/Staff_Nav.Master" AutoEventWireup="true" CodeBehind="HomePortfolio.aspx.cs" Inherits="bipj.HomePortfolio" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<br />
<br />
<br />
<h3>Create New Portfolio</h3>
<asp:TextBox ID="txtPortfolioName" runat="server" Placeholder="Enter portfolio name" />
<asp:Button ID="btnCreatePortfolio" runat="server" Text="Create" OnClick="btnCreatePortfolio_Click" />
<asp:Literal ID="litCreateMessage" runat="server" />
<br /><br />

<asp:GridView ID="gvPortfolios" runat="server" AutoGenerateColumns="false" >
    <Columns>
        <asp:BoundField DataField="PortfolioId" HeaderText="ID" />
        <asp:BoundField DataField="PortfolioName" HeaderText="Portfolio Name" />
        <asp:BoundField DataField="CreatedAt" HeaderText="Created On" />

        
        <asp:ButtonField ButtonType="Button" Text="View" CommandName="ViewPortfolio" />

        
        <asp:TemplateField>
            <ItemTemplate>
                <asp:Button ID="btnDelete" runat="server" Text="Delete" 
                    CommandName="DeletePortfolio" 
                    CommandArgument='<%# Eval("PortfolioId") %>' />
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>

</asp:Content>