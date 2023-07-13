using ACMESSPAttendance.SAMLUtilities.Schemas;
using System.Xml;

namespace ACMESSPAttendance.SAMLUtilities.Helpers
{
    public interface ISamlAssertionAlgorithms
    {
        AssertionType Create(SamlIntegrationSettings settings, Data.IUserDataRepository userData);
        bool Sign(string thumbprint, string responseId, ref XmlElement xmlAssertion);
        bool Encrypt(string thumbprint, ref XmlDocument xmlDocument);
    }
}