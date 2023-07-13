using ACMESSPAttendance.SAMLUtilities.Schemas;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using System.Xml;
using ACMESSPAttendance.Common;

namespace ACMESSPAttendance.SAMLUtilities.Helpers
{
    public class SamlResponseAlgorithms : ISamlResponseAlgorithms
    {
        private const string XsiSchema = @"http://www.w3.org/2001/XMLSchema-instance";
        private const string XsdSchema = @"http://www.w3.org/2001/XMLSchema";

        public SamlResponseAlgorithms()
        {
        }

        public ResponseType Create(SamlIntegrationSettings samlResponseSpecification, AssertionType assertion)
        {
            return new ResponseType
            {
                ID = samlResponseSpecification.PrependToId + Guid.NewGuid().ToString(),
                Issuer = new NameIDType
                {
                    Value = samlResponseSpecification.Issuer
                },
                IssueInstant = DateTime.UtcNow,
                Destination = samlResponseSpecification.Recipient,
                Version = "2.0",
                Status =
                    new StatusType
                    {
                        StatusCode = new StatusCodeType
                        {
                            Value = "urn:oasis:names:tc:SAML:2.0:status:Success"
                        }
                    },
                Items = new object[]
                {
                    assertion
                }
            };
        }

        public bool Sign(string thumbprint, string responseId, ref XmlDocument xmlSamlResponse)
        {
            X509Certificate2 x509 = null;
            bool result = false;

            try
            {
                x509 = X509CertificateHelper.GetCertificateByThumbprint(thumbprint, StoreName.My, StoreLocation.LocalMachine);

                if (x509 != null)
                {

                    SamlSignedXml samlSignedXml = SigningHelper.SignXml(xmlSamlResponse, x509, "ID", responseId);

                    // Get the XML representation of the signature and save it to an XmlElement object. 
                    XmlElement xmlDigitalSignature = samlSignedXml.GetXml();

                    // Put the sign as the first child of main Request tag.
                    if (xmlSamlResponse.DocumentElement != null)
                        xmlSamlResponse.DocumentElement.InsertAfter(xmlDigitalSignature,
                            xmlSamlResponse.DocumentElement.ChildNodes[0]);

                    result = true;
                }
                else
                {
                    LogWriter.LogWrite("SamlResponseAlgorithms - Sign Method -  X509 certificate not found!");
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite("SamlResponseAlgorithms - Sign Method - Exception: " + ex.Message + Environment.NewLine +
                                 (ex.InnerException != null ? "InnerException: " + ex.InnerException.Message : string.Empty)
                                 + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                x509?.Reset();
            }

            return result;
        }

        public XmlDocument SerializeToXml(ResponseType samlResponse)
        {
            string serializedXml;

            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = false,
                    Encoding = System.Text.Encoding.ASCII
                };

                using (var responseWriter = XmlWriter.Create(stringWriter, settings))
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("xsi", XsiSchema);
                    ns.Add("xsd", XsdSchema);
                    ns.Add("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
                    ns.Add("saml", "urn:oasis:names:tc:SAML:2.0:assertion");

                    XmlSerializer samlResponseSerializer = new XmlSerializer(samlResponse.GetType());
                    samlResponseSerializer.Serialize(responseWriter, samlResponse, ns);

                    serializedXml = stringWriter.ToString();
                }
            }

            var document = new XmlDocument();
            document.LoadXml(serializedXml);

            return document;
        }

    }
}