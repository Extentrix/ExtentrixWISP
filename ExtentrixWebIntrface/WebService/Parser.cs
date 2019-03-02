using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for Parser
/// </summary>
public class Parser
{
    static public ApplicationItemEx[] parseAppData(string responseText)
    {        
        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);   

        xmlDoc.LoadXml(responseText);
        string MyPath = "NFuseProtocol/ResponseAppData/AppDataSet/AppData";
        var appNodeList = xmlDoc.SelectNodes(MyPath);
        
        var appsList = new List<ApplicationItemEx>();
        foreach (XmlNode appNode in appNodeList)
        {           
            //if(appNode.SelectSingleNode("ClientType").FirstChild.Value == "content")
            //  continue;

            var app = new ApplicationItemEx();
            app.InternalName = appNode.SelectSingleNode("InName").FirstChild.Value;
            app.FreindlyName = appNode.SelectSingleNode("FName").FirstChild.Value;
                        
            // default 32 *32 icon 

            XmlNode iconNode = appNode.SelectSingleNode("Details/Icon");
            if (iconNode != null)
            {
                app.Icon = IconMaker.makeIcon(appNode.SelectSingleNode("Details/Icon").FirstChild.Value);                
            }
            //iconNode = appNode.selectSingleNode("IconData");
                //if(iconNode != null)
                //    apps[j].Icon = IconMaker.makeIcon((string)iconNode.firstChild.nodeValue,16);
           
            // access list
            XmlNode accessListNode = appNode.SelectSingleNode("Details/AccessList");
             if (accessListNode != null)
             {
                    XmlNodeList usersNodes = accessListNode.SelectNodes("User");
                    if (usersNodes != null && usersNodes.Count != 0)
                        app.users = setUsers(usersNodes);
                    XmlNodeList groupsNodes = accessListNode.SelectNodes("Group");
                    if (groupsNodes != null)
                        app.groups = setGroups(groupsNodes);

             }

            // settings
            bool nodeTypeFlag=false;
            XmlNode settingsNode = appNode.SelectSingleNode("Details/Settings");
            if (settingsNode == null)
            {
                settingsNode = appNode.SelectSingleNode("Details/ContentSettings");
                nodeTypeFlag = true;
            }
            if (settingsNode != null)
            {
                // add three nodes seqNo, ClientType, ServerType to settingNode
                XmlNode seqNo = appNode.SelectSingleNode("SeqNo");
                if (seqNo != null)
                    settingsNode.AppendChild(seqNo);
                XmlNode serverType = appNode.SelectSingleNode("ServerType");
                if (serverType != null)
                    settingsNode.AppendChild(serverType);
                XmlNode clientType = appNode.SelectSingleNode("ClientType");
                if (clientType != null)
                    settingsNode.AppendChild(clientType);
                // end block
                setDefaultSettings(ref app, settingsNode, nodeTypeFlag);
            }
            //file types
            XmlNodeList fileTypeNodes = appNode.SelectNodes("Details/FileType");
            if (fileTypeNodes != null && fileTypeNodes.Count != 0)
                app.fileTypes= setFileTypes(fileTypeNodes);
            //icon info
            XmlNodeList iconInfoNodes = appNode.SelectNodes("Details/IconInfo/IconType");
            if (iconInfoNodes != null && iconInfoNodes.Count != 0)
            {
                app.availableIcons = setIconInfo(iconInfoNodes);
            }
            appsList.Add(app);
        }

