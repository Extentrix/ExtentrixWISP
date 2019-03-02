using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;
/// <summary>
/// Contains all constants required by the web service
/// </summary>
public class WSConstants
{
    // codebase URL 
    public static string CODEBASE_URL = WebServiceConfig.EveryoneRole;
    // log errors and info flags
    public static bool LOGERRORS      = WebServiceConfig.LogErrors;
    public static bool LOGINFO = WebServiceConfig.LogInfo;
    public static bool LOGDETINFO = WebServiceConfig.LogDetInfo;
    // Citrix XML service URL and port number
    //todo read from sp list
    public static string CTX_XML = WebServiceConfig.ADDRESS;

    public static string CAG_FQDN = WebServiceConfig.CAG_FQDN;
    public static string CAG_Port = WebServiceConfig.CAG_Port;
    public static string STA_URL = WebServiceConfig.STA_URL;
    public static bool useCAG = WebServiceConfig.UseCAG;
    public static string ICAPORT = WebServiceConfig.ICAPORT;
    public static string ADDRESS = WebServiceConfig.ADDRESS;
    public static string STA_FailOver = WebServiceConfig.STA_FailOver;
    public static string CTXXMLPort = WebServiceConfig.CTXXMLPort;
       
    // Password encryption methods mapping strings
    public static string[] encryption = { "cleartext", "rsa56", "rsa128", "ctx1" };

    // Domain types mapping strings
    public static string[]  domains           = { "NT", "UNIX", "NDS" };
    public static string[]  allowedServerTypes = { "all","win32","x"};
    public static string    defaultServerType = "all";
    public static string[]  allowedClientTypes = { "ica30", "all", "rdp", "rade", "content" };
    public static string    defaultClientType = "all";
    public static string[]  allowedDetails = { "icon", "defaults", "file-type", "access-list", "icon-info" };
    public static string    defaultDetails = "defaults";
    public static int[]     allowedLicenseTypes = { 0,1,2};
    public static string    defaultIP = "0.0.0.0";

    // Supported image formats
    public static string imageFormats = "png|gif|raw|ico|tiff";

    //limits 
    public const int Max_CLIENTNAME_LENGTH = 20;

    //Error Messages
    public static string INVALID_ENCRYPTION_METHOD = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Invalid Password Encryption Method";
    public static string INVALID_DOMAIN_TYPE = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Inavlid Domain Type";
    public static string INVALID_IP_ADDRESS = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Invalid IP address";
    public static string APPLICATION_IS_NOT_PUBLISHED = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Application is not published for the specified user or application name is misspelled";
    public static string INVALID_USERNAME = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Application User name is null or empty";
    public static string INVALID_PASSWORD = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Application Password is null or empty";
    public static string INVALID_DOMAIN = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Application Domain is null or empty";

    public static string FILE_EXTENTION_IS_NOT_SUPPORTED = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_There is no published application to launch the given file";
    public static string INVALID_CLIENT_IDENTIFICATION_METHOD = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Invalid Client Identification method";
    public static string INVALID_SERVER_TYPE = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_One or more invalid server type(s) specified";
    public static string INVALID_CLIENT_TYPE = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_One or more invalid client type(s) specified";
    public static string INVALID_DESIRED_DETAILS = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_One or more invalid desired details specified";
    public static string NULL_APPNAME_PARAMETER = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_ Both application name and parameter are null or empty";
    public static string NULL_CLIENT_CREDENTIAL = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_ClientName or Credentials, or both have to be supplied.";
    public static string INVALID_CREDENTIAL = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_ Invalid Credentials.";
    public static string USER_IS_NOT_AUTHENTICATED = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_ User is not authenticated.";

    public static string INVALID_LICENSE_TYPE = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_invalid license type";

    public static string CLIENTNAME_EXCEEDS_LIMITS = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Client name exceeds limit (client name should be 20 characters or less)";
    public static string NO_CLIENT_ID_SPECIFIED = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_No client ID was specified.";
    public static string SERVER_DOES_NOT_HAVE_MULTI_ICON_CAPABILITY = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_The server does not have the multi-image-icons capability.";
    public static string ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED = "EXTENTRIX_WEBSERVICE_One or more Mandatory argument(s) is null-valued";
    public static string ARGUMENTS_NULL_VALUED = "EXTENTRIX_WEBSERVICE_? argument(s) is null-valued";
    public static string INVALID_SIZE_OR_CLORDEPTH = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Invalid size or color depth value, supported sizes : (16,32), supported depth : (4,32).";
    public static string INVALID_IMAGE_FORMAT = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_Invalid or unsupported image format.";
    public static string CAPAPILITY_NOT_SUPPORTED = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_The server doesn't support multi-image capability.";
    public static string STA_INVALID_RESPONSE = "EXTENTRIX_WEBSERVICE_APPLICATION_EDITION_can not contact the Security Ticket Authority";
    public static string[] LICENSE_ERROR_MESSAGES = new string[] { "Failed to open Extentrix Web Services 5.0 - Application Edition license file. Make sure that you have a license file", "Failed to open Extentrix Turbo Toolbar license file.Make sure that you have a license file", "Failed to open Extentrix Toolbar for Citrix license file.Make sure that you have a license file" };
    public static string[] LICENSE_FILES_NAMES = new string[] { "LICENSE", "TurboLICENSE1_0" ,"SAPLICENSE1_0"};
    public static string LIC_LICENSES_FOLDER = "Licenses";
    public static string PRODUCT_KEY = @"Software\Extentrix\WebServices\Application\5.0\";

    public static string ErrorMessages(string errorId)
    {
        if (errorId == "version-mismatch")
            return "Version mismatch, the server is unable to service requests of the requests version";
        else if (errorId == "failed-credentials")
            return "The supplied credentials were invalid, or inappropriate";
        else if (errorId == "no-server")
            return "No server available";
        else if (errorId == "must-change-credentials")
            return "Credentials provided are valid but security authority has specified that they can be used only to change the password";
        else if (errorId == "expired-credentials")
            return "Credentials provided were valid but have now expired";
        else if (errorId == "account-locked-out")
            return "The credentials provided refer to an account that has been locked out (possibly because of too many bad logon attempts)";
        else if (errorId == "invalid-logon-hours")
            return "The credentials provided refer to an account that is not permitted to login at this time";
        else if (errorId == "no-alt-address")
            return "No alternate address. There is no alternate address for the requested 'application'";
        else if (errorId == "Ticketing-Disabled")
            return "The Ticketing Feature has been disabled. This means that the FR1 license is not installed on the Citrix XML Server";
        else if (errorId == "GroupEnumeration-Disabled")
            return "The Group Enumeration Feature has been disabled. This means that the FR1 license is not installed on the Citrix XML Server";
        else if (errorId == "FolderEnumeration-Disabled")
            return "The Folder Enumeration Feature has been disabled. This means that the FR1 license is not installed on the Citrix XML Server";
        else if (errorId == "mfserver-overloaded")
            return "All applicable servers are currently at maximum load";
        else if (errorId == "not-trusted")
            return "The client is not trusted to perform the requested operation. This does not imply whether the credentials supplied are/are not valid";
        else if (errorId == "not-licensed")
            return "The remote Presentation Server does not have the required license to perform the requested activity";
        else if (errorId == "service-unavailable")
            return "The server is unable to process requests";
        else
            return "Unspecified Error";
    }
    static WSConstants()
    {
        
    }
    
}
