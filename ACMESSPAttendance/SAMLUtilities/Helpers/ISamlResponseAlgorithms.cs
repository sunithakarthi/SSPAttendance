using System.Xml;

namespace ACMESSPAttendance.SAMLUtilities.Schemas
{
    public interface ISamlResponseAlgorithms
    {
        ResponseType Create(SamlIntegrationSettings samlResponseSpecification, AssertionType assertion);
        bool Sign(string thumbprint, string responseId, ref XmlDocument xmlSamlResponse);
        XmlDocument SerializeToXml(ResponseType samlResponse);
    }
}