        return appsList.ToArray();
    }

    private static IconInfo[] setIconInfo(XmlNodeList iconInfoNodes)
    {
        var iconsList = new List<IconInfo>(iconInfoNodes.Count);
        foreach (XmlNode iconInfoNode in iconInfoNodes)
        {
            var iconInfo = new IconInfo();
            iconInfo.format = iconInfoNode.Attributes["format"].FirstChild.Value;
            iconInfo.size = int.Parse(iconInfoNode.Attributes["size"].FirstChild.Value);
            iconInfo.depth = int.Parse(iconInfoNode.Attributes["bpp"].FirstChild.Value);
            
            iconsList.Add(iconInfo);
        }
        return iconsList.ToArray();
    }

    private static Groups[] setGroups(XmlNodeList groupsNodes)
    {
        var groupsList = new List<Groups>(groupsNodes.Count);
        foreach (XmlNode groupsNode in groupsNodes)        
        {
            var groups = new Groups();
            XmlNodeList groupNames = groupsNode.SelectNodes("GroupName");
            if (groupNames != null && groupNames.Count != 0)
            {
                groups.GroupNames = new string[groupNames.Count];
                for (int j = 0; j < groupNames.Count; j++)
                {
                    XmlNode groupName = groupsNodes[j];
                    groups.GroupNames[j] = groupName.FirstChild.Value;
                }
            }
            groups.DomainName = groupsNode.SelectSingleNode("Domain").FirstChild.Value;
            groups.DomainType = groupsNode.SelectSingleNode("Domain").Attributes["type"].FirstChild.Value;

            groupsList.Add(groups);
        }
        return groupsList.ToArray();
    }

   static private User[] setUsers(XmlNodeList usersNodes)
    {
        var users = new List<User>(usersNodes.Count);
        foreach(XmlNode userNode in usersNodes)
        {         
            var user = new User();
            user.Name = userNode.SelectSingleNode("UserName").FirstChild.Value;
            user.DomainType = userNode.SelectSingleNode("Domain").Attributes[0].Value;
            if (userNode.SelectSingleNode("Domain").FirstChild != null)
            {
                user.DomainName = userNode.SelectSingleNode("Domain").FirstChild.Value;
            }
            users.Add(user);
        }
        return users.ToArray();
    }

    static public FileType[] setFileTypes(XmlNodeList fileTypeNodes)
    {
        var fileTypeList = new List<FileType>(fileTypeNodes.Count);
        foreach (XmlNode fileTypeNode in fileTypeNodes)
        {       
            var fileTypes = new FileType();            
            fileTypes.FileTypeName = fileTypeNode.Attributes["name"].FirstChild.Value;//.FirstChild.nodeValue;
            if (fileTypeNode.Attributes.Count >= 2)
            {
                string isDefault = fileTypeNode.Attributes["isdefault"].FirstChild.Value;
                fileTypes.IsDefault = (isDefault == "true");
            }
            if (fileTypeNode.Attributes.Count == 3)
            {
                string overwrite = fileTypeNode.Attributes["overwrite"].FirstChild.Value;
                fileTypes.Overwrite = (overwrite == "true");
            }
            XmlNodeList fileExtentionNodes = fileTypeNode.SelectNodes("FileExtension");
            if (fileExtentionNodes != null && fileExtentionNodes.Count != 0)
            {
                fileTypes.FileExtension = new string[fileExtentionNodes.Count];
                for (int j = 0; j < fileExtentionNodes.Count; j++)
                {
                    fileTypes.FileExtension[j] = fileExtentionNodes.Item(j).FirstChild.Value;
                }
            }
            XmlNodeList mimeTypes = fileTypeNode.SelectNodes("MimeType");
            if (mimeTypes != null && mimeTypes.Count != 0)
            {
                fileTypes.MimeType = new string[mimeTypes.Count];
                for (int j = 0; j < mimeTypes.Count; j++)
                {
                    fileTypes.MimeType[j] = mimeTypes.Item(j).FirstChild.Value;                    
                }
            }
            if (fileTypeNode.SelectSingleNode("Parameters") != null)
                fileTypes.Parameters = fileTypeNode.SelectSingleNode("Parameters").FirstChild.Value;

            fileTypeList.Add(fileTypes);

        }
        return fileTypeList.ToArray();
    }

    static public void setDefaultSettings(ref ApplicationItemEx applicationItemEx, XmlNode settingsNode, bool isContent)
    {
        XmlNode folderNode = settingsNode.SelectSingleNode("Folder");
        XmlNode descNode = settingsNode.SelectSingleNode("Description");
        XmlNode remoteAccessNode = settingsNode.SelectSingleNode("RemoteAccessEnabled");

        if (folderNode != null)
            if (folderNode.FirstChild != null )
                applicationItemEx.FolderName = folderNode.FirstChild.Value;
        if (descNode != null)
            if (descNode.FirstChild != null)
                applicationItemEx.Description = descNode.FirstChild.Value;
        if (remoteAccessNode != null)
            applicationItemEx.RemoteAccessEnabled = remoteAccessNode.FirstChild.Value;

        XmlNode seqNo = settingsNode.SelectSingleNode("SeqNo");
        if (seqNo != null)
            applicationItemEx.SequenceNumber = seqNo.FirstChild.Value;
        XmlNode serverType = settingsNode.SelectSingleNode("ServerType");
        if (serverType != null)
            applicationItemEx.ServerType = serverType.FirstChild.Value;
        XmlNode clientType = settingsNode.SelectSingleNode("ClientType");
        if (clientType != null)
            applicationItemEx.ClientType = clientType.FirstChild.Value;
        // those data returned only for applications that are not contents 
        if (!isContent)
        {
            try
            {
                applicationItemEx.WinWidth = settingsNode.SelectSingleNode("WinWidth").FirstChild.Value;
                applicationItemEx.WinHeight = settingsNode.SelectSingleNode("WinHeight").FirstChild.Value;
                if (settingsNode.SelectSingleNode("WinColor") != null)
                    applicationItemEx.WinColor = settingsNode.SelectSingleNode("WinColor").FirstChild.Value;
                if (settingsNode.SelectSingleNode("WinScale") != null)
                    applicationItemEx.WinScale = settingsNode.SelectSingleNode("WinScale").FirstChild.Value;
                if (settingsNode.SelectSingleNode("WinType") != null)
                    applicationItemEx.WinType = settingsNode.SelectSingleNode("WinType").FirstChild.Value;
                if (settingsNode.SelectSingleNode("SoundType") != null)
                {
                    applicationItemEx.SoundType = settingsNode.SelectSingleNode("SoundType").FirstChild.Value;
                    if (settingsNode.SelectSingleNode("SoundType").Attributes != null && settingsNode.SelectSingleNode("SoundType").Attributes.Count != 0)
                        applicationItemEx.SoundTypeMinimum = settingsNode.SelectSingleNode("SoundType").Attributes[0].FirstChild.Value;
                }
                if (settingsNode.SelectSingleNode("VideoType") != null)
                {
                    applicationItemEx.VideoType = settingsNode.SelectSingleNode("VideoType").FirstChild.Value;
                    if (settingsNode.SelectSingleNode("VideoType").Attributes != null && settingsNode.SelectSingleNode("VideoType").Attributes.Count != 0)
                        applicationItemEx.VideoTypeMinimum = settingsNode.SelectSingleNode("VideoType").Attributes[0].FirstChild.Value;
                }

                if (settingsNode.SelectSingleNode("Encryption") != null)
                {
                    applicationItemEx.Encryption = settingsNode.SelectSingleNode("Encryption").FirstChild.Value;
                    if (settingsNode.SelectSingleNode("Encryption").Attributes != null && settingsNode.SelectSingleNode("Encryption").Attributes.Count != 0)
                        applicationItemEx.EncryptionMinimum = settingsNode.SelectSingleNode("Encryption").Attributes[0].FirstChild.Value;
                }
                if (settingsNode.SelectSingleNode("PublisherName") != null)
                    applicationItemEx.PublisherName = settingsNode.SelectSingleNode("PublisherName").FirstChild.Value;
                applicationItemEx.SSLEnabled = settingsNode.SelectSingleNode("SSLEnabled").FirstChild.Value;
                if (settingsNode.SelectSingleNode("AppInStartmenu").FirstChild != null)
                {
                    applicationItemEx.StartmenuFolder = settingsNode.SelectSingleNode("AppInStartmenu").FirstChild.Value;
                    if (settingsNode.SelectSingleNode("AppInStartmenu").Attributes.Count == 2)
                        applicationItemEx.StartMenuRoot = settingsNode.SelectSingleNode("AppInStartmenu").Attributes["root"].FirstChild.Value;
                    if (settingsNode.SelectSingleNode("AppInStartmenu").Attributes != null && settingsNode.SelectSingleNode("AppInStartmenu").Attributes.Count != 0)
                        applicationItemEx.AppInStartmenu = settingsNode.SelectSingleNode("AppInStartmenu").Attributes["value"].FirstChild.Value;
                }
                applicationItemEx.AppOnDesktop = settingsNode.SelectSingleNode("AppOnDesktop").Attributes[0].FirstChild.Value;
            }
            catch
            {


            }
        }
        else
        {
            // contents only 
            try
            {
                applicationItemEx.ContentAddress = settingsNode.SelectSingleNode("ContentAddress").FirstChild.Value;
            }
            catch
            {

            }
        }
       

    }

    static public bool checkErrorResponse(string responseText, out string errorMessage)
    {
				if (string.IsNullOrEmpty(responseText))
				{
					errorMessage = "Empty response text";
					return true;
				}

        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);   
        xmlDoc.LoadXml(responseText);

        errorMessage = string.Empty;
        XmlNodeList errorList = xmlDoc.GetElementsByTagName("ErrorId");
        if (errorList.Count != 0)
        {
            string errorId = errorList[0].FirstChild.Value;
            errorMessage = WSConstants.ErrorMessages(errorId);
            XmlNodeList MPSErrorList = xmlDoc.GetElementsByTagName("MPSError");
            if (MPSErrorList.Count != 0)
            {
                string type = MPSErrorList[0].Attributes[0].FirstChild.Value;
                string errorCode = MPSErrorList[0].FirstChild.Value;
                errorMessage += (" " + "MPSERROR_" + type + "_" + errorCode);
            }
            XmlNodeList BrowserErrorList = xmlDoc.GetElementsByTagName("BrowserError");
            if (BrowserErrorList.Count != 0)
            {
                string errorCode = BrowserErrorList[0].FirstChild.Value;
                errorMessage += (" " + "BrowserError_" + errorCode);
            }
            return true;
        }
        else
            return false;
    }

   internal static string parseAddressResponse(string responseText,ref string serverAddress)
    {
        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);   

        xmlDoc.LoadXml(responseText);

        string MyPath = "NFuseProtocol/ResponseAddress/ServerAddress";
        XmlNode node = xmlDoc.SelectSingleNode(MyPath);
        serverAddress = node.FirstChild.Value;
        MyPath = "NFuseProtocol/ResponseAddress/TicketTag";
        node =  xmlDoc.SelectSingleNode(MyPath);
        string ticketTag = node.FirstChild.Value;
        return ticketTag;

    }

    internal static string parseTicketResponse(string responseText)
    {
        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);   
        xmlDoc.LoadXml(responseText);

        string MyPath = "NFuseProtocol/ResponseTicket/Ticket";
        XmlNode ticketNode = xmlDoc.SelectSingleNode(MyPath);
        string ticket = ticketNode.FirstChild.Value;
        return ticket;
    }

    internal static string parseFarmDataResponse(string responseText)
    {
        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);   
        xmlDoc.LoadXml(responseText);

        string MyPath = "NFuseProtocol/ResponseServerFarmData/ServerFarmData";
        XmlNode farmNode = xmlDoc.SelectSingleNode(MyPath);
        string farmName = farmNode.FirstChild.FirstChild.Value;
        return farmName;
    }

    internal static void parseSTATicketResponse(string responseText, ref string AuthorityID, ref string Ticket, ref string TicketVersion)
    {
        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);                 
        xmlDoc.LoadXml(responseText);

        string AuthorityIDPath = "CtxSTAProtocol/ResponseTicket/AuthorityID";
        XmlNode AuthorityIDNode = xmlDoc.SelectSingleNode(AuthorityIDPath);
        AuthorityID = AuthorityIDNode.FirstChild.Value;

        string TicketPath = "CtxSTAProtocol/ResponseTicket/Ticket";
        XmlNode TicketNode = xmlDoc.SelectSingleNode(TicketPath);
        Ticket = TicketNode.FirstChild.Value;

        string TicketVersionPath = "CtxSTAProtocol/ResponseTicket/TicketVersion";
        XmlNode TicketVersionNode = xmlDoc.SelectSingleNode(TicketVersionPath);
        TicketVersion = TicketVersionNode.FirstChild.Value;

        
    }

    internal static ICASession[] parseSessions(string responseText)
    {
        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);   
        xmlDoc.LoadXml(responseText);

        string MyPath = "NFuseProtocol/ResponseReconnectSessionData/AppDataSet/AppData";
        XmlNodeList sessionsNodeList = xmlDoc.SelectNodes(MyPath);
        ICASession[] sessions = new ICASession[sessionsNodeList.Count];
        for (int i = 0; i < sessions.Length; i++)
        {
            sessions[i] = new ICASession();
            sessions[i].InternalName = (sessionsNodeList[i].SelectSingleNode("InName").FirstChild == null ? "" : sessionsNodeList[i].SelectSingleNode("InName").FirstChild.Value);
            XmlNode dataTypeNode = sessionsNodeList[i].SelectSingleNode("DataType");
            if (dataTypeNode != null)
            {
                XmlNode valueNode = dataTypeNode.Attributes["value"];
                if (valueNode != null && valueNode.FirstChild != null)
                    sessions[i].DataType =  valueNode.FirstChild.Value;
            }
            sessions[i].ServerType = sessionsNodeList[i].SelectSingleNode("ServerType").FirstChild.Value;
            sessions[i].ClientType = sessionsNodeList[i].SelectSingleNode("ClientType").FirstChild.Value;
            sessions[i].ID = int.Parse(sessionsNodeList[i].SelectSingleNode("SessionId").FirstChild.Value);
       }
      
        return sessions;
    }

    internal static PresentationServer[] parseServers(string responseText)
    {
        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);   
        xmlDoc.LoadXml(responseText);

        string MyPath = "NFuseProtocol/ResponseServerData/ServerData";
        XmlNodeList serversNodeList = xmlDoc.SelectNodes(MyPath);
        PresentationServer[] servers = new PresentationServer[serversNodeList.Count];
        for (int i = 0; i < serversNodeList.Count; i++)
        {
            servers[i] = new PresentationServer();
            servers[i].ServerName = serversNodeList[i].SelectSingleNode("ServerName").FirstChild.Value;
            servers[i].ServerType = serversNodeList[i].SelectSingleNode("ServerType").FirstChild.Value;
            servers[i].ClientType = serversNodeList[i].SelectSingleNode("ClientType").FirstChild.Value;
        }
        return servers;
    }

    internal static string parseAltAddress(string responseText)
    {
        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);   
        xmlDoc.LoadXml(responseText);

        string MyPath = "NFuseProtocol/ResponseAddress/ServerAddress";
        XmlNode addressNode = xmlDoc.SelectSingleNode(MyPath);
        string address= addressNode.FirstChild.Value;
        return address;
    }

    public static bool parseCapabilities(string responseText)
    {
        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);   
        xmlDoc.LoadXml(responseText);
        string capID = "";
        string MyPath = "NFuseProtocol/ResponseCapabilities/CapabilityId";
        XmlNodeList capabiltyNodeList = xmlDoc.SelectNodes(MyPath);
        for (int i = 0; i < capabiltyNodeList.Count; i++)
        {
            capID = capabiltyNodeList[i].FirstChild.Value;
            if (capID == "multi-image-icons")
                return true;
        }
        return false;
    }


    public static string parseIconData(string responseText)
    {
        XmlDocument xmlDoc = new XmlDocument();
        responseText = Regex.Replace(responseText, "<!DOCTYPE.+?>", string.Empty);   

        xmlDoc.LoadXml(responseText);
        XmlNode iconDataNode = xmlDoc.SelectSingleNode("NFuseProtocol/ResponseAppData/AppDataSet/AppData/IconData");
        if (iconDataNode != null)
            return iconDataNode.FirstChild.Value;
        else
            return null;
    }
}
