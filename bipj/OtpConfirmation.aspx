<%@ Page Title="OTP Confirmation" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.master" AutoEventWireup="true" CodeBehind="OtpConfirmation.aspx.cs" Inherits="bipj.OtpConfirmation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-5">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header bg-info text-white">
                        <h4 class="mb-0">OTP Confirmation</h4>
                    </div>
                    <div class="card-body">
                        <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false"></asp:Label>
                        <div class="form-group">
                            <label for="txtOTP">Enter the OTP sent to your email</label>
                            <asp:TextBox ID="txtOTP" runat="server" CssClass="form-control" placeholder="OTP"></asp:TextBox>
                        </div>
                        <asp:Button ID="btnConfirm" runat="server" Text="Confirm OTP" CssClass="btn btn-info btn-block" OnClick="btnConfirm_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
