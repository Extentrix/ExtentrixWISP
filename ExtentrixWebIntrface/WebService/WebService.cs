using System;
using System.Linq;
using System.Collections;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Net;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;


/// <summary>
/// Summary description for WebService
/// </summary>
public class WebService
{
	public static string LastSTAServer = "";
	public static string LastCPSServer = "";
	public static Hashtable failedSTAUrls = new Hashtable();

	//load win api library
	// Logon User - required for user authorization
	[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
			int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
	//
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern bool CloseHandle(IntPtr handle);

	public WebService()
	{
	}

	/// The method returns the ICA file description to be used to launch an application 
	/// parameters:
	/// appName     - application name.
	/// credentials - an object contains the user credentials to authenticate his request.
	/// clientName  - the name of client's machine.
	/// ipAddress   - the IP address of the client.
	/// return value:
	/// a string represents the ICA file content
	public static string LaunchApplication(string appName, Credentials credentials, string clientName, string ipAddress)
	{
		ICAConnectionOptions option = new ICAConnectionOptions();
		option.connectionSpeed = ConnectionSpeed.High;
		option.windowSize = WindowSize._fullScreen;

		string errorMsg = "";
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LaunchApplication : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " IP: " + ipAddress + " Called API: LaunchApplication";

		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		if (String.IsNullOrEmpty(appName))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LaunchApplication: " + WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, 0);

			throw new SoapException(WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, SoapException.ClientFaultCode);
		}

		if (credentials != null)
		{
			if (credentials is WindowsCredentials)
			{
				if (!((WindowsCredentials)credentials).validateEncryptionMethod())
				{
					if (WSConstants.LOGERRORS)
						EventLogger.logEvent("LaunchApplication: " + WSConstants.INVALID_ENCRYPTION_METHOD, 0);
					throw new SoapException(WSConstants.INVALID_ENCRYPTION_METHOD, SoapException.ClientFaultCode);
				}
				else
					if (!((WindowsCredentials)credentials).validateDomainType())
					{
						if (WSConstants.LOGERRORS)
							EventLogger.logEvent("LaunchApplication: " + WSConstants.INVALID_DOMAIN_TYPE, 0);
						throw new SoapException(WSConstants.INVALID_DOMAIN_TYPE, SoapException.ClientFaultCode);
					}
			}
		}

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancate it to 20 chars
		//
		if (clientName == null)
			clientName = string.Empty;
		else if (clientName.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LaunchApplication: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 0);

			clientName = clientName.Substring(0, WSConstants.Max_CLIENTNAME_LENGTH);
		}

		string serverAddress = "";
		string ticket = ICAGenerator.getTicket(credentials, clientName, ipAddress, appName, ref serverAddress);
		string ica = "";
		if (WSConstants.useCAG)
		{
			ica = ICAGenerator.generateICAContentWithCAG(ticket, serverAddress, appName, credentials, clientName);
			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("LaunchApplication: Succeed with result:  " + ica, 1);
			return ica;
		}
		else
		{
			ica = ICAGenerator.generateICAContent(ticket, serverAddress, appName, credentials, clientName);
			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("LaunchApplication: Succeed with result:  " + ica, 1);
			return ica;
		}

