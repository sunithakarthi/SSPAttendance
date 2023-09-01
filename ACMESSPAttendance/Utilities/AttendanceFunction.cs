using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Web;

namespace ACMESSPAttendance.Utilities
{
    public class AttendanceFunction
    {

        public const int attAdminUserID = 176435;
        private const int ModuleID_AttendanceUser = 60;
        private const int Status_InProgress = 1;

        public const string SESS_USERID = "SignInUserID";
        public const string SESS_SCHOOLID = "SignInUserSchoolID";
        public const string SESS_TIMEIN = "SignIn_Time";

        public const string OFFICE365_API_URL = "https://mail.academyoflearning.net/User/CreateOffice/";
        public const string OFFICE365_API_AUTH_KEY = "56f7c85a-e195-4fcb-af35-06f5baf902f6";

        //  below is the server address for the Office365 SMTP client submission server.  It is set up to send email
        //  per the "Direct Send" method, which requires that the static IP address of the client be whitelisted 
        //  through the Office365 Exchange admin panel, and strongly recommends that the same IP add ress be added to 
        //  the domain's SPF1 record
        private static string mSMTPServer = "academyoflearning-com.mail.protection.outlook.com";
        public static string SMTPServer
        {
            get
            {
                return mSMTPServer;
            }
        }

        public AttendanceFunction()
        {

        }

        public static bool IsValidEmailAddress(string emailAddress)
        {
            bool MethodResult = false;

            try
            {
                MailAddress ma = new MailAddress(emailAddress);

                MethodResult = ma.Address == emailAddress;

            }
            catch //(Exception ex)
            {
                //ex.HandleException();

            }

            return MethodResult;

        }


