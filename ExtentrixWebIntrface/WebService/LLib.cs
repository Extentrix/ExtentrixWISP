/****************************************************************************

    LLIB.CS           Version 3.00 / June 2005

    C# class wrapper for LLIB Licensing Library
****************************************************************************/


using System;
using System.Runtime.InteropServices; 
using System.Text;
using System.IO;
using Microsoft.Win32;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;


	/// <summary>
	/// Summary description for CLLib.
	/// </summary>

namespace Licensing 
{
    public class Validation
    {
        public static int WEB_SERVICES = 0;
        public static int TURBO_TOOLBAR = 1;
        public static int SAP_TOOLBAR = 2;

        public static int WEB_SERVICES_VER = 4;
        public static int TURBO_TOOLBAR_VER = 2;
        public static int SAP_TOOLBAR_VER = 1;

        public static bool validateLicense(ref string errorMsg , int type)
        {
            //CHANGE TO THE LICENS
            return true;

          //DateTime now = DateTime.Now;


          //int year = now.Year;
          //int month = now.Month;

          //if ((year == 2010) && (month <= 7))
          //{
          //  return true;
          //}
          //else
          //{
          //  return false;
          //}
            //int r = CLLib.LLSetLibraryLicense("R110050010000656");
            //string path = Read("InstallRoot");
            //if (path == null)
            //{
            //    errorMsg = "Failed to read Extentrix Web Services 5.0 - Application Edition install path from registry.";
            //    return false;
            //}
            //path+=WSConstants.LIC_LICENSES_FOLDER+"\\"+WSConstants.LICENSE_FILES_NAMES[type]+".LIC";
            
            //CLLib.LLSetLicenseFile(path);
            //int res = CLLib.LLOpenLicenseFile(false);
            //if (res != 0)
            //{
            //    errorMsg = WSConstants.LICENSE_ERROR_MESSAGES[type];
            //    return false;
            //}
            //else
            //{
            //    int ret = -1;
            //    if(type == 0)
            //        ret = CLLib.LLCheckLicenseNo("TR", 1, WEB_SERVICES_VER, 1, 8);
            //    else if (type == 1)
            //        ret = CLLib.LLCheckLicenseNo("TR", 17, TURBO_TOOLBAR_VER, 1, 7);
            //    else if( type == 2)
            //        ret = CLLib.LLCheckLicenseNo("TR", 16, SAP_TOOLBAR_VER, 1, 8);
                
                
            //    if (ret != 0)
            //        CLLib.LLGetLicenseErrorMsg(ret, ref errorMsg);
                    
            //    CLLib.LLCloseLicenseFile();
            //    return ret == 0;
            //}
        }


        public static bool isTrialLicense(ref string errorMsg, int type)
        {
            CLLib.LLSetLibraryLicense("R110050010000656");
            string path = Read("InstallRoot");
            if (path == null)
            {
                errorMsg = "Failed to read Extentrix Web Services 5.0 - Application Edition install path from registry.";
                return false;
            }
            path += WSConstants.LIC_LICENSES_FOLDER + "\\" + WSConstants.LICENSE_FILES_NAMES[type] + ".LIC";

            CLLib.LLSetLicenseFile(path);
            if (CLLib.LLOpenLicenseFile(false) != 0)
            {
                errorMsg = WSConstants.LICENSE_ERROR_MESSAGES[type];
                return false;
            }
            else
            {
                int ret = -1;
                if (type == 0)
                    ret = CLLib.LLCheckLicenseNo("T", 1, WEB_SERVICES_VER, 1, 8);
                else if (type == 1)
                    ret = CLLib.LLCheckLicenseNo("T", 17, TURBO_TOOLBAR_VER, 1, 6);
                else if (type == 2)
                    ret = CLLib.LLCheckLicenseNo("T", 16, SAP_TOOLBAR_VER, 1, 8);

                CLLib.LLCloseLicenseFile();
                if (ret != 0)
                    CLLib.LLGetLicenseErrorMsg(ret, ref errorMsg);

                CLLib.LLCloseLicenseFile();
                return ret == 0;
            }
        }
       


        public static string Read(string KeyName)
        {
            // Opening the registry key
            RegistryKey rk = Registry.LocalMachine;
            // Open a subKey as read-only
            RegistryKey sk1 = rk.OpenSubKey(WSConstants.PRODUCT_KEY);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
            {
                if (WSConstants.LOGERRORS)
                    EventLogger.logEvent("Reading Entry doesn't Exist " + KeyName.ToUpper(), 0);
                return null;
            }
            else
            {
                try
                {
                    // If the RegistryKey exists I get its value
                    // or null is returned.
                    return (string)sk1.GetValue(KeyName.ToUpper());
                }
                catch 
                {
                    if (WSConstants.LOGERRORS)
                        EventLogger.logEvent("Reading registry " + KeyName.ToUpper(), 0);
                    return null;
                }
            }
        }
    }



