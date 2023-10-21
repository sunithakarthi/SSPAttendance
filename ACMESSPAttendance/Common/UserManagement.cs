using ACMESSPAttendance.Common;
using ACMESSPAttendance.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ACMESSPAttendance
{
    public class UserManagement
    {
        private StringBuilder s = new StringBuilder();
        public const string BR = "\r\n"; //client depend on this value.
        
        public static UserViewModel GetUser()
        {
            if (HttpContext.Current.Session[Utility.SessionUserNameKey] == null)
            {
                return null;
                //HttpContext.Current.Response.Redirect("/");
            }

            return (UserViewModel)HttpContext.Current.Session[Utility.SessionUserNameKey];
        }
        public static string GetSessionError(int err)
        {
            string ErrorDescription;
            switch (err)
            {
                case 0:
                    ErrorDescription = "Supplied SessionID, Username or Password are invalid!";
                    break;
                case -1:
                    ErrorDescription = "Your account is not active!";
                    break;
                case -2:
                    ErrorDescription = "Your account has been locked! Reason: too many failed login attempts.";
                    break;
                case -3:
                    ErrorDescription = "Your account has been locked! Reason: too many concurrent connections.";
                    break;
                case -4:
                    ErrorDescription = "Your account has been locked by an Administrator.";
                    break;
                case -5:
                    ErrorDescription = "Someone else has logged on using your account!";
                    break;
                default:
                    ErrorDescription = "Unknown Session Error.";
                    break;
            }
            return ErrorDescription;
        }

        public class UserRank
        {
            public const int Lead = 1;
            public const int Student = 2;
            public const int Facilitator = 3;
            public const int Admin = 4;
            public const int SchoolMain = 5;
            public const int HOStaff = 6;
        }
        public static UserViewModel Check2ProduceUserData(UserViewModel user)
        {
            if (user == null)
            {
                //s.Append("0");
                //s.Append(BR);
                //s.Append("Login Error, Check Username or Password.");
                return user;
            }

         var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_main_test_CS"].ConnectionString);
            conn.Open();
            try
            {
                //Get some info about the user:
                SqlCommand cmd = new SqlCommand("ACME_AOL_TEST.dbo.aol_GetUserInfo", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@user_id", SqlDbType.NVarChar);
                cmd.Parameters[0].Value = user.UserId;
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0)) { user.Schoolid = reader.GetInt32(0); }
                    if (!reader.IsDBNull(1)) { user.SchoolName = reader.GetString(1); }
                    if (!reader.IsDBNull(3)) { user.RoleId = reader.GetInt32(3); }
                    if (!reader.IsDBNull(4)) { user.FirstName = reader.GetString(4); }
                    if (!reader.IsDBNull(5)) { user.LastName = reader.GetString(5); }
                    if (!reader.IsDBNull(6)) { user.UserRank = reader.GetInt32(6); }
                    if (!reader.IsDBNull(7)) { user.AolccEmail = reader.GetString(7); }
                    if(!reader.IsDBNull(8)) { user.Email = reader.GetString(8);}
                    if (!reader.IsDBNull(9)) { user.CVSUserID = reader.GetInt32(9); }
                    if (!reader.IsDBNull(10)) { user.CVSAuthProviderID = reader.GetInt32(10); }
                    if (!reader.IsDBNull(11)) { user.LockID = reader.GetInt32(11); }
                    if (!reader.IsDBNull(12)) { user.AccountLockStatusID = reader.GetInt32(12); }

                }
                reader.Close();

                //Every student should have record in attendance recorder
                //check if User is a student if  
                

            }
            finally
            {
                conn.Close();
            }
            return user;
        }
        public static UserViewModel Login(string email, string password, string ipAddress)
        {
            //SqlConnection conn = new SqlConnection(ACME.DataAccess.DatabaseConnection.getAcmeMainTestConnectionString());
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_main_test_CS"].ConnectionString);
            conn.Open();
            try
            {
                #region Authenticate user and create user

                // Perform the Actual Authenticate with the stored procedure
                SqlCommand cmd = new SqlCommand("main_StudentAuthenticate", conn);
                //SqlCommand cmd = new SqlCommand("main_Authenticate", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //cmd.Parameters.Add("@username", SqlDbType.NVarChar);
                cmd.Parameters.Add("@email", SqlDbType.NVarChar);
                cmd.Parameters.Add("@password", SqlDbType.NVarChar);
                cmd.Parameters.Add("@ip_address", SqlDbType.NVarChar);
                cmd.Parameters.Add("@app_name", SqlDbType.NVarChar);
                cmd.Parameters.Add("@user_id", SqlDbType.Int);
                cmd.Parameters.Add("@status", SqlDbType.Int);
                cmd.Parameters[0].Value = email;
                cmd.Parameters[1].Value = password;
                cmd.Parameters[2].Value = ipAddress;
                cmd.Parameters[3].Value = "SSPAttendance";
                cmd.Parameters[4].Direction = ParameterDirection.Output;                                                                                                                   
                cmd.Parameters[5].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                if (Convert.ToInt32(cmd.Parameters[5].Value) < 1)
                {
                    throw new Exception(GetSessionError(Convert.ToInt32(cmd.Parameters[4].Value)));
                }

                // Create Current User Information
                UserViewModel user = new UserViewModel();
                user.UserId = Convert.ToInt32(cmd.Parameters[4].Value);
                user.SessionId = Convert.ToInt32(cmd.Parameters[5].Value);
                user.IPAddress = ipAddress;
                user.LoginDateTime = DateTime.Now;



                // generate the Random Session String for current user.
                byte[] random = new Byte[39];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(random);
                string base64SessionString = Convert.ToBase64String(random);
                string letterOrDigitString = Utility.LetterOrDigitString(base64SessionString);
                user.SessionString = letterOrDigitString.Substring(0, letterOrDigitString.Length > 200 ? 200 : letterOrDigitString.Length);

                // Save the Session Data to Database
                string sql = "Insert into UserSession(LoginIPAddress, SessionString, UserID, SessionID) " +
                    " Values(@LoginAddress, @SessionString, @UserID, @SessionID);" +
                    " SET @UserSessionID = SCOPE_IDENTITY() ";
                cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@LoginAddress", System.Data.SqlDbType.NChar, 15).Value = user.IPAddress;
                cmd.Parameters.Add("@SessionString", System.Data.SqlDbType.NChar, 255).Value = user.SessionString;
                cmd.Parameters.Add("@UserID", System.Data.SqlDbType.Int).Value = user.UserId;
                cmd.Parameters.Add("@SessionID", System.Data.SqlDbType.Int).Value = user.SessionId;
                cmd.Parameters.Add("@UserSessionID", SqlDbType.Int);
                cmd.Parameters["@UserSessionID"].Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                user.SessionId = (int)cmd.Parameters["@UserSessionID"].Value;

                /* Add SSP UserSession */
                conn.Close();
                conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString);
                conn.Open();

                cmd = new SqlCommand("SSP_SSPUserSession", conn);
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

                return user;

                #endregion
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
                return null;

            }
            finally
            {
                conn.Close();
            }
        }

        public static bool ValidateUser(string username, string password, ref bool isSchoolHolidayBlocked)
        {
            bool bValid = false;
            DataTable dtUser = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_aol_test_CS"].ConnectionString))
            {
                conn.Open();
                string sqlText = @"Select UserID, RoleID, SchoolID from [ACME_MAIN_TEST].[dbo].[User] 
                                Where Username = @Username and [Password] = @Password
	                              and RoleID = 2 AND LockID = 0";
                using (SqlDataAdapter da = new SqlDataAdapter(sqlText, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@Username", username);
                    da.SelectCommand.Parameters.AddWithValue("@Password", password);
                    da.Fill(dtUser);

                    //if (dtUser != null && dtUser.Rows.Count > 0)
                    //{
                    //    HttpContext.Current.Session[SESS_USERID] = dtUser.Rows[0]["UserID"];
                    //    HttpContext.Current.Session[SESS_SCHOOLID] = dtUser.Rows[0]["SchoolID"];
                    //    isSchoolHolidayBlocked = CheckIsSchoolHolidayBlocked(Convert.ToInt32(dtUser.Rows[0]["UserID"]), Convert.ToInt32(dtUser.Rows[0]["SchoolID"]));
                       bValid = true;
                    //}
                }
                conn.Close();
            }
            return bValid; 
        }

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

        public static bool AddAttachment(System.Web.UI.HtmlControls.HtmlInputFile file1, int RefNumber, int UserID)
        {
            int FileLen = file1.PostedFile.ContentLength;
            if (FileLen > 1000000)
                return false;

            string ContentType = file1.PostedFile.ContentType;
            byte[] buffer = new byte[FileLen];
            Stream stream = file1.PostedFile.InputStream;
            stream.Read(buffer, 0, FileLen);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["acme_helpdesk_CS"].ConnectionString))
            {
                conn.Open();
                string sqltext =
                    "DECLARE @AttachmentID int " +
                    "INSERT INTO HDAttachment (FileName, AddDateTime, AddUserID, AttachmentData) " +
                    "VALUES (@FileName, @AddDateTime, @AddUserID, @AttachmentData) " +
                    "SELECT @AttachmentID = SCOPE_IDENTITY() " +
                    "INSERT INTO HDRequestAttachment (RefNumber, AttachmentID) " +
                    "VALUES (@RefNumber, @AttachmentID) ";
                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.Parameters.Add("@FileName", SqlDbType.VarChar).Value = Path.GetFileName(file1.PostedFile.FileName);
                    cmd.Parameters.Add("@AddDateTime", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@AddUserID", SqlDbType.Int).Value = UserID;
                    cmd.Parameters.Add("@AttachmentData", SqlDbType.Image).Value = buffer;
                    cmd.Parameters.Add("@RefNumber", SqlDbType.Int).Value = RefNumber;
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }

            return true;
        }

        public static bool UserHaveCanvasAuthProvider()
        {
            var user = UserManagement.GetUser();

            if (user != null && user.CVSAuthProviderID == (int)CanvasAuthProvider.StudentSuccessPortal)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsUserLocked()
        {
            var user = UserManagement.GetUser();

            if (user != null && user.AccountLockStatusID == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }


}