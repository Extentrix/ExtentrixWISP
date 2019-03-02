using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Net.NetworkInformation;
using System.IO;
using System.Web;
using System.Security.AccessControl;
using Microsoft.SharePoint;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;
using System.Security.Cryptography.X509Certificates;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PLicenseInfo
    {
        public bool isTrial;
        public int nDay;
        public int nMonth;
        public int nYear;
        public int nNoOfMacAddresses;
        public IntPtr aMacAddresses;
    }

    public enum LicenseValidationCode
    {
        Ok,
        NotValid,
        Expired,
        SoonBeExpired
    }
    public static class LicenseManager
    {

        /*static PLicenseInfo licInfo;
        static bool isInit;*/

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        static extern bool FreeLibrary(IntPtr hModule);

        delegate bool IsLicenseValidDelegate(IntPtr arg1);
        delegate void GetLicenseInfoDelegate(IntPtr pLicenseInfo);

        public const int WarningDaysNumber = 10;

        private static int[] getMACasIntArr(string mac)
        {
            string[] arr_mac = mac.Split('-');
            int[] res = new int[arr_mac.Length];
            for (int i = 0; i < arr_mac.Length; i++)
            {
                res[i] = int.Parse(arr_mac[i], NumberStyles.HexNumber);
            }
            return res;
        }

        public static LicenseValidationCode CheckLicense(string macAdress)
        {
            IntPtr pointer = IntPtr.Zero;
            IntPtr licenseDll = IntPtr.Zero;
            int[] intMAC = getMACasIntArr(macAdress);
            var isValid = false;
            var validationCode = LicenseValidationCode.Ok;
            var licensePath = Path.Combine(HttpRuntime.BinDirectory, "Ext-License.dll");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string subj = string.Empty;
                try
                {
                    X509Certificate2 cert = new X509Certificate2(licensePath);
                    subj = cert.Subject;
                }
                catch (Exception ex)
                {
                    EventLogger.logEvent("License certificate error" + ex.Message, 0);
                    validationCode = LicenseValidationCode.NotValid;
                }

                if (subj.Contains("Extentrix Systems"))
                {
                    try
                    {
                        int nParts = intMAC.Length;
                        pointer = Marshal.AllocHGlobal(nParts * Marshal.SizeOf(typeof(int)));
                        Marshal.Copy(intMAC, 0, pointer, (int)intMAC.Length);

                        licenseDll = LoadLibrary(licensePath);
                        IntPtr procAddress = GetProcAddress(licenseDll, "fnIsLicenseValid");
                        IsLicenseValidDelegate isLicenseValidFunc = (IsLicenseValidDelegate)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(IsLicenseValidDelegate));

                        isValid = isLicenseValidFunc(pointer);
                        if (!isValid)
                        {
                            validationCode = LicenseValidationCode.NotValid;
                            //EventLogger.logEvent("License validation failed.", 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        validationCode = LicenseValidationCode.NotValid;
                        EventLogger.logEvent("Licensing Error: " + ex.Message, 0);
                    }
                    finally
                    {
                        FreeLibrary(licenseDll);
                        Marshal.FreeHGlobal(pointer);
                    }


                    var licenseInfo = GetLicenseInfo();
                    //EventLogger.logEvent("License validated successfully.", -1);
                    if (licenseInfo.isTrial)
                    {
                        DateTime expirationDate = new DateTime(licenseInfo.nYear, licenseInfo.nMonth, licenseInfo.nDay);
                        var todayDate = DateTime.Today;
                        if (expirationDate < todayDate)
                        {
                            validationCode = LicenseValidationCode.Expired;
                            //EventLogger.logEvent("License expired.", 0);
                        }
                        else if (expirationDate.AddDays(-WarningDaysNumber) < todayDate)
                        {
                            validationCode = LicenseValidationCode.SoonBeExpired;
                            //EventLogger.logEvent("License expires at " + expirationDate.ToShortDateString() + ".", -1);
                        }
                    }

                }
                else
                {
                    validationCode = LicenseValidationCode.NotValid;
                    EventLogger.logEvent("License certificate error.", 0);
                }
            });

            return validationCode;
        }

        private static PLicenseInfo GetLicenseInfo()
        {
            IntPtr pointer = IntPtr.Zero;
            IntPtr licenseDll = IntPtr.Zero;


            PLicenseInfo linf;
            linf.isTrial = false;
            linf.nDay = 0;
            linf.nMonth = 1;
            linf.nYear = 0;
            linf.nNoOfMacAddresses = 0;
            linf.aMacAddresses = IntPtr.Zero;
        		//linf.pszCompanyName = new char[260];
						//linf.pszCompanyName = "";
            PLicenseInfo licInfo = new PLicenseInfo();

            var licensePath = Path.Combine(HttpRuntime.BinDirectory, "Ext-License.dll");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string subj = string.Empty;
                try
                {
                    X509Certificate2 cert = new X509Certificate2(licensePath);
                    subj = cert.Subject;
                }
                catch (Exception ex)
                {
                    EventLogger.logEvent("License certificate error: " + ex.Message, 0);
                }

                if (subj.Contains("Extentrix Systems"))
                {
                    try
                    {
                        pointer = Marshal.AllocHGlobal(Marshal.SizeOf(linf));
                        licenseDll = LoadLibrary(licensePath);
                        IntPtr procAddress = GetProcAddress(licenseDll, "fnGetLicenseInfo");
                        GetLicenseInfoDelegate licenseInfoFunc = (GetLicenseInfoDelegate)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(GetLicenseInfoDelegate));

                        // Copy the struct to unmanaged memory.
                        Marshal.StructureToPtr(linf, pointer, false);

                        licenseInfoFunc(pointer);
                        licInfo = (PLicenseInfo)Marshal.PtrToStructure(pointer, typeof(PLicenseInfo));

                    }
                    catch (Exception ex)
                    {
                        EventLogger.logEvent("Licensing Error: " + ex.Message, 0);
                    }
                    finally
                    {
                        FreeLibrary(licenseDll);
                        Marshal.FreeHGlobal(pointer);
                    }
                }
                else
                {
                    EventLogger.logEvent("License certificate error.", 0);
                }

            });
            return licInfo;
        }

        public static string LicenseExpirationDate
        {
            get
            {
                var licInfo = GetLicenseInfo();
                return string.Format("{0}.{1}.{2}", licInfo.nDay, licInfo.nMonth, licInfo.nYear);
            }

        }

        public static bool? IsTrial
        {
            get
            {
                var licInfo = GetLicenseInfo();
                if (licInfo.nYear != 0)
                {
                    return licInfo.isTrial;
                }
                else
                {
                    return null;
                }
            }

        }

    }
}
