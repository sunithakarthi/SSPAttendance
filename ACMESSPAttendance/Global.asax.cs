using StudentSuccessPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace ACMESSPAttendance
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AppContext.SetSwitch("Switch.System.Security.Cryptography.Xml.UseInsecureHashAlgorithms", true);
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Pkcs.UseInsecureHashAlgorithms", true);

        }

        void Session_Start(object sender, EventArgs e)
        {
            Session.Timeout = 1440; // We maintain session as 24 hours.
        }
    }
}