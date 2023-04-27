using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using ACMESSPAttendance.Utilities;

namespace ACMESSPAttendance
{
    public partial class AttendanceLogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ASPxlblwarningInfo.Text = "";
            string password = txt_Password.Text.Trim();
            txt_Password.Attributes.Add("value", password);

            string selectedvalue = (!string.IsNullOrEmpty(ddl_course?.SelectedItem?.Value)) ? (ddl_course?.SelectedItem?.Value) : "";

            if (!string.IsNullOrEmpty(selectedvalue))
                ddl_course.Items.FindByValue(selectedvalue).Selected = true;


        }

        private bool PopulateCourse()
        {
            bool hasCourse = false;
            if (Session[AttendanceFunction.SESS_USERID] != null)
            {
                DataTable dtStudCourse = AttendanceFunction.GetStudentCourse(Convert.ToInt32(Session[AttendanceFunction.SESS_USERID]));

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

        protected void ASPxbtnCourse_Click(object sender, EventArgs e)
        {
            string username = txt_Username.Text.Trim();
            string password = txt_Password.Text.Trim();
            bool isSchHolidayBlocked = false;
            if (username.Length > 3 && password.Length > 4 && AttendanceFunction.ValidateUser(username, password, ref isSchHolidayBlocked))
            {
                if (!isSchHolidayBlocked)
                {
                    if (PopulateCourse())
                    {
                        if (AttendanceFunction.GetSignInStatus(Convert.ToInt32(Session[AttendanceFunction.SESS_USERID])))
                        {
                            btn_Login.Enabled = false;
                            btn_Logout.Enabled = true;
                        }
                        else
                        {
                            btn_Login.Enabled = true;
                            btn_Logout.Enabled = false;
                        }
                        ASPxlblInfo.Text = "";
                    }
                    else
                    {
                        ASPxlblInfo.Text = "Please contact your campus for assistance.";
                    }
                }
                else
                {
                    ASPxlblInfo.Text = "You could not sign in attendance during your campus holiday breaks.";
                }
            }
            else if (username.Length <= 3)
            {
                ASPxlblInfo.Text = "Please enter Username";
            }
            else if (password.Length <= 4)
            {
                ASPxlblInfo.Text = "Please enter Password";
            }
            else
            {
                ASPxlblInfo.Text = "Your Username or Password is invalid.";
            }
        }

        protected void ASPxbtnSignin_Click(object sender, EventArgs e)
        {
            bool isSuccessSignIn = false;
            if (AttendanceFunction.GetSignInStatus(Convert.ToInt32(Session[AttendanceFunction.SESS_USERID])))
            {
                ASPxlblInfo.Text = "You did not sign out yet. Please sign out first.";
            }
            else
            {
               
                if (ddl_course.Items.Count > 0 &&  !string.IsNullOrEmpty(ddl_course.SelectedItem.Value))
                {
                    isSuccessSignIn = AttendanceFunction.RecordSigninAttendance(Convert.ToInt32(Session[AttendanceFunction.SESS_USERID]));
                    if (isSuccessSignIn)
                    {
                        ASPxlblInfo.Text = string.Format("You have successfully signed in at {0}.", Session[AttendanceFunction.SESS_TIMEIN].ToString());
                        btn_Login.Enabled = true;
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
                }
            }
        }


        public void ASPxbtnSignOut_Click(object sender, EventArgs e)
        {
            if (Session[AttendanceFunction.SESS_USERID] != null && Session[AttendanceFunction.SESS_TIMEIN] != null)
            {
                bool isSuccessSignOut = false;
                if (ddl_course.Items.Count > 0 && !string.IsNullOrEmpty(ddl_course.SelectedItem.Value))
                {
                    ASPxlblwarningInfo.Text = "";
                    isSuccessSignOut = AttendanceFunction.RecordSignoutAttendance(Convert.ToInt32(Session[AttendanceFunction.SESS_USERID]), Convert.ToDateTime(Session[AttendanceFunction.SESS_TIMEIN]), !string.IsNullOrEmpty(ddl_course.SelectedItem.Value) ? Convert.ToInt32(ddl_course.SelectedItem.Value) : 0);
                    if (isSuccessSignOut)
                    {
                        ASPxlblInfo.Text = string.Format("You have successfully signed out at {0}.", Session[AttendanceFunction.SESS_TIMEIN].ToString());
                        btn_Logout.Enabled = true;
                    }
                    else
                        ASPxlblInfo.Text = "You sign out failed.";
                }
                else
                {
                    ASPxlblInfo.Text = "";
                    ASPxlblwarningInfo.Text = "Please select course from the dropdownlist";
                }
            }
        }

    }
}