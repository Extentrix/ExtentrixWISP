using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart
{
    public class ExtentrixWebPartProperties : EditorPart
    {

        protected Label lblVersionInfo;
        protected Label lblLicenseExpirationDate;
        protected Label lblTrialLicense;

        protected override void CreateChildControls()
        {

            this.Title = "About EWISP";

           
            lblVersionInfo = new Label();
            lblLicenseExpirationDate = new Label();
            lblTrialLicense = new Label();

            lblTrialLicense.Text = "<br/>No License";

            Controls.Add(lblVersionInfo);
            Controls.Add(lblLicenseExpirationDate);
            Controls.Add(lblTrialLicense);            
        }

        public override void SyncChanges()
        {
            EnsureChildControls();
            ExtentrixWIWebPart webpart = this.WebPartToEdit as ExtentrixWIWebPart;
            // initialize textbox with UserGreeting property
            lblVersionInfo.Text = webpart.VersionInfo;
            lblLicenseExpirationDate.Text = string.Format("<br/>License Expiration Date: {0}", webpart.LicenseExpirationDate);
            lblLicenseExpirationDate.Visible = webpart.LicenseTrial == true;
            if (webpart.LicenseTrial != null)
            {
                lblTrialLicense.Text = webpart.LicenseTrial == true ? "<br/>Trial Version License" : "";
            }
            else
            {
                lblTrialLicense.Text = "<br/>No License";
            }
            
            
        }

        public override bool ApplyChanges()
        {
            EnsureChildControls();
            
            // return true to force Web Part Manager to persist changes
            return true;
        }

    }
    
}
