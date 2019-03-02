using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;



namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{

    public static class WebServiceConfig
    {
        private static string _codeBaseUrl = null;
        private static bool _logErrors = true;
        private static bool _logInfo = true;
        private static bool _logDetInfo = true;
        private static string _CAG_FQDN = null;
        private static string _CAG_Port = null;
        private static string _STA_URL = null;
        private static bool _useCAG = true;
        private static string _ICAPORT = null;
        private static string _ADDRESS = null;
        private static string _STA_FailOver = null;
        private static string _CTXXMLPort = null;
				private static string _iconEveryoneRole = null;
        private const string ConfigFileName = "Extentrix.config";

        private static void Init()
        {
            try
            {
                var filePath = Path.Combine(SPUtility.GetGenericSetupPath("CONFIG\\Extentrix"), ConfigFileName);
                //var filePath = Path.Combine("c:\\Program Files\\Common Files\\Microsoft Shared\\Web Server Extensions\\14\\CONFIG\\Extentrix", ConfigFileName);

                var fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = filePath;
                var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                _codeBaseUrl = config.AppSettings.Settings["CodeBaseUrl"].Value.Trim().TrimEnd('/');
                _logErrors = bool.Parse(config.AppSettings.Settings["LogErrors"].Value);
                _logInfo = bool.Parse(config.AppSettings.Settings["LogInfo"].Value);
                _logDetInfo = bool.Parse(config.AppSettings.Settings["LogDetInfo"].Value);
                _CAG_FQDN = config.AppSettings.Settings["CAG_FQDN"].Value;
                _CAG_Port = config.AppSettings.Settings["CAG_PORT"].Value;
                _STA_URL = config.AppSettings.Settings["STA_URL"].Value;
                _useCAG = bool.Parse(config.AppSettings.Settings["UseCAG"].Value);
                _ICAPORT = config.AppSettings.Settings["ICAPort"].Value;
                _ADDRESS = config.AppSettings.Settings["PSAddress"].Value;
                _STA_FailOver = config.AppSettings.Settings["STA_FailOver"].Value;
                _CTXXMLPort = config.AppSettings.Settings["CTXXMLPort"].Value;
								_iconEveryoneRole = config.AppSettings.Settings["EveryoneRole"].Value;
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }

        }

				public static string EveryoneRole
				{
					get
					{
						if (_iconEveryoneRole == null)
						{
							Init();
						}
						return _iconEveryoneRole;
					}
				}

        public static string CodeBaseUrl
        {
            get
            {
                if (_codeBaseUrl == null)
                {
                    Init();
                }
                return _codeBaseUrl;
            }
        }

        public static bool LogErrors
        {
            get
            {
                Init();

                return _logErrors;
            }
        }

        public static bool LogInfo
        {
            get
            {
                Init();
                return _logInfo;
            }
        }

        public static bool LogDetInfo
        {
            get
            {
                Init();
                return _logDetInfo;
            }
        }

        public static string CAG_FQDN
        {
            get
            {
                if (_CAG_FQDN == null)
                {
                    Init();
                }
                return _CAG_FQDN;
            }
        }

        public static string CAG_Port
        {
            get
            {
                if (_CAG_Port == null)
                {
                    Init();
                }
                return _CAG_Port;
            }
        }

        public static string STA_URL
        {
            get
            {
                if (_STA_URL == null)
                {
                    Init();
                }
                return _STA_URL;
            }
        }

        public static bool UseCAG
        {
            get
            {
                Init();
                return _useCAG;
            }
        }

        public static string ICAPORT
        {
            get
            {
                if (_ICAPORT == null)
                {
                    Init();
                }
                return _ICAPORT;
            }
        }

        public static string ADDRESS
        {
            get
            {
                if (_ADDRESS == null)
                {
                    Init();
                }
                return _ADDRESS;
            }
        }


        public static string STA_FailOver
        {
            get
            {
                if (_STA_FailOver == null)
                {
                    Init();
                }
                return _STA_FailOver;
            }
        }

        public static string CTXXMLPort
        {
            get
            {
                if (_CTXXMLPort == null)
                {
                    Init();
                }
                return _CTXXMLPort;
            }
        }
    }

}