		//}
	}




	/// The method returns all applications published for a specific user given his credentials ,machine name , and IP Address 
	/// parameters:
	/// credentials - an object contains the user credentials to authenticate his request.
	/// clientName  - the name of client's machine.
	/// ipAddress   - the IP address of the client.
	/// return value:
	/// an arreay of ApplicationItem 
	public static ApplicationItem[] GetApplicationsByCredentials(Credentials credentials, string clientName, string ipAddress, string[] desiredDetails, string[] serverTypes, string[] clientTypes)
	{

		//WindowsCredentials credentials = (WindowsCredentials)credential;
		string errorMsg = string.Empty;
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentials : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " IP: " + ipAddress + " Called API: GetApplicationsByCredentials";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		//
		// Validate Credentials
		//
		if (credentials != null)
		{
			if (credentials is WindowsCredentials)
			{
				ValidateWindowsCredential((WindowsCredentials)credentials);
				//if (!((WindowsCredentials)credentials).validateEncryptionMethod())
				//{
				//    if (WSConstants.LOGERRORS)
				//        EventLogger.logEvent("GetApplicationsByCredentials: " + WSConstants.INVALID_ENCRYPTION_METHOD, 0);
				//    throw new SoapException(WSConstants.INVALID_ENCRYPTION_METHOD, SoapException.ClientFaultCode);
				//}
				//else if (!((WindowsCredentials)credentials).validateDomainType())
				//{
				//    if (WSConstants.LOGERRORS)
				//        EventLogger.logEvent("GetApplicationsByCredentials: " + WSConstants.INVALID_DOMAIN_TYPE, 0);
				//    throw new SoapException(WSConstants.INVALID_DOMAIN_TYPE, SoapException.ClientFaultCode);

				//}
			}
		}

		if (!Validator.validateAddress(ref ipAddress))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentials: " + WSConstants.INVALID_IP_ADDRESS, 0);
			throw new SoapException(WSConstants.INVALID_IP_ADDRESS, SoapException.ClientFaultCode);
		}

		//
		// Validate the serverTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateServerTypes(ref serverTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentials: " + WSConstants.INVALID_SERVER_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_SERVER_TYPE, SoapException.ClientFaultCode);
		}

		//
		// Validate the clientTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateClientTypes(ref clientTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentials: " + WSConstants.INVALID_CLIENT_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_CLIENT_TYPE, SoapException.ClientFaultCode);
		}

		//
		// Validate the desiredDetails argument, if Null set it to default "defaults"
		//
		if (!Validator.validateDesiredDetails(ref desiredDetails))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentials: " + WSConstants.INVALID_DESIRED_DETAILS, 0);
			throw new SoapException(WSConstants.INVALID_DESIRED_DETAILS, SoapException.ClientFaultCode);
		}

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancated it to 20 chars
		//
		if (clientName == null)
			clientName = string.Empty;
		else if (clientName.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentials: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 0);

			clientName = clientName.Substring(0, WSConstants.Max_CLIENTNAME_LENGTH);
		}

		ApplicationItem[] apps;
		string detailsTag = Assembler.assembleDetails(desiredDetails);
		string serversTag = Assembler.assembleServerTypes(serverTypes);
		string clientsTag = Assembler.assembleClientTypes(clientTypes);

		string postData = "<?xml version='1.0' encoding='utf-8'?>"
											+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
											+ "<NFuseProtocol version='4.1'>"
											+ "<RequestAppData>"
											+ "<Scope traverse='onelevel'/>"
											+ detailsTag
											+ serversTag
											+ clientsTag
											+ (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
											+ "<ClientName>" + HttpUtility.HtmlAttributeEncode(clientName) + "</ClientName>"
											+ "<ClientAddress>" + ipAddress + "</ClientAddress>"
											+ "</RequestAppData>"
											+ "</NFuseProtocol>";

		//Logger.Default.Info(LogLocationEnum.Default, string.Format("GetApplicationsByCredentials GetHttpResponse adresses={0} port={1} postData={2}", WSConstants.CTX_XML, WSConstants.CTXXMLPort, postData));
		//Logger.Default.Info(LogLocationEnum.Text, string.Format("GetApplicationsByCredentials GetHttpResponse adresses={0} port={1} postData={2}", WSConstants.CTX_XML, WSConstants.CTXXMLPort, postData));
		string ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
		//Logger.Default.Info(LogLocationEnum.Default, string.Format("GetApplicationsByCredentials ResponseText={0}", ResponseText));
		//Logger.Default.Info(LogLocationEnum.Text, string.Format("GetApplicationsByCredentials ResponseText={0}", ResponseText));
		if (Parser.checkErrorResponse(ResponseText, out errorMsg))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentials: " + errorMsg, 0);

			throw new SoapException(errorMsg, SoapException.ServerFaultCode);
		}
		else
		{
			apps = Parser.parseAppData(ResponseText);

			if (WSConstants.LOGDETINFO)
			{
				string publishedApp = "";
				for (int i = 0; i < apps.Length; i++)
					publishedApp += "\nApp:" + apps[i].FreindlyName + " InternalName: " + apps[i].InternalName + " Folder: " + apps[i].FolderName + "\n";

				Logger.Default.Info(LogLocationEnum.Default, string.Format("GetApplicationsByCredentials Succeed with result:={0}", publishedApp));
				Logger.Default.Info(LogLocationEnum.Text, string.Format("GetApplicationsByCredentials Succeed with result:={0}", publishedApp));
				EventLogger.logEvent("GetApplicationsByCredentials:Succeed with result: " + publishedApp, 1);
			}
			return apps;
		}
	}

	//Get Authorized used
	// type could be 
	// 0 - use current logged user
	// 1 - use provided windows credentials
	public static WindowsIdentity authorizeUser(int type, WindowsCredentials cred)
	{
		//get current logged user
		WindowsIdentity identity = new WindowsIdentity();
		System.Security.Principal.WindowsIdentity wi = null;

		if (type != 0 && type != 1)
			return identity;

		switch (type)
		{
			case 0: // Get credential from currently logged user 
				{
					System.Security.Principal.IPrincipal pr = System.Web.HttpContext.Current.User;

					if (!pr.Identity.IsAuthenticated)
					{
						if (WSConstants.LOGERRORS)
							EventLogger.logEvent("authorizeUser: User is not Authenticated. User name:" + pr.Identity.Name, 0);
						throw new SoapException("User is not Authenticated! Current WS user is Anonumous.", SoapException.ClientFaultCode);
					}

					//get user name and domen 
					String principal = pr.Identity.Name;
					String[] splArr = principal.Split("\\".ToCharArray());
					principal = splArr[splArr.Length - 1];
					//get WindowsIdentity object
					wi = new System.Security.Principal.WindowsIdentity(principal, pr.Identity.AuthenticationType);

					break;
				}
			case 1:
				{
					IntPtr newUserHandle = new IntPtr(0);
					if (cred == null)
					{
						if (WSConstants.LOGERRORS)
							EventLogger.logEvent("authorizeUser: Provided credentials are null.", 0);
						throw new SoapException("User Credentials could no be null.", SoapException.ClientFaultCode);
					}

					//login with provided user credentials
					string user = cred.WinUserName != null ? cred.WinUserName : cred.UserName;
					string domen = cred.WinDomain != null ? cred.WinDomain : cred.Domain;
					Boolean rest = LogonUser(user, domen, cred.Password, 3, 0, ref newUserHandle);
					//create WindowsIdentity object
					wi = new System.Security.Principal.WindowsIdentity(newUserHandle);
					CloseHandle(newUserHandle);

					break;
				}

		}
		//Create Extentrix Windows Identity object.

		identity.UPN = wi.Name;
		// set the values for web services windows identity
		identity.SAM = wi.Name;

		// get all the groups that the user belongs to.                   
		System.Security.Principal.IdentityReferenceCollection irc = wi.Groups;

		// define list to contains groups
		ArrayList list = new ArrayList();

		// Add the SID for each group
		foreach (System.Security.Principal.IdentityReference ir in irc)
		{
			list.Add(ir.Value);
		}

		// Add the SID for the current login user
		list.Add(wi.User.Value);

		// assgine the collected SID list (user and the groups belongs to).
		identity.SIDs = (string[])list.ToArray(typeof(string));
		if (WSConstants.LOGINFO)
			EventLogger.logEvent("authorizeUser:Succeed with result: " + identity, 1);

		return identity;

	}

	public static string LaunchApplicationWithParameter(string appName, string parameter, Credentials credentials, string clientName, string ipAddress)
	{
		string errorMsg = "";
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LaunchApplicationWithParameter : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " IP: " + ipAddress + " Called API: LaunchApplicationWithParameter";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancate it to 20 chars
		//
		if (clientName == null)
			clientName = string.Empty;
		else if (clientName.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LaunchApplicationWithParameter: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 1);

			clientName = clientName.Substring(0, WSConstants.Max_CLIENTNAME_LENGTH);
		}

		if (String.IsNullOrEmpty(appName))
		{
			if (!string.IsNullOrEmpty(parameter))
			{
				string ext = parameter.Substring(parameter.LastIndexOf('.'));
				if (ext == null)
				{
					if (WSConstants.LOGERRORS)
						EventLogger.logEvent("LaunchApplicationWithParameter: " + "File names should have an extension", 0);
					throw new SoapException("File names should have an extension", SoapException.ClientFaultCode);
				}
				else
				{
					string[] details = { "icon", "file-type", "access-list" };
					ApplicationItem[] apps = GetApplicationsByCredentials(credentials, clientName, ipAddress, details, null, null);
					for (int i = 0; i < apps.Length; i++)
						if (apps[i].fileExtentionExists(ext))
						{
							appName = apps[i].InternalName;
							break;
						}
					if (appName == null)
					{
						if (WSConstants.LOGERRORS)
							EventLogger.logEvent("LaunchApplicationWithParameter: " + WSConstants.FILE_EXTENTION_IS_NOT_SUPPORTED, 0);

						throw new SoapException(WSConstants.FILE_EXTENTION_IS_NOT_SUPPORTED, SoapException.ClientFaultCode);
					}
				}
			}
			else
			{
				//no app name and no parameter
				throw new SoapException(WSConstants.NULL_APPNAME_PARAMETER, SoapException.ClientFaultCode);
			}
		}
		string icaContent = LaunchApplication(appName, credentials, clientName, ipAddress);
		//if ((parameter.Length > 2 && parameter[0] == '\\' && parameter[1] == '\\') || parameter.StartsWith("http://"))
		icaContent = icaContent.Replace("InitialProgram=#" + appName, "InitialProgram=#\"" + appName + "\"");
		icaContent = icaContent.Replace("LongCommandLine=", "LongCommandLine=" + parameter);
		//else
		//   icaContent = icaContent.Replace("InitialProgram=#" + appName, "InitialProgram=#\"" + appName + "\" \\\\client\\" + parameter);
		if (WSConstants.LOGDETINFO)
		{
			EventLogger.logEvent("LaunchApplicationWithParameter:Succeed with result: " + icaContent, 1);
		}
		return icaContent;
	}



	public static string GetCodebaseURL()
	{
		string errorMsg = "";
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetCodebaseURL : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = " Called API: GetCodebaseURL";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		if (WSConstants.LOGDETINFO)
			EventLogger.logEvent("GetCodebaseURL succeed: " + WSConstants.CODEBASE_URL, 1);

		return WSConstants.CODEBASE_URL;
	}




	/// The method returns information about an application published for a specific user given his credentials ,machine name , and IP Address 
	/// parameters:
	/// appName     - the name of the application .
	/// credentials - an object contains the user credentials to authenticate his request.
	/// clientName  - the name of client's machine.
	/// ipAddress   - the IP address of the client.
	/// return value:
	/// an object of ApplicationItem 
	public static ApplicationItem GetApplicationInfo(string appName, Credentials credentials, string clientName, string ipAddress, string[] desiredDetails, string[] serverTypes, string[] clientTypes)
	{
		//string errorMsg = "";
		//if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfo : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
		//    throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		//}


		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " IP: " + ipAddress + " Called API: GetApplicationInfo";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		if (String.IsNullOrEmpty(appName))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationInfo: " + WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, 0);

			throw new SoapException(WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, SoapException.ClientFaultCode);
		}

		// validate Credentials
		// if (credentials != null)
		// {

		//if (credentials is WindowsCredentials)
		//{
		//    if (!((WindowsCredentials)credentials).validateEncryptionMethod())
		//    {
		//        if (WSConstants.LOGERRORS)
		//            EventLogger.logEvent("GetApplicationInfo: " + WSConstants.INVALID_ENCRYPTION_METHOD, 0);

		//        throw new SoapException(WSConstants.INVALID_ENCRYPTION_METHOD, SoapException.ClientFaultCode);
		//    }
		//    else if (!((WindowsCredentials)credentials).validateDomainType())
		//    {
		//        if (WSConstants.LOGERRORS)
		//            EventLogger.logEvent("GetApplicationInfo: " + WSConstants.INVALID_DOMAIN_TYPE, 0);

		//        throw new SoapException(WSConstants.INVALID_DOMAIN_TYPE, SoapException.ClientFaultCode);
		//    }
		//}
		//}
		//if (!Validator.validateAddress(ref ipAddress))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfo: " + WSConstants.INVALID_IP_ADDRESS, 0);

		//    throw new SoapException(WSConstants.INVALID_IP_ADDRESS, SoapException.ClientFaultCode);
		//}

		//
		// Validate the serverTypes argument, if Null set it to default "all"
		//
		//if (!Validator.validateServerTypes(ref serverTypes))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfo: " + WSConstants.INVALID_SERVER_TYPE, 0);
		//    throw new SoapException(WSConstants.INVALID_SERVER_TYPE, SoapException.ClientFaultCode);
		//}

		//
		// Validate the clientTypes argument, if Null set it to default "all"
		//
		//if (!Validator.validateClientTypes(ref clientTypes))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfo: " + WSConstants.INVALID_CLIENT_TYPE, 0);
		//    throw new SoapException(WSConstants.INVALID_CLIENT_TYPE, SoapException.ClientFaultCode);
		//}

		//
		// Validate the desiredDetails argument, if Null set it to default "defaults"
		//
		//if (!Validator.validateDesiredDetails(ref desiredDetails))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfo: " + WSConstants.INVALID_DESIRED_DETAILS, 0);
		//    throw new SoapException(WSConstants.INVALID_DESIRED_DETAILS, SoapException.ClientFaultCode);
		//}

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancated it to 20 chars
		//
		//if (clientName == null)
		//    clientName = string.Empty;
		//else if (clientName.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfo: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 1);

		//    clientName = clientName.Substring(0, WSConstants.Max_CLIENTNAME_LENGTH);
		//}

		ApplicationItem[] apps = null;
		//string detailsTag = Assembler.assembleDetails(desiredDetails);
		//string serversTag = Assembler.assembleServerTypes(serverTypes);
		//string clientsTag = Assembler.assembleClientTypes(clientTypes);

		apps = GetApplicationsByCredentials(credentials, clientName, ipAddress, desiredDetails, serverTypes, clientTypes);

		if (apps != null && apps.Length != 0)
		{
			for (int i = 0; i < apps.Length; i++)
				if (apps[i].InternalName.Equals(appName, StringComparison.OrdinalIgnoreCase))
				{
					if (WSConstants.LOGINFO)
						EventLogger.logEvent("GetApplicationInfo: Succeed with result: info about App:" + apps[i].FreindlyName, 1);
					return apps[i];
				}

		}

		if (WSConstants.LOGERRORS)
			EventLogger.logEvent("GetApplicationInfo: " + WSConstants.APPLICATION_IS_NOT_PUBLISHED, 0);

		throw new SoapException(WSConstants.APPLICATION_IS_NOT_PUBLISHED, SoapException.ClientFaultCode);
	}




	private static ApplicationItem[] copytoApplicationItem(ApplicationItemEx[] appsEx)
	{
		ApplicationItem[] apps = null;
		if (appsEx != null)
		{
			apps = new ApplicationItem[appsEx.Length];
			for (int i = 0; i < appsEx.Length; i++)
			{
				apps[i] = new ApplicationItem();
				apps[i].InternalName = appsEx[i].InternalName;
				apps[i].FreindlyName = appsEx[i].FreindlyName;
				apps[i].fileTypes = appsEx[i].fileTypes;
				apps[i].Encryption = appsEx[i].Encryption;
				apps[i].AppInStartmenu = appsEx[i].AppInStartmenu;
				apps[i].AppOnDesktop = appsEx[i].AppOnDesktop;
				apps[i].ChangeCount = appsEx[i].ChangeCount;
				apps[i].ClientType = appsEx[i].ClientType;
				apps[i].ContentAddress = appsEx[i].ContentAddress;
				apps[i].Description = appsEx[i].Description;
				apps[i].EncryptionMinimum = appsEx[i].EncryptionMinimum;
				apps[i].FolderName = appsEx[i].FolderName;
				apps[i].Icon = appsEx[i].Icon;
				apps[i].PublisherName = appsEx[i].PublisherName;
				apps[i].RemoteAccessEnabled = appsEx[i].RemoteAccessEnabled;
				apps[i].ServerType = appsEx[i].ServerType;
				apps[i].SoundType = appsEx[i].SoundType;
				apps[i].SoundTypeMinimum = appsEx[i].SoundTypeMinimum;
				apps[i].SSLEnabled = appsEx[i].SSLEnabled;
				apps[i].StartmenuFolder = appsEx[i].StartmenuFolder;
				apps[i].StartMenuRoot = appsEx[i].StartMenuRoot;
				apps[i].VideoType = appsEx[i].VideoType;
				apps[i].VideoTypeMinimum = appsEx[i].VideoTypeMinimum;
				apps[i].WinColor = appsEx[i].WinColor;
				apps[i].WinHeight = appsEx[i].WinHeight;
				apps[i].WinScale = appsEx[i].WinScale;
				apps[i].WinType = appsEx[i].WinType;
				apps[i].WinWidth = appsEx[i].WinWidth;
			}
		}
		return apps;
	}



	public static bool ValidateCredentials(Credentials credentials)
	{
		//string errorMsg = "";
		//if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("ValidateCredentials : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
		//    throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		//}

		/*
		 <RequestValidateCredentials>
<Credentials> .... </Credentials>
</RequestValidateCredentials>
		 */

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " Called API: ValidateCredentials";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		if (credentials == null)
		{
			if (WSConstants.LOGERRORS)
			{
				EventLogger.logEvent("ValidateCredentials: " + WSConstants.ARGUMENTS_NULL_VALUED.Replace("?", "Credentials"), 0);

			}

			throw new SoapException(WSConstants.ARGUMENTS_NULL_VALUED.Replace("?", "Credentials"), SoapException.ClientFaultCode);

		}

		string[] details = { "icon", "file-type" };
		try
		{
			GetApplicationsByCredentials(credentials, " ", "1.1.1.1", details, null, null);
			if (WSConstants.LOGINFO)
				EventLogger.logEvent("ValidateCredentials Succeed: Valid Credentials", 1);
			//string postData = "<?xml version='1.0' encoding='utf-8'?>"
			//             + "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
			//             + "<NFuseProtocol version='1'>"
			//             + "<RequestValidateCredentials>"
			//             + credentials.ToXML(true)
			//             + "</RequestValidateCredentials>"
			//             + "</NFuseProtocol>";

			//string ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);

			//if (Parser.checkErrorResponse(ResponseText, out errorMsg))
			//{
			//    if (WSConstants.LOGERRORS)
			//        EventLogger.logEvent("GetApplicationsByCredentials: " + errorMsg, 0);

			//    throw new SoapException(errorMsg, SoapException.ServerFaultCode);
			//}

			return true;
		}
		catch
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("ValidateCredentials: " + "Invalid Credentials", 0);
			//   throw new SoapException(WSConstants.INVALID_CREDENTIAL, SoapException.ClientFaultCode);
			//no need to throw Exception
			return false;
		}
	}




	public static bool IsApplicationPublished(string appName, Credentials credentials, string clientName, string ipAddress)
	{
		string errorMsg = "";
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("IsApplicationPublished: Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " Called API: IsApplicationPublished";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		if (String.IsNullOrEmpty(appName))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationInfo: " + WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, 0);

			throw new SoapException(WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, SoapException.ClientFaultCode);
		}

		try
		{
			ApplicationItem app = GetApplicationInfo(appName, credentials, clientName, ipAddress, null, null, null);
			if (app != null && app.InternalName != "")
			{
				if (WSConstants.LOGINFO)
					EventLogger.logEvent("IsApplicationPublished Succeed: APP:" + appName + " is published", 1);
				return true;
			}
			else
			{
				if (WSConstants.LOGINFO)
					EventLogger.logEvent("IsApplicationPublished Succeed: APP: " + appName + " is not published", 1);

				throw new SoapException(WSConstants.APPLICATION_IS_NOT_PUBLISHED, SoapException.ServerFaultCode);
				//return false;
			}
		}
		catch (Exception ex)
		{
			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("IsApplicationPublished, Error at calling this API", 0);

			throw new SoapException(ex.Message, SoapException.ServerFaultCode);

			//  return false;
		}
	}



	public static ICASession[] GetDiscSessions(int idMethod, string clientID, Credentials credentials)
	{
		string errorMsg = String.Empty;
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetDiscSessions: Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " Called API: GetDiscSessions";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancate it to 20 chars
		//
		if (string.IsNullOrEmpty(clientID))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetDiscSessions: " + WSConstants.ARGUMENTS_NULL_VALUED.Replace("?", "Client ID"), 0);

			throw new SoapException(WSConstants.ARGUMENTS_NULL_VALUED.Replace("?", "Client ID"), SoapException.ClientFaultCode);


		}

		else if (clientID.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetDiscSessions: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 0);

			throw new SoapException(WSConstants.CLIENTNAME_EXCEEDS_LIMITS, SoapException.ClientFaultCode);
		}

		if (credentials != null)
		{
			if (credentials is WindowsCredentials)
			{
				if (!((WindowsCredentials)credentials).validateEncryptionMethod())
				{
					if (WSConstants.LOGERRORS)
						EventLogger.logEvent("GetDiscSessions: " + WSConstants.INVALID_ENCRYPTION_METHOD, 0);

					throw new SoapException(WSConstants.INVALID_ENCRYPTION_METHOD, SoapException.ClientFaultCode);
				}
				else if (!((WindowsCredentials)credentials).validateDomainType())
				{
					if (WSConstants.LOGERRORS)
						EventLogger.logEvent("GetDiscSessions: " + WSConstants.INVALID_DOMAIN_TYPE, 0);

					throw new SoapException(WSConstants.INVALID_DOMAIN_TYPE, SoapException.ClientFaultCode);
				}
			}
		}
		else
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetDiscSessions: " + WSConstants.ARGUMENTS_NULL_VALUED.Replace("?", "Credentials"), 0);

			throw new SoapException(WSConstants.ARGUMENTS_NULL_VALUED.Replace("?", "Credentials"), SoapException.ClientFaultCode);
		}

		//
		// Validate idMethod
		//
		if (!Validator.validateIDMethod(idMethod))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetDiscSessions: " + WSConstants.INVALID_CLIENT_IDENTIFICATION_METHOD, 0);

			throw new SoapException(WSConstants.INVALID_CLIENT_IDENTIFICATION_METHOD, SoapException.ClientFaultCode);
		}

		//
		// Build the request
		//
		string clientIDTag = Assembler.assembleClientID(idMethod, clientID); ;
		string postData = "<?xml version='1.0' encoding='utf-8'?>"
											+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
											+ "<NFuseProtocol version=" + (idMethod == 0 || idMethod == -1 ? "'4.6'" : "'4.6'") + ">"
											+ "<RequestReconnectSessionData>"
											+ (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
											+ clientIDTag
											+ "<ServerType>all</ServerType>"
											+ "<ClientType>all</ClientType>"
											+ "<SessionType>disconnected</SessionType>"
											+ "</RequestReconnectSessionData>"
											+ "</NFuseProtocol>";

		errorMsg = string.Empty;
		string responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
		if (Parser.checkErrorResponse(responseText, out errorMsg))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetDiscSessions: " + errorMsg, 0);

			throw new SoapException(errorMsg, SoapException.ServerFaultCode);
		}
		else
		{
			ICASession[] sessions = Parser.parseSessions(responseText);
			if (WSConstants.LOGDETINFO)
			{
				string discSessions = "";
				for (int i = 0; i < sessions.Length; i++)
					discSessions += sessions[i].ID + "-" + sessions[i].InternalName + "-" + sessions[i].ServerType + "-" + sessions[i].DataType + "\n";


				EventLogger.logEvent("GetDiscSessions, Succeed with(" + sessions.Length + ") disconnected sessions: " + discSessions, 1);

			}

			return sessions;
		}

	}



	public static bool DisconnectSessions(int idMethod, string clientID, Credentials credentials, string[] serverTypes, string[] clientTypes)
	{
		string errorMsg = string.Empty;
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("DisconnectSessions: Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " Called API: DisconnectSessions";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);


		if (credentials != null)
		{
			if (credentials is WindowsCredentials)
			{
				if (!((WindowsCredentials)credentials).validateEncryptionMethod())
				{
					if (WSConstants.LOGERRORS)
						EventLogger.logEvent("DisconnectSessions: " + WSConstants.INVALID_ENCRYPTION_METHOD, 0);

					throw new SoapException(WSConstants.INVALID_ENCRYPTION_METHOD, SoapException.ClientFaultCode);
				}
				else if (!((WindowsCredentials)credentials).validateDomainType())
				{
					if (WSConstants.LOGERRORS)
						EventLogger.logEvent("DisconnectSessions: " + WSConstants.INVALID_DOMAIN_TYPE, 0);

					throw new SoapException(WSConstants.INVALID_DOMAIN_TYPE, SoapException.ClientFaultCode);
				}
			}

			if (string.IsNullOrEmpty(credentials.UserName))
			{
				if (string.IsNullOrEmpty(clientID))
				{

					if (WSConstants.LOGERRORS)
						EventLogger.logEvent("DisconnectSessions: " + WSConstants.NULL_CLIENT_CREDENTIAL, 0);

					throw new SoapException(WSConstants.NULL_CLIENT_CREDENTIAL, SoapException.ClientFaultCode);
				}
			}


		}
		else
		{
			if (string.IsNullOrEmpty(clientID))
			{

				if (WSConstants.LOGERRORS)
					EventLogger.logEvent("DisconnectSessions: " + WSConstants.NULL_CLIENT_CREDENTIAL, 0);

				throw new SoapException(WSConstants.NULL_CLIENT_CREDENTIAL, SoapException.ClientFaultCode);
			}
		}

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancated it to 20 chars
		//
		if (clientID == null)
			clientID = string.Empty;
		else if (clientID.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("DisconnectSessions: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 0);

			throw new SoapException(WSConstants.CLIENTNAME_EXCEEDS_LIMITS, SoapException.ClientFaultCode);
		}



				//
		// validate the idMethod argument
		//
		else if (idMethod == -1 || !Validator.validateIDMethod(idMethod))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("DisconnectSessions: " + WSConstants.INVALID_CLIENT_IDENTIFICATION_METHOD, 0);

			throw new SoapException(WSConstants.INVALID_CLIENT_IDENTIFICATION_METHOD, SoapException.ClientFaultCode);
		}

		//
		// Validate the serverTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateServerTypes(ref serverTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("DisconnectSessions: " + WSConstants.INVALID_SERVER_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_SERVER_TYPE, SoapException.ClientFaultCode);
		}

		//
		// Validate the clientTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateClientTypes(ref clientTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("DisconnectSessions: " + WSConstants.INVALID_CLIENT_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_CLIENT_TYPE, SoapException.ClientFaultCode);
		}

		// build the request
		string clientIDTag = Assembler.assembleClientID(idMethod, clientID); ;
		string postData = "<?xml version='1.0' encoding='utf-8'?>"
											+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
											+ "<NFuseProtocol version=" + (idMethod == 0 ? "'4.1'" : "'4.6'") + ">"
											+ "<RequestDisconnectUserSessions>"
											 + Assembler.assembleClientID(idMethod, clientID)
											+ (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
											+ Assembler.assembleClientTypes(clientTypes)
											+ Assembler.assembleServerTypes(serverTypes)
											+ "</RequestDisconnectUserSessions>"
											+ "</NFuseProtocol>";

		string errorMessage = "";
		string responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
		if (Parser.checkErrorResponse(responseText, out errorMessage))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("DisconnectSessions: " + errorMessage, 0);

			throw new SoapException(errorMessage, SoapException.ServerFaultCode);
			//return false;
		}
		else
		{
			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("DisconnectSessions: Succeed", 1);
			return true;
		}

	}

	private static bool ValidateWindowsCredential(WindowsCredentials credentials)
	{
		if (!credentials.validateEncryptionMethod())
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("ValidateWindowsCredential: " + WSConstants.INVALID_ENCRYPTION_METHOD, 0);

			throw new SoapException(WSConstants.INVALID_ENCRYPTION_METHOD, SoapException.ClientFaultCode);
		}
		if (!credentials.validateDomainType())
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("ValidateWindowsCredential: " + WSConstants.INVALID_DOMAIN_TYPE, 0);

			throw new SoapException(WSConstants.INVALID_DOMAIN_TYPE, SoapException.ClientFaultCode);
		}

		return true;
	}


	public static bool LogoffSessions(int idMethod, string clientID, Credentials credentials, string[] serverTypes, string[] clientTypes)
	{
		string errorMsg = string.Empty;
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LogoffSessions: Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " Called API: LogoffSessions";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancate it to 20 chars
		//
		if (string.IsNullOrEmpty(clientID))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LogoffSessions: " + WSConstants.ARGUMENTS_NULL_VALUED.Replace("?", "Client ID"), 0);

			throw new SoapException(WSConstants.ARGUMENTS_NULL_VALUED.Replace("?", "Client ID"), SoapException.ClientFaultCode);

		}

		else if (clientID.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LogoffSessions: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 0);

			throw new SoapException(WSConstants.CLIENTNAME_EXCEEDS_LIMITS, SoapException.ClientFaultCode);
		}

		if (credentials != null)
		{
			if (credentials is WindowsCredentials)
			{
				ValidateWindowsCredential((WindowsCredentials)credentials);
			}
		}
		else
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LogoffSessions: " + WSConstants.ARGUMENTS_NULL_VALUED.Replace("?", "Credentials"), 0);

			throw new SoapException(WSConstants.ARGUMENTS_NULL_VALUED.Replace("?", "Credentials"), SoapException.ClientFaultCode);
		}

		//
		// validate idMethod
		//
		if (idMethod == -1 || !Validator.validateIDMethod(idMethod))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LogoffSessions: " + WSConstants.INVALID_CLIENT_IDENTIFICATION_METHOD, 0);

			throw new SoapException(WSConstants.INVALID_CLIENT_IDENTIFICATION_METHOD, SoapException.ClientFaultCode);
		}

		//
		// Validate the serverTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateServerTypes(ref serverTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LogoffSessions: " + WSConstants.INVALID_SERVER_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_SERVER_TYPE, SoapException.ClientFaultCode);
		}

		//
		// Validate the clientTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateClientTypes(ref clientTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LogoffSessions: " + WSConstants.INVALID_CLIENT_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_CLIENT_TYPE, SoapException.ClientFaultCode);
		}

		// build the request
		string clientIDTag = Assembler.assembleClientID(idMethod, clientID); ;
		string postData = "<?xml version='1.0' encoding='utf-8'?>"
											+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
											+ "<NFuseProtocol version=" + (idMethod == 0 ? "'4.1'" : "'4.6'") + ">"
											+ "<RequestLogoffUserSessions>"
											+ (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
											+ Assembler.assembleClientID(idMethod, clientID)
											+ Assembler.assembleClientTypes(clientTypes)
											+ Assembler.assembleServerTypes(serverTypes)
											+ "</RequestLogoffUserSessions>"
											+ "</NFuseProtocol>";

		string errorMessage = "";
		string responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
		if (Parser.checkErrorResponse(responseText, out errorMessage))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LogoffSessions: " + errorMessage, 0);

			throw new SoapException(errorMessage, SoapException.ServerFaultCode);
			//return false;
		}
		else
		{
			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("LogoffSessions: Succeed", 1);
			return true;
		}
	}



	public static PresentationServer[] GetServers(string[] serverTypes, string[] clientTypes)
	{
		string errorMsg = "";
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetServers: Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = " Called API: GetServers";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		//
		// Validate the serverTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateServerTypes(ref serverTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetServers: " + WSConstants.INVALID_SERVER_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_SERVER_TYPE, SoapException.ClientFaultCode);
		}

		//
		// Validate the clientTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateClientTypes(ref clientTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetServers: " + WSConstants.INVALID_CLIENT_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_CLIENT_TYPE, SoapException.ClientFaultCode);
		}

		string postData = "<?xml version='1.0' encoding='utf-8'?>"
											+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
											+ "<NFuseProtocol version='4.1'>"
											+ "<RequestServerData>"
											+ Assembler.assembleServerTypes(serverTypes)
											+ Assembler.assembleClientTypes(clientTypes)
											+ "</RequestServerData>"
											+ "</NFuseProtocol>";
		string responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
		string errorMessage = "";
		if (Parser.checkErrorResponse(responseText, out errorMessage))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetServers: " + errorMessage, 0);
			throw new SoapException("GetServers: " + errorMessage, SoapException.ServerFaultCode);
		}
		else
		{
			PresentationServer[] servers = Parser.parseServers(responseText);
			if (WSConstants.LOGDETINFO)
			{
				string serversDet = "";
				for (int i = 0; i < servers.Length; i++)
				{
					serversDet += servers[i].ServerName + "\n";
				}

				EventLogger.logEvent("GetServers, succees with(" + servers.Length + ") CPS Servers " + serversDet, 1);
			}
			return servers;
		}
	}



	public static string GetAltServerAddress(string serverName)
	{
		string errorMsg = "";
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GeAltServerAddress: Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "Called API: GeAltServerAddress";
		if (String.IsNullOrEmpty(serverName))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GeAltServerAddress: " + WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, 0);
			throw new SoapException(WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, SoapException.ServerFaultCode);
		}

		bool noalt = false;
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		string postData = "<?xml version='1.0' encoding='utf-8'?>"
											+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
											+ "<NFuseProtocol version='4.1'>"
											+ "<RequestAddress>"
											+ "<Name><ServerName>" + HttpUtility.HtmlAttributeEncode(serverName) + "</ServerName></Name>"
											+ "<Flags>alt-addr</Flags>"
											+ "</RequestAddress>"
											+ "</NFuseProtocol>";
		string errorMessage = "";
		string responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);

		if (Parser.checkErrorResponse(responseText, out errorMessage))
		{
			if (errorMessage == "No alternate address. There is no alternate address for the requested 'application'")
				noalt = true;
			else
			{
				if (WSConstants.LOGERRORS)
					EventLogger.logEvent("GeAltServerAddress: " + errorMessage, 0);
				throw new SoapException("GeAltServerAddress: " + errorMessage, SoapException.ServerFaultCode);
			}
		}

		if (noalt)
		{
			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("GeAltServerAddress Succeed with result: " + "No alternate address", 1);
			return "No alternate address";
		}

		string result = Parser.parseAltAddress(responseText);
		if (WSConstants.LOGDETINFO)
			EventLogger.logEvent("GeAltServerAddress Succeed with result: " + result, 1);

		return result;
	}


	public static ApplicationItemEx[] GetApplicationsByCredentialsEx(Credentials credentials, string clientName, string ipAddress, string[] desiredDetails, string[] serverTypes, string[] clientTypes)
	{

		// if (credential is WindowsIdentity)
		//     return GetApplications((WindowsIdentity)credential, clientName, ipAddress, desiredDetails, serverTypes, clientTypes);



		string errorMsg = "";
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " Called API: GetApplicationsByCredentialsEx";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		// validate Credentials
		if (credentials != null)
		{


			if (credentials is WindowsCredentials)
			{
				ValidateWindowsCredential((WindowsCredentials)credentials);
			}


		}
		if (!Validator.validateAddress(ref ipAddress))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + WSConstants.INVALID_IP_ADDRESS, 0);
			throw new SoapException(WSConstants.INVALID_IP_ADDRESS, SoapException.ClientFaultCode);
		}

		//
		// Validate the serverTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateServerTypes(ref serverTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + WSConstants.INVALID_SERVER_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_SERVER_TYPE, SoapException.ClientFaultCode);
		}

		//
		// Validate the clientTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateClientTypes(ref clientTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + WSConstants.INVALID_CLIENT_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_CLIENT_TYPE, SoapException.ClientFaultCode);
		}

		//
		// Validate the desiredDetails argument, if Null set it to default "defaults"
		//
		if (!Validator.validateDesiredDetails(ref desiredDetails))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + WSConstants.INVALID_DESIRED_DETAILS, 0);
			throw new SoapException(WSConstants.INVALID_DESIRED_DETAILS, SoapException.ClientFaultCode);
		}

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancated it to 20 chars
		//
		if (clientName == null)
			clientName = string.Empty;
		else if (clientName.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 0);

			clientName = clientName.Substring(0, WSConstants.Max_CLIENTNAME_LENGTH);
		}


		ApplicationItemEx[] apps;
		string detailsTag = null;
		string serversTag = null;
		string clientsTag = null;
		string postData = "";

		detailsTag = Assembler.assembleDetails(desiredDetails);
		serversTag = Assembler.assembleServerTypes(serverTypes);
		clientsTag = Assembler.assembleClientTypes(clientTypes);

		postData = "<?xml version='1.0' encoding='utf-8'?>"
							+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
							+ "<NFuseProtocol version='4.1'>"
							+ "<RequestAppData>"
							+ "<Scope traverse='onelevel'/>"
							+ detailsTag
							+ serversTag
							+ clientsTag
							+ (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
							+ "<ClientName>" + HttpUtility.HtmlAttributeEncode(clientName) + "</ClientName>"
							+ "<ClientAddress>" + ipAddress + "</ClientAddress>"
							+ "</RequestAppData>"
							+ "</NFuseProtocol>";

		string ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
		string errorMessage = "";
		if (Parser.checkErrorResponse(ResponseText, out errorMessage))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + errorMessage, 0);
			throw new SoapException("GetApplicationsByCredentialsEx: " + errorMessage, SoapException.ServerFaultCode);
		}
		else
		{
			apps = Parser.parseAppData(ResponseText);
			if (WSConstants.LOGDETINFO)
			{
				string appDet = "";
				for (int i = 0; i < apps.Length; i++)
				{
					appDet += apps[i].FreindlyName + "\n";
				}
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + appDet, 1);
			}
			return apps;
		}
	}


	public static ApplicationItemEx GetApplicationInfoEx(string appName, Credentials credentials, string clientName, string ipAddress, string[] desiredDetails, string[] serverTypes, string[] clientTypes)
	{
		//string errorMsg = string.Empty;
		//if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfoEx : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
		//    throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		//}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " IP: " + ipAddress + " Called API: GetApplicationInfoEx";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		if (String.IsNullOrEmpty(appName))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationInfoEx: " + WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, 0);

			throw new SoapException(WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, SoapException.ClientFaultCode);
		}
		//if (credentials != null)
		//{
		//    if (credentials is WindowsCredentials)
		//   {
		//       ValidateWindowsCredential((WindowsCredentials)credentials);
		//   }
		//}
		//if (!Validator.validateAddress(ref ipAddress))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfoEx: " + WSConstants.INVALID_IP_ADDRESS, 0);

		//    throw new SoapException(WSConstants.INVALID_IP_ADDRESS, SoapException.ClientFaultCode);
		//}

		////
		//// Validate the serverTypes argument, if Null set it to default "all"
		////
		//if (!Validator.validateServerTypes(ref serverTypes))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfoEx: " + WSConstants.INVALID_SERVER_TYPE, 0);
		//    throw new SoapException(WSConstants.INVALID_SERVER_TYPE, SoapException.ClientFaultCode);
		//}

		////
		//// Validate the clientTypes argument, if Null set it to default "all"
		////
		//if (!Validator.validateClientTypes(ref clientTypes))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfoEx: " + WSConstants.INVALID_CLIENT_TYPE, 0);
		//    throw new SoapException(WSConstants.INVALID_CLIENT_TYPE, SoapException.ClientFaultCode);
		//}

		////
		//// Validate the desiredDetails argument, if Null set it to default "defaults"
		////
		//if (!Validator.validateDesiredDetails(ref desiredDetails))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfoEx: " + WSConstants.INVALID_DESIRED_DETAILS, 0);
		//    throw new SoapException(WSConstants.INVALID_DESIRED_DETAILS, SoapException.ClientFaultCode);
		//}

		////
		//// Validate the clientName argument, if it exceeds the 20 characters limit trancated it to 20 chars
		////
		//if (clientName == null)
		//    clientName = string.Empty;
		//else if (clientName.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("GetApplicationInfoEx: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 0);

		//    clientName = clientName.Substring(0, WSConstants.Max_CLIENTNAME_LENGTH);
		//}

		ApplicationItemEx[] apps = null;
		//string detailsTag = Assembler.assembleDetails(desiredDetails);
		//string serversTag = Assembler.assembleServerTypes(serverTypes);
		//string clientsTag = Assembler.assembleClientTypes(clientTypes);

		apps = GetApplicationsByCredentialsEx(credentials, clientName, ipAddress, desiredDetails, serverTypes, clientTypes);

		if (apps != null && apps.Length != 0)
		{
			for (int i = 0; i < apps.Length; i++)
				if (apps[i].InternalName.Equals(appName, StringComparison.OrdinalIgnoreCase))
				{
					if (WSConstants.LOGINFO)
						EventLogger.logEvent("GetApplicationInfoEx: Succeed with result: info about App:" + apps[i].FreindlyName, 1);
					return apps[i];
				}

		}

		if (WSConstants.LOGERRORS)
			EventLogger.logEvent("GetApplicationInfoEx: " + WSConstants.APPLICATION_IS_NOT_PUBLISHED, 0);

		throw new SoapException(WSConstants.APPLICATION_IS_NOT_PUBLISHED, SoapException.ClientFaultCode);

	}



	public static string GetApplicationIcon(string appName, int size, int colorDepth, string icoFormat, Credentials credentials)
	{

		string errorMsg = string.Empty;
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationInfoEx : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		// validate Credentials
		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " Called API: GetAppIcon";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		if (String.IsNullOrEmpty(appName))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationIcon: " + WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, 0);

			throw new SoapException(WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, SoapException.ClientFaultCode);
		}

		if (credentials != null)
		{
			if (credentials is WindowsCredentials)
			{
				ValidateWindowsCredential((WindowsCredentials)credentials);
			}
		}

		ApplicationItemEx app = GetApplicationInfoEx(appName, credentials, "", "", new string[] { "icon-info" }, new string[] { "all" }, new string[] { "all" });
		IconInfo[] ii = app.availableIcons;
		bool isSizeAvailable = false;

		for (int i = 0; i < ii.Length; i++)
		{
			IconInfo inf = ii[i];
			if (colorDepth == inf.depth && size == inf.size)
			{
				isSizeAvailable = true;
				break;
			}
		}



		//if (!((size == 32 || size == 16) && (colorDepth == 4 || colorDepth == 32)))
		if (!isSizeAvailable)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationIcon: " + WSConstants.INVALID_SIZE_OR_CLORDEPTH, 0);

			throw new SoapException(WSConstants.INVALID_SIZE_OR_CLORDEPTH, SoapException.ClientFaultCode);
		}

		if (String.IsNullOrEmpty(icoFormat) || WSConstants.imageFormats.IndexOf(icoFormat.ToLower()) < 0)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationIcon: " + WSConstants.INVALID_IMAGE_FORMAT, 0);

			throw new SoapException(WSConstants.INVALID_IMAGE_FORMAT, SoapException.ClientFaultCode);
		}

		string postData = "<?xml version='1.0' encoding='utf-8'?>"
											+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
											+ "<NFuseProtocol version='4.1'>"
											+ "<RequestCapabilities><Nil/>"
											+ "</RequestCapabilities>"
											+ "</NFuseProtocol>";
		string ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);

		if (!Parser.parseCapabilities(ResponseText))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationIcon: " + WSConstants.CAPAPILITY_NOT_SUPPORTED, 0);

			throw new SoapException(WSConstants.CAPAPILITY_NOT_SUPPORTED, SoapException.ClientFaultCode);
		}

		postData = "<?xml version='1.0' encoding='utf-8'?>"
											+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
											+ "<NFuseProtocol version='4.1'>"
											+ "<RequestAppData>"
											+ "<Scope traverse='subtree'/>"
											+ "<AppName>" + HttpUtility.HtmlAttributeEncode(appName) + "</AppName>"
											+ "<ServerType>all</ServerType>"
											+ "<ClientType>all</ClientType>"
											+ (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
											+ "<DesiredIconData size='" + size + "' bpp='" + colorDepth + "' format= 'raw' />"
											+ "</RequestAppData>"
											+ "</NFuseProtocol>";
		ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
		string errorMessage = "";

		if (Parser.checkErrorResponse(ResponseText, out errorMessage))
		{

			/*if (size == 16)
			{
					postData = "<?xml version='1.0' encoding='utf-8'?>"
										+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
										+ "<NFuseProtocol version='4.1'>"
										+ "<RequestAppData>"
										+ "<Scope traverse='subtree'/>"
										+ "<AppName>" + appName + "</AppName>"
										+ "<ServerType>all</ServerType>"
										+ "<ClientType>all</ClientType>"
										+ (credentials == null ? Assembler.assembleAllUser() : credentials.toXML(true))
										+ "<DesiredIconData size='" + "32" + "' bpp='" + colorDepth + "' format= 'raw' />"
										+ "</RequestAppData>"
										+ "</NFuseProtocol>";
					ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
					//errorMessage = "";
					if (Parser.checkErrorResponse(ResponseText, out errorMessage))
					{
							if (WSConstants.LOGERRORS)
									EventLogger.logEvent("GetApplicationIcon: " + errorMessage, 0);

							throw new SoapException("GetApplicationIcon: " + errorMessage, SoapException.ServerFaultCode);
					}
					else
					{
							string iconData = Parser.parseIconData(ResponseText);
							if (icoFormat.ToLower() == "raw")
							{
									if (WSConstants.LOGDETINFO)
											EventLogger.logEvent("GetApplicationIcon Succeed " + iconData, 1);
									return getImage(true, iconData, 32, size, icoFormat);
							}
							else
							{
									if (WSConstants.LOGDETINFO)
											EventLogger.logEvent("GetApplicationIcon Succeed " + IconMaker.makeIconEx(iconData, size, colorDepth, icoFormat), 1);
									return getImage(true, IconMaker.makeIconEx(iconData, size, colorDepth, icoFormat), 32, 16, icoFormat);
							}
					}
			}

			else
			{*/
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationIcon: " + errorMessage, 0);

			throw new SoapException("GetApplicationIcon: " + errorMessage, SoapException.ServerFaultCode);
			// }
		}
		else
		{
			bool resize = false;
			string iconData = Parser.parseIconData(ResponseText);

			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("GetApplicationIcon: Icon Data: " + iconData, 1);

			if (string.IsNullOrEmpty(iconData))
			{
				if (size == 16)
				{
					if (WSConstants.LOGDETINFO)
						EventLogger.logEvent("GetApplicationIcon: No Icon Data, try to get icon with size 32*32 and resize it ", 1);

					postData = "<?xml version='1.0' encoding='utf-8'?>"
										+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
										+ "<NFuseProtocol version='4.1'>"
										+ "<RequestAppData>"
										+ "<Scope traverse='subtree'/>"
										+ "<AppName>" + appName + "</AppName>"
										+ "<ServerType>all</ServerType>"
										+ "<ClientType>all</ClientType>"
										+ (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
										+ "<DesiredIconData size='" + "32" + "' bpp='" + colorDepth + "' format= 'raw' />"
										+ "</RequestAppData>"
										+ "</NFuseProtocol>";
					ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
					//errorMessage = "";
					if (Parser.checkErrorResponse(ResponseText, out errorMessage))
					{
						if (WSConstants.LOGERRORS)
							EventLogger.logEvent("GetApplicationIcon: " + errorMessage, 0);

						throw new SoapException("GetApplicationIcon: " + errorMessage, SoapException.ServerFaultCode);
					}
					else
						iconData = Parser.parseIconData(ResponseText);

					if (WSConstants.LOGDETINFO)
						EventLogger.logEvent("GetApplicationIcon: Icon Data (32*32): " + (iconData == null ? "Null" : ""), 1);

					resize = true;
				}
				else if (colorDepth == 32) //end if (size == 16)
				{
					if (WSConstants.LOGDETINFO)
						EventLogger.logEvent("GetApplicationIcon: No Icon Data, try to get icon with size 32*32 with color depth 4", 1);
					colorDepth = 4;
					postData = "<?xml version='1.0' encoding='utf-8'?>"
										+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
										+ "<NFuseProtocol version='4.1'>"
										+ "<RequestAppData>"
										+ "<Scope traverse='subtree'/>"
										+ "<AppName>" + appName + "</AppName>"
										+ "<ServerType>all</ServerType>"
										+ "<ClientType>all</ClientType>"
										+ (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
										+ "<DesiredIconData size='" + size + "' bpp='" + colorDepth + "' format= 'raw' />"
										+ "</RequestAppData>"
										+ "</NFuseProtocol>";
					ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
					//errorMessage = "";
					if (Parser.checkErrorResponse(ResponseText, out errorMessage))
					{
						if (WSConstants.LOGERRORS)
							EventLogger.logEvent("GetApplicationIcon: " + errorMessage, 0);

						throw new SoapException("GetApplicationIcon: " + errorMessage, SoapException.ServerFaultCode);
					}
					else
						iconData = Parser.parseIconData(ResponseText);

					if (WSConstants.LOGDETINFO)
						EventLogger.logEvent("GetApplicationIcon: Icon Data (32*32): " + (iconData == null ? "Null" : ""), 1);

				}
			} // end if (string.IsNullOrEmpty(iconData))


			if (icoFormat.ToLower() == "raw")
			{
				if (resize)
				{
					if (WSConstants.LOGDETINFO)
						EventLogger.logEvent("GetApplicationIcon, Resizeing Not supported with format 'raw' ", 1);
					return null;
				}
				return iconData;

			}
			else
			{
				//if (icoFormat.ToLower() == "ico")
				//    if (resize)
				//    {
				//        if (WSConstants.LOGDETINFO)
				//            EventLogger.logEvent("GetApplicationIcon, resizing not supporting for ico format", 1);

				//        return null;
				//    }


				iconData = getImage(resize, IconMaker.makeIconEx(iconData, (resize ? 32 : size), colorDepth, icoFormat), 32, 16, icoFormat);


				if (WSConstants.LOGDETINFO)
					EventLogger.logEvent("GetApplicationIcon succeed with Icon Data: " + iconData, 1);


				if (WSConstants.LOGINFO)
					EventLogger.logEvent("GetApplicationIcon succeed", 1);


				return iconData;
			}
		}
	}

	public static string GetApplicationIcon(ApplicationItemEx app, int size, int colorDepth, string icoFormat, Credentials credentials)
	{

		string errorMsg = string.Empty;
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationInfoEx : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		// validate Credentials
		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " Called API: GetAppIcon for: " + app.InternalName;
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		if (String.IsNullOrEmpty(app.InternalName))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationIcon: " + WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, 0);

			throw new SoapException(WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, SoapException.ClientFaultCode);
		}

		if (credentials != null)
		{
			if (credentials is WindowsCredentials)
			{
				ValidateWindowsCredential((WindowsCredentials)credentials);
			}
		}

		IconInfo[] ii = app.availableIcons;
		bool isSizeAvailable = false;

		if (ii != null)
		{
			ii = ii.OrderByDescending(x => x.depth).ToArray();
			for (int i = 0; i < ii.Length; i++)
			{
				IconInfo inf = ii[i];
				if (size == inf.size)
				{
					colorDepth = inf.depth;
					isSizeAvailable = true;
					break;
				}
				EventLogger.logEvent("GetApplicationIcon: " + string.Format("icon info format size={0} colordepth={1}", inf.size, inf.depth), 1);
			}
			
			if ((!isSizeAvailable) && (ii.Length > 0))
			{
				colorDepth = ii[0].depth;
			}
		}
		else
		{
			EventLogger.logEvent("GetApplicationIcon: " + "No avaliable icons formats present.", 0);
		}



		//if (!((size == 32 || size == 16) && (colorDepth == 4 || colorDepth == 32)))
		/*if (!isSizeAvailable)
		{
				if (WSConstants.LOGERRORS)
						EventLogger.logEvent("GetApplicationIcon: " + WSConstants.INVALID_SIZE_OR_CLORDEPTH, 0);

				throw new SoapException(WSConstants.INVALID_SIZE_OR_CLORDEPTH, SoapException.ClientFaultCode);
		}*/

		if (String.IsNullOrEmpty(icoFormat) || WSConstants.imageFormats.IndexOf(icoFormat.ToLower()) < 0)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationIcon: " + WSConstants.INVALID_IMAGE_FORMAT, 0);

			throw new SoapException(WSConstants.INVALID_IMAGE_FORMAT, SoapException.ClientFaultCode);
		}


		string postData = "<?xml version='1.0' encoding='utf-8'?>"
											+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
											+ "<NFuseProtocol version='4.1'>"
											+ "<RequestAppData>"
											+ "<Scope traverse='subtree'/>"
											+ "<AppName>" + HttpUtility.HtmlAttributeEncode(app.InternalName) + "</AppName>"
											+ "<ServerType>all</ServerType>"
											+ "<ClientType>all</ClientType>"
											+ (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
											+ "<DesiredIconData size='" + size + "' bpp='" + colorDepth + "' format= 'raw' />"
											+ "</RequestAppData>"
											+ "</NFuseProtocol>";
		string ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
		string errorMessage = "";

		if (Parser.checkErrorResponse(ResponseText, out errorMessage))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationIcon: " + errorMessage, 0);

			throw new SoapException("GetApplicationIcon: " + errorMessage, SoapException.ServerFaultCode);

		}
		else
		{
			bool resize = false;
			string iconData = Parser.parseIconData(ResponseText);

			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("GetApplicationIcon: Icon Data: " + iconData, 1);

			if (string.IsNullOrEmpty(iconData))
			{
				var colorDepthes = new int[] { 32, 16, 8, 4 };
				foreach (var depth in colorDepthes)
				{
					if (depth != colorDepth)
					{
						if (WSConstants.LOGDETINFO)
							EventLogger.logEvent("GetApplicationIcon: No Icon Data, try to get icon with size 32*32 with color depth 4", 1);
						postData = "<?xml version='1.0' encoding='utf-8'?>"
						           + "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
						           + "<NFuseProtocol version='4.1'>"
						           + "<RequestAppData>"
						           + "<Scope traverse='subtree'/>"
						           + "<AppName>" + app.InternalName + "</AppName>"
						           + "<ServerType>all</ServerType>"
						           + "<ClientType>all</ClientType>"
						           + (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
						           + "<DesiredIconData size='" + size + "' bpp='" + depth + "' format= 'raw' />"
						           + "</RequestAppData>"
						           + "</NFuseProtocol>";
						ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
						//errorMessage = "";
						if (Parser.checkErrorResponse(ResponseText, out errorMessage))
						{
							if (WSConstants.LOGERRORS)
								EventLogger.logEvent("GetApplicationIcon: " + errorMessage, 0);

							throw new SoapException("GetApplicationIcon: " + errorMessage, SoapException.ServerFaultCode);
						}
						else
						{
							iconData = Parser.parseIconData(ResponseText);
						}

						if (WSConstants.LOGDETINFO)
							EventLogger.logEvent(string.Format("GetApplicationIcon: Icon Data ({0}*{0}) colorDepth={1}: ", size, depth) + (iconData == null ? "Null" : ""), 1);
						if (!string.IsNullOrEmpty(iconData))
						{
							break;
						}
					}
				}


				if (size == 16)
				{
					if (WSConstants.LOGDETINFO)
						EventLogger.logEvent("GetApplicationIcon: No Icon Data, try to get icon with size 32*32 and resize it ", 1);

					postData = "<?xml version='1.0' encoding='utf-8'?>"
										+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
										+ "<NFuseProtocol version='4.1'>"
										+ "<RequestAppData>"
										+ "<Scope traverse='subtree'/>"
										+ "<AppName>" + app.InternalName + "</AppName>"
										+ "<ServerType>all</ServerType>"
										+ "<ClientType>all</ClientType>"
										+ (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
										+ "<DesiredIconData size='" + "32" + "' bpp='" + colorDepth + "' format= 'raw' />"
										+ "</RequestAppData>"
										+ "</NFuseProtocol>";
					ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
					//errorMessage = "";
					if (Parser.checkErrorResponse(ResponseText, out errorMessage))
					{
						if (WSConstants.LOGERRORS)
							EventLogger.logEvent("GetApplicationIcon: " + errorMessage, 0);

						throw new SoapException("GetApplicationIcon: " + errorMessage, SoapException.ServerFaultCode);
					}
					else
						iconData = Parser.parseIconData(ResponseText);

					if (WSConstants.LOGDETINFO)
						EventLogger.logEvent("GetApplicationIcon: Icon Data (32*32): " + (iconData == null ? "Null" : ""), 1);

					resize = true;
				}
			} // end if (string.IsNullOrEmpty(iconData))


			if (icoFormat.ToLower() == "raw")
			{
				if (resize)
				{
					if (WSConstants.LOGDETINFO)
						EventLogger.logEvent("GetApplicationIcon, Resizeing Not supported with format 'raw' ", 1);
					return null;
				}
				return iconData;

			}
			else
			{
				//if (icoFormat.ToLower() == "ico")
				//    if (resize)
				//    {
				//        if (WSConstants.LOGDETINFO)
				//            EventLogger.logEvent("GetApplicationIcon, resizing not supporting for ico format", 1);

				//        return null;
				//    }


				if (!string.IsNullOrEmpty(iconData))
				{
					iconData = getImage(resize, IconMaker.makeIconEx(iconData, (resize ? 32 : size), colorDepth, icoFormat), 32, 16, icoFormat);
				}


				if (WSConstants.LOGDETINFO)
					EventLogger.logEvent("GetApplicationIcon succeed with Icon Data: " + iconData, 1);


				if (WSConstants.LOGINFO)
					EventLogger.logEvent("GetApplicationIcon succeed", 1);


				return iconData;
			}
		}
	}



	private static string getImage(bool resize, string iconData, int oldSize, int newSize, string format)
	{
		try
		{
			if (resize)
			{

				byte[] imageBuffer = ImageUtility.getImageBytes(iconData);
				Image fullSizeImg = ImageUtility.createImage(imageBuffer);
				var newImage = ImageUtility.ResizeImage(16, 16, fullSizeImg, format);
				iconData = Convert.ToBase64String(newImage);
				/*switch (format.ToLower())
				{
					case "png":
						iconData = Convert.ToBase64String(ImageUtility.ConvertImageToByteArray(newImage, System.Drawing.Imaging.ImageFormat.Png));
						break;

					case "gif":
						iconData = Convert.ToBase64String(ImageUtility.ConvertImageToByteArray(newImage, System.Drawing.Imaging.ImageFormat.Gif));
						break;

					case "tiff":
						iconData = Convert.ToBase64String(ImageUtility.ConvertImageToByteArray(newImage, System.Drawing.Imaging.ImageFormat.Tiff));
						break;

					case "ico":
						iconData = Convert.ToBase64String(ImageUtility.ConvertImageToByteArray(newImage, System.Drawing.Imaging.ImageFormat.Icon));
						break;
				}*/


			}


		}
		catch (Exception ex)
		{
			if (WSConstants.LOGINFO)
				EventLogger.logEvent("getImage, error at resizing image: " + ex.Message, 1);

		}
		return iconData;

	}



	/// The method validate if the web services has license for Extentrix turbo toolbar or SAP toolbar 
	/// parameters:
	/// LicenseType     - Turbo toolbar or SAP toolbar.
	/// logInfo - logging information
	public static int ValidateLicense(int LicenseType, string logInfo)
	{

		string errorMsg = "";
		//if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("ValidateLicense : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
		//    throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		//}
		string info = "Call ValidateLicense API for LicenseType " + LicenseType + "\n" + logInfo;
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		//if (Licensing.Validation.isTrialLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES) )
		//{
		//    return 2;//trial
		//}
		// validate Credentials




		if (!Validator.validateAgainstRange(LicenseType, WSConstants.allowedLicenseTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("ValidateLicense: " + WSConstants.INVALID_LICENSE_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_LICENSE_TYPE, SoapException.ClientFaultCode);
		}

		if (!Licensing.Validation.validateLicense(ref errorMsg, LicenseType))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("ValidateLicense : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			return 0;//invalid
		}
		else
		{

			if (Licensing.Validation.isTrialLicense(ref errorMsg, LicenseType))
			{
				if (WSConstants.LOGDETINFO)
					EventLogger.logEvent("ValidateLicense :Succeed: Trial License ", 1);

				return 2;//trial
			}

			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("ValidateLicense :Succeed: Released License ", 1);
			return 1;// release
		}

		//return true;


	}


	/// The method returns the ICA file description to be used to launch an application 
	/// parameters:
	/// appName     - application name.
	/// credentials - an object contains the user credentials to authenticate his request.
	/// clientName  - the name of client's machine.
	/// ipAddress   - the IP address of the client.
	/// return value:
	/// a string represents the ICA file content
	public static string LaunchApplicationEx(string appName, Credentials credentials, string clientName, string ipAddress, ICAConnectionOptions options)
	{

		string errorMsg = "";
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LaunchApplicationEx : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " IP: " + ipAddress + " Called API: LaunchApplication";

		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		if (String.IsNullOrEmpty(appName))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LaunchApplicationEx: " + WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, 0);

			throw new SoapException(WSConstants.ONE_OR_MORE_MANDATORY_ARGUMENTS_NULL_VALUED, SoapException.ClientFaultCode);
		}
		if (credentials != null)
		{
			if (credentials is WindowsCredentials)
			{
				ValidateWindowsCredential((WindowsCredentials)credentials);
			}
		}

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancate it to 20 chars
		//
		if (clientName == null)
			clientName = string.Empty;
		else if (clientName.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("LaunchApplicationEx: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 0);

			clientName = clientName.Substring(0, WSConstants.Max_CLIENTNAME_LENGTH);
		}

		string serverAddress = string.Empty;
		string ticket = ICAGenerator.getTicket(credentials, clientName, ipAddress, appName, ref serverAddress);
		string ica = string.Empty;
		if (WSConstants.useCAG)
		{
			ica = ICAGenerator.generateICAContentWithOptionAndCAG(ticket, serverAddress, appName, credentials, clientName, options);
			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("LaunchApplicationEx: Succeed with result:  " + ica, 1);
			return ica;
		}
		else
		{
			ica = ICAGenerator.generateICAContentWithOption(ticket, serverAddress, appName, credentials, clientName, options);
			if (WSConstants.LOGDETINFO)
				EventLogger.logEvent("LaunchApplicationEx: Succeed with result:  " + ica, 1);
			return ica;
		}
	}





	public static string LaunchApplicationWithParameterEx(string appName, string parameter, Credentials credentials, string clientName, string ipAddress, ICAConnectionOptions options)
	{
		// string errorMsg = "";
		//if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("LaunchApplicationWithParameterEx : Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
		//    throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		//}

		string info = "User: " + (credentials == null ? "" : credentials.UserName) + " IP: " + ipAddress + " Called API: LaunchApplicationWithParameter";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancate it to 20 chars
		//
		//if (clientName == null)
		//    clientName = string.Empty;
		//else if (clientName.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		//{
		//    if (WSConstants.LOGERRORS)
		//        EventLogger.logEvent("LaunchApplicationWithParameterEx: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 1);

		//    clientName = clientName.Substring(0, WSConstants.Max_CLIENTNAME_LENGTH);
		//}

		if (String.IsNullOrEmpty(appName))
		{
			string ext = parameter.Substring(parameter.LastIndexOf('.'));
			if (ext == null)
			{
				if (WSConstants.LOGERRORS)
					EventLogger.logEvent("LaunchApplicationWithParameterEx: " + "File names should have an extension", 0);
				throw new SoapException("File names should have an extension", SoapException.ClientFaultCode);
			}
			else
			{
				string[] details = { "icon", "file-type", "access-list" };
				ApplicationItem[] apps = GetApplicationsByCredentials(credentials, clientName, ipAddress, details, null, null);
				for (int i = 0; i < apps.Length; i++)
					if (apps[i].fileExtentionExists(ext))
					{
						appName = apps[i].InternalName;
						break;
					}
				if (appName == null)
				{
					if (WSConstants.LOGERRORS)
						EventLogger.logEvent("LaunchApplicationWithParameterEx: " + WSConstants.FILE_EXTENTION_IS_NOT_SUPPORTED, 0);

					throw new SoapException(WSConstants.FILE_EXTENTION_IS_NOT_SUPPORTED, SoapException.ServerFaultCode);
				}
			}
		}
		string icaContent = LaunchApplicationEx(appName, credentials, clientName, ipAddress, options);

		icaContent = icaContent.Replace("InitialProgram=#" + appName, "InitialProgram=#\"" + appName + "\" " + parameter);


		if (WSConstants.LOGDETINFO)
		{
			EventLogger.logEvent("LaunchApplicationWithParameterEx:Succeed with result: " + icaContent, 1);
		}
		return icaContent;
	}


	public static ApplicationItemEx[] GetApplications(WindowsIdentity windowsIdentity, string clientName, string ipAddress, string[] desiredDetails, string[] serverTypes, string[] clientTypes)
	{
		string errorMsg = "";
		if (!Licensing.Validation.validateLicense(ref errorMsg, Licensing.Validation.WEB_SERVICES))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("validateLicense: Licensing Error, Contact Your Site Administrator " + errorMsg, 0);
			throw new Exception("Licensing Error, Contact Your Site Administrator " + errorMsg);
		}

		string info = "User: " + (windowsIdentity == null ? "" : windowsIdentity.SAM) + " Called API: GetAppsIcon";
		if (WSConstants.LOGINFO)
			EventLogger.logEvent(info, 1);

		// validate Credentials
		if (windowsIdentity != null)
		{
			//validate windows identoty
		}
		if (!Validator.validateAddress(ref ipAddress))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("validateLicense: " + WSConstants.INVALID_IP_ADDRESS, 0);
			throw new SoapException(WSConstants.INVALID_IP_ADDRESS, SoapException.ClientFaultCode);
		}

		//
		// Validate the serverTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateServerTypes(ref serverTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + WSConstants.INVALID_SERVER_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_SERVER_TYPE, SoapException.ClientFaultCode);
		}

		//
		// Validate the clientTypes argument, if Null set it to default "all"
		//
		if (!Validator.validateClientTypes(ref clientTypes))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + WSConstants.INVALID_CLIENT_TYPE, 0);
			throw new SoapException(WSConstants.INVALID_CLIENT_TYPE, SoapException.ClientFaultCode);
		}

		//
		// Validate the desiredDetails argument, if Null set it to default "defaults"
		//
		if (!Validator.validateDesiredDetails(ref desiredDetails))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + WSConstants.INVALID_DESIRED_DETAILS, 0);
			throw new SoapException(WSConstants.INVALID_DESIRED_DETAILS, SoapException.ClientFaultCode);
		}

		//
		// Validate the clientName argument, if it exceeds the 20 characters limit trancated it to 20 chars
		//
		if (clientName == null)
			clientName = string.Empty;
		else if (clientName.Length > WSConstants.Max_CLIENTNAME_LENGTH)
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + WSConstants.CLIENTNAME_EXCEEDS_LIMITS, 0);

			clientName = clientName.Substring(0, WSConstants.Max_CLIENTNAME_LENGTH);
		}


		ApplicationItemEx[] apps;
		string detailsTag = null;
		string serversTag = null;
		string clientsTag = null;
		string postData = "";

		detailsTag = Assembler.assembleDetails(desiredDetails);
		serversTag = Assembler.assembleServerTypes(serverTypes);
		clientsTag = Assembler.assembleClientTypes(clientTypes);

		postData = "<?xml version='1.0' encoding='utf-8'?>"
							+ "<!DOCTYPE NFuseProtocol SYSTEM 'NFuse.dtd'>"
							+ "<NFuseProtocol version='4.1'>"
							+ "<RequestAppData>"
							+ "<Scope traverse='onelevel'/>"
							+ detailsTag
							+ serversTag
							+ clientsTag
							+ (windowsIdentity == null ? Assembler.assembleAllUser() : windowsIdentity.ToXML())
							+ "<ClientName>" + HttpUtility.HtmlAttributeEncode(clientName) + "</ClientName>"
							+ "<ClientAddress>" + ipAddress + "</ClientAddress>"
							+ "</RequestAppData>"
							+ "</NFuseProtocol>";

		string ResponseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
		string errorMessage = "";
		if (Parser.checkErrorResponse(ResponseText, out errorMessage))
		{
			if (WSConstants.LOGERRORS)
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + errorMessage, 0);
			throw new SoapException("GetApplicationsByCredentialsEx: " + errorMessage, SoapException.ServerFaultCode);
		}
		else
		{
			apps = Parser.parseAppData(ResponseText);
			if (WSConstants.LOGDETINFO)
			{
				string appDet = "";
				for (int i = 0; i < apps.Length; i++)
				{
					appDet += apps[i].FreindlyName + "\n";
				}
				EventLogger.logEvent("GetApplicationsByCredentialsEx: " + appDet, 1);
			}
			return apps;
		}
	}


	/// <summary>
	/// validate that the EncryptionMethod is valid and the doamin type is valid
	/// </summary>
	/// <param name="credentials"></param>
	/// <returns></returns>
	public static bool IsValidWindowsCredentials(WindowsCredentials credentials, ref string errorMessage)
	{

		if (credentials != null)
		{
			if (!credentials.validateEncryptionMethod())
			{
				errorMessage = WSConstants.INVALID_ENCRYPTION_METHOD;
				return false;
			}

			if (!credentials.validateDomainType())
			{
				errorMessage = WSConstants.INVALID_DOMAIN_TYPE;
				return false;
			}
			if (string.IsNullOrEmpty(credentials.UserName))
			{
				errorMessage = WSConstants.INVALID_USERNAME;
				return false;
			}
			if (string.IsNullOrEmpty(credentials.Password))
			{
				errorMessage = WSConstants.INVALID_PASSWORD;
				return false;
			}
			if (string.IsNullOrEmpty(credentials.Domain))
			{
				errorMessage = WSConstants.INVALID_DOMAIN;
				return false;
			}
			return true;
		}
		errorMessage = "Windows Credentials is null";
		return false;
	}

	public static bool IsValidWindowsIdentity(WindowsIdentity credentials, ref string errorMessage)
	{
		if (string.IsNullOrEmpty(credentials.SAM) && string.IsNullOrEmpty(credentials.UPN) && (credentials.SIDs == null || credentials.SIDs.Length == 0))
		{
			errorMessage = "Invalid Windows Identoty";
			return false;
		}
		return true;
	}
}