	public class CLLib {
		
		[DllImport("LLib32d.dll")]
		public static extern int LLSetLibraryLicense(string spLicense);
		[DllImport("LLib32d.dll")]
		public static extern void LLSetLicenseFile(string spFile);

		[DllImport("LLib32d.dll")]
		private static extern void LLGetLicenseFile([MarshalAs(UnmanagedType.LPStr)] StringBuilder spFile);
		public static void LLGetLicenseFile(ref string spFile) {
			StringBuilder slString = new StringBuilder(512);
			LLGetLicenseFile(slString);
			spFile = slString.ToString();
		}

		[DllImport("LLib32d.dll")]
		public static extern int  LLOpenLicenseFile(bool bpLock);
		[DllImport("LLib32d.dll")]
		public static extern void LLCloseLicenseFile();
		[DllImport("LLib32d.dll")]
		public static extern int  LLLockLicenseFile(bool bpRetry);
		[DllImport("LLib32d.dll")]
		public static extern void LLUnLockLicenseFile();
		[DllImport("LLib32d.dll")]
		public static extern int  LLWriteDefaultLicense(string spOption, int ipModulus, int ipProduct, int ipAppVersion, int ipUsers, int ipPeriod, int ipLLibVer);
		[DllImport("LLib32d.dll")]
		public static extern int  LLWriteLicense(string spLicense, string spUser, string spCompany);
		[DllImport("LLib32d.dll")]
		public static extern int  LLWriteLicenseUser(string spUser, string spCompany);
		[DllImport("LLib32d.dll")]
		public static extern int  LLCheckLicenseNo(string spOptions, int ipProduct, int ipVersion, int ipKey, int ipModulus);

		[DllImport("LLib32d.dll")]
		private static extern int  LLHttpCheckLicenseNo(string spUrl, string spLicense, string spMachineId, ref int ipRc, [MarshalAs(UnmanagedType.LPStr)] StringBuilder spMessage);
		public static int  LLHttpCheckLicenseNo(string spUrl, string spLicense, string spMachineId, ref int ipRc, ref string spMessage) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLHttpCheckLicenseNo(spUrl, spLicense, spMachineId, ref ipRc, slString);
			spMessage = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		private static extern int  LLGetLicense([MarshalAs(UnmanagedType.LPStr)] StringBuilder spLicense);
		public static int  LLGetLicense(ref string spLicense) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLGetLicense(slString);
			spLicense = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		private static extern int  LLGetLicenseOption([MarshalAs(UnmanagedType.LPStr)] StringBuilder spOption);
		public static int  LLGetLicenseOption(ref string spOption) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLGetLicenseOption(slString);
			spOption = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		public static extern int  LLGetLicenseOptions(ref int ipOptions);

		[DllImport("LLib32d.dll")]
		private static extern int  LLGetLicenseUser([MarshalAs(UnmanagedType.LPStr)] StringBuilder spUser);
		public static int  LLGetLicenseUser(ref string spUser) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLGetLicenseUser(slString);
			spUser = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		private static extern int  LLGetLicenseCompany([MarshalAs(UnmanagedType.LPStr)] StringBuilder spCompany);
		public static int  LLGetLicenseCompany(ref string spCompany) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLGetLicenseCompany(slString);
			spCompany = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		public static extern int  LLGetExpiry(ref int ipDay, ref int ipMonth, ref int ipYear, ref int ipRuns);

		[DllImport("LLib32d.dll")]
		private static extern int  LLDescribeLicense([MarshalAs(UnmanagedType.LPStr)] StringBuilder spDescription);
		public static int  LLDescribeLicense(ref string spDescription) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLDescribeLicense(slString);
			spDescription = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		public static extern bool LLIsLicenseLimited();
		[DllImport("LLib32d.dll")]
		public static extern bool LLIsRunLimited();
		[DllImport("LLib32d.dll")]
		public static extern bool LLIsTimeLimited();
		[DllImport("LLib32d.dll")]
		public static extern bool LLIsFunctionAvailable(int lpFunction);

