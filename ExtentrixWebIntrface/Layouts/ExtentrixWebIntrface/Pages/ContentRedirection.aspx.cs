using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Threading;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;
using System.Collections;
using ExtentrixWebIntrface.ContentRedirection;
using Microsoft.SharePoint.Administration;

namespace ExtentrixWebIntrface.Layouts.ExtentrixWebIntrface
{
    public partial class ContentRedirection : LayoutsPageBase
    {
        protected static Logger PageLogger = Logger.GetLogger("Pages");
        
        const string ActionKey = "Action";
        const string DocumentAppKey = "App";
        const string DocumentFarmKey = "FarmName";
        const string FileTypeKey = "FileType";
        const string FileUrlKey = "FileUrl";

        private WebServiceAPI _service;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                _service = new WebServiceAPI();                  
                var queryString = Request.QueryString;

                try
                {
                    string action = queryString[ActionKey];

                    switch (action)
                    {
                        case "New":
                            string appName = queryString[DocumentAppKey];
                        var app = FileTypesUtility.GetApplicationByName(appName);
                        LaunchApplication(Page.Request.LogonUserIdentity, GetCurrentHost(), app);
                            break;
                        case "Edit":
                        string fileType = queryString[FileTypeKey];
                            app = FileTypesUtility.GetApplicationByFileType(fileType);
                        
                        string fileUrl = queryString[FileUrlKey];
                        LaunchApplicationWithParameter(Page.Request.LogonUserIdentity, GetCurrentHost(), app, fileUrl);
                            break;
                        case "EditThrough":
                            appName = queryString[DocumentAppKey];
                            string farmName = queryString[DocumentFarmKey];

                            app = new RedirectionApplication() {InternalName = appName, FarmName = farmName};
                            fileUrl = queryString[FileUrlKey];
                            LaunchApplicationWithParameter(Page.Request.LogonUserIdentity, GetCurrentHost(), app, fileUrl);
                            break;
                    }
                    
                  

                }
                catch (ThreadAbortException)
                {
                    //Do nothing
                }
                catch (Exception ex)
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                    PageLogger.Error("An error occured during contetent redirection", ex);
                }
            }


        }

       
        public void LaunchApplication(System.Security.Principal.WindowsIdentity user, string host, RedirectionApplication redirectApp)
        {
            try
            {

                ApplicationItem appInfo = _service.GetApplicationInfo(redirectApp.InternalName, _service.GetUser(user), host, "", null, null, null, redirectApp.FarmName);
                if (!string.IsNullOrEmpty(appInfo.ContentAddress))
                {
                    String address = appInfo.ContentAddress;
                    if (address != null)
                        this.Page.Response.Redirect(address);
                }
                else
                {

                    string app = _service.LaunchApplication(redirectApp.InternalName, _service.GetUser(user), host, "", redirectApp.FarmName);
                    this.Page.Response.Clear();
                    this.Page.Response.ClearContent();
                    this.Page.Response.ClearHeaders();
                    this.Page.Response.ContentType = "application/x-ica";
                    this.Page.Response.AddHeader("Content-Type", "application/x-ica");
                    this.Page.Response.BinaryWrite(this.Page.Response.ContentEncoding.GetBytes(app));
                    this.Page.Response.End();
                }
            }
            catch (ThreadAbortException)
            {
                //Do nothing
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                PageLogger.Error("An error occured during launch application", ex);
            }

        }

        public void LaunchApplicationWithParameter(System.Security.Principal.WindowsIdentity user, string host, RedirectionApplication redirectApp, string fileUrl)
        {
            try
            {

                ApplicationItem appInfo = _service.GetApplicationInfo(redirectApp.InternalName, _service.GetUser(user), host, "", null, null, null, redirectApp.FarmName);
                if (!string.IsNullOrEmpty(appInfo.ContentAddress))
                {
                    String address = appInfo.ContentAddress;
                    if (address != null)
                        this.Page.Response.Redirect(address);
                }
                else
                {

                    string app = _service.LaunchApplicationWithParameter(redirectApp.InternalName, fileUrl, _service.GetUser(user), host, "", redirectApp.FarmName);
                    this.Page.Response.Clear();
                    this.Page.Response.ClearContent();
                    this.Page.Response.ClearHeaders();
                    this.Page.Response.ContentType = "application/x-ica";
                    this.Page.Response.AddHeader("Content-Type", "application/x-ica");
                    this.Page.Response.BinaryWrite(this.Page.Response.ContentEncoding.GetBytes(app));
                    this.Page.Response.End();
                }
            }
            catch (ThreadAbortException)
            {
                //Do nothing
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                PageLogger.Error("An error occured during launch application", ex);
            }

        }

        public string GetCurrentHost()
        {
            return SPContext.Current.Site.HostName;
        }
        
    }
}
