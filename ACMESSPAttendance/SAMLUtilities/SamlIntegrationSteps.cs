using ACMESSPAttendance.SAMLUtilities.Schemas;
using System;
using System.Collections.Generic;
using System.Xml;
using ACMESSPAttendance.SAMLUtilities.Data;
using ACMESSPAttendance.SAMLUtilities.Helpers;
using ACMESSPAttendance.Common;

namespace ACMESSPAttendance.SAMLUtilities
{
    public class SamlIntegrationSteps : ISamlIntegrationSteps
    {
        private const string UriFormat = "{0}://{1}";
        private const string IdPrefix = "_";
        private const string XsiSchema = @"http://www.w3.org/2001/XMLSchema-instance";
        private const string XsdSchema = @"http://www.w3.org/2001/XMLSchema";
        private const string LogoutUriParameter = "LogoutUri";
        private const string ReturnUriParameter = "ReturnUri";
        private const string StartUriParameter = "StartUri";

        // An example of custom parameters used for integration with third-party.
        private const string UserIdParameter = "userId";
        private const string UserNameParameter = "username";
        private const string EmailParameter = "email";
        private const string Is_Portal_UserParameter = "is_portal_user";
        private const string EmailAddressParameter = "EmailAddress";

        private readonly ISamlAssertionAlgorithms _assertionAlgs;
        private readonly ISamlResponseAlgorithms _responseAlgs;
        private readonly IUserDataRepository _userDataRepository;
        private readonly IIntegrationConfiguration _configuration;

        public SamlIntegrationSteps(ISamlAssertionAlgorithms assertionAlgs,
            ISamlResponseAlgorithms responseAlgs,
            IUserDataRepository userDataRepository,
            IIntegrationConfiguration configuration)
        {
            _assertionAlgs = assertionAlgs;
            _responseAlgs = responseAlgs;
            _userDataRepository = userDataRepository;
            _configuration = configuration;
        }

        public SamlIntegrationSteps(
            SamlAssertionAlgorithms assertionAlgs,
            SamlResponseAlgorithms responseAlgs,
            UserDataRepository userDataRepository,
            IntegrationConfiguration configuration)
        {
            _assertionAlgs = assertionAlgs;
            _responseAlgs = responseAlgs;
            _userDataRepository = userDataRepository;
            _configuration = configuration;
        }

        /// <summary>
        /// Builds authentication request data that is singed, encrypted and ready for use
        /// by rules of the SAML 2.0 integraton protocol.
        /// </summary>
        /// <returns>String of data ready to be written into POST request.</returns>
        public string BuildEncodedSamlResponse()
        {
            try
            {
                LogWriter.LogWrite("Step 2: BuildEncodedSamlResponse Method Called.");

                // NOTE! This is just an example.
                // The list of actual required attributes depends on the third-party vendor requirements.
                Dictionary<string, string> attributes = new Dictionary<string, string>
            {
                { UserIdParameter, _userDataRepository.GetUserID() },
                { UserNameParameter, _userDataRepository.GetUserEmail() },
                { EmailParameter, _userDataRepository.GetUserEmail() },
                { Is_Portal_UserParameter, "true" },
                { EmailAddressParameter, _userDataRepository.GetUserEmail() }
            };

                Uri audienceUri = new Uri(_configuration.ServiceProviderUri);

                var settings = new SamlIntegrationSettings(
                    _configuration.ServiceProviderUri,
                    _configuration.IssuerUri,
                    _configuration.ServiceProviderSAMLUri,
                    _configuration.SigningCertificateThumbprint,
                    prependToId: IdPrefix,
                    assertionEncryptionCertificateThumbprint: _configuration.AssertionEncryptionCertificateThumbprint);

                settings.Attributes = attributes;

                return BuildAndSignSamlResponse(settings);

            }
            catch(Exception ex)
            {
                string exception = "Issue occured in BuildEncodedSamlResponse Method, Exception: " + ex.Message + Environment.NewLine +
                 (ex.InnerException != null ? "InnerException: " + ex.InnerException.Message : string.Empty)
                 + Environment.NewLine + ex.StackTrace;

                LogWriter.LogWrite(exception);

                return "";
            }
        }

        private string BuildAndSignSamlResponse(SamlIntegrationSettings settings)
        {
            try
            {
                LogWriter.LogWrite("Step 3: BuildAndSignSamlResponse Method Called ");

                AssertionType assertion = _assertionAlgs.Create(settings, _userDataRepository);

                var samlResponse = _responseAlgs.Create(settings, assertion);

                var xmlSamlResponse = _responseAlgs.SerializeToXml(samlResponse);

                // Serialize assertion to XML
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlSamlResponse.NameTable);
                namespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
                namespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
                namespaceManager.AddNamespace("xsi", XsiSchema);
                namespaceManager.AddNamespace("xsd", XsdSchema);

                XmlElement xmlAssertion = (XmlElement)xmlSamlResponse.SelectSingleNode("/samlp:Response/saml:Assertion", namespaceManager);

                // Sign assertion
                if (!_assertionAlgs.Sign(settings.CertificateThumbprint, assertion.ID, ref xmlAssertion))
                {
                    LogWriter.LogWrite("Exception - BuildAndSignSamlResponse Method - Unable to sign SAML assertion!");

                    return null;
                }

                // Encrypt assertion
                if (!string.IsNullOrWhiteSpace(settings.AssertionEncryptionCertificateThumbprint) &&
                    !_assertionAlgs.Encrypt(settings.AssertionEncryptionCertificateThumbprint, ref xmlSamlResponse))
                {
                    LogWriter.LogWrite("Exception - BuildAndSignSamlResponse Method - Unable to encrypt SAML assertion!");

                    return null;
                }

                // Sign Response
                if (!_responseAlgs.Sign(settings.CertificateThumbprint, samlResponse.ID, ref xmlSamlResponse))
                {
                    LogWriter.LogWrite("Exception - BuildAndSignSamlResponse Method - Unable to sign SAML response!");

                    return null;
                }

                string result = xmlSamlResponse.OuterXml;

                LogWriter.LogWrite("BuildAndSignSamlResponse Method Result SAML: " + result);

                return Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(result));
            }
            catch(Exception ex)
            {
                string exception = "Issue occured in BuildAndSignSamlResponse Method, Exception: " + ex.Message + Environment.NewLine +
                 (ex.InnerException != null ? "InnerException: " + ex.InnerException.Message : string.Empty)
                 + Environment.NewLine + ex.StackTrace;

                LogWriter.LogWrite(exception);

                return "";
            }
        }
    }
}
