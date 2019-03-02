using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Serialization;


    [XmlTypeAttribute(Namespace = NamespaceConstants.v1)]
    [Serializable]
    [SoapInclude(typeof(WindowsCredentials)), SoapInclude(typeof(WindowsIdentity))]
    public class Credentials
    {
        public string UserName;
        public string Domain;

        internal virtual string ToXML(bool withDomain)
        {
            return "";
        }

        internal virtual string ToXML()
        {
            return "";
        }


    }

