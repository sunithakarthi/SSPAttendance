<%@ Page  Title="Attendance Login Page" Language="C#" MasterPageFile="~/LoginSite.Master" AutoEventWireup="true" CodeBehind="AttendanceLogin.aspx.cs" Inherits="ACMESSPAttendance.AttendanceLogin" %>
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
        .labeltxt{
            color:#8bc34a;
        }

        .btn-centre{
            margin:0 auto;
            display: block;
        }

        .margin20{
            margin: 20px 0px;
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

    <script>
        function setButtonClicked() {
            document.getElementById('<%= HiddenField1.ClientID %>').value = 'true';
        }
    </script>

    <script type="text/javascript">
        var intervalId;
        function stopTimer() {       
            var value = $("#MainContent_timer").text();    
            document.getElementById("<%=hdshowtimer.ClientID %>").value = value;
            clearInterval(intervalId);
        }
        function countdown() {            
            var startTime = new Date().getTime(); 

            intervalId = setInterval(function () {
                var currentTime = new Date().getTime(); 
                var elapsedTime = currentTime - startTime; 
                
                var hours = Math.floor(elapsedTime / (1000 * 60 * 60));
            var minutes = Math.floor((elapsedTime % (1000 * 60 * 60)) / (1000 * 60));
            var seconds = Math.floor((elapsedTime % (1000 * 60)) / 1000);
            var formattedTime = hours.toString().padStart(2, "0") + ":" +
                minutes.toString().padStart(2, "0") + ":" +
                seconds.toString().padStart(2, "0");       
                document.getElementById('<%= timer.ClientID %>').innerHTML = formattedTime;                
        }, 1000); 
        }




    </script>

    <script type="text/javascript">

        $(function () {
            $("input[type='submit']").click(function () {
                if ($(this).hasClass('changeTarget')) {
                    window.document.forms[0].target = '_blank';
                } else {
                    window.document.forms[0].target = '_self';
                }
            });
        });
    </script>


</asp:Content>   
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
   

    <div>


        <asp:Panel ID="pnl_Login" runat="server" DefaultButton="btn_Login" Class="loginpnl">
            <div>
                <h2 class="head-title">Attendance Sign In/Out</h2>

                <asp:Panel ID="pnl_Incorrect" runat="server" Visible="false">
                    <asp:Label ID="Label2" runat="server" Text="The username / password entered is incorrect. Please try again." ForeColor="Red" />
                    <div style="padding-bottom: 10px;"></div>
                </asp:Panel>
                <div class="form-group mb-3">
                    <label for="emailaddress" class="labeltxt">Email / UserName</label>
                    <asp:TextBox ID="txt_Username" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server"
                        ControlToValidate="txt_Username" ErrorMessage="Please enter your email / username"
                        ForeColor="Red"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group mb-3">
                    <label for="password" class="labeltxt">Password</label>
                    <asp:TextBox ID="txt_Password" runat="server"  Password="true" TextMode="Password" CssClass="form-control" AutoPostBack="true" ClientIDMode="Static" />
                    <asp:HiddenField ID="HiddenField1" runat="server" Value="false" />
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server"
                        ControlToValidate="txt_Password" ErrorMessage="Please enter your password"
                        ForeColor="Red"></asp:RequiredFieldValidator>
                </div>
                <%--<div class="form-group mb-3">
                    <label class="labeltxt">Select Course </label>                     
                     <asp:DropDownList ID="ddl_course" runat="server" ValueType="System.String"  Font-Size="9pt" Font-Names="Frutiger" style="padding:5px;width:178px;height: 35px; border: 3px solid #8bc34a !important;">
                        <Items>                            
                        </Items>
                    </asp:DropDownList>
                </div>--%>
                <%--<asp:Button ID="ASPxbtnCourse" OnClick="ASPxbtnCourse_Click" runat="server"  CssClass="btn btn-primary btn-lg width-lg btn-rounded" style="text-align:center;padding:10px" Text="Select Course" Width="100%" ></asp:button>--%>
               &nbsp;<asp:Label ID="ASPxlblwarningInfo" runat="server" Text="" Font-Size="10pt" ForeColor="red">
                </asp:Label>

                &nbsp;<asp:Label ID="ASPxlblInfo" runat="server" Text="" Font-Size="10pt" ForeColor="#99cc00">
                </asp:Label>
                               
                <br />
                <div id="timer" runat="server" class="head-title" ></div>        
                <asp:HiddenField ID="hdshowtimer" runat="server" />               
                <div class="form-group text-center mb-3">
                  
                    <asp:Button ID="btn_Login" runat="server"  AutoPostBack="true" ClientIDMode="Static" onclick="ASPxbtnSignin_Click" OnClientClick="setButtonClicked();" CssClass="btn btn-primary btn-lg width-lg btn-rounded" Text="Sign In" Width="100%" ></asp:button>

                    <asp:Button ID="btn_Logout" runat="server"  AutoPostBack="true" ClientIDMode="Static" onclick="ASPxbtnSignOut_Click" OnClientClick="stopTimer();"  CssClass="btn btn-primary btn-lg width-lg btn-rounded" Text="Sign Out" Width="100%" ></asp:button>
                </div>

                <div class="margin20">
                    <asp:Button ID="btn_myALOCC" runat="server" onclick="ASPxbtnmyALOCC_Click" CssClass="btn btn-primary btn-lg width-lg btn-rounded btn-centre changeTarget" Text="myAOLCC" Width="100%" Visible="false" ></asp:button>
                </div>
        
            </div>
        </asp:Panel>
    </div>

</asp:Content>

