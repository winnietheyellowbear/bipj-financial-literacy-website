<%@ Page Title="Login" Language="C#" MasterPageFile="~/Customer_Nav_LoggedIn.Master" AutoEventWireup="true" CodeBehind="Loginpage.aspx.cs" Inherits="bipj.Loginpage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">



    <div class="container mt-5">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header bg-primary text-white">
                        <h4 class="mb-0">Login</h4>
                    </div>
                    <div class="card-body">
                        <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false"></asp:Label>
                        
                        <div class="form-group">
                            <label for="txtEmail">Email</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter your email"></asp:TextBox>
                        </div>
                        
                        <div class="form-group">
                            <label for="txtPassword">Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter your password"></asp:TextBox>
                        </div>
                        
                        <div class="form-group form-check">
                            <asp:CheckBox ID="chkRememberMe" runat="server" CssClass="form-check-input" />
                            <label class="form-check-label" for="chkRememberMe">Remember me</label>
                        </div>
                        
                        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary btn-block" OnClick="btnLogin_Click" />
                        
                        <div class="text-center mt-3">
                            <asp:LinkButton ID="btnForgotPassword" runat="server" CssClass="text-muted" OnClick="btnForgotPassword_Click">Forgot password?</asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>