        public static bool IsExistingUser(string strFirstName, string strLastName, string strEmail, string strStudentNumber, ref int userID)
        {
            bool isExistUser = false;
            string sqlText = @"select UserID FROM ACME_AOL_TEST.dbo.UserDetail 
                                        where FirstName = @FirstName AND LastName = @LastName AND (Email = @Email OR StudentNumber = @StudentNumber)";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlText, conn))
                {
                    cmd.Parameters.Add("@FirstName", SqlDbType.Char).Value = strFirstName;
                    cmd.Parameters.Add("@LastName", SqlDbType.Char).Value = strLastName;
                    cmd.Parameters.Add("@Email", SqlDbType.Char).Value = strEmail;
                    cmd.Parameters.Add("@StudentNumber", SqlDbType.Char).Value = strStudentNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows && dr.Read())
                        {
                            isExistUser = true;
                            userID = dr.IsDBNull(0) ? 0 : dr.GetInt32(0);
                        }
                    }
                }
                conn.Close();
            }
            return isExistUser;
        }

        public static bool SaveSINnum(int userID, string SINNum)
        {
            string sqlText = "Update top (1) ACME_AOL_TEST.dbo.UserDetail Set SIN = @SIN Where UserID = @UserID";
            try
            {
                int SINnumber = GetSINNum(SINNum);
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlText, conn))
                    {
                        cmd.Parameters.Add("@SIN", SqlDbType.Char).Value = SINnumber;
                        cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                return true;
            }
            catch (Exception exc)
            {
                exc.Message.ToString();
                return false;
            }
        }

        private static int GetSINNum(string SINNum)
        {
            return Convert.ToInt32(SINNum.Trim().Replace("-", ""));
        }

        /// <summary>
        /// Validate User's username/password and check if user has permission
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        /// <returns>User table</returns>  
        public static bool ValidateUser(string username, string password, ref bool isSchoolHolidayBlocked)
        {
            bool bValid = false;
            DataTable dtUser = new DataTable();
            DataTable dtUserID = new DataTable();
            DataTable dtUserName = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();

                if (username.Contains("@my-aolcc.com"))
                {
                    string sqlQuery = "SELECT top 1  UserID from[ACME_AOL_TEST].[dbo].UserDetail where AOLEmail = @email";
                    using (SqlDataAdapter da = new SqlDataAdapter(sqlQuery, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@email", username);
                        da.Fill(dtUserID);

                        if (dtUserID != null && dtUserID.Rows.Count > 0)
                        {
                            var userId = dtUserID.Rows[0]["UserID"];

                            string sqlquery = "SELECT Top 1 username from [ACME_MAIN_TEST].[dbo].[User] where userid=@userid";
                            using (SqlDataAdapter daa = new SqlDataAdapter(sqlquery, conn))
                            {
                                daa.SelectCommand.Parameters.AddWithValue("@userid", userId);
                                daa.Fill(dtUserName);
                                if(dtUserName!=null && dtUserName.Rows.Count>0)
                                {
                                    username = dtUserName.Rows[0]["UserName"].ToString();
                                }

                            }
                        }
                    }
                }

                string sqlText = @"Select UserID, RoleID, SchoolID from [ACME_MAIN_TEST].[dbo].[User] 
                                Where Username = @Username and [Password] = @Password
	                              and RoleID = 2 AND LockID = 0";
                using (SqlDataAdapter da = new SqlDataAdapter(sqlText, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@Username", username);
                    da.SelectCommand.Parameters.AddWithValue("@Password", password);
                    da.Fill(dtUser);

                    if (dtUser != null && dtUser.Rows.Count > 0)
                    {
                        HttpContext.Current.Session[SESS_USERID] = dtUser.Rows[0]["UserID"];
                        HttpContext.Current.Session[SESS_SCHOOLID] = dtUser.Rows[0]["SchoolID"];
                        isSchoolHolidayBlocked = CheckIsSchoolHolidayBlocked(Convert.ToInt32(dtUser.Rows[0]["UserID"]), Convert.ToInt32(dtUser.Rows[0]["SchoolID"]));
                        bValid = true;
                    }
                }
                conn.Close();
            }
            return bValid;
        }

        private static bool CheckIsSchoolHolidayBlocked(int UserID, int SchoolID)
        {
            bool isSchoolHB = false;
            DateTime currentTime = GetCurrentTimebyUserTimeZone(UserID);
            DataSet dsSchoolHoliday = new DataSet();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();
                string sqlText = @"Select isBlockAttdHolidaysSignIn from SchoolDetail Where SchoolId = @SchoolID;
                               Select * from SchoolHoliday Where SchoolID = @SchoolID and IsActive = 1 and Datediff(dd, HolidayDate, @LocalTime) = 0 ";
                using (SqlDataAdapter da = new SqlDataAdapter(sqlText, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@SchoolID", SchoolID);
                    da.SelectCommand.Parameters.AddWithValue("@LocalTime", currentTime);
                    da.Fill(dsSchoolHoliday);
                    if (dsSchoolHoliday != null && dsSchoolHoliday.Tables.Count > 0)
                    {
                        if (!(dsSchoolHoliday.Tables[0].Rows[0][0]).Equals(DBNull.Value) && (bool)dsSchoolHoliday.Tables[0].Rows[0][0] == true)
                        {
                            if (dsSchoolHoliday.Tables.Count > 1 && dsSchoolHoliday.Tables[1].Rows.Count > 0)
                            {
                                isSchoolHB = true;
                            }
                        }
                    }
                }
                conn.Close();
            }
            return isSchoolHB;
        }

        /// <summary>
        /// Get in progress courses of student by the given userID
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns>in progress courses table</returns>
        public static DataTable GetStudentCourse(int UserID)
        {
            DataTable dtStdCourse = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();
                string sqlText = @"Select c.Code + ' ' + c.Name as CourseName, cc.ContractCourseID as ccid from [ACME_AOL_TEST].[dbo].ContractCourse cc
                                INNER JOIN [ACME_AOL_TEST].[dbo].Course c on c.CourseID = cc.CourseID 
                                Where cc.UserID = @UserID and cc.StudyStatusID = 1 ";
                using (SqlDataAdapter da = new SqlDataAdapter(sqlText, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@UserID", UserID);
                    da.Fill(dtStdCourse);
                }
                conn.Close();
            }
            return dtStdCourse;
        }

        ///
        public static bool GetSignInStatus(int UserID)
        {
            bool IsSignin = false;
            try
            {
                DateTime currentTime = GetCurrentTimebyUserTimeZone(UserID);
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
                {
                    conn.Open();
                    string sqlText = @"SELECT AttendanceID, TimeIn From Attendance
                                 WHERE UserID = @userid
                                 AND CONVERT(date, TimeIn) = CONVERT(date, @CurrentTime) AND TimeOut IS NULL 
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
                            HttpContext.Current.Session[SESS_TIMEIN] = dreader.GetDateTime(1);
                            IsSignin = true;
                        }
                        else
                            IsSignin = false;
                        dreader.Dispose();
                        dreader.Close();
                    }
                    conn.Close();
                }
            }
            catch (SqlException)
            {

            }

            return IsSignin;
        }

        /// <summary>
        /// Use to record a student sign in.
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public static bool RecordSigninAttendance(int UserID)
        {
            int attendanceid = -1;
            bool success = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();
                try
                {
                    SqlCommand cmd = new SqlCommand("at_InsertRecord_Check", conn);

                    DateTime timein = GetCurrentTimebyUserTimeZone(UserID);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@userid", UserID);
                    cmd.Parameters.Add("@timein", SqlDbType.DateTime);
                    cmd.Parameters["@timein"].Value = timein;
                    cmd.Parameters.Add("@timeout", SqlDbType.DateTime);
                    cmd.Parameters["@timeout"].Value = DBNull.Value;
                    cmd.Parameters.Add("@attendanceid", SqlDbType.Int);
                    cmd.Parameters["@attendanceid"].Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    attendanceid = (int)cmd.Parameters["@attendanceid"].Value;

                    string studentText =
                        "DECLARE @Password NVARCHAR(255); " +
                        "SELECT @Password = [Password] FROM ACME_MAIN_TEST.dbo.[User] WHERE UserID = @UserID ; " +
                        "INSERT INTO StudentSignInOut (UserID, [Password], SignInSignOut, CreateDateTime, AttendanceID, AdminUserID) " +
                        "VALUES (@UserID, @Password, 0, GetDate(), @AttendanceID, @AdminUserID) ";
                    cmd = new SqlCommand(studentText, conn);
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    cmd.Parameters.AddWithValue("@AttendanceID", attendanceid);
                    cmd.Parameters.AddWithValue("@AdminUserID", attAdminUserID);
                    cmd.ExecuteNonQuery();

                    success = true;
                    cmd.Dispose();

                }
                catch (SqlException)
                {
                    success = false;
                }
                finally
                {
                    conn.Close();
                }
            }
            return success;
        }



        /// <summary>
        /// Used to record a student sign out.  
        /// 
        public static bool RecordSignoutAttendance(int UserID, DateTime dtTimein, int ccid=0, DateTime? dtTimeout = null)
        {
            int attendanceid = -1;
            bool success = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();
                string sqltext;
                try
                {
                    sqltext = "SELECT AttendanceID From Attendance" +
                        " WHERE UserID = @userid" +
                        " AND TimeIn = @timein AND TimeOut IS NULL";
                    SqlCommand cmd = new SqlCommand(sqltext, conn);
                    cmd.Parameters.Add("@userid", SqlDbType.Int);
                    cmd.Parameters["@userid"].Value = UserID;
                    cmd.Parameters.Add("@timein", SqlDbType.DateTime);
                    cmd.Parameters["@timein"].Value = Convert.ToDateTime(dtTimein);
                    object obj = cmd.ExecuteScalar();

                    DateTime timeout = GetCurrentTimebyUserTimeZone(UserID, dtTimeout);
                    if (obj != null)    // update record in Attendance
                    {
                        attendanceid = (int)obj;
                        sqltext = @"UPDATE Attendance SET TimeOut = @timeout
                                WHERE AttendanceID = @attendanceid";
                        cmd = new SqlCommand(sqltext, conn);
                        cmd.Parameters.Add("@timeout", SqlDbType.DateTime);
                        cmd.Parameters[0].Value = timeout;
                        cmd.Parameters.Add("@attendanceid", SqlDbType.Int);
                        cmd.Parameters[1].Value = attendanceid;
                        cmd.ExecuteNonQuery();
                    }
                    else        // insert new record in Attendance
                    {
                        cmd = new SqlCommand("at_InsertRecord_Check", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@userid", SqlDbType.Int);
                        cmd.Parameters["@userid"].Value = UserID;
                        cmd.Parameters.Add("@timein", SqlDbType.DateTime);
                        cmd.Parameters["@timein"].Value = Convert.ToDateTime(dtTimein);     //?
                        cmd.Parameters.Add("@timeout", SqlDbType.DateTime);
                        cmd.Parameters["@timeout"].Value = timeout;
                        cmd.Parameters.Add("@attendanceid", SqlDbType.Int);
                        cmd.Parameters["@attendanceid"].Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        attendanceid = (int)cmd.Parameters["@attendanceid"].Value;
                    }

                    //Add a record to AttendanceDetail
                    //Changed the attendance minutes subtract to minutes only, do not consider the seconds as full minute.
                    int minutes = (timeout.Hour - dtTimein.Hour) * 60 + (timeout.Minute - dtTimein.Minute);
                    cmd = new SqlCommand("at_InsertDetail", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@attendanceid", SqlDbType.Int);
                    cmd.Parameters[0].Value = attendanceid;
                    cmd.Parameters.Add("@contractcourseid", SqlDbType.Int);
                    cmd.Parameters[1].Value = ccid;
                    cmd.Parameters.Add("@minutes", SqlDbType.Int);
                    cmd.Parameters[2].Value = minutes;
                    cmd.Parameters.Add("@status", SqlDbType.Int);
                    cmd.Parameters[3].Value = Status_InProgress;
                    cmd.ExecuteNonQuery();

                    //Add attendance log
                    string studentText =
                        "DECLARE @Password NVARCHAR(255); " +
                        "SELECT @Password = [Password] FROM ACME_MAIN_TEST.dbo.[User] WHERE UserID = @UserID ; " +
                        "INSERT INTO StudentSignInOut (UserID, [Password], SignInSignOut, CreateDateTime, AttendanceID, AdminUserID) " +
                        "VALUES (@UserID, @Password, 1, GetDate(), @AttendanceID, @AdminUserID) ";
                    cmd = new SqlCommand(studentText, conn);
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    cmd.Parameters.AddWithValue("@AttendanceID", attendanceid);
                    cmd.Parameters.AddWithValue("@AdminUserID", attAdminUserID);
                    cmd.ExecuteNonQuery();

                    success = true;
                    cmd.Dispose();
                }
                catch (SqlException)
                {
                    success = false;
                }
                finally
                {
                    conn.Close();
                }
            }
            return success;
        }

        public static DateTime GetCurrentTimebyUserTimeZone(int UserID, DateTime? signoutDate = null)
        {
            //The default time is EST, which get from server time.
            DateTime curTime = DateTime.Now;
            //if (signoutDate != null)
            //{
            //    curTime = Convert.ToDateTime(signoutDate);
            //}
            string province = GetUserProvinceByUserID(UserID);
            switch (province.ToUpper())
            {
                case "BC":
                    curTime = curTime.AddHours(-3);     //PST
                    break;
                case "AB":
                case "SK":
                    curTime = curTime.AddHours(-2);     //MST
                    break;
                case "MB":
                    curTime = curTime.AddHours(-1);     //CST
                    break;
                case "ON":
                    break;                              //EST
                case "PE":
                case "NB":
                case "NF":
                case "NS":
                    curTime = curTime.AddHours(1);      //AST
                    break;
                default:
                    break;
            }
            HttpContext.Current.Session[SESS_TIMEIN] = curTime;
            return curTime;
        }

        private static string GetUserProvinceByUserID(int UserID)
        {
            string proCode = "ON";
            string sqlText = @"SELECT sd.Province FROM ACME_MAIN_TEST.dbo.[User] u 
                           INNER JOIN SchoolDetail sd on sd.SchoolID = u.SchoolID 
                           WHERE u.UserID = @UserID";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlText, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    object obj = cmd.ExecuteScalar();
                    if (obj != null)
                        proCode = obj.ToString();
                }
                conn.Close();
            }
            return proCode;
        }

        /// <summary>
        /// Get students personal information by school or group
        /// </summary>
        /// <param name="SchoolID"></param>
        /// <returns></returns>
        public static DataTable GetPersonalInfobySchoolID(int SchoolID)
        {
            DataTable dtStdInfo = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();
                string sqlText = "sf_GetPersonalInfoBySchoolID";
                using (SqlDataAdapter da = new SqlDataAdapter(sqlText, conn))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@SchoolID", SchoolID);
                    da.Fill(dtStdInfo);
                }
                conn.Close();
            }
            return dtStdInfo;
        }

        /// <summary>
        /// Get students program enrollment information by school or group
        /// </summary>
        /// <param name="SchoolID"></param>
        /// <returns></returns>
        public static DataTable GetProgramEnrollInfobySchoolID(int SchoolID)
        {
            DataTable dtProgInfo = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();
                string sqlText = "sf_GetProgramEnrollmentInfo";
                using (SqlDataAdapter da = new SqlDataAdapter(sqlText, conn))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@SchoolID", SchoolID);
                    da.Fill(dtProgInfo);
                }
                conn.Close();
            }
            return dtProgInfo;
        }

        /// <summary>
        /// Get students course enrollment information by school or group
        /// </summary>
        /// <param name="SchoolID"></param>
        /// <returns></returns>
        public static DataTable GetCourseEnrollInfobySchoolID(int SchoolID)
        {
            DataTable dtCourseEnrollInfo = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();
                string sqlText = "sf_GetCourseEnrollmentInfo";
                using (SqlDataAdapter da = new SqlDataAdapter(sqlText, conn))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@SchoolID", SchoolID);
                    da.Fill(dtCourseEnrollInfo);
                }
                conn.Close();
            }
            return dtCourseEnrollInfo;
        }


    }

}
