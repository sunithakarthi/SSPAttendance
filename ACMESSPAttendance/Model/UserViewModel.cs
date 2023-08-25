using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACMESSPAttendance.Model
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public int SessionId { get; set; }
        public string IPAddress { get; set; }
        public DateTime LoginDateTime { get; set; }
        public string SessionString { get; set; }
        public int Schoolid { get; set; }
        public string SchoolName { get; set; }
        public int UserRank { get; set; }
        public int RoleId { get; set; }
        public string AolccEmail { get; set; }

        public string Location { get; set; }
        public int CVSUserID { get; set; }
        public int CVSAuthProviderID { get; set; }
        public int LockID { get; set; }
    }
}