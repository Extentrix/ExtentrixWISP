using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using Microsoft.Web.Administration;
using System.IO;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;
using System.Collections.Generic;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Threading;
using System.Text;
using System.Web.UI.HtmlControls;


namespace ExtentrixWebIntrface.ExtentrixWIWebPart
{
    public enum View
    {
        Undefined,
        Icons,
        List,
        Details,
        Tree,
        Favorites,
        Search
    }


    public delegate PublishedApplication[] DelegateGetApplications(Credentials user);

    public partial class ExtentrixWIWebPartUserControl : UserControl
    {
        #region Session consts
        public const string CurrentFolder = "CurrentFolder";
        public const string PublishedApps = "PublishedApps";
        #endregion

        ApplicationsTree tree;
        Favorites favorites;
        //       PublishedApplication[] apps;


        public int MainBodyBorderWidth
        {
            get;
            set;
        }

        public Color MainBodyBorderColor
        {
            get;
            set;
        }

        public Color MainBodyBackColor
        {
            get;
            set;
        }

        public Color ToolBarBackColor
        {
            get;
            set;
        }

        public string PrefixClientName
        {
            get;
            set;
        }

        public LogLocationEnum LogLocation
        {
            get;
            set;
        }

        public bool IsAppFilterApplied
        {
            get;
            set;
        }

        public bool IsUseSecureStore
        {
            get;
            set;
        }

        public int CredentialLiveTime
        {
            get;
            set;
        }

        public string AppFilter
        {
            get;
            set;
        }

        public string IcaClientUrl
        {
            get;
            set;
        }

        public string IcaClientVersion
        {
            get;
            set;
        }

        public string LicenseExpirationDaysLeft
        {
            get;
            set;
        }

				public Dictionary<string, View> Views { get; set; }
				