		[DllImport("LLib32d.dll")]
		private static extern int  LLGetMachineId([MarshalAs(UnmanagedType.LPStr)] StringBuilder spMachineId);
		public static int  LLGetMachineId(ref string spMachineId) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLGetMachineId(slString);
			spMachineId = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		public static extern void LLSetMachineIdDrive(string spDrive);
	    
		[DllImport("LLib32d.dll")]
		private static extern int  LLCalcLicenseNo(int ipVersion, int bpType, string lpszpOption, int ipMod, int ipKey,
													int ipOptions,
													int ipRuns, int ipDays, string lpszpExpiry, string lpszpMachineId,
													int lpLicNo, int lpRegNo, int ipUsers,
													string lpszpName, string lpszpCompany, int ipProduct, int ipAppVersion,
													int lpFlags, [MarshalAs(UnmanagedType.LPStr)] StringBuilder spBuffer);
		public static int  LLCalcLicenseNo(int ipVersion, int bpType, string spOption, int ipMod, int ipKey,
											int ipOptions,
											int ipRuns, int ipDays, string spExpiry, string spMachineId,
											int lpLicNo, int lpRegNo, int ipUsers,
											string spName, string spCompany, int ipProduct, int ipAppVersion,
											int lpFlags, ref string spBuffer) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLCalcLicenseNo(ipVersion, bpType, spOption, ipMod, ipKey,
									   ipOptions,
									   ipRuns, ipDays, spExpiry, spMachineId,
									   lpLicNo, lpRegNo, ipUsers,
									   spName, spCompany, ipProduct, ipAppVersion,
									   lpFlags, slString);
			spBuffer = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		public static extern int  LLValidateLicenseNo(string spOption, int ipMod,
		                                       int ipProduct, string spLicense,
			                                   string spUser, string spCompany);
	    
		[DllImport("LLib32d.dll")]
		private static extern void LLGetLicenseErrorMsg(int ipError, [MarshalAs(UnmanagedType.LPStr)] StringBuilder spMessage);
		public static void LLGetLicenseErrorMsg(int ipError, ref string spMessage) {
			StringBuilder slString = new StringBuilder(512);
			LLGetLicenseErrorMsg(ipError, slString);
			spMessage = slString.ToString();
		}
	    
		[DllImport("LLib32d.dll")]
		public static extern int  LLIncreaseLicenseRuns();
		[DllImport("LLib32d.dll")]
		public static extern int  LLGetLicenseRuns();
	    
		[DllImport("LLib32d.dll")]
		public static extern int  LLGetLicenseUsers();
		[DllImport("LLib32d.dll")]
		public static extern int  LLIncreaseLicenseUsers();
		[DllImport("LLib32d.dll")]
		public static extern int  LLDecreaseLicenseUsers();
		[DllImport("LLib32d.dll")]
		public static extern int  LLGetFreeLicenseUsers();
		[DllImport("LLib32d.dll")]
		public static extern int  LLGetUsedLicenseUsers();    
		[DllImport("LLib32d.dll")]
		public static extern int  LLLockUserLicense();
		[DllImport("LLib32d.dll")]
		public static extern int  LLUnlockUserLicense();

		[DllImport("LLib32d.dll")]
		public static extern int  LLSetLicenseVariable(string spVarName, string spValue);

		[DllImport("LLib32d.dll")]
		private static extern int  LLGetLicenseVariable(string spVarName, [MarshalAs(UnmanagedType.LPStr)] StringBuilder spValue);
		public static int  LLGetLicenseVariable(string spVarName, ref string spValue) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLGetLicenseVariable(spVarName, slString);
			spValue = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		private static extern int  LLGetLicenseVariableName(int ipIndex, [MarshalAs(UnmanagedType.LPStr)] StringBuilder spVarName);
		public static int  LLGetLicenseVariableName(int ipIndex, ref string spVarName) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLGetLicenseVariableName(ipIndex, slString);
			spVarName = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		public static extern int  LLGetLicenseVersion(ref int ipVersion);
		[DllImport("LLib32d.dll")]
		public static extern int  LLSetLicenseVersion(int ipVersion);

		[DllImport("LLib32d.dll")]
		public static extern int  LLWriteExtensionPack(string spLicense);
		[DllImport("LLib32d.dll")]
		public static extern int  LLDeleteExtensionPack(int ipIndex);

		[DllImport("LLib32d.dll")]
		private static extern int  LLGetExtensionPack(int ipIndex, [MarshalAs(UnmanagedType.LPStr)] StringBuilder spLicense);
		public static int  LLGetExtensionPack(int ipIndex, ref string spLicense) {
			StringBuilder slString = new StringBuilder(512);
			int ilRc = LLGetExtensionPack(ipIndex, slString);
			spLicense = slString.ToString();
			return ilRc;
		}

