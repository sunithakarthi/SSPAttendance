namespace ACMESSPAttendance.SAMLUtilities.Data
{
    public interface IIntegrationConfiguration
    {
        string ServiceProviderUri { get; }
        string ServiceProviderSAMLUri { get; }
        string LogoutUri { get; }
        string ReturnUri { get; }
        string StartUri { get; }
        string IssuerUri { get; }
        string SigningCertificateThumbprint { get; }
        string AssertionEncryptionCertificateThumbprint { get; }
    }

    public class IntegrationConfiguration : IIntegrationConfiguration
    {
        public string ServiceProviderUri { get { return "http://mynew.aolcc.ca/login/saml"; } }
        public string ServiceProviderSAMLUri { get { return "http://mynew.aolcc.ca/saml2"; } }
        public string LogoutUri { get { return System.Configuration.ConfigurationManager.AppSettings["HostUrl"].ToString(); } }
        public string ReturnUri { get { return System.Configuration.ConfigurationManager.AppSettings["HostUrl"].ToString(); } }
        public string StartUri { get { return System.Configuration.ConfigurationManager.AppSettings["HostUrl"].ToString(); } }
        public string IssuerUri { get { return System.Configuration.ConfigurationManager.AppSettings["HostUrl"].ToString(); } }
        public string SigningCertificateThumbprint { get { return "20E75F469726BC47C39FAD2A80C88CD7F4D21593"; } }
        public string AssertionEncryptionCertificateThumbprint { get { return null; } }
    }
}