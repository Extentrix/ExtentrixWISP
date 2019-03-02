using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using ExtentrixWebIntrface.ContentRedirection;
using System.Collections.Generic;
using Microsoft.SharePoint.Utilities;

namespace ExtentrixWebIntrface.Layouts.ExtentrixWebIntrface.Pages
{
    public partial class ContetentRedirectionAdminPage : LayoutsPageBase
    {
        private Dictionary<string, RedirectionApplication> _fileTypes;

        protected void Page_Load(object sender, EventArgs e)
        {
            var user = SPContext.Current.Web.CurrentUser;
            if ((user != null) && user.IsSiteAdmin)
            {
            if (!IsPostBack)
            {
                    chkAlwaysGoThrough.Checked = FileTypesUtility.GetGoThroughValue();

                _fileTypes = FileTypesUtility.GetFileTypes();
                lstFileTypes.DataSource = _fileTypes.Keys;
                lstFileTypes.DataBind();

                lstFileTypes.SelectedIndex = 0;
            }
            }
            else
            {
                var source =
                    string.Format(@"Source={0}/_layouts/ExtentrixWebIntrface/Pages/ContentRedirectionAdminPage.aspx",
                                  SPContext.Current.Web.Url);
                SPUtility.Redirect("/_layouts/AccessDenied.aspx", SPRedirectFlags.Default, Context, source);
            }

        }
        protected void GoThrough_CheckedChanged(object sender, EventArgs e)
        {
            FileTypesUtility.SetGoThroughValue(chkAlwaysGoThrough.Checked);
        }
        
        protected void DeleteFileType_Button(object sender, EventArgs e)
        {
            if (lstFileTypes.SelectedIndex >= 0)
            {
                FileTypesUtility.DeleteFileType(lstFileTypes.SelectedValue);
                _fileTypes = FileTypesUtility.GetFileTypes();
                lstFileTypes.DataSource = _fileTypes.Keys;
                lstFileTypes.DataBind();
            }
        }

    }
}
