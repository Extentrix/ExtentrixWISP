using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;
using ExtentrixWebIntrface.ExtentrixWIWebPart;
using System.Xml;
using Microsoft.SharePoint.Administration;

namespace ExtentrixWebIntrface.ContentRedirection
{
    static class FileTypesUtility
    {
        public static bool IsFileTypeActive(string fileType)
        {
            var web = SPContext.Current.Web.Site.RootWeb;
            var list = web.Lists[Constants.FileTypesList];
            SPQuery query = new SPQuery();
            query.Query = string.Format(@"<Where>
                                             <And>
                                                <Eq>
                                                   <FieldRef Name=""{0}""/>
                                                   <Value Type=""Boolean"">{1}</Value>
                                                </Eq>
                                                <Eq>
                                                   <FieldRef Name=""{2}""/>
                                                   <Value Type=""Boolean"">{3}</Value>
                                                 </Eq>
                                               </And>
                                           </Where>", "Title", fileType, "Active", 1);
            var items = list.GetItems(query);
            if ((items != null) && items.Count > 0)
            {
                return true;
            }
            return false;
        }

       public static Dictionary<string, RedirectionApplication> GetAllApplications()
        {
            var web = SPContext.Current.Web.Site.RootWeb;
            var list = web.Lists[Constants.FileTypesList];
           
            SPListItemCollection items = list.Items;
            var applications = new Dictionary<string, RedirectionApplication>();
            if ((items != null) && items.Count > 0)
            {
                foreach (SPListItem item in items)
                {
                    var appName = (string)item["AppInternalName"];
                    if (!applications.ContainsKey(appName))
                    {
                        applications.Add(appName, new RedirectionApplication()
                        {
                            InternalName = appName,
                            DisplayName = (string)item["AppDisplayName"],
                            FarmName = (string)item["FarmName"]
                        });
                    }
                }

            }
            return applications;
        }
       public static Dictionary<string, RedirectionApplication> GetFileTypes()
       {
           var web = SPContext.Current.Web.Site.RootWeb;
           var list = web.Lists[Constants.FileTypesList];

           SPListItemCollection items = list.Items;
           var fileTypes = new Dictionary<string, RedirectionApplication>();
           if ((items != null) && items.Count > 0)
           {
               foreach (SPListItem item in items)
               {
                   var fileType = (string)item["Title"];
                   if (!fileTypes.ContainsKey(fileType))
                   {
                       fileTypes.Add(fileType, new RedirectionApplication()
                       {
                           InternalName = (string)item["AppInternalName"],
                           DisplayName = (string)item["AppDisplayName"],
                           FarmName = (string)item["FarmName"]
                       });
                   }
               }

           }
           return fileTypes;
       }

        public static RedirectionApplication GetApplicationByName(string appName)
        {
            var web = SPContext.Current.Web.Site.RootWeb;
            var list = web.Lists[Constants.FileTypesList];
            SPQuery query = new SPQuery();
            query.Query = string.Format(@"<Where>                                             
                                             <Eq>
                                               <FieldRef Name=""{0}""/>
                                                 <Value Type=""Text"">{1}</Value>
                                             </Eq>                                              
                                           </Where>", "AppInternalName", appName);
            SPListItemCollection items = list.GetItems(query);
            
            if ((items != null) && items.Count > 0)
            {
                SPListItem item = items[0];
                RedirectionApplication app = new RedirectionApplication()
                        {
                            InternalName = appName,
                            DisplayName = (string)item["AppDisplayName"],
                            FarmName = (string)item["FarmName"]
                        };
                return app;
            }
            return null;
        }

        public static RedirectionApplication GetApplicationByFileType(string fileType)
        {
            var web = SPContext.Current.Web.Site.RootWeb;
            var list = web.Lists[Constants.FileTypesList];
            SPQuery query = new SPQuery();
            query.Query = string.Format(@"<Where>                                             
                                             <Eq>
                                               <FieldRef Name=""{0}""/>
                                                 <Value Type=""Text"">{1}</Value>
                                             </Eq>                                              
                                           </Where>", "Title", fileType);
            SPListItemCollection items = list.GetItems(query);

            if ((items != null) && items.Count > 0)
            {
                SPListItem item = items[0];
                RedirectionApplication app = new RedirectionApplication()
                {
                    InternalName = (string)item["AppInternalName"],
                    DisplayName = (string)item["AppDisplayName"],
                    FarmName = (string)item["FarmName"]
                };
                return app;
            }
            return null;
        }

        public static bool GetGoThroughValue()
        {
            var web = SPContext.Current.Web.Site.RootWeb;
            var list = web.Lists[Constants.SettingsList];
            SPQuery query = new SPQuery();
            query.Query = string.Format(@"<Where>                                             
                                             <Eq>
                                               <FieldRef Name=""{0}""/>
                                                 <Value Type=""Text"">{1}</Value>
                                             </Eq>                                              
                                           </Where>", "Title", "GoThrough");
            SPListItemCollection items = list.GetItems(query);

            if ((items != null) && items.Count > 0)
            {
                var value = (string) items[0]["Value"];
                return bool.Parse(value);
            }
            return false;
        }

        public static void SetGoThroughValue(bool value)
        {
            var web = SPContext.Current.Web.Site.RootWeb;
            var list = web.Lists[Constants.SettingsList];
            SPQuery query = new SPQuery();
            query.Query = string.Format(@"<Where>                                             
                                             <Eq>
                                               <FieldRef Name=""{0}""/>
                                                 <Value Type=""Text"">{1}</Value>
                                             </Eq>                                              
                                           </Where>", "Title", "GoThrough");
            SPListItemCollection items = list.GetItems(query);

            if ((items != null) && items.Count > 0)
            {
                var item = items[0];
                
                item["Value"] = value.ToString().ToLower();
                item.Update();
                list.Update();
            }
           
        }

        public static PublishedApplication[] GetPublishedApplications(System.Security.Principal.WindowsIdentity user)
        {
            //Todo get published application from web service

            WebServiceAPI service = new WebServiceAPI();
            WindowsIdentity usr = service.GetUser(user);
// orig source
//            PublishedApplication[] apps = service.GetApplicationsByCredentials(usr, usr.UserName, "", new string[] { "defaults" }, new string[] { "all" }, new string[] { "all" });

            PublishedApplication[] apps = null;
            try
            {
                apps = service.GetApplicationsByCredentials(usr, usr.UserName, "", new string[] { "defaults" }, new string[] { "all" }, new string[] { "all" });
            }
            catch (Exception ex)
            {               
               SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
               Logger.Default.Error("An error occured during getting published applications", ex);
            }

            /*XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(SPContext.Current.Web.Url + "/_layouts/Images/ExtentrixWebIntrface/Applications.xml");
            string xml = xmlDoc.InnerXml;
            List<PublishedApplication> appsList = Util.GetPublishedApplications();
            PublishedApplication[] apps = appsList.ToArray();*/

            return apps;
        }

        public static void DeleteFileType(string fileType)
        {
            var web = SPContext.Current.Web.Site.RootWeb;
            var list = web.Lists[Constants.FileTypesList];
            SPQuery query = new SPQuery();
            query.Query = string.Format(@"<Where>                                             
                                             <Eq>
                                               <FieldRef Name=""{0}""/>
                                                 <Value Type=""Text"">{1}</Value>
                                             </Eq>                                              
                                           </Where>", "Title", fileType);
            SPListItemCollection items = list.GetItems(query);

            if ((items != null) && items.Count > 0)
            {
                items[0].Delete();
            }
            list.Update();
        }

        public static void AddFileType(string fileType, RedirectionApplication redirectionApp)
        {
            var web = SPContext.Current.Web.Site.RootWeb;
            var list = web.Lists[Constants.FileTypesList];
            
            var addedItem = list.Items.Add();
            addedItem["Title"] = fileType;
            addedItem["AppInternalName"] = redirectionApp.InternalName;
            addedItem["AppDisplayName"] = redirectionApp.DisplayName;
            addedItem["FarmName"] = redirectionApp.FarmName;

            addedItem.Update();
        }
    }
}
