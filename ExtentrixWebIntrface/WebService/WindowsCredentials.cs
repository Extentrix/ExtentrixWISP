using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Serialization;
using System;


    /// <summary>
    /// Credentials Class stores user's information
    /// Properties:
    /// username
    /// password
    /// domain
    /// passEncryptionMethod:
    /// - cleartext  0
    /// - rsa56      1
    /// - rsa128     2
    /// - ctx1       3 
    /// domainType:
    /// - NT    0
    /// - UNIX  1 
    /// - NDS   2 
    /// </summary>
    /// 

    [XmlTypeAttribute(Namespace = NamespaceConstants.v1)]
    [Serializable]

    public class WindowsCredentials : Credentials
    {
        private string userName;
        private string password;
        private string domain;
        private int passEncryptionMethod;
        private int domainType;
        internal enum EncryptionMethods
        {
            CLEAR,
            RSA56,
            RSA128,
            CTX1
        }
        internal enum DomainTypes
        {

            NT,
            UNIX,
            NDS
        }
        public WindowsCredentials()
        {
            this.passEncryptionMethod = 0;
            this.domainType = 0;
            this.UserName = this.password = this.Domain = "";
        }
        //required for java support
        public string WinUserName
        {
            get
            {
                return this.userName;
            }
            set
            {
                this.userName = value;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }

        //required for java support
        public string WinDomain
        {
            get
            {
                return this.domain;
            }
            set
            {
                this.domain = value;
            }
        }
        
        public int DomainType
        {
            get
            {
                return this.domainType;
            }
            set
            {
                this.domainType = value;
            }
        }
        public int PasswordEncryptionMethod
        {
            get
            {
                return this.passEncryptionMethod;
            }
            set
            {
                this.passEncryptionMethod = value;
            }
        }
        /// <summary>
        /// Validates the encryption method and checks if the entered value 
        /// is a valid one
        /// </summary>
        /// <returns>true if the encryptionmethod valid
        /// and false if not
        /// </returns>
        internal bool validateEncryptionMethod()
        {
            return !(this.passEncryptionMethod > (int)EncryptionMethods.CTX1 || this.passEncryptionMethod < (int)EncryptionMethods.CLEAR);

        }
        /// <summary>
        /// Validates the domain type and checks if the entered value 
        /// is a valid one
        /// </summary>
        /// <returns>true if the domain type valid
        /// and false if not
        /// </returns>
        internal bool validateDomainType()
        {
            return !(this.domainType > (int)DomainTypes.NDS || this.domainType < (int)DomainTypes.NT);
        }

        internal override string ToXML(bool withDomain)
        {
            if (this.UserName == null || this.UserName.Equals(""))
            {
                return Assembler.assembleAnonymousUser();
            }

            return "<Credentials>"
                   + "<UserName>" + HttpUtility.HtmlAttributeEncode(this.UserName) + "</UserName>"
                   + "<Password encoding='" + WSConstants.encryption[this.PasswordEncryptionMethod] + "'>" + HttpUtility.HtmlAttributeEncode(this.Password) + "</Password>"
                   + (withDomain ? " <Domain type='" + WSConstants.domains[this.DomainType] + "'>" + HttpUtility.HtmlAttributeEncode(this.Domain) + "</Domain>" : "")
                   + "</Credentials>";
        }
    }

