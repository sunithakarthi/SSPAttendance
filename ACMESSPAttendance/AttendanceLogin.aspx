<%@ Page  Title="Attendance Login Page" Language="C#" MasterPageFile="~/LoginSite.Master" AutoEventWireup="true" CodeBehind="AttendanceLogin.aspx.cs" Inherits="ACMESSPAttendance.AttendanceLogin" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeaderContent" runat="server">

    <link href="Content/login.css" rel="stylesheet" />

    <script src="Scripts/Common/serverDate.js"></script>

    <script type="text/javascript">

        var modalpopupshown = false;
        var intervalId;
        var modelpopupopenatnext2hoursIntervalID;
        var modelpopupopenatnexttwohoursandfiveminutesIntervalID;

        $(document).ready(function () {
            $('#attendancedialog').on('click', function () {
                $('#attendancepopup').modal('show');
            });
        });

        $('#btnYes').on('click', function () {
            $('#attendancepopup').modal('hide');
        });

        $(function () {
            $("input[type='submit']").click(function () {
                if ($(this).hasClass('changeTarget')) {
                    window.document.forms[0].target = '_blank';
                } else {
                    window.document.forms[0].target = '_self';
                }
            });
        });

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

        function setButtonClicked() {
            document.getElementById('<%= HiddenField1.ClientID %>').value = 'true';
        }

        function stopTimer() {
            var value = $("#MainContent_timer").text();
            document.getElementById("<%=hdshowtimer.ClientID %>").value = value;
            clearInterval(intervalId);
            clearInterval(modelpopupopenatnext2hoursIntervalID);
            clearInterval(modelpopupopenatnexttwohoursandfiveminutesIntervalID);
        }

        <%--function countdown(serverdatetime) {

            console.log('serverdatetime : ', serverdatetime);
            var currentDate = new Date();
            var startTime = currentDate.getTime();
            //var twohoursCountdown = new Date(currentDate.getTime() + (2 * 60 * 60000));
            var twohoursCountdown = new Date(currentDate.getTime() + (5 * 60000));
            console.log(twohoursCountdown);

            var twohoursandfiveminutesCountdown = new Date(twohoursCountdown.getTime() + (2 * 60000));
            console.log(twohoursandfiveminutesCountdown);

            var st = srvTime();
            var serverdatetime = new Date(st);
            console.log('javascript serverdatetime : ', serverdatetime);

            intervalId = setInterval(function () {

                var newcurrentdate = new Date();

                var currentTime = newcurrentdate.getTime();

                var elapsedTime = currentTime - startTime;

                // Find the distance between now and the count down date
                var distance = twohoursCountdown - currentTime;

                var fiveminutesdistance = twohoursandfiveminutesCountdown - currentTime;

                var hours = Math.floor(elapsedTime / (1000 * 60 * 60));
                var minutes = Math.floor((elapsedTime % (1000 * 60 * 60)) / (1000 * 60));
                var seconds = Math.floor((elapsedTime % (1000 * 60)) / 1000);
                var formattedTime = hours.toString().padStart(2, "0") + ":" +
                    minutes.toString().padStart(2, "0") + ":" +
                    seconds.toString().padStart(2, "0");
                document.getElementById('<%= timer.ClientID %>').innerHTML = formattedTime;

                var formattednewcurrentdatetime = ("0" + newcurrentdate.getDate()).slice(-2) + "-" + ("0" + (newcurrentdate.getMonth() + 1)).slice(-2) + "-" +
                    newcurrentdate.getFullYear() + " " + ("0" + newcurrentdate.getHours()).slice(-2) + ":" + ("0" + newcurrentdate.getMinutes()).slice(-2) + ":" + ("0" + newcurrentdate.getSeconds()).slice(-2);

                // If the count down is over, write some text 
                if (distance < 0 && modalpopupshown == false) {
                    console.log("distance : " + distance);
                    console.log("distance - formattednewcurrentdatetime : " + formattednewcurrentdatetime);
                    document.getElementById("<%=hdnloggedhours.ClientID %>").value = formattednewcurrentdatetime;
                    showModal();
                }
                else if (fiveminutesdistance < 0 && modalpopupshown == true) {
                    console.log("fiveminutesdistance : " + fiveminutesdistance);
                    console.log("fiveminutesdistance - formattednewcurrentdatetime : " + formattednewcurrentdatetime);
                    clearInterval(intervalId);
                    document.getElementById("<%=hdnloggedhours.ClientID %>").value = formattednewcurrentdatetime;
                    hideModal();
                    document.getElementById('<%= btnSubmit.ClientID %>').click();
                }
            }, 1000);
        }--%>

        function countdown() {

            let startTime = new Date().getTime();

            modelpopupopenatnext2hours();

            intervalId = setInterval(function () {

                let newcurrentdate = new Date();

                let currentTime = newcurrentdate.getTime();

                let elapsedTime = currentTime - startTime;

                var hours = Math.floor(elapsedTime / (1000 * 60 * 60));
                var minutes = Math.floor((elapsedTime % (1000 * 60 * 60)) / (1000 * 60));
                var seconds = Math.floor((elapsedTime % (1000 * 60)) / 1000);
                var formattedTime = hours.toString().padStart(2, "0") + ":" +
                    minutes.toString().padStart(2, "0") + ":" +
                    seconds.toString().padStart(2, "0");
                document.getElementById('<%= timer.ClientID %>').innerHTML = formattedTime;

            }, 1000);
        }

        function modelpopupopenatnext2hours() {

            //var twohoursCountdown = new Date(new Date().getTime() + (2 * 60 * 60000));
            let twohoursCountdown = new Date(new Date().getTime() + (5 * 60000));
            console.log('Next popup will show at :- ', twohoursCountdown);

            modelpopupopenatnext2hoursIntervalID = setInterval(function () {

                let currentTime = new Date().getTime();
                let _twohoursCountdowntime = twohoursCountdown.getTime();
                let distance = _twohoursCountdowntime - currentTime;

                if (distance < 0) {
                    showModal();
                    modelpopupopenatnexttwohoursandfiveminutes();
                }

            }, (5 * 60000));
        }

        function modelpopupopenatnexttwohoursandfiveminutes() {

            let twohoursandfiveminutesCountdown = new Date(new Date().getTime() + (2 * 60000));
            console.log('Next popup will close at :- ', twohoursandfiveminutesCountdown);

            modelpopupopenatnexttwohoursandfiveminutesIntervalID = setInterval(function () {

                let newcurrentdate = new Date();

                let currentTime = newcurrentdate.getTime();

                let _twohoursandfiveminutesCountdown = twohoursandfiveminutesCountdown.getTime();

                let fiveminutesdistance = _twohoursandfiveminutesCountdown - currentTime;

                if (fiveminutesdistance < 0 && modalpopupshown == true) {
                    autoSignout();
                }

            }, (1000));
        }

        function autoSignout() {
            clearInterval(intervalId);
            clearInterval(modelpopupopenatnext2hoursIntervalID);
            clearInterval(modelpopupopenatnexttwohoursandfiveminutesIntervalID);
            hideModal();
            var value = $("#MainContent_timer").text();
            document.getElementById("<%=hdshowtimer.ClientID %>").value = value;
            document.getElementById('<%= btnSubmit.ClientID %>').click();
        }

        function btnNoClick() {
            clearInterval(intervalId);
            clearInterval(modelpopupopenatnext2hoursIntervalID);
            clearInterval(modelpopupopenatnexttwohoursandfiveminutesIntervalID);
            hideModal();
            var value = $("#MainContent_timer").text();
            document.getElementById("<%=hdshowtimer.ClientID %>").value = value;
            document.getElementById('<%= btnSubmit.ClientID %>').click();
        }

        function btnYesClick() {
            clearInterval(modelpopupopenatnexttwohoursandfiveminutesIntervalID);
            modelpopupopenatnexttwohoursandfiveminutesIntervalID = undefined;
            hideModal();
        }

        function targetMeBlank() {
            document.forms[0].target = "_blank";
            $('#attendancepopup').modal('hide');
        }

        function init() {
            var element = $('#attendancepopup').detach();
            $($("form")[0]).append(element);
        }
        window.addEventListener('DOMContentLoaded', init, false);

        function showModal() {
            $('#attendancepopup').modal('show');
            modalpopupshown = true;
        }

        function hideModal() {
            $('#attendancepopup').modal('hide');
            modalpopupshown = false;
        }

    </script>

