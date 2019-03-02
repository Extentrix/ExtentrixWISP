using System;
using System.Linq;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using ExtentrixWebIntrface.ContentRedirection;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;
using Microsoft.SharePoint.Administration;

namespace ExtentrixWebIntrface.Layouts.ExtentrixWebIntrface.Pages
{
    public partial class NewDocumentApp : LayoutsPageBase
    {
        protected static Logger PageLogger = Logger.GetLogger("Pages");

        #region Session consts
        public const string PublishedApps = "PublishedApps";
        #endregion Session consts
        private PublishedApplication[] _apps;

        const string ActionKey = "Action";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SPContext.Current.IsPopUI)
            {

                var queryString = Request.QueryString;

                try
                {
                    string action = queryString[ActionKey];

                    if (action == "New")
                    {
                        if (!Page.IsPostBack)
                        {
                            var applications = FileTypesUtility.GetAllApplications();
                            foreach (var key in applications.Keys)
                            {
                                var app = applications[key];
                                ApplicationsList.Items.Add(new ListItem(app.DisplayName, app.InternalName));
                            }
                            ApplicationsList.Items[0].Selected = true;
                        }
                    }
                    else if (action == "Edit")
                    {
                        if (Page.Session[PublishedApps] == null)
                        {
                            _apps = FileTypesUtility.GetPublishedApplications(Page.Request.LogonUserIdentity);
                            Page.Session[PublishedApps] = _apps;
                        }
                        else
                        {
                            _apps = Page.Session[PublishedApps] as PublishedApplication[];
                        }

                        if (!Page.IsPostBack)
                        {
                            foreach (var app in _apps)
                            {
                                ApplicationsList.Items.Add(new ListItem(app.Item.FreindlyName, app.AppName));
                            }
                            ApplicationsList.Items[0].Selected = true;
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                    PageLogger.Error("An error occured during contetent redirection", ex);
                }

            }
        }
        protected void OkButton_Clicked(object sender, EventArgs e)
        {
            string appName = ApplicationsList.SelectedValue;
            string action = Request.QueryString[ActionKey];
            string script = string.Format("<script type='text/javascript'>window.frameElement.commonModalDialogClose('{0};{1}', 1);</script>", action, appName);
            if (action == "Edit")
            {
                PublishedApplication publishedApp = _apps.First(app => app.AppName == appName);
                script = string.Format("<script type='text/javascript'>window.frameElement.commonModalDialogClose('{0};{1};{2}', 1);</script>", action, appName, publishedApp.FarmName);
            }
            CloseModalDialog(script);
        }


        private void CloseModalDialog(string script)
        {
            if ((SPContext.Current != null) && SPContext.Current.IsPopUI)
            {

                Context.Response.Write(script);
                Context.Response.Flush();
                Context.Response.End();
            }
        }
    }
}