		[DllImport("LLib32d.dll")]
		public static extern int  LLGetMetric(int ipMetric, int ipParam1);



		// License version
		public const int LLMINFILEVERSION	= 103;
		public const int LLFILEVERSION		= 300;

		// General size definitions
		public const int LLMAXLICENSELEN					= 56;
		public const int LLMINLICENSELEN					= 22;
		public const int LLMAXEXTENSIONPACKLICENSELEN		= 30;
		public const int LLMAXUSERNAMELEN        	        = 32;
		public const int LLMAXCOMPANYNAMELEN         	    = 32;
		public const int LLMAXEXTENSIONPACKS				= 16;
		public const int LLMAXPRODCODELEN					= 3;
		public const int LLMAXPRODNAMELEN					= 32;
		public const int LLMAXADDRESSLEN					= 32;
		public const int LLMAXPHONELEN						= 32;
		public const int LLMAXEMAILLEN						= 64;
		public const int LLMAXMACHINEIDLEN					= 8;
		public const int LLMAXMACHINENAMELEN				= 32;
		public const int LLMAXFUNCTIONNAMELEN				= 24;
		public const int LLMAXLICVARIABLES					= 6;
		public const int LLMAXVARTEXTLEN					= 32;

		public const int LLMAXFILENAMELEN					= 255;
	
		// Error codes
		public const int LLER_OK                  			= 0;
		public const int LLER_LICENSEFILEMISSING  			= 1;
		public const int LLER_LICENSEEXPIRED      			= 2;
		public const int LLER_LICENSEUNRECFILE    			= 3;
		public const int LLER_LICENSEUNRECVER     			= 4;
		public const int LLER_LICENSEUNRECLICENSENOVER		= 5;
		public const int LLER_LICENSEINVALIDOPTION			= 6;
		public const int LLER_LICENSEINVALIDPRODCODE		= 7;
		public const int LLER_LICENSEINVALID				= 8;
		public const int LLER_LICENSECREATEFAIL				= 9;
		public const int LLER_EXPIRESONDATE					= 10;
		public const int LLER_EXPIRESONRUNS					= 11;
		public const int LLER_LICENSENOUSERSLEFT			= 12;
		public const int LLER_LICENSETAMPERED				= 13;
		public const int LLER_LICENSEINVALIDMODULUS			= 14;
		public const int LLER_NOTRUNLIMITED					= 15;
		public const int LLER_FILEALREADYEXISTS				= 16;
		public const int LLER_LICENSEDISABLED				= 18;
		public const int LLER_LICENSEOPENFAIL				= 20;
		public const int LLER_LOCKCREATEFAIL				= 21;
		public const int LLER_LOCKOPENFAIL					= 22;
		public const int LLER_LICENSEFILENOTOPEN			= 23;
		public const int LLER_LICENSENOTSET					= 28;
		public const int LLER_INVALIDEXTENSIONPACKLICENSE	= 29;
		public const int LLER_INVALIDPACKFORPRODREG			= 30;
		public const int LLER_EXTENSIONPACKALREADYENTERED	= 31;
		public const int LLER_NOEXTENSIONPACKSPACE			= 32;
		public const int LLER_CANNOTSETDUETOEXTENSIONPACKS	= 33;
		public const int LLER_INVALIDEXTENSIONPACKINDEX		= 35;
		public const int LLER_MACHINEIDERR					= 36;
		public const int LLER_INVALIDMACHINEID				= 37;
		public const int LLER_INVALIDLICENSENOLEN			= 38;
		public const int LLER_INVALIDMACHINEIDFORMAT		= 39;
		public const int LLER_INVALIDENCRYPTIONKEY			= 40;
		public const int LLER_INVALIDEXPIRYDATE				= 41;
		public const int LLER_LICENSEINVALIDTYPE			= 42;
		public const int LLER_INVALIDUSERNAME				= 43;
		public const int LLER_INVALIDCOMPANY				= 44;
		public const int LLER_EXTENSIONPACKSNOTSUPPORTED	= 45;
		public const int LLER_CANTREENTERUNREGLICENSE		= 46;
		public const int LLER_LICENSEINVALIDKEY				= 47;
		public const int LLER_NOLICVARSPACE					= 48;
		public const int LLER_VARIABLENOTFOUND				= 49;
		public const int LLER_VARIABLESNOTSUPPORTED			= 50;
		public const int LLER_INVALIDFILEVERSION			= 51;
		public const int LLER_LLMEMALLOCFAIL				= 52;
		public const int LLER_INVALIDAPPVERSION				= 53;
		public const int LLER_HTTPNOTFOUND					= 54;
		public const int LLER_HTTPNORESPONSE				= 55;

	
		// License types
		public const int LICTYPE_STANDARD					= 0;
		public const int LICTYPE_USERPACK					= 1;