</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="Content/home.css" rel="stylesheet" />
    <div class="bg-img">
        <div class="bgcolor">
            <div class="rowbodycontainer" style="width: 100%; height: 100%;">
                <div class="row ">
                    <div class="col-sm-12">
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
                                        <asp:TextBox ID="txt_Username" runat="server" CssClass="form-control" AutoPostBack="false" />
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server"
                                            ControlToValidate="txt_Username" ErrorMessage="Please enter your email / username"
                                            ForeColor="Red"></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="form-group mb-3">
                                        <label for="password" class="labeltxt">Password</label>
                                        <asp:TextBox ID="txt_Password" runat="server" Password="true" TextMode="Password" CssClass="form-control" AutoPostBack="false" />
                                        <asp:HiddenField ID="HiddenField1" runat="server" Value="false" />
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server"
                                            ControlToValidate="txt_Password" ErrorMessage="Please enter your password"
                                            ForeColor="Red"></asp:RequiredFieldValidator>
                                    </div>
                                    &nbsp;<asp:Label ID="ASPxlblwarningInfo" runat="server" Text="" Font-Size="10pt" ForeColor="red">
                                    </asp:Label>

                                    &nbsp;<asp:Label ID="ASPxlblInfo" runat="server" Text="" Font-Size="10pt" ForeColor="#99cc00">
                                    </asp:Label>

                                    <br />
                                    <div id="timer" runat="server" class="head-title"></div>
                                    <asp:HiddenField ID="hdshowtimer" runat="server" />
                                    <asp:HiddenField ID="hdnloggedhours" runat="server" />
                                    <div class="form-group text-center mb-3">

                                        <asp:Button ID="btn_Login" runat="server" CausesValidation="False" OnClick="ASPxbtnSignin_Click" CssClass="btn btn-primary btn-lg width-lg btn-rounded" Text="Sign In" Width="100%"></asp:Button>

                                        <asp:Button ID="btn_Logout" runat="server" CausesValidation="False" OnClick="ASPxbtnSignOut_Click" OnClientClick="stopTimer();" CssClass="btn btn-primary btn-lg width-lg btn-rounded" Text="Sign Out" Width="100%"></asp:Button>
                                    </div>

                                    <div class="margin20">
                                        <asp:Button ID="btn_myALOCC" runat="server" OnClick="ASPxbtnmyALOCC_Click" CssClass="btn btn-primary btn-lg width-lg btn-rounded btn-centre changeTarget" Text="myAOLCC" Width="100%" Visible="false"></asp:Button>
                                    </div>

                                </div>
                            </asp:Panel>

                        </div>
                    </div>
                    <div class="clearfix"></div>
                </div>
                <div class="row attendance-tablecontainer">
                    <div class="">
                        <div class="col-sm-12">

                            <asp:Panel ID="pnl_Records" class="pnlrecord" runat="server">

                                <div>
                                    <h3>My Attendance</h3>
                                </div>

                                <div class="attendance-table">
                                    <asp:Repeater ID="rpt_Records" runat="server" OnItemDataBound="rpt_Records_ItemDataBound">
                                        <HeaderTemplate>
                                            <table id="records" class="scrolldown table-bordered">
                                                <thead>
                                                    <tr>
                                                        <th class=" border-right tableheader">DATE</th>
                                                        <th class=" border-right tableheader">START TIME</th>
                                                        <th class=" border-right tableheader">END TIME</th>
                                                        <th class=" border-right tableheader">TIME STUDIED</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <tr>
                                                <td class="border-right tablerow">
                                                    <asp:Literal runat="server" ID="lit_Date" />
                                                </td>
                                                <td class="border-right tablerow">
                                                    <asp:Literal runat="server" ID="lit_TimeIn" />
                                                </td>
                                                <td class="border-right tablerow">
                                                    <asp:Literal runat="server" ID="lit_TimeOut" />
                                                </td>
                                                <td class="border-right tablerow tablerowlast">
                                                    <asp:Literal runat="server" ID="lit_TimeStudied" />
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            </tbody>
                 </table>
                            <div id="dvNoRecords" runat="server" visible="false" style="text-align: center; padding-top: 50px;">
                                <b>No records Found.
                                </b>
                            </div>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                </div>
                            </asp:Panel>
                        </div>
                        <div class="clearfix"></div>
                        <div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div id="attendancepopup" class="modal fade" data-keyboard="false" data-backdrop="static" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header" style="padding-bottom: 0px; padding-top: 8px; border-bottom: none;">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <form id="popupForm" method="post">
                        <table id="Table3" class="attendancedialog" cellspacing="0" cellpadding="0" width="100%">
                            <tr style="background-color: white !important;">
                                <td style="font-size: 15px; word-break: unset !important;">Are you still on myAOLCC?
                                </td>
                            </tr>
                        </table>
                    </form>
                </div>
                <div class="modal-footer">
                    <button id="btnYes" cssclass="btn btn-info waves-effect waves-light btn-primary" onclick="btnYesClick(); return false;">Yes</button>
                    <button id="btnNo" cssclass="btn btn-info waves-effect waves-light btn-primary" onclick="btnNoClick(); return false;">No</button>
                    <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-info waves-effect waves-light btn-primary" Text="No" OnClick="ASPxbtnSignOut_Click" Style="display: none" />
                </div>
            </div>
            <!-- Modal content-->
        </div>
    </div>



</asp:Content>

