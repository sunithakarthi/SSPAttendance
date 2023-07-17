using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACMESSPAttendance.Model
{
    public class AttendanceViewModel
    {
        public int UserID { get; set; }
        public DateTime Date { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string TimeStudied { get; set; }
    }
}