		// License options
		public const string OPTION_UNREGISTERED				= "U";
		public const string OPTION_LIMITED                  = "L";
		public const string OPTION_TEMPORARYREGISTRATION    = "T";
		public const string OPTION_FULLREGISTRATION         = "R";
		public const string OPTION_EXTRAUSERLICENSE			= "X";

		// License functionality options
		public const int OPTION_RUNLIMITED					= 1;
		public const int OPTION_DAYLIMITED					= 2;
		public const int OPTION_EXPIRYDATE					= 4;
		public const int OPTION_MACHINEID					= 8;
		public const int OPTION_USERS						= 16;
		public const int OPTION_NAME						= 32;
		public const int OPTION_COMPANY						= 64;
		public const int OPTION_FUNCTIONFLAGS				= 128;
		public const int OPTION_ACCEPTSEXTENSIONPACKS		= 256;
		public const int OPTION_ACCEPTSVARIABLES			= 512;
		public const int OPTION_APPVERSION					= 1024;

		// Function flags
		public const int LLFUNCTION_1						= 0x1;
		public const int LLFUNCTION_2						= (0x1 << 1);
		public const int LLFUNCTION_3						= (0x1 << 2);
		public const int LLFUNCTION_4						= (0x1 << 3);
		public const int LLFUNCTION_5						= (0x1 << 4);
		public const int LLFUNCTION_6						= (0x1 << 5);
		public const int LLFUNCTION_7						= (0x1 << 6);
		public const int LLFUNCTION_8						= (0x1 << 7);
		public const int LLFUNCTION_9						= (0x1 << 8);
		public const int LLFUNCTION_10						= (0x1 << 9);
		public const int LLFUNCTION_11						= (0x1 << 10);
		public const int LLFUNCTION_12						= (0x1 << 11);
		public const int LLFUNCTION_13						= (0x1 << 12);
		public const int LLFUNCTION_14						= (0x1 << 13);
		public const int LLFUNCTION_15						= (0x1 << 14);
		public const int LLFUNCTION_16						= (0x1 << 15);
		public const int LLFUNCTION_17						= (0x1 << 16);
		public const int LLFUNCTION_18						= (0x1 << 17);
		public const int LLFUNCTION_19						= (0x1 << 18);
		public const int LLFUNCTION_20						= (0x1 << 19);
		public const int LLFUNCTION_21						= (0x1 << 20);
		public const int LLFUNCTION_22						= (0x1 << 21);
		public const int LLFUNCTION_23						= (0x1 << 22);
		public const int LLFUNCTION_24						= (0x1 << 23);
		public const int LLFUNCTION_25						= (0x1 << 24);
		public const int LLFUNCTION_26						= (0x1 << 25);
		public const int LLFUNCTION_27						= (0x1 << 26);
		public const int LLFUNCTION_28						= (0x1 << 27);
		public const int LLFUNCTION_29						= (0x1 << 28);
		public const int LLFUNCTION_30						= (0x1 << 29);
		public const int LLFUNCTION_31						= (0x1 << 30);
		public const int LLFUNCTION_32						= (0x1 << 31);

		// Metrics
		public const int LLMETRIC_NEXTVERSION				= 1;
		public const int LLMETRIC_MINVERSION				= 2;
		public const int LLMETRIC_MAXVERSION				= 3;
		public const int LLMETRIC_NAMELEN					= 4;
		public const int LLMETRIC_COMPANYLEN				= 5;
		public const int LLMETRIC_MINKEY					= 6;
		public const int LLMETRIC_MAXKEY					= 7;
		public const int LLMETRIC_SUPPORTSKEYS				= 8;
		public const int LLMETRIC_SUPPORTSEXTPACKS			= 9;
		public const int LLMETRIC_SUPPORTSVARIABLES			= 10;
		public const int LLMETRIC_SUPPORTSMACHINEID			= 11;
		public const int LLMETRIC_SUPPORTSNAMEENCODING		= 12;


		// License checking - used by sample applications to perform remote validation
		public const string HTTPCHECKURL = "http://www.gppsoftware.com/LLLIB/LLIBTest.asp";
		//public const string HTTPCHECKURL	"http://ittgplap/LLIBTest.asp"
	
	
		// Methods
	}
}
