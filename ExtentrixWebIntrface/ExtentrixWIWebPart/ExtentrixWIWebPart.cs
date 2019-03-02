using System;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using System.Collections;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration.Claims;
using Microsoft.SharePoint.WebControls;
using System.Xml;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.SharePoint.Administration;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.SharePoint.WebPartPages;
using System.Threading;
using System.IO;
using Microsoft.SharePoint.Utilities;
using System.Text.RegularExpressions;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart
{
    [ToolboxItemAttribute(false)]
    public class ExtentrixWIWebPart : System.Web.UI.WebControls.WebParts.WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/ExtentrixWebIntrface/ExtentrixWIWebPart/ExtentrixWIWebPartUserControl.ascx";
        private const string _ascxErrPath = @"~/_CONTROLTEMPLATES/ExtentrixWebIntrface/ErrorPageUserControl.ascx";
        private const string _ascxLicenseValidPath = @"~/_CONTROLTEMPLATES/ExtentrixWebIntrface/LicenseValidationControl.ascx";

        //ExtentrixWebServicesForCPS service = new ExtentrixWebServicesForCPS();
        private WebServiceAPI service = null;


				private bool _isApplyDefaultClientName = true;
				[Personalizable(PersonalizationScope.Shared),
						WebBrowsable(true),
						WebDisplayName("Apply Default Client Name"),
						WebDescription("Apply Default Client Name"),
						Category("Advanced")]
				public bool ApplyDefaultClientName
				{
					get { return _isApplyDefaultClientName; }
					set { _isApplyDefaultClientName = value; }
				}

        private string _prefixClientName = "";
        [Personalizable(PersonalizationScope.Shared, false),
         WebBrowsable(true),
         WebDisplayName("Client Name prefix"),
         WebDescription("Client Name prefix"),
         EditorBrowsable(EditorBrowsableState.Never),
         Category("Advanced")]

        public string PrefixClientName
        {
            get
            {
                return _prefixClientName;
            }
            set
            { _prefixClientName = value; }
        }


        private int _mainBodyBorderWidth = 2;
        [Personalizable(PersonalizationScope.User),
         WebBrowsable(true),
         WebDisplayName("Body border width"),
         WebDescription("Set main body border width"),
         Category("Style")]
        public int MainBodyBorderWidth
        {
            get
            {
                return _mainBodyBorderWidth;
            }
            set
            {
                _mainBodyBorderWidth = value;
            }
        }


        private LogLocationEnum _logLocation = LogLocationEnum.Default;
        [Personalizable(PersonalizationScope.Shared),
         WebBrowsable(true),
         WebDisplayName("Log Location"),
         WebDescription("Specify log location: Text file or EventViewer"),
         Category("Advanced")]
        public LogLocationEnum LogLocation
        {
            get
            {
                return _logLocation;
            }
            set
            {
                _logLocation = value;
            }
        }

				private IconsExpirationPeriod _iconsExpiration = IconsExpirationPeriod.Never;
				[Personalizable(PersonalizationScope.Shared),
				 WebBrowsable(true),
				 WebDisplayName("Icons Expiration Period"),
				 WebDescription("Set icons expiration period"),
				 Category("Advanced")]
				public IconsExpirationPeriod IconsExpirationPeriod
				{
					get
					{
						return _iconsExpiration;
					}
					set
					{
						_iconsExpiration = value;
					}
				}

        private bool _isFilterApplied = false;
        [Personalizable(PersonalizationScope.Shared),
         WebBrowsable(true),
         WebDisplayName("Apply application filter"),
         WebDescription("Apply application filter"),
         Category("Advanced")]
        public bool IsAppFilterApplied
        {
            get
            {
                return _isFilterApplied;
            }
            set
            {
                _isFilterApplied = value;
            }
        }

        private string _appFilter = "";
        [Personalizable(PersonalizationScope.Shared),
         WebBrowsable(true),
         WebDisplayName("Application filter"),
         WebDescription("Set Application filter"),
         Category("Advanced")]
        public string AppFilter
        {
            get
            {
                return _appFilter;
            }
            set
            {
                _appFilter = value;
            }
        }

        private bool _isUseSecureStore = false;
        [Personalizable(PersonalizationScope.Shared),
            WebBrowsable(true),
            WebDisplayName("Use Secure Store"),
            WebDescription("Use Secure Store"),
            Category("Secure Store")]
        public bool UseSecureStore
        {
            get { return _isUseSecureStore; }
            set { _isUseSecureStore = value; }
        }

        private int _credentialLiveTime = 1;
        [Personalizable(PersonalizationScope.Shared),
            WebBrowsable(true),
            WebDisplayName("Credentials Live Time (minutes)"),
            WebDescription("Credentials Live Time in minutes"),
            Category("Secure Store")]
        public int CredentialLiveTime
        {
            get { return _credentialLiveTime; }
            set { _credentialLiveTime = value; }
        }

        private string _icaClientUrl = "http://citrix.com";
        [Personalizable(PersonalizationScope.Shared),
         WebBrowsable(true),
         WebDisplayName("ICA client URL"),
         WebDescription("Set ICA client URL"),
         Category("ICA Client")]
        public string IcaClientUrl
        {
            get
            {
                return _icaClientUrl;
            }
            set
            {
                _icaClientUrl = value;
            }
        }

        private string _icaClientVersion = "";
        [Personalizable(PersonalizationScope.Shared),
         WebBrowsable(true),
         WebDisplayName("ICA client version"),
         WebDescription("Set ICA client version"),
         Category("ICA Client")]
        public string IcaClientVersion
        {
            get
            {
                return _icaClientVersion;
            }
            set
            {
                _icaClientVersion = value;
            }
        }


				[Personalizable(PersonalizationScope.Shared),
				 WebBrowsable(true),
				 WebDisplayName("Default ICA file content"),
				 WebDescription("Default ICA file content"),
				 Category("ICA Client")]
				public string DefaultIcaFile { get; set; }

        private string _mainBodyBorderColor = "#D6E8FF";
        [Personalizable(PersonalizationScope.User),
         WebBrowsable(true),
         WebDisplayName("Body border color"),
         WebDescription("Set main body border color in html format"),
         Category("Style")]
        public string MainBodyBorderColor
        {
            get
            {
                return _mainBodyBorderColor;
            }
            set
            {
                _mainBodyBorderColor = value;
            }
        }

        private string _mainBodyBackColor = "#FFFFFF";
        [Personalizable(PersonalizationScope.User),
         WebBrowsable(true),
         WebDisplayName("Body background color"),
         WebDescription("Set main body color in html format"),
         Category("Style")]
        public string MainBodyBackColor
        {
            get
            {
                return _mainBodyBackColor;
            }
            set
            {
                _mainBodyBackColor = value;
            }
        }

        private string _toolbarBackColor = "#D6E8FF";
        [Personalizable(PersonalizationScope.User),
         WebBrowsable(true),
         WebDisplayName("Toolbar background color"),
         WebDescription("Set toolbar back color in html format"),
         Category("Style")]
        public string ToolbarBackColor
        {
            get
            {
                return _toolbarBackColor;
            }
            set
            {
                _toolbarBackColor = value;
            }
        }

				[Personalizable(PersonalizationScope.Shared),
				 WebBrowsable(true),
				 WebDisplayName("Icons"),
				 WebDescription("Show Icons View"),
				 Category("Views")]
				public bool ShowIcons { get; set; }

				[Personalizable(PersonalizationScope.Shared),
					 WebBrowsable(true),
					 WebDisplayName("List"),
					 WebDescription("Show List View"),
					 Category("Views")]
				public bool ShowList { get; set; }

				[Personalizable(PersonalizationScope.Shared),
						 WebBrowsable(true),
						 WebDisplayName("Details"),
						 WebDescription("Show Details View"),
						 Category("Views")]
				public bool ShowDetails { get; set; }

				[Personalizable(PersonalizationScope.Shared),
							 WebBrowsable(true),
							 WebDisplayName("Tree"),
							 WebDescription("Show Tree View"),
							 Category("Views")]
				public bool ShowTree { get; set; }

				[Personalizable(PersonalizationScope.Shared),
								 WebBrowsable(true),
								 WebDisplayName("Search"),
								 WebDescription("Show Search View"),
								 Category("Views")]
				public bool ShowSearch { get; set; }

				[Personalizable(PersonalizationScope.Shared),
									 WebBrowsable(true),
									 WebDisplayName("Favourites"),
									 WebDescription("Show Favourites View"),
									 Category("Views")]
				public bool ShowFavourites { get; set; }

        private string versionInfo = @"Extentrix Web Interface for SharePoint 2010<br/>
                                       Version: 2.9.0 - Build: 990<br/>
                                       Copyright: Extentrix Systems &copy;2011-2012";
        [Personalizable(PersonalizationScope.User, false),
         WebBrowsable(false)]

        public string VersionInfo
        {
            get
            {
                return versionInfo;
            }
            set
            {
            }
        }



        private string _licenseExpirationDate;
        [Personalizable(PersonalizationScope.User, false),
         WebBrowsable(false)]
        public string LicenseExpirationDate
        {
            get
            {
                return _licenseExpirationDate;
            }
            set
            { }
        }

        private bool? _isTrial = null;
        [Personalizable(PersonalizationScope.User, false),
         WebBrowsable(false)]
        public bool? LicenseTrial
        {
            get
            {
                return _isTrial;
            }
            set
            { }
        }

        protected string MacAddress
        {
            get
            {
                return ViewState["MacAddress"] != null ? ViewState["MacAddress"] as string : GetMacAdress();
            }
            set
            {
                ViewState["MacAddress"] = value;
            }
        }


        private static string GetMacAdress()
        {
            NetworkInterface[] nInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nInterface in nInterfaces)
            {
                if (nInterface.OperationalStatus == OperationalStatus.Up && nInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    var bytes = nInterface.GetPhysicalAddress().GetAddressBytes();
                    StringBuilder address = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        address.Append(bytes[i].ToString("X2"));
                        if (i != bytes.Length - 1)
                        {
                            address.Append("-");
                        }
                    }
                    return address.ToString();
                }
            }
            return null;
        }

				public ExtentrixWIWebPart()
				{
					ShowIcons = ShowList = ShowTree = ShowSearch = ShowFavourites = true;
				}

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {
                if (!Page.IsPostBack)
                {
                    MacAddress = GetMacAdress();
                }

                if (SPWebPartManager.GetCurrentWebPartManager(Page).DisplayMode != WebPartManager.BrowseDisplayMode)
                {
                    _licenseExpirationDate = LicenseManager.LicenseExpirationDate;
                    _isTrial = LicenseManager.IsTrial;
                }
            }
            catch (Exception ex)
            {
                Logger.Default.Error(LogLocation, "User - " + Page.Request.LogonUserIdentity.Name + " ; Error = " + ex.Message, ex);
            }
        }

        protected override void CreateChildControls()
        {
            Control control = null;
            try
            {
                var validationCode = LicenseManager.CheckLicense(MacAddress);
							//var validationCode = LicenseValidationCode.Ok;
                switch (validationCode)
                {
                    case LicenseValidationCode.Ok:
                        Logger.Default.Info(LogLocation, "License has been validated successeful.");                       
                        break;
                    case LicenseValidationCode.SoonBeExpired:
                        Logger.Default.Info(LogLocation, "License will expire at " + LicenseManager.LicenseExpirationDate);                        
                        break;
                    case LicenseValidationCode.Expired:
                        Logger.Default.Info(LogLocation, "License expired.");                        
                        break;
                    case LicenseValidationCode.NotValid:
                        Logger.Default.Info(LogLocation, "License is not valid.");                        
                        break;

                }
                if ((validationCode != LicenseValidationCode.NotValid)
                    && (validationCode != LicenseValidationCode.Expired))
                {
                    //if (LogLocation == LogLocationEnum.Text)
                    //    Logger.Default.Info("License has been validated successeful.");
                    //else
                    //    EventLogger.logEvent("License has been validated successeful.", 1);

                    var queryString = Page.Request.QueryString;
                    if (queryString.HasKeys() && !string.IsNullOrEmpty(queryString["EWISP"]))
                    {
                        if (queryString["EWISP"].ToString().Equals("hide"))
                        {
                            return;
                        }

                    }
										var watch = Stopwatch.StartNew();
										Logger.Default.Info(LogLocation, string.Format("Start load web part={0}", DateTime.Now));                
                    if (SPContext.Current.Web.CurrentUser != null)
                    {
                        service = new WebServiceAPI();
                        control = Page.LoadControl(_ascxPath);

                        if (control is ExtentrixWIWebPartUserControl)
                        {

                            if (validationCode == LicenseValidationCode.SoonBeExpired)
                            {
                                ((ExtentrixWIWebPartUserControl)control).LicenseExpirationDaysLeft = LicenseManager.WarningDaysNumber.ToString();
                            }

                            ((ExtentrixWIWebPartUserControl)control).MainBodyBorderWidth = MainBodyBorderWidth;

                            Color color = ColorTranslator.FromHtml(MainBodyBorderColor);
                            ((ExtentrixWIWebPartUserControl)control).MainBodyBorderColor = color;

                            color = ColorTranslator.FromHtml(MainBodyBackColor);
                            ((ExtentrixWIWebPartUserControl)control).MainBodyBackColor = color;

                            color = ColorTranslator.FromHtml(ToolbarBackColor);
                            ((ExtentrixWIWebPartUserControl)control).ToolBarBackColor = color;

                            ((ExtentrixWIWebPartUserControl)control).LogLocation = LogLocation;

                            ((ExtentrixWIWebPartUserControl)control).IsAppFilterApplied = IsAppFilterApplied;

                            ((ExtentrixWIWebPartUserControl)control).AppFilter = AppFilter;

                            ((ExtentrixWIWebPartUserControl)control).PrefixClientName = PrefixClientName;

                            ((ExtentrixWIWebPartUserControl)control).IcaClientUrl = IcaClientUrl;

                            ((ExtentrixWIWebPartUserControl)control).IcaClientVersion = IcaClientVersion;

                            ((ExtentrixWIWebPartUserControl)control).IsUseSecureStore = UseSecureStore;

                            ((ExtentrixWIWebPartUserControl)control).CredentialLiveTime = CredentialLiveTime;

                        		var views = new Dictionary<string, View>();
														if (ShowIcons)
														{
															views.Add("0", View.Icons);
														}

														if (ShowList)
														{
															views.Add("1", View.List);
														}

														if (ShowDetails)
														{
															views.Add("5", View.Details);
														}

														if (ShowTree)
														{
															views.Add("2", View.Tree);
														}

														if (ShowFavourites)
														{
															views.Add("3", View.Favorites);
														}

														if (ShowSearch)
														{
															views.Add("4", View.Search);
														}


                        		((ExtentrixWIWebPartUserControl) control).Views = views;
                        }
                        Controls.Add(control);
                    }
										watch.Stop();
										Logger.Default.Info(LogLocation, string.Format("End load web part={0}", watch.Elapsed));                

                }
                else
                {
                    _isTrial = null;
                    control = Page.LoadControl(_ascxLicenseValidPath);
                    Controls.Add(control);
										
                }
            }
            catch (ThreadAbortException)
            {
                //Do nothing
            }
            catch (Exception e)
            {
                Logger.Default.Error(LogLocation, "User - " + Page.Request.LogonUserIdentity.Name + " ; Error = " + e.Message, e);                
                //control = Page.LoadControl(_ascxErrPath);
                control = Page.LoadControl(_ascxPath);
                Controls.Add(control);
            }
        }

        public override EditorPartCollection CreateEditorParts()
        {
            EditorPart editor = new ExtentrixWebPartProperties();
            editor.ID = this.ID + "_editor";
            EditorPart[] parts = { editor };
            return new EditorPartCollection(parts);
        }

        public WebServiceAPI getService()
        {
            return service;
        }

        public string getCurrentHost()
        {
					 return ApplyDefaultClientName ? SPContext.Current.Site.HostName : "";
        }

        public PublishedApplication[] getApplication(Credentials user)
        {
            PublishedApplication[] apps = null;
					
            string clientName = "";
						if (ApplyDefaultClientName)
						{
							clientName = user.UserName;
							clientName = PrefixClientName + "_" + clientName.Replace('\\', '_');
						}
						
						apps = service.GetApplicationsByCredentials(user, clientName, "", new string[] { "defaults" }, new string[] { "all" }, new string[] { "all" });
						for (int i = 0; i < apps.Length; i++)
						{
							if (IconsManager.CheckLargeIcon(apps[i].IconFileName, IconsExpirationPeriod))
							{
								apps[i].LargeIcon = apps[i].IconFileName;
							}

							if (IconsManager.CheckSmallIcon(apps[i].IconFileName, IconsExpirationPeriod))
							{
								apps[i].SmallIcon = apps[i].IconFileName;
							}
						}
        	return apps;
        }

				public void ProcessIcons(Credentials user, WebServiceAPI service)
				{
					var watch = Stopwatch.StartNew();
					Logger.Default.Info(LogLocation, string.Format("LoadWebPartView: Start creating icons {0}", DateTime.Now));
					
					PublishedApplication[] apps = Page.Session["PublishedApps"] as PublishedApplication[];
					var iconsApp = service.GetApplicationsByCredentialsEx(user, "", "", new string[] { "icon-info" }, new string[] { "all" }, new string[] { "all" });

					if ((apps != null) && (iconsApp != null))
					{
						for (int i = 0; i < apps.Length; i++)
						{
							apps[i].LargeIcon = "";
							apps[i].SmallIcon = "";

							try
							{
								var iconInfoApp = iconsApp.FirstOrDefault(x => x.Item.InternalName.Equals(apps[i].Item.InternalName, StringComparison.OrdinalIgnoreCase));
								if (iconInfoApp != null)
								{
									if (!IconsManager.CheckLargeIcon(apps[i].IconFileName, IconsExpirationPeriod))
									{
										apps[i].LargeIcon = service.GetApplicationIcon(iconInfoApp.Item, 32, 32, "ICO", user, apps[i].FarmName);
										if (apps[i].LargeIcon != null)
										{
											IconsManager.CreateLargeIcon(apps[i]);
											apps[i].LargeIcon = apps[i].IconFileName;
										}
									}
									else
									{
										apps[i].LargeIcon = apps[i].IconFileName;
									}

									if (!IconsManager.CheckSmallIcon(apps[i].IconFileName, IconsExpirationPeriod))
									{
										apps[i].SmallIcon = service.GetApplicationIcon(iconInfoApp.Item, 16, 32, "ICO", user, apps[i].FarmName);
										if (apps[i].SmallIcon == null)
										{
											apps[i].SmallIcon = service.GetApplicationIcon(iconInfoApp.Item, 8, 32, "ICO", user, apps[i].FarmName);
										}
										if (apps[i].SmallIcon == null)
										{
											apps[i].SmallIcon = service.GetApplicationIcon(iconInfoApp.Item, 4, 32, "ICO", user, apps[i].FarmName);
										}

										if (apps[i].SmallIcon != null)
										{
											IconsManager.CreateSmallIcon(apps[i]);
											apps[i].SmallIcon = apps[i].IconFileName;
										}
									}
									else
									{
										apps[i].SmallIcon = apps[i].IconFileName;
									}
									
								}
							}
							catch (Exception ex)
							{
								Logger.Default.Error(LogLocation, "User - " + user.UserName + " ; Error = " + ex.Message, ex);
							}
						}
						Page.Session["PublishedApps"] = apps;
					}
					Logger.Default.Info(LogLocation, string.Format("LoadWebPartView: End creating icons {0}", watch.Elapsed));
				}

        public Credentials GetCredentials(SPUser user)
        {
        	System.Security.Principal.WindowsIdentity winUser = System.Security.Principal.WindowsIdentity.GetCurrent();
					if (SPClaimProviderManager.IsClaimsUser())
					{
						Logger.Default.Error(LogLocation, string.Format("Can't get applications for {0}. You should use Secure Store option for web peb part.", winUser != null ? winUser.Name : null), new ApplicationException("Claim based authentication not supported"));
					}
          return service.GetUser(winUser);
        }

        public void launchApplication(Credentials user, string application, string host, string farmName)
        {

            try
            {
                var validationCode = LicenseManager.CheckLicense(MacAddress);
							//var validationCode = LicenseValidationCode.Ok;
                switch (validationCode)
                {
                    case LicenseValidationCode.Ok:
                        Logger.Default.Info(LogLocation, "License has been validated successeful.");
                        break;
                    case LicenseValidationCode.SoonBeExpired:
                        Logger.Default.Info(LogLocation, "License will expire at " + LicenseManager.LicenseExpirationDate);
                        break;
                    case LicenseValidationCode.Expired:
                        Logger.Default.Info(LogLocation, "License expired.");
                        break;
                    case LicenseValidationCode.NotValid:
                        Logger.Default.Info(LogLocation, "License is not valid.");
                        break;
                }

                if ((validationCode != LicenseValidationCode.NotValid)
                    && (validationCode != LicenseValidationCode.Expired))
                {
                    ApplicationItem appInfo = service.GetApplicationInfo(application, user, host, "", null, null, null, farmName);
                    if (!string.IsNullOrEmpty(appInfo.ContentAddress))
                    {
                        String address = appInfo.ContentAddress;
                        if (address != null)
                            this.Page.Response.Redirect(address);
                    }
                    else
                    {
                        string app = service.LaunchApplication(application, user, host, "", farmName);
                        /*this.Page.Response.Clear();
                        this.Page.Response.ClearContent();
                        this.Page.Response.ClearHeaders();
                        this.Page.Response.ContentType = "application/x-ica";*/
                        //this.Page.Response.AddHeader("Content-disposition", "attachment; filename=launch.ica");
                        /* this.Page.Response.BinaryWrite(this.Page.Response.ContentEncoding.GetBytes(app));
                        this.Page.Response.End();*/
                        string folderPath = SPUtility.GetGenericSetupPath("ISAPI\\Extentrix");
												
												
                        var fileName = string.Format("{0}_{1}.ica", Regex.Replace(SPContext.Current.Web.CurrentUser.LoginName, @"[#:.\\/|]", "_"), application);
                        var path = Path.Combine(folderPath, fileName);
												
                    		using (StreamWriter icaFile = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.ReadWrite)))
                        {
                            icaFile.Write(app);
														if (!string.IsNullOrEmpty(DefaultIcaFile))
														{
															icaFile.Write(DefaultIcaFile);
														}
                            icaFile.Flush();
                            icaFile.Close();
                        }

                        this.Page.Response.Redirect("/_vti_bin/Extentrix/" + fileName);
                    }

                }
                else
                {
                    Controls.Add(Page.LoadControl(_ascxLicenseValidPath));
                }
            }
            catch (ThreadAbortException)
            {
                //Do nothing
            }
            catch (Exception e)
            {
                Logger.Default.Error(LogLocation, "User - " + Page.Request.LogonUserIdentity.Name + " ; Error = " + e.Message, e);
                Controls.Add(Page.LoadControl(_ascxErrPath));
                //Controls.Add(Page.LoadControl(_ascxPath));
            }
        }

        public void DisconnectSessions(Credentials user)
        {
            //Substituite missed parameters
            try
            {
                service.DisconnectSessions(0, null, user, new String[] { "all" }, new String[] { "all" });
            }
            catch (Exception e)
            {
                Logger.Default.Error(LogLocation, "User - " + Page.Request.LogonUserIdentity.Name + " ; Error = " + e.Message, e);

                Controls.Add(Page.LoadControl(_ascxErrPath));
            }
        }
    }
}
