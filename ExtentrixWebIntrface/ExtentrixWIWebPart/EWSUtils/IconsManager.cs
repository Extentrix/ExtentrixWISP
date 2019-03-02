using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Microsoft.SharePoint.Utilities;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
	public class IconsManager
	{
		public static string DefaultIcon = "default.gif";
		public static void CreateLargeIcon(PublishedApplication app)
		{
			 CreateIcon("large", app.LargeIcon, app.IconFileName);
		}

		public static void CreateSmallIcon(PublishedApplication app)
		{
			 CreateIcon("small", app.SmallIcon, app.IconFileName);
			
		}
		
		private static void CreateIcon(string folder, string icon, string fileName)
		{
			//store and upload the .ico files to be desplay   
			if (!string.IsNullOrEmpty(icon))
			{
				byte[] byteArray = Convert.FromBase64String(icon);

				string folderPath = SPUtility.GetGenericSetupPath("TEMPLATE\\IMAGES");
				folderPath += "\\ExtentrixWebIntrface\\icons\\" + folder;
				
				string path = Path.Combine(folderPath, fileName);
				using (FileStream iconFile = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
				{
					iconFile.Write(byteArray, 0, byteArray.Length);
					iconFile.Close();
				}

				FileSecurity fSecurity = File.GetAccessControl(path);
				fSecurity.AddAccessRule(new FileSystemAccessRule(WebServiceConfig.EveryoneRole, FileSystemRights.Read, AccessControlType.Allow));
				File.SetAccessControl(path, fSecurity);

			}
			
		}

		public static bool CheckLargeIcon(string fileName, IconsExpirationPeriod period)
		{
			return CheckIcon("large", fileName, period);
		}

		public static bool CheckSmallIcon(string fileName, IconsExpirationPeriod period)
		{
			return CheckIcon("small", fileName, period);
		}

		private static bool CheckIcon(string folder, string fileName, IconsExpirationPeriod period)
		{
			string folderPath = SPUtility.GetGenericSetupPath("TEMPLATE\\IMAGES");
			folderPath += "\\ExtentrixWebIntrface\\icons\\" + folder;

			string path = Path.Combine(folderPath, fileName);
			if (!File.Exists(path))
			{
				return false;
			}
			else
			{
				var creationDate = File.GetCreationTime(path);
				var today = DateTime.Now.Date;
				switch (period)
				{
					case IconsExpirationPeriod.Never:
						return true;
					case IconsExpirationPeriod.Day:
						return creationDate.AddDays(1) < today;
					case IconsExpirationPeriod.Week:
						return creationDate.AddDays(7) < today;
					case IconsExpirationPeriod.Month:
						return creationDate.AddMonths(1) < today;
				}
			}
			return true;

		}
	}
}
