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
        public string LogoutUri { get { return "https://ssp.academyoflearning2022.net"; } }
        public string ReturnUri { get { return "https://ssp.academyoflearning2022.net"; } }
        public string StartUri { get { return "https://ssp.academyoflearning2022.net"; } }
        public string IssuerUri { get { return "https://ssp.academyoflearning2022.net"; } }
        public string SigningCertificateThumbprint { get { return "57361ac014aff31b2ff9bc1df39a1e0afac1b99b"; } }
        public string AssertionEncryptionCertificateThumbprint { get { return null; } }
    }
}