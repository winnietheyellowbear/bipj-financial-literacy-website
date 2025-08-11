<%@ Page Title="Login" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.master"AutoEventWireup="true" CodeBehind="Loginpage.aspx.cs" Inherits="bipj.Loginpage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


    <br />
    <br />
    <div class="container mt-5">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header text-white" style="background-color: #3B387E">
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
                        
                       <div class="d-flex gap-2 mt-2 flex-wrap">
  <asp:Button ID="btnLogin" runat="server"
      Text="Login"
      CssClass="btn btn-primary px-4 py-2"
      OnClick="btnLogin_Click" />

</div>

                       <div class="text-center mt-3">
    <asp:LinkButton ID="btnForgotPassword" runat="server"
        CssClass="text-muted"
        OnClick="btnForgotPassword_Click">
        Forgot password?
    </asp:LinkButton>

    <div class="mt-2">
        <asp:HyperLink ID="lnkFaceLogin" runat="server"
            CssClass="link-primary text-decoration-none small">
            Login with Face ID
        </asp:HyperLink>
    </div>
</div>
                    </div>
                </div>
            </div>
        </div>
    </div>

   <script>
       document.addEventListener('DOMContentLoaded', function () {
           var link = document.getElementById('<%= lnkFaceLogin.ClientID %>');
  var email = document.getElementById('<%= txtEmail.ClientID %>');
    if (link && email) {
        link.addEventListener('click', function () {
            this.href = 'FacialLogin.aspx' + (email.value ? ('?email=' + encodeURIComponent(email.value)) : '');
        });
    }
});
   </script>

</asp:Content>
