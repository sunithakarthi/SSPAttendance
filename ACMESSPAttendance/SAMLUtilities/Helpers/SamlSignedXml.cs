using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Security.Cryptography.Xml;


namespace ACMESSPAttendance.SAMLUtilities.Helpers
{
    public class SamlSignedXml : SignedXml
    {
        private string _referenceAttributeId = string.Empty;

        public SamlSignedXml(XmlDocument document, string referenceAttributeId) : base(document)
        {
            _referenceAttributeId = referenceAttributeId;
        }

        public SamlSignedXml(XmlElement element, string referenceAttributeId) : base(element)
        {
            _referenceAttributeId = referenceAttributeId;
        }

        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            return (XmlElement)document.SelectSingleNode(string.Format("//*[@{0}='{1}']", _referenceAttributeId, idValue));
        }
    }
}