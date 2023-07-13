using ACMESSPAttendance.Common;
using ACMESSPAttendance.SAMLUtilities.Data;
using ACMESSPAttendance.SAMLUtilities.Helpers;
using ACMESSPAttendance.SAMLUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ACMESSPAttendance
{
    public partial class Canvas : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool isUserAuthenticated = UserHaveCanvasAuthProvider();
                if (isUserAuthenticated)
                {
                    RedirectToCanvasSiteWithPostData();
                }
                else
                {
                    Response.Redirect("https://mynew.aolcc.ca/login/canvas", true);
                }
            }
        }

        bool UserHaveCanvasAuthProvider()
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

        void RedirectToCanvasSiteWithPostData()
        {
            string samlResponse = GetSAMLResponse();

            HttpResponse response = HttpContext.Current.Response;
            response.Clear();

            StringBuilder s = new StringBuilder();
            s.Append("<html>");
            s.AppendFormat("<body onload='document.forms[\"form\"].submit()'>");
            s.AppendFormat("<form name='form' action='{0}' method='post'>", "https://mynew.aolcc.ca/login/saml");
            s.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", "SAMLResponse", samlResponse);
            s.Append("</form></body></html>");
            response.Write(s.ToString());
            response.End();
        }

        string GetSAMLResponse()
        {
            string samlResponse = "";
            try
            {
                LogWriter.LogWrite("Step 1: User Aolcc Email : " + UserManagement.GetUser()?.AolccEmail);

                LogWriter.LogWrite("Step 1: Get Canvas redirect url. GetRedirectUrl Method Called.");

                // Prepare authentication request data according to SAML 2.0
                var steps = new SamlIntegrationSteps(
                    new SamlAssertionAlgorithms(),
                    new SamlResponseAlgorithms(),
                    new UserDataRepository(),
                    new IntegrationConfiguration());

                samlResponse = steps.BuildEncodedSamlResponse();
            }
            catch (System.Threading.ThreadAbortException ex)
            {
                Context.ApplicationInstance.CompleteRequest();
                Response.Redirect("/");
            }
            catch (Exception ex)
            {
                Context.ApplicationInstance.CompleteRequest();
                Response.Redirect("/");
            }
            return string.IsNullOrEmpty(samlResponse) ? "" : samlResponse;
        }
    }
}