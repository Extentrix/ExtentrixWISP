using System.Web.Script.Serialization;
using Microsoft.SharePoint;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
    public class UserProfile
    {
        private SPListItem _userItem;
        public UserProfile()
        {
        }
        

        public object this[string key]
        {
            get
            {
							var spUser = SPContext.Current.Web.CurrentUser;
            	object returnValue = null;
							SPSecurity.RunWithElevatedPrivileges(delegate()
							{
								using (SPSite site = new SPSite(SPContext.Current.Site.ID))
								{
									using (SPWeb web = site.OpenWeb())
									{
										var userList = web.SiteUserInfoList;
										_userItem = userList.GetItemById(spUser.ID);
										returnValue = _userItem.Properties[key];  
									}
								}
							});
            	return returnValue;
            }
            set
            {
							var spUser = SPContext.Current.Web.CurrentUser;
							SPSecurity.RunWithElevatedPrivileges(delegate()
							{
								using (SPSite site = new SPSite(SPContext.Current.Site.ID))
								{
									using (SPWeb web = site.OpenWeb())
									{
										var userList = web.SiteUserInfoList;
										_userItem = userList.GetItemById(spUser.ID);
										if (_userItem.Properties[key] != null)
										{
											_userItem.Properties[key] = value;
										}
										else
										{
											_userItem.Properties.Add(key, value);
										}
										SPContext.Current.Web.AllowUnsafeUpdates = true;
										_userItem.Update();
										SPContext.Current.Web.AllowUnsafeUpdates = false;
									}
								}
							});
            }
        }

        public T GetValue<T>(string key)
        {
            var serializer = new JavaScriptSerializer();
            var str = this[key] as string;
            var res = default(T);
            if (str != null)
            {
                res = serializer.Deserialize<T>(str);
            }
            return res;
        }

        public void SetValue(string key, object value)
        {
            var serializer = new JavaScriptSerializer();
            this[key] = serializer.Serialize(value);
        }
    }
}
