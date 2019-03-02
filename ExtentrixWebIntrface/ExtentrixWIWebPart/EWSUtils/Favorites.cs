using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
    class Favorites
    {
        private ApplicationsTree _tree;
        private string _currentUserLogin;
        public Favorites(ApplicationsTree tree)
        {
            _tree = tree;
            _currentUserLogin = SPContext.Current.Web.CurrentUser.LoginName;
        }


        public void DeleteFavoriteItem(string appName)
        {
            SPSecurity.RunWithElevatedPrivileges(
            () =>
            {
                using (var site = new SPSite(SPContext.Current.Web.Site.ID))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        web.AllowUnsafeUpdates = true;
                        SPList list = web.Lists.TryGetList(Constants.FavoritesList);
                        var query = new SPQuery();
                        query.RowLimit = 1;
                        query.Query = string.Format(
                           @"<Where>
                                <And>
                                    <Eq>
                                        <FieldRef Name=""UserLogin""/>
                                        <Value Type=""Text"">{0}</Value>
                                     </Eq>
                                    <Eq>
                                        <FieldRef Name=""Title""/>
                                        <Value Type=""Text"">{1}</Value>
                                     </Eq>
                                 </And>
                            </Where>", _currentUserLogin, appName);

                        var items = list.GetItems(query);
                        if (items != null && items.Count != 0)
                        {
                            items[0].Delete();
                        }
                        list.Update();
                        web.AllowUnsafeUpdates = false;
                    }

                }
            });
        }
        public List<PublishedApplication> GetFavoritesApps()
        {
            SPWeb web = SPContext.Current.Web.Site.RootWeb;

            
            SPList favoritesList = web.Lists.TryGetList(Constants.FavoritesList);
            SPQuery query = new SPQuery();


            query.Query = string.Format(@"<Where>                                             
                                             <Eq>
                                                <FieldRef Name=""UserLogin""/>
                                                <Value Type=""Text"">{0}</Value>
                                              </Eq>                                                    
                                          </Where>", _currentUserLogin);

            List<PublishedApplication> appsFavorites = new List<PublishedApplication>();
            if (favoritesList.GetItems(query).Count != 0)
            {
                foreach (SPListItem item in favoritesList.GetItems(query))
                {
                    if (item != null)
                    {
                        string appName = item["Title"].ToString();
                        appsFavorites.Add(_tree.GetApplication(appName));
                    }
                }
            }

            return appsFavorites;
        }

        public void AddFavoriteItem(string appName)
        {
            SPSecurity.RunWithElevatedPrivileges(
             () =>
             {
                 using (var site = new SPSite(SPContext.Current.Web.Site.ID))
                 {
                     using (SPWeb web = site.OpenWeb())
                     {
                         web.AllowUnsafeUpdates = true;
                         SPList list = web.Lists.TryGetList(Constants.FavoritesList);

                         SPListItem newItem = list.Items.Add();
                         newItem["Title"] = appName;
                         newItem["UserLogin"] = _currentUserLogin;                        
                         newItem.Update();

                         web.AllowUnsafeUpdates = false;
                     }

                 }
             });
        }
    }
}
