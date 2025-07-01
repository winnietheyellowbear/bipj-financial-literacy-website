<%@ Page Language="C#" AutoEventWireup="true"  CodeBehind="LoginPage.aspx.cs" Inherits="bipj.LoginPage" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Login - FinClarity</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
    <style>
        .login-container {
            max-width: 380px;
            margin: 70px auto;
            padding: 2.5rem 2rem 2rem 2rem;
            background: #fff;
            border-radius: 14px;
            box-shadow: 0 6px 30px rgba(80,60,160,0.10);
        }
        .btn-login {
            background-color: #5e4bd3;
            color: #fff;
            border: none;
        }
        .btn-login:hover {
            background-color: #4e3cc7;
        }
    </style>
</head>
<body style="background:#f4f2fd;">
    <form id="form1" runat="server">
        <div class="login-container">
            <h3 class="mb-3 text-center">Login</h3>
            <div class="mb-3">
                <label for="txtName" class="form-label">Username</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter your username" />
            </div>
            <div class="mb-3">
                <label for="txtPassword" class="form-label">Password</label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter your password" />
            </div>
            <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" />
            <div class="mt-3 d-grid">
                <asp:Button ID="btnLogin" runat="server" CssClass="btn btn-login" Text="Login" OnClick="btnLogin_Click" />
            </div>
        </div>
    </form>
</body>
</html>
