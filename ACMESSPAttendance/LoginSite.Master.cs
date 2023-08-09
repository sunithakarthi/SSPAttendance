using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StudentSuccessPortal
{
    public partial class LoginSiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string fontName = ACMESSPAttendance.Utility.GetFontFamilyName();
            myattendanceloginbody.Attributes.CssStyle.Add("font-family", fontName);
        }
    }
}