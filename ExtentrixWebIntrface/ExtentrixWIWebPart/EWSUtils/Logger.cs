using System.IO;
using Microsoft.SharePoint.Utilities;
using System;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using System.Diagnostics;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
    public class Logger
    {

        private const string ConfigFileName = "Extentrix.config";

        private static Logger _default;

        static Logger()
        {
            string path = Path.Combine(SPUtility.GetGenericSetupPath("CONFIG\\Extentrix"), ConfigFileName);
            var configFile = new FileInfo(path);
            log4net.Config.XmlConfigurator.Configure(configFile);
        }

        private readonly log4net.ILog _log;

        private Logger(log4net.ILog log)
        {
            _log = log;
        }

        public static Logger GetLogger(string name)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(name);
            return new Logger(log);
        }

        public static Logger Default
        {
            get
            {
                if (_default == null)
                {
                    log4net.ILog log = log4net.LogManager.GetLogger("Logger");
                    _default = new Logger(log);
                    _default.Info("Extentrix WISP Log File: log.txt");
                    _default.Info("Copyright: Extentrix Systems Copyright 2011");
                    Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    _default.Info("Product Version: " + v.Revision + " Build Number:" + v.Build);
                    DateTime now = System.DateTime.Now;
                    _default.Info("Date : " + now.ToLongDateString());
                    _default.Info("Time : " + now.ToLongTimeString());
                    _default.Info("--------------------------");
                }


                return _default;
            }
        }

        public void Info(string message)
        {            
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                _log.Info(message);
            });
        }

        public void Info(LogLocationEnum location, string message)
        {
					if (!WSConstants.LOGINFO)
					{
						//return;
					}
            switch (location)
            {
                case LogLocationEnum.Default:
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Monitorable, EventSeverity.Information), TraceSeverity.Monitorable, message);
                    break;
                case LogLocationEnum.Text:
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        _log.Info(message);
                    });
                    break;
                case LogLocationEnum.EventViewer:
                    try
                    {
                        SPSecurity.RunWithElevatedPrivileges(delegate()
                        {
                            string source = "Extentrix Web Interface for SharePoint";
                            string log = "EXTWISP";

                            if (!EventLog.SourceExists(source))
                                EventLog.CreateEventSource(source, log);

                            EventLog aLog = new EventLog(log);
                            aLog.Source = source;
                            aLog.Log = log;
                            aLog.WriteEntry(source + ": " + message, EventLogEntryType.Information);
                            aLog.Close();

                        });
                    }
                    catch (Exception e)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, e.Message, e.StackTrace);
                    }
                    break;
                default:
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Monitorable, EventSeverity.Information), TraceSeverity.Monitorable, message);
                    break;
            }


        }

        public void Debug(string message)
        {
            _log.Debug(message);
        }

        public void Warn(string message)
        {
            _log.Warn(message);
        }

        public void Error(string message)
        {
            _log.Error(message);
        }

        public void Error(string message, Exception ex)
        {            
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                _log.Error(message, ex);
            });
        }

        public void Error(LogLocationEnum location, string message, Exception ex)
        {
            switch (location)
            {
                case LogLocationEnum.Default:
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, message, ex.StackTrace);
                    break;
                case LogLocationEnum.Text:
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        _log.Error(message, ex);
                    });
                    break;
                case LogLocationEnum.EventViewer:
                    try
                    {                        
                        SPSecurity.RunWithElevatedPrivileges(delegate()
                        {
                            string source = "Extentrix Web Interface for SharePoint";
                            string log = "EXTWISP";

                            if (!EventLog.SourceExists(source))
                                EventLog.CreateEventSource(source, log);

                            EventLog aLog = new EventLog(log);
                            aLog.Source = source;
                            aLog.Log = log;
                            aLog.WriteEntry(source + ": " + message, EventLogEntryType.Error);
                            aLog.Close();

                        }
                        );
                    }
                    catch (Exception e)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, e.Message, e.StackTrace);
                    }
                    break;
                default:
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, message, ex.StackTrace);
                    break;
            }
            
            
        }

        public void Fatal(string message, Exception ex)
        {
            _log.Fatal(message, ex);
        }

    }
}
