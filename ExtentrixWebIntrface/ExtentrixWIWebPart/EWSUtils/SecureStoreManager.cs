using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Security;
using System.Runtime.InteropServices;
using Microsoft.SharePoint.Administration.Claims;
using Microsoft.Office.SecureStoreService.Server;
using Microsoft.SharePoint.Administration;
using Microsoft.BusinessData.Infrastructure.SecureStore;
using System.Web.UI;
using Microsoft.SharePoint.Utilities;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
    public static class SecureStoreManager
    {
        public static SecureString MakeSecureString(string str)
        {
            if (str == null)
            {
                return null;
            }
            SecureString str2 = new SecureString();
            char[] chArray = str.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                str2.AppendChar(chArray[i]);
                chArray[i] = '0';
            }
            return str2;
        }

        public static string ReadSecureString(SecureString sstrIn)
        {
            if (sstrIn == null)
            {
                return null;
            }
            IntPtr ptr = Marshal.SecureStringToBSTR(sstrIn);
            string str = Marshal.PtrToStringBSTR(ptr);
            Marshal.ZeroFreeBSTR(ptr);
            return str;
        }



        public static Credentials GetExtentrixWindowsCredentials(Page page, LogLocationEnum LogLocation, SPUser user)
        {
            WindowsCredentials extentrixCredentials = null;

            SecureStoreCredentialCollection ssCreds = null;
            SPServiceContext context = SPServiceContext.GetContext(SPServiceApplicationProxyGroup.Default, SPSiteSubscriptionIdentifier.Default);

            SecureStoreServiceProxy ssp = new SecureStoreServiceProxy();
            ISecureStore iss = ssp.GetSecureStore(context);

            try
            {
                ssCreds = iss.GetCredentials(Constants.TargetAppID);

                if (ssCreds != null && ssCreds.Count() > 0)
                {
                    extentrixCredentials = new WindowsCredentials();
                    IList<TargetApplicationField> applicationFields = GetTargetApplicationFields(Constants.TargetAppID);

                    foreach (TargetApplicationField taf in applicationFields)
                    {
                        switch (taf.Name)
                        {
                            case "Windows User Name":
                                extentrixCredentials.UserName =
                                    ReadSecureString(ssCreds[applicationFields.IndexOf(taf)].Credential);
                                break;

                            case "Windows Password":
                                extentrixCredentials.Password =
                                    ReadSecureString(ssCreds[applicationFields.IndexOf(taf)].Credential);
                                break;

                            case "Domain":
                                extentrixCredentials.Domain =
                                    ReadSecureString(ssCreds[applicationFields.IndexOf(taf)].Credential);
                                break;
                        }
                    }

                }
            }
            catch (SecureStoreServiceCredentialsNotFoundException ex)
            {
                Logger.Default.Error(LogLocation, ex.Message, ex);
            }
						catch (Exception ex)
						{
							Logger.Default.Info(LogLocation, "SecureStore: Exception getting Windows Credentials");
							Logger.Default.Error(LogLocation, ex.Message, ex);
						}

            return extentrixCredentials;
        }


        public static IList<TargetApplicationField> GetTargetApplicationFields(string targetApplicationID)
        {
            IList<TargetApplicationField> applicationFields = null;
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               using (SPSite site = new SPSite(SPContext.Current.Site.ID))
               {
                   using (SPWeb web = site.OpenWeb())
                   {
                       SPServiceContext context = SPServiceContext.GetContext(SPServiceApplicationProxyGroup.Default, SPSiteSubscriptionIdentifier.Default);

                       SecureStoreServiceProxy ssp = new SecureStoreServiceProxy();
                       ISecureStore iss = ssp.GetSecureStore(context);
                       applicationFields = iss.GetUserApplicationFields(targetApplicationID);
                   }
               }
           });

            return applicationFields;
        }

        public static void SetExtentrixWindowsCredentials(LogLocationEnum LogLocation, SPUser user, WindowsCredentials extentrixCredentials)
        {
            try
            {

                IList<TargetApplicationField> applicationFields = GetTargetApplicationFields(Constants.TargetAppID);
                IList<ISecureStoreCredential> creds = new List<ISecureStoreCredential>(applicationFields.Count);

								using (SecureStoreCredentialCollection credentials = new SecureStoreCredentialCollection(creds))
								{

									foreach (TargetApplicationField taf in applicationFields)
									{
										switch (taf.Name)
										{
											case "Windows User Name":
												creds.Add(new SecureStoreCredential(MakeSecureString(extentrixCredentials.UserName),
												                                    SecureStoreCredentialType.WindowsUserName));
												break;

											case "Windows Password":
												creds.Add(new SecureStoreCredential(MakeSecureString(extentrixCredentials.Password),
												                                    SecureStoreCredentialType.WindowsPassword));
												break;

											case "Domain":
												creds.Add(new SecureStoreCredential(MakeSecureString(extentrixCredentials.Domain)
												                                    , SecureStoreCredentialType.Generic));
												break;
										}

									}

									SPSecurity.RunWithElevatedPrivileges(delegate()
									{
										using (SPSite site = new SPSite(SPContext.Current.Site.ID))
										{
											using (SPWeb web = site.OpenWeb())
											{
												SPServiceContext context = SPServiceContext.GetContext(SPServiceApplicationProxyGroup.Default, SPSiteSubscriptionIdentifier.Default);
												SecureStoreServiceProxy ssp = new SecureStoreServiceProxy();
												ISecureStore iss = ssp.GetSecureStore(context);

												iss.SetUserCredentials(Constants.TargetAppID, GetSSClaim(user), credentials);
											}
										}
									});
								}
            }
            catch (Exception ex)
            {
								Logger.Default.Info(LogLocation, "SecureStore: Exception setting windows credentials");
								Logger.Default.Error(LogLocation, ex.Message, ex);
            }

        }

        public static void DeleteExtentrixWindowsCredentials(Page page, LogLocationEnum LogLocation, SPUser user)
        {
            try
            {
								SPSecurity.RunWithElevatedPrivileges(delegate()
								{
								  using (SPSite site = new SPSite(SPContext.Current.Site.ID))
								  {
								    using (SPWeb web = site.OpenWeb())
								    {
											SPServiceContext context = SPServiceContext.GetContext(SPServiceApplicationProxyGroup.Default, SPSiteSubscriptionIdentifier.Default);

											SecureStoreServiceProxy ssp = new SecureStoreServiceProxy();
											ISecureStore iss = ssp.GetSecureStore(context);

											SPContext.Current.Web.AllowUnsafeUpdates = true;
            					//SPUtility.ValidateFormDigest();
											
											iss.DeleteUserCredentials(Constants.TargetAppID, GetSSClaim(user));
								      SPContext.Current.Web.AllowUnsafeUpdates = false;
										}
									}
								});

                
            }
            catch (SecureStoreServiceCredentialsNotFoundException ex)
            {
                Logger.Default.Error(LogLocation, ex.Message, ex);
            }
						catch (Exception ex)
						{
							Logger.Default.Info(LogLocation, "SecureStore: Exception delete Windows Credentials");
							Logger.Default.Error(LogLocation, ex.Message, ex);
						}
        }

				private static SecureStoreServiceClaim GetSSClaim(SPUser user)
				{
					SPClaim spClaim = null;
					var manager = SPClaimProviderManager.Local;
					if ((manager != null) && SPClaimProviderManager.IsEncodedClaim(user.LoginName))
					{
						spClaim = manager.DecodeClaim(user.LoginName);

					}
					else
					{
						spClaim = SPClaimProviderManager.CreateUserClaim(user.LoginName, SPOriginalIssuerType.Windows);
					}
					SecureStoreServiceClaim ssClaim = new SecureStoreServiceClaim(spClaim);
					return ssClaim;
				}
        public static void SetCredentialsLiveTime(int minutes)
        {
            var profile = new UserProfile();
            //profile.SetValue("ExpiredTime", DateTime.UtcNow.Date.AddDays(days));
						profile.SetValue("ExpiredTime", minutes != -1 ? DateTime.UtcNow.AddMinutes(minutes) : (DateTime?)null);
        }

				public static bool IsCredentialsExpired(int minutes)
        {
            var profile = new UserProfile();
            var expiredTime = profile.GetValue<DateTime?>("ExpiredTime");
						
							if (expiredTime == null)
							{
								return minutes != -1;
							}
						
					//return expiredTime <= DateTime.UtcNow.Date;
            return expiredTime <= DateTime.UtcNow;
        }

    }
}
