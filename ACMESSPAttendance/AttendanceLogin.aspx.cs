﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using ACMESSPAttendance.Utilities;
using System.Web.Services;
using System.Data.SqlClient;
using System.Configuration;
using ACMESSPAttendance.Model;
using ACMESSPAttendance.Common;
using System.Text;
using ACMESSPAttendance.SAMLUtilities.Data;
using ACMESSPAttendance.SAMLUtilities.Helpers;
using ACMESSPAttendance.SAMLUtilities;
using System.Web.UI.HtmlControls;
using Dapper;

namespace ACMESSPAttendance
{
    public partial class AttendanceLogin : System.Web.UI.Page
    {       
        protected void Page_Load(object sender, EventArgs e)
        {                   
            ASPxlblwarningInfo.Text = "";
            string password = txt_Password.Text.Trim();
            txt_Password.Attributes.Add("value", password);
            // timer.InnerText = Request.Form[hdshowtimer.UniqueID];
            if (!IsPostBack)
            {
                var resetStatus = Request.QueryString["resetattendancepage"];
                if(resetStatus != null && bool.Parse(resetStatus) == true)
                {
                    resetAttendancePage();
                }
                else
                {
                    int userid = Convert.ToInt32(Request.QueryString["userid"]);
                    if (userid != 0)
                    {
                        FillLogin(userid);
                    }
                    else
                    {
                        ClearAllSession();
                    }
                }
                string _username = txt_Username.Text.Trim();
                string _password = txt_Password.Text.Trim();
                bool isSchHolidayBlocked = false;
                if(AttendanceFunction.ValidateUser(_username, _password, ref isSchHolidayBlocked))
                {
                    if (!isSchHolidayBlocked)
                    {
                        PopulateCourse();
                    }
                }
                GetStudentAttendanceDetails();
            }
            btn_Logout.Enabled = false;
        }

