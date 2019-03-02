using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
	public class EventLogger
	{
		public static void logEvent(string info, int type)
		{
			try
			{
				SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", type == 0 ? TraceSeverity.Unexpected : TraceSeverity.Monitorable, type == 0 ? EventSeverity.Error : EventSeverity.Information), type == 0 ? TraceSeverity.Unexpected : TraceSeverity.Monitorable, info);
				SPSecurity.RunWithElevatedPrivileges(delegate()
				{
					string source = "Extentrix Web Interface for SharePoint";
					string log = "EXTWISP";

					/*if (!EventLog.SourceExists(source))
						EventLog.CreateEventSource(source, log);

					EventLog aLog = new EventLog(log);
					aLog.Source = source;
					aLog.Log = log;
					aLog.WriteEntry(source + ": " + info, (type == 0 ? EventLogEntryType.Error : EventLogEntryType.Information));
					aLog.Close();*/

				});
			}
			catch (Exception ex)
			{
				SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Extentrix", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message);
			}
		}
	}
}
