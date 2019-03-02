using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// Summary description for Assembler
/// </summary>
public class Assembler
{
    static public string assembleClientTypes(string[] clientTypes)
    {
        bool[] requiredClients ={ false, false, false, false, false };
        string[] Clients = { "ica30", "rdp", "content", "rade", "all" };
        string returnTag = null;
        for (int i = 0; i < clientTypes.Length; i++)
            for (int j = 0; j < Clients.Length; j++)
                if (clientTypes[i] == Clients[j])
                    requiredClients[j] = true;
        for (int i = 0; i < requiredClients.Length; i++)
            if (requiredClients[i])
                returnTag += "<ClientType>" + Clients[i] + "</ClientType>";
        return returnTag;

    }
    static public string assembleServerTypes(string[] serverTypes)
    {
        bool[] requiredServers ={ false, false, false };
        string[] servers = { "win32", "x", "all" };
        string returnTag = "";
        for (int i = 0; i < serverTypes.Length; i++)
            for (int j = 0; j < servers.Length; j++)
                if (serverTypes[i] == servers[j])
                    requiredServers[j] = true;
        for (int i = 0; i < requiredServers.Length; i++)
            if (requiredServers[i])
                returnTag += "<ServerType>" + servers[i] + "</ServerType>";
        return returnTag;

    }
    static public string assembleDetails(string[] desiredDetails)
    {
        bool[] requiredDetail ={ false, false, false, false, false, false };
        string[] details = { "defaults", "icon", "access-list", "file-type", "rade-offline-mode", "icon-info" };
        string returnTag = "";
        if (desiredDetails != null)
        {
            for (int i = 0; i < desiredDetails.Length; i++)
                for (int j = 0; j < details.Length; j++)
                    if (desiredDetails[i] == details[j])
                        requiredDetail[j] = true;
            for (int i = 0; i < requiredDetail.Length; i++)
                if (requiredDetail[i])
                    returnTag += " <DesiredDetails>" + details[i] + "</DesiredDetails>";
        }

        return returnTag;

    }

   static public string assembleClientID(int idMethod, string clientID)
    {
        if (idMethod == 0)
            return "<ClientName>" + HttpUtility.HtmlAttributeEncode(clientID) + "</ClientName>";
        else if (idMethod == 1)
            return "<DeviceId>" + HttpUtility.HtmlAttributeEncode(clientID) + "</DeviceId>";
        else
            return "<ClientName/>";
    }

   //static public string assembleGroups(Groups[] groups)
   // {
   //     string resultTag = "";
   //     for (int i = 0; i < groups.Length; i++)
   //     {
   //         resultTag += "<Group>";
   //         for (int j = 0; j < groups[i].GroupNames.Length; j++)
   //             resultTag += ("<GroupName>" + groups[i].GroupNames[j] + "</GroupName>");
   //         resultTag += ("<Domain type =\"" + groups[i].DomainType + "\">");
   //         if (groups[i].DomainType != "UNIX")
   //             resultTag += groups[i].DomainName;
   //         resultTag += "</Domain>";
   //         resultTag += "</Group>";
   //     }
   //     return resultTag;
   // }

    internal static string reconstructResponse(string lcXml)
    {
        string []separator = {"\r\n"};
        string [] chunks = lcXml.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        string response = "";
        for (int i = 1; i < chunks.Length; i += 2)
            response += chunks[i];
        return response;
        
    }

    internal static string assembleAnonymousUser()
    {
        return "<Credentials><AnonymousUser/></Credentials>";
    }

    internal static string assembleAllUser()
    {
        return "";
    }
}