        private void FillLogin(int id)
        {
            string password = "", username = "";
            string sqlText = "Select ud.AOLEmail,u.Password, u.Username from ACME_MAIN_TEST.dbo.[User] u join ACME_AOL_TEST.dbo.UserDetail ud on ud.UserID = u.UserID Where u.UserID = @UserID";
            try
            {                
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlText, conn))
                    {
                        cmd.Parameters.Add("@UserID", SqlDbType.Char).Value = id;
                        SqlDataReader dreader = cmd.ExecuteReader();
                        if (dreader.HasRows && dreader.Read())
                        {
                            if (dreader[0] != DBNull.Value)
                            {
                                username = dreader.GetString(0);
                            }
                            else
                            {
                                username = dreader.GetString(2);
                            }
                            password = dreader.GetString(1);
                        }
                        txt_Username.Text = username;
                        txt_Password.Attributes.Add("value", password);
                        txt_Password.Text = password;
                        //SetUserSession(txt_Username.Text?.Trim(), password?.Trim());
                    }
                    conn.Close();
                }                
            }
            catch (Exception exc)
            {
                exc.Message.ToString();                
            }
        }

        protected void ASPxbtnSignin_Click(object sender, EventArgs e)
        {
            bool isSuccessSignIn = false;
            string username = txt_Username.Text.Trim();
            string password = txt_Password.Text.Trim();
            bool isSchHolidayBlocked = false;
            if (username.Length > 3 && password.Length > 4 && AttendanceFunction.ValidateUser(username, password, ref isSchHolidayBlocked))
            {
                if (!isSchHolidayBlocked)
                {
                    if (AttendanceFunction.GetSignInStatus(GetCurrentUserID()))
                    {
                        ASPxlblInfo.Text = "You did not sign out yet. Please sign out first.";
                        btn_Logout.Enabled = true;
                        btn_myALOCC.Visible = true;
                        SetUserSession(username, password);
                        GetStudentAttendanceDetails();
                    }
                    else
                    {

                        if (ddl_course.Items.Count > 0 && !string.IsNullOrEmpty(ddl_course.SelectedItem.Value))
                        {
                            isSuccessSignIn = AttendanceFunction.RecordSigninAttendance(GetCurrentUserID());
                            if (isSuccessSignIn)
                            {
                                ASPxlblInfo.Text = string.Format("You have successfully signed in at {0}.", Session[AttendanceFunction.SESS_TIMEIN].ToString());
                                btn_Login.Enabled = false;
                                btn_Logout.Enabled = true;
                                btn_myALOCC.Visible = true;
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "countdown", "countdown();", true);
                                SetUserSession(username, password);
                                GetStudentAttendanceDetails();
                                ddl_course.Enabled = false;
                            }
                            else
                            {
                                ASPxlblInfo.Text = "You sign in failed.";
                            }
                        }
                        else
                        {
                            ASPxlblInfo.Text = "";
                            ASPxlblwarningInfo.Text = "Please select course from the dropdownlist";
                            PopulateCourse();
                        }
                    }
                }
                else
                {
                    ASPxlblInfo.Text = "You could not sign in attendance during your campus holiday breaks.";
                }
            }
            else if (username.Length <= 3)
            {
                ASPxlblInfo.Text = "";
                ASPxlblwarningInfo.Text = "Please enter Username";
            }
            else if (password.Length <= 4)
            {
                ASPxlblInfo.Text = "";
                ASPxlblwarningInfo.Text = "Please enter Password";
            }
            else
            {
                ASPxlblInfo.Text = "";
                ASPxlblwarningInfo.Text = "Your Username or Password is invalid.";
            }
        }


        public void ASPxbtnSignOut_Click(object sender, EventArgs e)
        {
            string exception = "Sign Out process : " + Environment.NewLine + "Current UserID : " + GetCurrentUserID() + Environment.NewLine + "Session[AttendanceFunction.SESS_TIMEIN] : " + GetSignInDateTime();
            LogWriter.LogWrite(exception);

            if (GetCurrentUserID() > 0 && GetSignInDateTime() != DateTime.MinValue)
            {
                bool isSuccessSignOut = false;
                if (ddl_course.Items.Count > 0 && !string.IsNullOrEmpty(ddl_course.SelectedItem.Value))
                {
                    ASPxlblwarningInfo.Text = "";

                    bool bDateDifference = AttendanceFunction.IsDateDifference(GetSignInDateTime(), GetCurrentTimebyUserTimeZone());

                    string bDateDifference_exception = "Auto Sign Out process : " + Environment.NewLine + "Current date now : " + GetCurrentTimebyUserTimeZone() + Environment.NewLine + "Session[AttendanceFunction.SESS_TIMEIN] : " + GetSignInDateTime() + Environment.NewLine + "bDateDifference : " + bDateDifference;
                    LogWriter.LogWrite(bDateDifference_exception);

                    if (!bDateDifference)
                    {
                        isSuccessSignOut = AttendanceFunction.RecordSignoutAttendance(GetCurrentUserID(), GetSignInDateTime(), !string.IsNullOrEmpty(ddl_course.SelectedItem.Value) ? Convert.ToInt32(ddl_course.SelectedItem.Value) : 0);
                        if (isSuccessSignOut)
                        {
                            ASPxlblInfo.Text = string.Format("You have successfully signed out at {0}.", Session[AttendanceFunction.SESS_TIMEOUT].ToString());
                            btn_Logout.Enabled = false;
                            btn_Login.Enabled = true;
                            btn_myALOCC.Visible = false;
                            ddl_course.Enabled = true;

                            SignOut();

                            GetStudentAttendanceDetails();
                            ClearSession();
                        }
                        else
                        {
                            ASPxlblInfo.Text = "You sign out failed.";
                        }
                    }
                    else
                    {
                        DateTime dtTimeout = AttendanceFunction.AbsoluteEnd(GetSignInDateTime());
                        AttendanceFunction.UpdateAutoSignOutAttendance(GetCurrentUserID(), GetSignInDateTime(), dtTimeout, !string.IsNullOrEmpty(ddl_course.SelectedItem.Value) ? Convert.ToInt32(ddl_course.SelectedItem.Value) : 0);

                        DateTime dtTimein = AttendanceFunction.AbsoluteStart(GetCurrentTimebyUserTimeZone());

                        AttendanceFunction.UpdateAutoSigninAttendance(GetCurrentUserID(), dtTimein);

                        isSuccessSignOut = AttendanceFunction.RecordSignoutAttendance(GetCurrentUserID(), dtTimein, !string.IsNullOrEmpty(ddl_course.SelectedItem.Value) ? Convert.ToInt32(ddl_course.SelectedItem.Value) : 0);

                        if (isSuccessSignOut)
                        {
                            ASPxlblInfo.Text = string.Format("You have successfully signed out at {0}.", Session[AttendanceFunction.SESS_TIMEOUT].ToString());
                            btn_Logout.Enabled = false;
                            btn_Login.Enabled = true;
                            btn_myALOCC.Visible = false;
                            ddl_course.Enabled = true;
                            SignOut();
                            GetStudentAttendanceDetails();
                            ClearSession();
                        }
                        else
                        {
                            ASPxlblInfo.Text = "You sign out failed.";
                        }
                    }
                }
                else
                {
                    ASPxlblInfo.Text = "";
                    ASPxlblwarningInfo.Text = "Please select course from the dropdownlist";
                }
            }
            else
            {
                ASPxlblInfo.Text = "";
                ASPxlblwarningInfo.Text = "Your session cleared. Please sign in again and continue it.";
                ClearSession();

                btn_Logout.Enabled = false;
                btn_Login.Enabled = true;
                btn_myALOCC.Visible = false;
                ddl_course.Enabled = true;
                GetStudentAttendanceDetails();
            }
        }

        bool resetAttendancePage()
        {
            Session.Clear();
            Session.Abandon();
            return true;
        }

        void ClearSession()
        {
            Session.Remove(Utility.SessionUserNameKey);
        }

        bool ClearAllSession()
        {
            Session.Clear();
            Session.Abandon();
            return true;
        }

        protected void ASPxbtnmyALOCC_Click(object sender, EventArgs e) 
        {
            Response.Redirect("Canvas.aspx");
        }
        
        bool SetUserSession(string userName, string password)
        {
            ClearSession();
            bool isSuccess = false;
            var user = new UserViewModel();
            user = UserManagement.Login(userName, password, Request.UserHostAddress);

            user = UserManagement.Check2ProduceUserData(user);
            if (user != null)
            {
                Session[Utility.SessionUserNameKey] = user;
                isSuccess = true;
            }
            return isSuccess;
        }

        private void GetStudentAttendanceDetails()
        {
            try
            {

                int LoggedUserID = 0;

                if(GetCurrentUserID() > 0)
                {
                    LoggedUserID = GetCurrentUserID();
                }
                
                if (LoggedUserID > 0)
                {
                    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString);
                    using (var db = con)
                    {
                        var param = new DynamicParameters();

                        param.Add("@UserID", LoggedUserID);

                        try
                        {
                            //sp_ProgramDetail
                            using (var results = db.QueryMultiple("sp_GetAttendanceDetailsByUserID_SSP", param, commandType: System.Data.CommandType.StoredProcedure))
                            {
                                var result = results.Read<AttendanceViewModel>().ToList();
                                if (result == null)
                                    Response.Redirect("/");
                                rpt_Records.DataSource = result;
                                rpt_Records.DataBind();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message?.ToString());
                        }
                    }
                }
                else
                {
                    rpt_Records.DataSource = new List<AttendanceViewModel>();
                    rpt_Records.DataBind();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/");
            }
        }

        protected void rpt_Records_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {

            int LoggedUserID = 0;

            if (GetCurrentUserID() > 0)
            {
                LoggedUserID = GetCurrentUserID();
            }

            if (LoggedUserID > 0)
            {
                if (rpt_Records.Items.Count < 1)
                {
                    if (e.Item.ItemType == ListItemType.Footer)
                    {
                        HtmlGenericControl dvNoRec = e.Item.FindControl("dvNoRecords") as HtmlGenericControl;
                        if (dvNoRec != null)
                        {
                            dvNoRec.Visible = true;
                        }
                    }
                }

                if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
                {
                    var lit_Date = (Literal)e.Item.FindControl("lit_Date");
                    var lit_TimeIn = (Literal)e.Item.FindControl("lit_TimeIn");
                    var lit_TimeOut = (Literal)e.Item.FindControl("lit_TimeOut");
                    var lit_TimeStudied = (Literal)e.Item.FindControl("lit_TimeStudied");

                    var record = (AttendanceViewModel)e.Item.DataItem;
                    if (record != null)
                    {
                        lit_Date.Text = record.Date.ToString("MM/dd/yyyy");

                        lit_TimeIn.Text = record.TimeIn;

                        lit_TimeOut.Text = record.TimeOut;

                        lit_TimeStudied.Text = record.TimeStudied;
                    }
                }
            }
            else
            {
                if (rpt_Records.Items.Count < 1)
                {
                    if (e.Item.ItemType == ListItemType.Footer)
                    {
                        HtmlGenericControl dvNoRec = e.Item.FindControl("dvNoRecords") as HtmlGenericControl;
                        if (dvNoRec != null)
                        {
                            dvNoRec.Visible = true;
                        }
                    }
                }
            }
        }

        private DateTime GetCurrentTimebyUserTimeZone()
        {
            return AttendanceFunction.GetCurrentTimebyUserTimeZone(GetCurrentUserID());
        }

        private bool PopulateCourse()
        {
            bool hasCourse = false;
            if (GetCurrentUserID() > 0)
            {
                DataTable dtStudCourse = AttendanceFunction.GetStudentCourse(GetCurrentUserID());

                ddl_course.Items.Clear();
                if (dtStudCourse != null && dtStudCourse.Rows.Count > 0)
                {
                    foreach (DataRow rowSC in dtStudCourse.Rows)
                    {
                        ddl_course.Items.Add(new ListItem(rowSC["CourseName"].ToString(), Convert.ToInt32(rowSC["ccid"]).ToString()));


                    }
                    hasCourse = true;
                }
                else
                {
                    hasCourse = false;
                }
                ddl_course.Items.Insert(0, new ListItem("--Please Select--", ""));
                //ddl_course.Items.Clear();

            }
            return hasCourse;
        }

        private void SignIn()
        {
            var user = UserManagement.GetUser();

            if (user != null && user.UserId > 0)
            {
                /* Add SSP UserSession */
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString);
                conn.Open();

                SqlCommand cmd = new SqlCommand("SSP_SSPUserSession", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@mode", SqlDbType.NVarChar);
                cmd.Parameters.Add("@UserID", SqlDbType.Int);
                cmd.Parameters.Add("@SessionID", SqlDbType.Int);
                cmd.Parameters.Add("@ProjectTypeID", SqlDbType.Int);
                cmd.Parameters.Add("@status", SqlDbType.Int);
                cmd.Parameters[0].Value = "Insert_SSPUserSession";
                cmd.Parameters[1].Value = user.UserId;
                cmd.Parameters[2].Value = user.SessionId;
                cmd.Parameters[3].Value = 3;
                cmd.Parameters[4].Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                user.SSPUserSessionID = (int)cmd.Parameters["@status"].Value;
            }
        }

        private void SignOut()
        {
            var user = UserManagement.GetUser();

            if (user != null && user.UserId > 0)
            {
                DateTime currentDatetime = AttendanceFunction.GetCurrentTimebyUserTimeZone(user.UserId);

                /* Update SSP UserSession */
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_main_test_CS"].ConnectionString);
                conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString);
                conn.Open();

                SqlCommand cmd = new SqlCommand("SSP_SSPUserSession", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@mode", SqlDbType.NVarChar);
                cmd.Parameters.Add("@CurrentDateTime", SqlDbType.DateTime);
                cmd.Parameters.Add("@SSPUserSessionID", SqlDbType.Int);
                cmd.Parameters[0].Value = "Update_SSPUserSession";
                cmd.Parameters[1].Value = currentDatetime;
                cmd.Parameters[2].Value = user.SSPUserSessionID;
                cmd.ExecuteNonQuery();
            }
        }
        private int GetCurrentUserID()
        {
            int UserID = Convert.ToInt32(Request.QueryString["userid"]);
            return UserID;
        }
        private DateTime GetSignInDateTime()
        {
            int UserID = Convert.ToInt32(Request.QueryString["userid"]);
            DateTime SignInDateTime = DateTime.MinValue;
            try
            {
                DateTime currentTime = AttendanceFunction.GetCurrentTimebyUserTimeZone(UserID);
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
                {
                    conn.Open();
                    string sqlText = @"SELECT AttendanceID, TimeIn From Attendance
                                 WHERE UserID = @userid
                                 AND (CONVERT(date, TimeIn) = CONVERT(date, @CurrentTime) or DATEDIFF(Hour,TimeIn, @CurrentTime) <= 24 )
                                 AND TimeOut IS NULL 
                                 Order by AttendanceID desc";
                    using (SqlCommand cmd = new SqlCommand(sqlText, conn))
                    {
                        cmd.Parameters.Add("@userid", SqlDbType.Int);
                        cmd.Parameters["@userid"].Value = UserID;
                        cmd.Parameters.Add("@CurrentTime", SqlDbType.DateTime);
                        cmd.Parameters["@CurrentTime"].Value = currentTime;
                        SqlDataReader dreader = cmd.ExecuteReader();
                        if (dreader.HasRows && dreader.Read())
                        {
                            SignInDateTime = dreader.GetDateTime(1);
                        }
                        dreader.Dispose();
                        dreader.Close();
                    }
                    conn.Close();
                }
            }
            catch (SqlException)
            {
            }
            return SignInDateTime;
        }
    }
}