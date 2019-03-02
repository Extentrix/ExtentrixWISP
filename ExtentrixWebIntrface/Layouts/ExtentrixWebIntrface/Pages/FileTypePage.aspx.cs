using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using ExtentrixWebIntrface.ContentRedirection;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;
using System.Collections.Generic;
using System.Linq;

namespace ExtentrixWebIntrface.Layouts.ExtentrixWebIntrface.Pages
{
    public partial class FileTypePage : LayoutsPageBase
    {
        #region Session consts
        public const string PublishedApps = "PublishedApps";
        #endregion Session consts
        private PublishedApplication[] _apps;
        protected void Page_Load(object sender, EventArgs e)
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
            
            if (!IsPostBack)
            {
                dropDownAppName.DataSource = _apps;
                dropDownAppName.DataTextField = "AppName";
                dropDownAppName.DataBind();
            }
        }
    

        protected void Save_OnClick(object sender, EventArgs e)
        {
            string appName = dropDownAppName.SelectedValue;
            PublishedApplication publishedApp = _apps.First(app => app.AppName == appName);
            RedirectionApplication redirectionApp = new RedirectionApplication()
                                                        {
                                                            InternalName = publishedApp.AppName,
                                                            DisplayName = publishedApp.Item.FreindlyName,
                                                            FarmName = publishedApp.FarmName
                                                        };
            FileTypesUtility.AddFileType(txtFileType.Text, redirectionApp);

            string script = "<script type='text/javascript'>window.frameElement.commitPopup('1');</script>";
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
