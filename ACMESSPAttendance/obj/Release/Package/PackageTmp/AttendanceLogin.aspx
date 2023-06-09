﻿<%@ Page Title="Attendance Login Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AttendanceLogin.aspx.cs" Inherits="ACMESSPAttendance.AttendanceLogin" %>
<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeaderContent" runat="server">
    <link href="Content/login.css" rel="stylesheet" />
    <style type="text/css">
        body {
            color: #000 !important;
        }

        .form-control {
            border: 3px solid #8bc34a !important;
        }



        .btn-primary {
            color: #fff !important;
            background-color: #8bc34a !important;
            border-color: #8bc34a !important;
            max-width: 102px !important;
            max-height: 35px !important;
            border: 1px !important;
            border-radius: 4px !important;
            font-size: 12px;
        }
    </style>
    <script type="text/javascript">
        function onClickCollapseInstructionChanges(s, e) {
            var curFImageUrl = s.GetImageUrl();
            if (curFImageUrl.indexOf('Images/Expand.png') > 0) {
                ASPxImgISCollapse.SetImageUrl('../Images/Collapse.png');
                $("#divFolder").slideDown("slow");
            }
            else {
                ASPxImgISCollapse.SetImageUrl('../Images/Expand.png');
                $("#divFolder").slideUp("slow");
            }
        }
    </script>

</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <div class="bgimag">


        <asp:Panel ID="pnl_Login" runat="server" DefaultButton="btn_Login" Class="loginpnl">
            <div>
                <h2 class="head-title">Attendance Sign In/Out</h2>

                <asp:Panel ID="pnl_Incorrect" runat="server" Visible="false">
                    <asp:Label ID="Label2" runat="server" Text="The username / password entered is incorrect. Please try again." ForeColor="Red" />
                    <div style="padding-bottom: 10px;"></div>
                </asp:Panel>
                <div class="form-group mb-3">
                    <label for="emailaddress">Email / UserName</label>
                    <asp:TextBox ID="txt_Username" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server"
                        ControlToValidate="txt_Username" ErrorMessage="Please enter your email / username"
                        ForeColor="Red"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group mb-3">
                    <label for="password">Password</label>
                    <asp:TextBox ID="txt_Password" runat="server"  Password="true" TextMode="Password" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server"
                        ControlToValidate="txt_Password" ErrorMessage="Please enter your password"
                        ForeColor="Red"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group mb-3">

                   <%-- <asp:Button ID="ASPxbtnCourse" runat="server" Text="Select Course" Font-Size="11pt"
                        AutoPostBack="true" OnClick="ASPxbtnCourse_Click"></asp:Button>--%>
                    <asp:Button ID="ASPxbtnCourse" OnClick="ASPxbtnCourse_Click" runat="server"  CssClass="btn btn-primary btn-lg width-lg btn-rounded" style="text-align:center;padding:10px" Text="Select Course" Width="100%" ></asp:button>
                     <asp:DropDownList ID="ddl_course" runat="server" ValueType="System.String"  Font-Size="9pt" Font-Names="Frutiger" style="padding:5px;width:178px;height: 35px; border: 3px solid #8bc34a !important;">
                        <Items>
                        </Items>
                    </asp:DropDownList>
                </div>
              

                &nbsp;<asp:Label ID="ASPxlblInfo" runat="server" Text="" Font-Size="10pt" ForeColor="Navy">
                </asp:Label>

                <%--<asp:Button ID="ASPxbtnSignin" runat="server" Text="Sign In" ClientEnabled="false" Font-Size="11pt"
                    AutoPostBack="true" ></asp:Button>

                <asp:Button ID="ASPxbtnSignOut" runat="server" Text="Sign Out" Font-Size="11pt"
                    ClientEnabled="false" AutoPostBack="true" ></asp:Button>--%>

                <div class="form-group text-center mb-3">
                    <%--  <div>
                        <div class="flex-dir">
                            <input type="checkbox" class="active-checkbox" />
                            <label class="sign-in">Stay Signed in</label>
                        </div>
                        <a class="forget-sec" href="#">Forget Password?</a>
                    </div>--%>
                    <asp:Button ID="btn_Login" runat="server"  AutoPostBack="true" onclick="ASPxbtnSignin_Click" CssClass="btn btn-primary btn-lg width-lg btn-rounded" Text="Sign In" Width="100%" ></asp:button>

                    <asp:Button ID="btn_Logout" runat="server"  AutoPostBack="true" onclick="ASPxbtnSignOut_Click"  CssClass="btn btn-primary btn-lg width-lg btn-rounded" Text="Sign Out" Width="100%" ></asp:button>
                </div>
                <%--  <div class="reg-div">
                    <button class="student-reg">NOT A STUDENT?</button>
                </div>--%>
            </div>
        </asp:Panel>
    </div>

</asp:Content>