        protected Credentials Credentials
        {
            get
            {
                return ViewState["Credentials"] != null ? ViewState["Credentials"] as Credentials : null;
            }
            set
            {
                ViewState["Credentials"] = value;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {

            ExtentrixWIWebPart wp = getWebPartCurrentInstance();

            //set appearance properties
            MainBodyPanel.BorderWidth = MainBodyBorderWidth;

            MainBodyPanel.BorderColor = MainBodyBorderColor;
            MainBodyPanel.BackColor = MainBodyBackColor;
            ToolBarPanel.BackColor = ToolBarBackColor;

            linkIcaClientUrl.DataBind();
            fldIcaClientVersion.DataBind();

            if (!Page.IsPostBack)
            {
                fldShowExpirationMessage.DataBind();
            }
            else
            {
                fldShowExpirationMessage.Value = "";
            }

            //Page.Response.Cache.SetNoServerCaching();

            //to create current folder Application
            if (Page.Session[CurrentFolder] == null)
            {
                Page.Session[CurrentFolder] = string.Empty;
            }

						//fill view switcher
						if (!Page.IsPostBack)
						{
							FillViewSwitcher();
						}

        	var queryString = Page.Request.QueryString;
            if (queryString.HasKeys()
                && !string.IsNullOrEmpty(queryString["app"])
                && !string.IsNullOrEmpty(queryString["farmName"]))
            {
                // Lunch application passed by query string                 
                string id = queryString["app"].ToString();
                string frmName = queryString["farmName"].ToString();
                string hostName = wp.getCurrentHost();

                if (!IsUseSecureStore)
                {
                    wp.launchApplication(wp.GetCredentials(SPContext.Current.Web.CurrentUser), id, hostName, frmName);
                }
                else
                {
                    var credentials = Credentials ?? SecureStoreManager.GetExtentrixWindowsCredentials(Page, LogLocation, SPContext.Current.Web.CurrentUser);
                    if (credentials != null)
                    {
                        if (CredentialLiveTime != -1)
                        {
														if (!SecureStoreManager.IsCredentialsExpired(CredentialLiveTime))
                            {
                                wp.launchApplication(credentials, id, hostName, frmName);
                            }
                            else
                            {
                                SecureStoreManager.DeleteExtentrixWindowsCredentials(Page, LogLocation, SPContext.Current.Web.CurrentUser);
                                LoadCredentialsView(true);
                            }
                        }
                        else
                        {
                            var isValid = true;
                            if (!Page.IsPostBack)
                            {
                                var service = new WebServiceAPI();
                                if (service.IsCredentialsValid((WindowsCredentials)credentials))
                                {
                                    Credentials = credentials;
                                }
                                else
                                {
                                    isValid = false;
                                    SecureStoreManager.DeleteExtentrixWindowsCredentials(Page, LogLocation, SPContext.Current.Web.CurrentUser);
                                }
                            }
                            if (isValid)
                            {
															wp.launchApplication(Credentials, id, hostName, frmName);
                            }
                            else
                            {
                                LoadCredentialsView(false);
                            }
                        }
                    }
                    else
                    {
											LoadCredentialsView(SecureStoreManager.IsCredentialsExpired(CredentialLiveTime));
                    }
                }

                return;
            }
            else
            {
                try
                {
                    if (!IsUseSecureStore)
                    {
                        if (Credentials == null)
                        {
													Credentials = wp.GetCredentials(SPContext.Current.Web.CurrentUser);
                        }
                        LoadWebPartView(wp);
                    }
                    else
                    {
                        var credentials = Credentials ?? SecureStoreManager.GetExtentrixWindowsCredentials(Page, LogLocation, SPContext.Current.Web.CurrentUser);
                        if (credentials != null)
                        {
                            if (CredentialLiveTime != -1)
                            {
															  if (!SecureStoreManager.IsCredentialsExpired(CredentialLiveTime))
                                {
                                    Credentials = credentials;
                                    LoadWebPartView(wp);
                                }
                                else
                                {
                                    SecureStoreManager.DeleteExtentrixWindowsCredentials(Page, LogLocation, SPContext.Current.Web.CurrentUser);
                                    LoadCredentialsView(true);
                                }
                            }
                            else
                            {
                                var isValid = true;
                                if (!Page.IsPostBack)
                                {
                                    var service = new WebServiceAPI();
                                    if (service.IsCredentialsValid((WindowsCredentials)credentials))
                                    {
                                        Credentials = credentials;
                                    }
                                    else
                                    {
                                        isValid = false;
                                        SecureStoreManager.DeleteExtentrixWindowsCredentials(Page, LogLocation, SPContext.Current.Web.CurrentUser);
                                    }
                                }
                                if (isValid)
                                {
                                    LoadWebPartView(wp);
                                }
                                else
                                {
                                    LoadCredentialsView(false);
                                }
                            }
                        }
                        else
                        {
													LoadCredentialsView(SecureStoreManager.IsCredentialsExpired(CredentialLiveTime));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Default.Error(LogLocation, "User - " + Page.Request.LogonUserIdentity.Name + " ; Error = " + ex.Message, ex);
                    AppMultiView.ActiveViewIndex = 5;
                }

            }

        }

        private void LoadCredentialsView(bool isExpired)
        {
            lblCredentialsExpired.Visible = isExpired;
            WebPartPanel.Visible = false;
            CredentialsPanel.Visible = true;
        }

        protected void ValidateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    LoadWebPartView(getWebPartCurrentInstance());
                    WebPartPanel.Visible = true;
                    CredentialsPanel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Default.Error(LogLocation, "User - " + Page.Request.LogonUserIdentity.Name + " ; Error = " + ex.Message, ex);
                AppMultiView.ActiveViewIndex = 5;
            }
        }

        protected void OnValidateCredentials(object source, ServerValidateEventArgs args)
        {
            try
            {
                var service = new WebServiceAPI();
                var credentials = new WindowsCredentials()
                {
                    WinUserName = txtUserName.Text,
                    UserName = txtUserName.Text,
                    Password = txtPassword.Text,
                    WinDomain = txtDomain.Text,
                    Domain = txtDomain.Text
                };
                args.IsValid = service.IsCredentialsValid(credentials);
                if (args.IsValid)
                {
                    SecureStoreManager.SetExtentrixWindowsCredentials(LogLocation, SPContext.Current.Web.CurrentUser, credentials);
                    SecureStoreManager.SetCredentialsLiveTime(CredentialLiveTime);
                    Credentials = credentials;
                }
            }
            catch (Exception ex)
            {
                Logger.Default.Error(LogLocation, "User - " + Page.Request.LogonUserIdentity.Name + " ; Error = " + ex.Message, ex);
                AppMultiView.ActiveViewIndex = 5;
            }
        }

        private void LoadWebPartView(ExtentrixWIWebPart wp)
        {
            //if (Page.Session[PublishedApps] == null)
						PublishedApplication[] publishedApps = Page.Session[PublishedApps] as PublishedApplication[];
						if (!Page.IsPostBack && publishedApps == null)
            {
								var watch = Stopwatch.StartNew();
								Logger.Default.Info(LogLocation, string.Format("LoadWebPartView: Start getting applications {0}", DateTime.Now));
                DelegateGetApplications delGetApp = new DelegateGetApplications(wp.getApplication);
                
                IAsyncResult result = delGetApp.BeginInvoke(Credentials, null, null);

                publishedApps = delGetApp.EndInvoke(result);

								Logger.Default.Info(LogLocation, string.Format("LoadWebPartView: End getting applications {0}", watch.Elapsed));
                //check is app filter applied

								if ((publishedApps != null) && (publishedApps.Length > 0))
								{
									Page.Session[PublishedApps] = publishedApps;
									new Thread(() => wp.ProcessIcons(Credentials, wp.getService())).Start();
									
								}
            }
					
						if (publishedApps != null)
						{
							if (IsAppFilterApplied)
							{
								var appsFiltered = new List<PublishedApplication>();
								string[] filter = AppFilter.Split(',');
								for (int i = 0; i < filter.Length; i++)
								{
									foreach (PublishedApplication app in publishedApps)
									{
										if (app.AppName.ToLower().Contains(filter[i].ToLower()))
										{
											appsFiltered.Add(app);
										}
									}
								}
								publishedApps = appsFiltered.ToArray();
							}

							Logger.Default.Info(LogLocation, "LoadWebPartView: Start creating app tree");
							tree = new ApplicationsTree(publishedApps);
							Logger.Default.Info(LogLocation, "LoadWebPartView: End creating app tree");
							if (SPContext.Current.Web.CurrentUser != null)
							{
								favorites = new Favorites(tree);
							}
							
						}

        		if (SPContext.Current.Web.CurrentUser == null)
            {
                DropDownViewSwitcher.Items.Remove(DropDownViewSwitcher.Items.FindByText("Favorites"));                
            }
						if ((publishedApps != null) && (publishedApps.Length > 0))
						{
							FillView(GetCurrentView(DropDownViewSwitcher.SelectedValue));
						}
        }

        private View GetCurrentView(string dropDownValue)
        {
            switch (dropDownValue)
            {
                case "0":
                    return View.Icons;
                case "1":
                    return View.List;
                case "5":
										return View.Details;
                case "2":
										return View.Tree;
                case "3":
										return View.Favorites;
                case "4":
										return View.Search;   
                default:
                    return View.Icons;
            }
        }

				private void FillViewSwitcher()
				{
					if (Views != null)
					{
						string firstKey = "0";
						bool setFirstKey = false;
						foreach (var view in Views)
						{
								DropDownViewSwitcher.Items.Add(new ListItem(view.Value.ToString("G"), view.Key));
								if (!setFirstKey)
								{
									firstKey = view.Key;
									setFirstKey = true;
								}
						}
						switcher.Visible = Views.Count > 1;
						searchToolbar.Visible = Views.ContainsKey("4");
						AppMultiView.ActiveViewIndex = int.Parse(firstKey);
					}
					else
					{
						DropDownViewSwitcher.Items.Add(new ListItem("0", View.Icons.ToString("G")));
					}
					
				}

        private void FillView(View view)
        {
					var watch = Stopwatch.StartNew();
					Logger.Default.Info(LogLocation, string.Format("FillView: Start render {0}", DateTime.Now));
            string currentFolderPath = (string)Page.Session[CurrentFolder];
            Folder currentFolder = string.IsNullOrEmpty(currentFolderPath) ? tree.Root : tree.GetFolder(currentFolderPath);

            CreateBreadCrumb(currentFolderPath);


            if ((view == View.Icons) || (view == View.List)
                || (view == View.Details))
            {
                HomeImageButton.Visible = true;
                UpImageButton.Visible = true;
            }
            else
            {
                HomeImageButton.Visible = false;
                UpImageButton.Visible = false;
            }

            if (view == View.Favorites)
            {
                FavoritesConfiguration.Visible = true;
            }
            else
            {
                FavoritesConfiguration.Visible = false;
            }

            if ((view == View.Favorites) || (view == View.Search))
            {
                BreadCrumbsTable.Visible = false;
            }
            else
            {
                BreadCrumbsTable.Visible = true;
            }
            switch (view)
            {
                case View.Icons:

                    FillIconsTable(currentFolderPath);
                    break;
                case View.List:

                    FillListTable(currentFolderPath);
                    break;

                case View.Tree:
                    FillTreeView(currentFolderPath);
                    break;

                case View.Favorites:
                    if (SPContext.Current.Web.CurrentUser != null)
                    {
                        bool isConfigurationMode = FavoritesConfigurationChoice.SelectedValue == "1";
                        FillFavoritesView(isConfigurationMode);
                    }
                    break;

                case View.Search:
                    FillSearchView(SearchTextBox.Text);
                    break;               
                case View.Details:
                    FillListTable(currentFolderPath);
                    break;
                default:
                    break;
            }
						watch.Stop();
						Logger.Default.Info(LogLocation, string.Format("FillView: End render {0}", watch.Elapsed));
        }


        private void FillFavoritesView(bool isConfigurationView)
        {
            if (SPContext.Current.Web.CurrentUser == null)
                return;
            try
            {
                FavoritesTable.Rows.Clear();
                List<PublishedApplication> appFavorites = favorites.GetFavoritesApps();
                foreach (PublishedApplication app in appFavorites)
                {
                    AddFavoriteToTable(app, isConfigurationView);
                }

                if (isConfigurationView)
                {
                    NotFavoritesLabel.Visible = true;
                    NotFavoritesTable.Visible = true;
                    NotFavoritesTable.Rows.Clear();
                    List<PublishedApplication> notAppFavorites = new List<PublishedApplication>();

                    PublishedApplication[] apps = Page.Session[PublishedApps] as PublishedApplication[];
                    notAppFavorites.AddRange(apps);
                    foreach (var item in appFavorites)
                    {
                        notAppFavorites.Remove(item);
                    }

                    foreach (PublishedApplication app in notAppFavorites)
                    {
                        AddNotFavoriteToTable(app);
                    }
                }
                else
                {
                    NotFavoritesLabel.Visible = false;
                    NotFavoritesTable.Visible = false;
                }

            }
            catch (Exception ex)
            {
                Logger.Default.Error(LogLocation, "User - " + Page.Request.LogonUserIdentity.Name + " ; Error = " + ex.Message, ex);
                AppMultiView.ActiveViewIndex = 5;
            }

        }

        private void AddNotFavoriteToTable(PublishedApplication app)
        {
            string name = app.Item.FreindlyName;
            string currentUrl = SPContext.Current.Web.Url;

            string iconUri = "~/_layouts/images/ExtentrixWebIntrface/icons/large/default.gif";
            if (!string.IsNullOrEmpty(app.LargeIcon))
                iconUri = currentUrl + "/_layouts/images/ExtentrixWebIntrface/icons/large/" + app.IconFileName;

            int rowIndex = NotFavoritesTable.Rows.Count;
            TableRow notFavoriteRow = new TableRow();

            TableCell cell = new TableCell();
            cell.ID = "notFavoriteCell" + rowIndex + notFavoriteRow.Cells.Count;
            //cell.Text = "<a href=" + app.LaunchAppUri + " target=_blank > <img src=" + iconUri + " border=0 /></a>";
            System.Web.UI.WebControls.Image icon = new System.Web.UI.WebControls.Image();
            icon.ImageUrl = iconUri;

            cell.Controls.Add(icon);
            notFavoriteRow.Cells.Add(cell);

            cell = new TableCell();
            cell.ID = "notFavoriteCell" + rowIndex + notFavoriteRow.Cells.Count;
            cell.Text = name;
            notFavoriteRow.Cells.Add(cell);


            cell = new TableCell();
            cell.ID = "notFavoriteCell" + rowIndex + notFavoriteRow.Cells.Count;

            LinkButton addFavoriteButton = new LinkButton();
            addFavoriteButton.ID = "addFavoriteButton" + rowIndex + notFavoriteRow.Cells.Count;
            addFavoriteButton.Text = "Add";
            addFavoriteButton.Command += new CommandEventHandler(addFavoriteButton_Command);
            addFavoriteButton.CommandArgument = app.AppName;
            cell.Controls.Add(addFavoriteButton);

            notFavoriteRow.Cells.Add(cell);

            NotFavoritesTable.Rows.Add(notFavoriteRow);
        }

        void addFavoriteButton_Command(object sender, CommandEventArgs e)
        {
            string appName = e.CommandArgument.ToString();
            favorites.AddFavoriteItem(appName);

            FillFavoritesView(true);
        }
        private void AddFavoriteToTable(PublishedApplication app, bool isConfigurationView)
        {
            string name = app.Item.FreindlyName;
            string currentUrl = SPContext.Current.Web.Url;

            string iconUri = "~/_layouts/images/ExtentrixWebIntrface/icons/large/default.gif";
            if (!string.IsNullOrEmpty(app.LargeIcon))
                iconUri = currentUrl + "/_layouts/images/ExtentrixWebIntrface/icons/large/" + app.IconFileName;

            int rowIndex = FavoritesTable.Rows.Count;
            TableRow favoriteRow = new TableRow();

            TableCell cell = new TableCell();
            cell.ID = "favoriteCell" + rowIndex + favoriteRow.Cells.Count;
            cell.Text = "<a href=" + Page.Request.Url.AbsolutePath + app.LaunchParameters + " target=_blank > <img src=" + iconUri + " border=0 /></a>";
            favoriteRow.Cells.Add(cell);

            cell = new TableCell();
            cell.ID = "favoriteCell" + rowIndex + favoriteRow.Cells.Count;
            cell.Text = name;
            favoriteRow.Cells.Add(cell);

            if (isConfigurationView)
            {
                cell = new TableCell();
                cell.ID = "favoriteCell" + rowIndex + favoriteRow.Cells.Count;

                LinkButton removeFavoriteButton = new LinkButton();
                removeFavoriteButton.ID = "removeFavoriteButton" + rowIndex + favoriteRow.Cells.Count;
                removeFavoriteButton.Text = "Remove";
                removeFavoriteButton.Command += new CommandEventHandler(removeFavoriteButton_Command);
                removeFavoriteButton.CommandArgument = app.AppName;
                cell.Controls.Add(removeFavoriteButton);

                favoriteRow.Cells.Add(cell);
            }
            FavoritesTable.Rows.Add(favoriteRow);
        }

        void removeFavoriteButton_Command(object sender, CommandEventArgs e)
        {
            string appName = e.CommandArgument.ToString();
            favorites.DeleteFavoriteItem(appName);

            FillFavoritesView(true);
        }


        private void CreateBreadCrumb(string currentFolderPath)
        {
            TableRow row = BreadCrumbsTable.Rows[0];
            row.Cells.Clear();
            TableCell rootCell = new TableCell();
            //rootCell.Text = "Home:";
            LinkButton folderLink = new LinkButton();
            folderLink.ID = "folderBreadCrumb" + row.Cells.Count;
            folderLink.CommandArgument = string.Empty;
            folderLink.Command += new CommandEventHandler(FolderLink_Command);
            folderLink.Text = "Home:";
            rootCell.Controls.Add(folderLink);

            row.Cells.Add(rootCell);
            if (!string.IsNullOrEmpty(currentFolderPath))
            {
                Folder currentFolder = tree.GetFolder(currentFolderPath);
                AddCrumb(currentFolder);
                UpImageButton.Enabled = true;
                HomeImageButton.Enabled = true;


                if (currentFolder.Depth > 3)
                {
                    int numberToRemove = (currentFolder.Depth - 3) * 2;
                    //int numberToRemove = (currentFolder.Depth - 3);
                    for (int i = 0; i < numberToRemove; i++)
                    {
                        BreadCrumbsTable.Rows[0].Cells.RemoveAt(1);
                    }
                }
            }
            else
            {
                UpImageButton.Enabled = false;
                HomeImageButton.Enabled = false;
            }
        }

        private void AddCrumb(Folder currentFolder)
        {
            if (!string.IsNullOrEmpty(currentFolder.ParentPath))
            {
                AddCrumb(tree.GetFolder(currentFolder.ParentPath));
                //UpImageButton.Enabled = true;
                //HomeImageButton.Enabled = true;
            }
            //else
            //{
            //    UpImageButton.Enabled = true;
            //    HomeImageButton.Enabled = true;
            //}

            TableCell cell = new TableCell();
            cell.Text = "\\";
            BreadCrumbsTable.Rows[0].Cells.Add(cell);
            cell = new TableCell();
            int cells_count = BreadCrumbsTable.Rows[0].Cells.Count;
            LinkButton folderLink = new LinkButton();
            folderLink.ID = "folderBreadCrumb" + BreadCrumbsTable.Rows[0].Cells.Count;
            folderLink.CommandArgument = currentFolder.Path;
            folderLink.Command += new CommandEventHandler(FolderLink_Command);
            folderLink.Text = currentFolder.Name;
            cell.Controls.Add(folderLink);
            cell.Enabled = false;
            BreadCrumbsTable.Rows[0].Cells[cells_count - 2].Enabled = true;
            IEnumerator ie = BreadCrumbsTable.Rows[0].Cells[cells_count - 2].Controls.GetEnumerator();

            while (ie.MoveNext())
            {
                UpImageButton.ToolTip = "Up to: " + ((LinkButton)ie.Current).Text;

            }


            BreadCrumbsTable.Rows[0].Cells.Add(cell);
        }

        private ExtentrixWIWebPart getWebPartCurrentInstance()
        {
            ExtentrixWIWebPart wp_res = null;
            WebPartManager web_manager = WebPartManager.GetCurrentWebPartManager(this.Page);
            IEnumerator it = web_manager.WebParts.GetEnumerator();
            while (it.MoveNext())
            {
                if (it.Current != null)
                {

                    if (it.Current.GetType() == typeof(ExtentrixWIWebPart))
                    {
                        wp_res = (ExtentrixWIWebPart)it.Current;
                        break;
                    }
                }

            };
            return wp_res;
        }

        private void FillListTable(string currentFolderPath)
        {
            bool isDetails = GetCurrentView(DropDownViewSwitcher.SelectedValue) == View.Details;
            ListTable.Rows.Clear();

            TableRow headerRow = new TableRow();

            TableCell headerCell = new TableCell();
            headerCell.ID = "headerImageName";
            headerCell.Width = 32;
            headerRow.Cells.Add(headerCell);

            headerCell = new TableCell();
            headerCell.ID = "headerCellName";
            headerCell.Text = "Name";
            headerRow.Cells.Add(headerCell);

            if (isDetails)
            {
                headerCell = new TableCell();
                headerCell.ID = "headerCellDescription";
                headerCell.Text = "Description";
                headerRow.Cells.Add(headerCell);
            }

            ListTable.Rows.Add(headerRow);

            Folder currentFolder = string.IsNullOrEmpty(currentFolderPath) ? tree.Root : tree.GetFolder(currentFolderPath);

            foreach (var folder in currentFolder.Folders)
            {
                AddFolderToListTable(folder.Path, folder.Name);
            }

            foreach (var app in tree.GetFolderApplications(currentFolder))
            {
                AddApplicationToListTable(app, isDetails);
            }
        }

        private void AddFolderToListTable(string path, string name)
        {
            int rowIndex = ListTable.Rows.Count;
            TableRow listRow = new TableRow();
            TableCell cell = new TableCell();
            cell.ID = "listCell" + rowIndex + listRow.Cells.Count;


            ImageButton folderButton = new ImageButton();
            folderButton.ID = "listIconsFolder" + rowIndex + listRow.Cells.Count; ;

            folderButton.ImageUrl = "~/_layouts/images/ExtentrixWebIntrface/icons/small/folder.gif";
            //folderButton.PostBackUrl = Page.Request.Url.AbsolutePath;
            folderButton.CommandArgument = path;
            folderButton.Command += new CommandEventHandler(ListIconsFolderButton_Command);

            cell.Controls.Add(folderButton);
            listRow.Cells.Add(cell);

            cell = new TableCell();
            cell.ID = "listCell" + rowIndex + listRow.Cells.Count;

            cell.Text = name;

            listRow.Cells.Add(cell);

            ListTable.Rows.Add(listRow);
        }

        private void AddApplicationToListTable(PublishedApplication app, bool isDetails)
        {
            string name = app.Item.FreindlyName;
            string currentUrl = SPContext.Current.Web.Url;
            string iconUri = "~/_layouts/images/ExtentrixWebIntrface/icons/small/default.gif";

            if (!string.IsNullOrEmpty(app.SmallIcon))
                iconUri = currentUrl + "/_layouts/images/ExtentrixWebIntrface/icons/small/" + app.IconFileName;

            int rowIndex = ListTable.Rows.Count;
            TableRow listRow = new TableRow();

            TableCell cell = new TableCell();
            cell.ID = "listCell" + rowIndex + listRow.Cells.Count;
            cell.Text = "<a href=" + Page.Request.Url.AbsolutePath + app.LaunchParameters + " target=_blank > <img src=" + iconUri + " border=0 /></a>";
            listRow.Cells.Add(cell);

            cell = new TableCell();
            cell.ID = "listCell" + rowIndex + listRow.Cells.Count;
            cell.Text = name;
            listRow.Cells.Add(cell);

            if (isDetails)
            {
                cell = new TableCell();
                cell.ID = "listCell" + rowIndex + listRow.Cells.Count;
                cell.Text = app.Item.Description;
                listRow.Cells.Add(cell);
            }

            ListTable.Rows.Add(listRow);
        }
        private void FillIconsTable(string currentFolderPath)
        {
            //IconsList.InnerHtml = "";
					 IconsList.Controls.Clear();
            //bodyTable.BorderColor = System.Drawing.Color.FromArgb(0x92, 0xBB, 0xF1);
            //bodyTable.BorderStyle = BorderStyle.Double;


            Folder currentFolder = string.IsNullOrEmpty(currentFolderPath) ? tree.Root : tree.GetFolder(currentFolderPath);


            
            int iconsIndex = 0;
            foreach (var item in currentFolder.Folders)
            {
							AddFolderToIconsTable(iconsIndex, item.Name, item.Path);
            	iconsIndex++;
            }

            foreach (var app in tree.GetFolderApplications(currentFolder))
            {
							AddApplicationToIconsTable(iconsIndex, app);
							iconsIndex++;
            }
        }


        
        private void AddEmptyCell(int rowIndex)
        {
            //TableRow iconsRow = IconsTable.Rows[rowIndex];
            //TableRow namesRow = IconsTable.Rows[rowIndex + 1];
						//int index = rowIndex * 2;
						//TableRow iconsRow = IconsTable.Rows[index - 2];
						//TableRow namesRow = IconsTable.Rows[index - 1];

						//TableCell cell = new TableCell();
						//cell.ID = "iconsCell" + rowIndex + iconsRow.Cells.Count;
						//cell.HorizontalAlign = HorizontalAlign.Center;
						//cell.Width = 100;

						//iconsRow.Cells.Add(cell);

						//cell = new TableCell();
						//cell.ID = "namesCell" + rowIndex + namesRow.Cells.Count;
						//cell.HorizontalAlign = HorizontalAlign.Center;
						//cell.Width = 100;
						//namesRow.Cells.Add(cell);
        }
        private void AddApplicationToIconsTable(int rowIndex, PublishedApplication app)
        {
            string name = app.Item.FreindlyName;
            string currentUrl = SPContext.Current.Web.Url;
            string iconUri = "~/_layouts/images/ExtentrixWebIntrface/icons/large/default.gif";
            if (!string.IsNullOrEmpty(app.LargeIcon))
                iconUri = currentUrl + "/_layouts/images/ExtentrixWebIntrface/icons/large/" + app.IconFileName;

        	var liControl = new HtmlGenericControl("li");
        	liControl.InnerHtml = "<a href=" + Page.Request.Url.AbsolutePath + app.LaunchParameters + " target=_blank > <img src=" + iconUri + " border=0 /></a><p>" + app.Item.FreindlyName + "</p>";
					IconsList.Controls.Add(liControl);
            
        }

        private void AddFolderToIconsTable(int rowIndex, string name, string path)
        {
           
            ImageButton folderButton = new ImageButton();
        		folderButton.ID = "iconsFolder" + rowIndex;

            folderButton.ImageUrl = "~/_layouts/images/ExtentrixWebIntrface/icons/large/folder.gif";
            folderButton.PostBackUrl = Page.Request.Url.AbsolutePath;
            // folderButton.Click += new ImageClickEventHandler(IconsFolderClick);
            folderButton.CommandArgument = path;
            folderButton.Command += new CommandEventHandler(IconsFolderButton_Command);

        		var liControl = new HtmlGenericControl("li");
						liControl.Controls.Add(folderButton);


        	liControl.Controls.Add(new Literal()
        	                       	{
        	                       		Text = "<p>" + name + "</p>"
        	                       	});
					IconsList.Controls.Add(liControl);
        }

        void IconsFolderButton_Command(object sender, CommandEventArgs e)
        {
            string folderPath = e.CommandArgument.ToString();

            Page.Session[CurrentFolder] = folderPath;


            CreateBreadCrumb(folderPath);
            FillIconsTable(folderPath);
        }

        protected void FolderLink_Command(object sender, CommandEventArgs e)
        {
            string folderPath = e.CommandArgument.ToString();
            Page.Session[CurrentFolder] = folderPath;

            FillView(GetCurrentView(DropDownViewSwitcher.SelectedValue));
            CreateBreadCrumb(folderPath);
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {

            DropDownViewSwitcher.SelectedIndex = DropDownViewSwitcher.Items.IndexOf(DropDownViewSwitcher.Items.FindByText("Search"));//5;
            FillSearchView(SearchTextBox.Text);
        }

        protected void DisconnectSessionsButton_Click(object sender, EventArgs e)
        {
            ExtentrixWIWebPart wp = getWebPartCurrentInstance();
            wp.DisconnectSessions(Credentials);
        }

        protected void ReconnectSessionsButton_Click(object sender, EventArgs e)
        {

        }

        private void FillSearchView(string search)
        {
            BreadCrumbsTable.Visible = false;
            FavoritesConfiguration.Visible = false;
            HomeImageButton.Visible = false;
            UpImageButton.Visible = false;

            SearchTable.Rows.Clear();
            TableRow headerRow = new TableRow();

            TableCell headerCell = new TableCell();
            headerRow.Cells.Add(headerCell);

            headerCell = new TableCell();
            headerCell.Text = "Name";
            headerRow.Cells.Add(headerCell);

            headerCell = new TableCell();
            headerCell.Width = 100;
            headerRow.Cells.Add(headerCell);

            headerCell = new TableCell();
            headerCell.Text = "Location";
            headerRow.Cells.Add(headerCell);

            SearchTable.Rows.Add(headerRow);

            List<PublishedApplication> apps = tree.FindApplications(search);
            foreach (PublishedApplication app in apps)
            {
                AddSearchResultToTable(app);
            }
            AppMultiView.ActiveViewIndex = 4;


        }

        private void AddSearchResultToTable(PublishedApplication app)
        {
            string name = app.Item.FreindlyName;
            string currentUrl = SPContext.Current.Web.Url;

            string iconUri = "~/_layouts/images/ExtentrixWebIntrface/icons/small/default.gif";
            if (!string.IsNullOrEmpty(app.SmallIcon))
                iconUri = currentUrl + "/_layouts/images/ExtentrixWebIntrface/icons/small/" + app.IconFileName;

            int rowIndex = SearchTable.Rows.Count;
            TableRow row = new TableRow();

            TableCell cell = new TableCell();
            cell.ID = "searchCell" + rowIndex + row.Cells.Count;
            cell.Text = "<a href=" + Page.Request.Url.AbsolutePath + app.LaunchParameters + " target=_blank > <img src=" + iconUri + " border=0 /></a>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.ID = "searchCell" + rowIndex + row.Cells.Count;
            cell.Text = name;
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.Width = 50;
            row.Cells.Add(cell);

            string folderName = string.IsNullOrEmpty(app.Item.FolderName) ? "Home:" : tree.GetFolder(app.Item.FolderName).Name;
            cell = new TableCell();
            cell.ID = "locationCell" + rowIndex + row.Cells.Count;

            System.Web.UI.WebControls.Image icon = new System.Web.UI.WebControls.Image();
            icon.ImageUrl = "~/_layouts/images/ExtentrixWebIntrface/icons/small/folder.gif";
            cell.Controls.Add(icon);
            cell.Controls.Add(new LiteralControl(folderName));

            row.Cells.Add(cell);

            SearchTable.Rows.Add(row);
        }
        protected void ListIconsFolderButton_Command(object sender, CommandEventArgs e)
        {
            //chang the curren folder in Application then reload the web page
            string folderPath = e.CommandArgument.ToString();

            Page.Session[CurrentFolder] = folderPath;

            CreateBreadCrumb(folderPath);
            FillListTable(folderPath);
        }


        protected void homeClicked(object sender, ImageClickEventArgs e)
        {
            // restor the empty current folder and reload the web page
            Page.Session[CurrentFolder] = string.Empty;
            CreateBreadCrumb(string.Empty);
            FillView(GetCurrentView(DropDownViewSwitcher.SelectedValue));
        }

        protected void upClicked(object sender, ImageClickEventArgs e)
        {
            //restore the previous curren folder in Application then reload the web page
            string upFolder = string.Empty;
            upFolder = (string)Page.Session[CurrentFolder];
            if (upFolder.Contains("\\"))
            {
                //int length = upFolder.LastIndexOf("\\") - 1;
                upFolder = upFolder.Substring(0, upFolder.LastIndexOf("\\"));
            }
            else
            {
                upFolder = string.Empty;
            }

            Page.Session[CurrentFolder] = upFolder;
            CreateBreadCrumb(upFolder);
            FillView(GetCurrentView(DropDownViewSwitcher.SelectedValue));
        }



        protected void TreeViewApp_TreeNodeExpanded(object sender, TreeNodeEventArgs e)
        {
            string folderPath = ConvertToFolderPath(e.Node.ValuePath);

            Page.Session[CurrentFolder] = folderPath;
            CreateBreadCrumb(folderPath);

        }

        protected void DropDownViewSwitcher_SelectedIndexChanged(object sender, EventArgs e)
        {
            int viewIndex = int.Parse(DropDownViewSwitcher.SelectedValue);
            if (viewIndex == 5)
            {
                viewIndex = 1;
            }
            AppMultiView.ActiveViewIndex = viewIndex;

        }

        protected void FavoritesConfigurationChoice_ImgClick(object sender, EventArgs e)
        {
            bool isConfigurationMode = !NotFavoritesTable.Visible;
            string imgPath = ((ImageButton)sender).ImageUrl;
            if (imgPath.Equals("~/_layouts/images/ExtentrixWebIntrface/edit.ico"))
            {
                ((ImageButton)sender).ImageUrl = "~/_layouts/images/ExtentrixWebIntrface/stopedit.ico";
                FavoritesConfigurationChoice.SelectedValue = "1";
                FavoritesConfigurationChoice_SelectedIndexChanged(sender, e);
            }
            else
            {
                ((ImageButton)sender).ImageUrl = "~/_layouts/images/ExtentrixWebIntrface/edit.ico";
                FavoritesConfigurationChoice.SelectedValue = "0";
                FavoritesConfigurationChoice_SelectedIndexChanged(sender, e);
            }

            //FavoritesConfigurationChoice.SelectedValue

            //FillFavoritesView(isConfigurationMode);
        }

        protected void FavoritesConfigurationChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isConfigurationMode = FavoritesConfigurationChoice.SelectedValue == "1";
            FillFavoritesView(isConfigurationMode);
        }

        protected void TreeViewApp_SelectedNodeChanged(object sender, EventArgs e)
        {
            string folderPath = ConvertToFolderPath(TreeViewApp.SelectedNode.ValuePath);
            FillTreeView(folderPath);
        }
        private void FillTreeView(string currentFolderPath)
        {
            Folder currentFolder = string.IsNullOrEmpty(currentFolderPath) ? tree.Root : tree.GetFolder(currentFolderPath);

            TreeNode rootNode = TreeViewApp.Nodes[0];
            if (rootNode.ChildNodes.Count == 0)
            {
                FillAppTreeView(rootNode, tree.Root);
            }

            TreeViewApp.CollapseAll();


            TreeNode currentNode = string.IsNullOrEmpty(currentFolderPath) ? rootNode : TreeViewApp.FindNode(ConvertToValuePath(currentFolderPath));


            if (currentNode != null)
            {
                currentNode.Expand();

                ExpandToRoot(currentNode);
            }
            Page.Session[CurrentFolder] = currentFolderPath;
            CreateBreadCrumb(currentFolderPath);
        }


        private void ExpandToRoot(TreeNode currentNode)
        {
            if (currentNode.Parent != null)
            {
                ExpandToRoot(currentNode.Parent);
                currentNode.Parent.Expand();
            }
        }
        private void FillAppTreeView(TreeNode currentNode, Folder currentFolder)
        {

            foreach (var folder in currentFolder.Folders)
            {
                TreeNode node = new TreeNode(folder.Name);
                node.Value = folder.Name;
                node.PopulateOnDemand = true;
                node.SelectAction = TreeNodeSelectAction.SelectExpand;
                node.ImageUrl = "~/_layouts/images/ExtentrixWebIntrface/icons/small/folder.gif";
                FillAppTreeView(node, folder);
                currentNode.ChildNodes.Add(node);
            }

            foreach (var app in tree.GetFolderApplications(currentFolder))
            {
                TreeNode appNode = new TreeNode(app.AppName);
                appNode.PopulateOnDemand = false;
                appNode.Target = "_blank";

                //string name = app.Item.FreindlyName;
                //string currentUrl = SPContext.Current.Web.Url;

                //appNode.ImageUrl = currentUrl + "/_layouts/images/ExtentrixWebIntrface/icons/small/" + app.IconFileName;

                string navigationURL = Page.Request.Url.AbsolutePath + app.LaunchParameters;
                appNode.NavigateUrl = navigationURL;
                currentNode.ChildNodes.Add(appNode);
            }

        }

        private string ConvertToFolderPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            path = path.Substring(1);
            return path.Replace("/", "\\");
        }

        private string ConvertToValuePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            path = "/" + path;
            return path.Replace("\\", "/");
        }

        private string ConvertToTreePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            return path.Replace("\\\\", "\\");
        }


    }
}
