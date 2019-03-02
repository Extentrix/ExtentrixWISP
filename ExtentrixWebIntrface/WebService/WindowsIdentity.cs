using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

    /// <summary>
    /// For Single Sign On or Smart Card Authentication
    ///When configured for Single Sign On or Smart Card authentication, web service caller detects
    ///the user’s existing Windows domain authentication and enumerates applications for that
    ///user automatically, without requiring the user to provide their user name and password.
    ///IIS, having authenticated the user using Integrated Windows Authentication, is
    ///able to identify the user’s domain and user name, but not their password.
    ///Therefore a normal enumeration of application icons is not possible as it is with
    ///explicit enumeration. Instead, the Web server determines the list of domain
    ///groups to which the user belongs
    /// Web Services constructs an XML request containing all of the
    ///user’s individual and group SIDs and forwards this list of SIDs to the MetaFrame
    ///XML Broker. 
    /// </summary>

[XmlTypeAttribute(Namespace = NamespaceConstants.v35)]
[Serializable]
    public class WindowsIdentity : Credentials
    {
        private string sam;//NETBIOS style identity	SAM
        public string UPN;//User Principal Name	UPN
        public string[] SIDs;//NT Security Identifier	SID

        public string SAM
        {
            get
            {
                return sam;
            }
            set
            {
                this.sam = value;
                this.UserName = value;
                this.Domain = GetDomain(ref UserName, false);
            }
        }

        private string GetDomain(ref string userName, bool includeUPNFormat)
        {
            string[] domainAndUser = userName.Split(new char[] { '\\' });

            if (domainAndUser.Length > 2)
                throw new ArgumentException("Username format incorrect.");

            string domain = null;

            if (domainAndUser.Length == 2)
            {
                domain = domainAndUser[0];
                userName = domainAndUser[1];
            }

            else if (includeUPNFormat)
            {

                domainAndUser = userName.Split(new char[] { '@' });

                if (domainAndUser.Length > 2)

                    throw new ArgumentException("Username format incorrect");

                if (domainAndUser.Length == 2)
                {

                    domain = domainAndUser[1];

                    userName = domainAndUser[0];

                }

            }

            if (domain == null)

                domain = ".";

            return domain;

        }


        internal override string ToXML(bool withDomain)
        {

            string cred = "<Credentials>"
                + (SAM == null ? "" : "<ID type=\"SAM\">" + SAM + "</ID>")
                + (UPN == null ? "" : "<ID type=\"UPN\">" + UPN + "</ID>");

            if (SIDs != null)
                for (int i = 0; i < SIDs.Length; i++)
                {
                    cred += "<ID type=\"SID\">" + SIDs[i] + "</ID>";
                }

            cred += "</Credentials>";

            return cred;

        }

    }



