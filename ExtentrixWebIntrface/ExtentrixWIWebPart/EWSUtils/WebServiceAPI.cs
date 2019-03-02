using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using Microsoft.SharePoint.Administration;


namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{

    public class WebServiceAPI
    {
        private Farms farms;
        // ExtentrixWebServicesForCPS proxy = new ExtentrixWebServicesForCPS();        
        public WebServiceAPI()
        {
            farms = new Farms();
        }

        public string LaunchApplication(string appName, Credentials credentials, string clientName, string ipAddress, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.LaunchApplication(appName, credentials, clientName, ipAddress);
        }

        public PublishedApplication[] GetApplicationsByCredentials(Credentials credentials, string clientName, string ipAddress, string[] desiredDetails, string[] serverTypes, string[] clientTypes)
        {
            ApplicationItem[] items = null;
            List<PublishedApplication> apps = new List<PublishedApplication>();
            foreach (Farm farm in farms)
            {
                WSConstants.CTX_XML = farm.Url;
                WSConstants.CTXXMLPort = farm.Port;

                items = WebService.GetApplicationsByCredentials(credentials, clientName, ipAddress, desiredDetails, serverTypes, clientTypes);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        PublishedApplication app = new PublishedApplication();
                        app.FarmName = farm.Name;
                        app.Item = item;
                        apps.Add(app);
                    }
                }

            }
            return apps.ToArray();
        }

        public string LaunchApplicationWithParameter(string appName, string parameter, Credentials credentials, string clientName, string ipAddress, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.LaunchApplicationWithParameter(appName, parameter, credentials, clientName, ipAddress);
        }

        public string GetCodebaseURL(string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.GetCodebaseURL();
        }

        public ApplicationItem GetApplicationInfo(string appName, Credentials credentials, string clientName, string ipAddress, string[] desiredDetails, string[] serverTypes, string[] clientTypes, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.GetApplicationInfo(appName, credentials, clientName, ipAddress, desiredDetails, serverTypes, clientTypes);
        }

        public bool ValidateCredentials(Credentials credentials, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.ValidateCredentials(credentials);
        }

        public bool IsApplicationPublished(string appName, Credentials credentials, string clientName, string ipAddress, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.IsApplicationPublished(appName, credentials, clientName, ipAddress);
        }

        public ICASession[] GetDiscSessions(int idMethod, string clientID, Credentials credentials, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.GetDiscSessions(idMethod, clientID, credentials);
        }

        public bool DisconnectSessions(int idMethod, string clientID, Credentials credentials, string[] serverTypes, string[] clientTypes)
        {
            bool res = true;
            foreach (Farm farm in farms)
            {
                WSConstants.CTX_XML = farm.Url;
                WSConstants.CTXXMLPort = farm.Port;
                res = WebService.DisconnectSessions(idMethod, clientID, credentials, serverTypes, clientTypes);
            }
            return res;
        }

        public bool LogoffSessions(int idMethod, string clientID, Credentials credentials, string[] serverTypes, string[] clientTypes, string farmName)
        {
            bool res = true;
            foreach (Farm farm in farms)
            {
                WSConstants.CTX_XML = farm.Url;
                WSConstants.CTXXMLPort = farm.Port;
                res = WebService.LogoffSessions(idMethod, clientID, credentials, serverTypes, clientTypes);
            }
            return res;
        }

        public PresentationServer[] GetServers(string[] serverTypes, string[] clientTypes, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.GetServers(serverTypes, clientTypes);
        }

        public string GetAltServerAddress(string serverName, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.GetAltServerAddress(serverName);
        }

        public PublishedApplicationEx[] GetApplicationsByCredentialsEx(Credentials credentials, string clientName, string ipAddress, string[] desiredDetails, string[] serverTypes, string[] clientTypes)
        {
            ApplicationItemEx[] items = null;
            List<PublishedApplicationEx> apps = new List<PublishedApplicationEx>();
            foreach (Farm farm in farms)
            {
                WSConstants.CTX_XML = farm.Url;
                WSConstants.CTXXMLPort = farm.Port;

                items = WebService.GetApplicationsByCredentialsEx(credentials, clientName, ipAddress, desiredDetails, serverTypes, clientTypes);

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        PublishedApplicationEx app = new PublishedApplicationEx();                            
                        app.Item = item;
                        apps.Add(app);
                    }
                }
            }
            return apps.ToArray();
        }

        public bool IsValidWindowsCredentials(WindowsCredentials windowsCredentials, ref string errorMsg, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.IsValidWindowsCredentials(windowsCredentials, ref errorMsg);
        }

        public bool IsValidWindowsIdentity(WindowsIdentity windowsIdentity, ref string errorMsg, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.IsValidWindowsIdentity(windowsIdentity, ref errorMsg);
        }

        public ApplicationItemEx GetApplicationInfoEx(string appName, Credentials credentials, string clientName, string ipAddress, string[] desiredDetails, string[] serverTypes, string[] clientTypes, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.GetApplicationInfoEx(appName, credentials, clientName, ipAddress, desiredDetails, serverTypes, clientTypes);
        }

        public string GetApplicationIcon(string appName, int size, int colorDepth, string format, Credentials credentials, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.GetApplicationIcon(appName, size, colorDepth, format, credentials);
        }

        public string GetApplicationIcon(ApplicationItemEx app, int size, int colorDepth, string format, Credentials credentials, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.GetApplicationIcon(app, size, colorDepth, format, credentials);
        }

        public int ValidateLicense(int licenseType, string logInfo, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.ValidateLicense(licenseType, logInfo);

        }

        public string LaunchApplicationEx(string appName, Credentials credentials, string clientName, string ipAddress, ICAConnectionOptions options, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.LaunchApplicationEx(appName, credentials, clientName, ipAddress, options);
        }

        public string LaunchApplicationWithParameterEx(string appName, string parameter, Credentials credentials, string clientName, string ipAddress, ICAConnectionOptions options, string farmName)
        {
            WSConstants.CTX_XML = farms[farmName].Url;
            WSConstants.CTXXMLPort = farms[farmName].Port;
            return WebService.LaunchApplicationWithParameterEx(appName, parameter, credentials, clientName, ipAddress, options);
        }

        public bool IsCredentialsValid(WindowsCredentials credentials)
        {

            bool res = false;
            foreach (Farm farm in farms)
            {
                WSConstants.CTX_XML = farm.Url;
                WSConstants.CTXXMLPort = farm.Port;
                try
                {
                    res = WebService.DisconnectSessions(0, null, credentials, new String[] { "all" }, new String[] { "all" });
                }
                catch (Exception e)
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, e.Message, e.StackTrace);
                    //do nothing
                }
                if (res)
                {
                    return res;
                }
            }
            return res;
        }

        
        public WindowsIdentity GetUser(System.Security.Principal.WindowsIdentity user)
        {

            //Create Extentrix Windows Identity object.
            WindowsIdentity identity = new WindowsIdentity();

            //Use name domain
            string[] dn = user.Name.Split(new char[] { '\\' });
            if (dn.Length > 1)
            {
                identity.Domain = dn[0];
                identity.UserName = dn[1];
            }

            identity.UPN = user.Name;
            // set the values for web services windows identity
            identity.SAM = user.Name;

            // get all the groups that the user belongs to.                   
            System.Security.Principal.IdentityReferenceCollection irc = user.Groups;

            // define list to contains groups
            ArrayList list = new ArrayList();

            // Add the SID for each group
            foreach (System.Security.Principal.IdentityReference ir in irc)
            {
                list.Add(ir.Value);
            }

            // Add the SID for the current login user
            list.Add(user.User.Value);

            // assgine the collected SID list (user and the groups belongs to).
            identity.SIDs = (string[])list.ToArray(typeof(string));

            return identity;
        }


    }
}
