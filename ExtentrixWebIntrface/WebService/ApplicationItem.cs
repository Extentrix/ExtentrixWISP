using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Serialization;

/// <summary>
/// Summary description for ApplicationItem
/// </summary>
/// 
[Serializable]
[XmlTypeAttribute(Namespace = NamespaceConstants.v1)]
public class ApplicationItem
{
    public string InternalName = "";
    public string FreindlyName = "";
    public string FolderName = "";
    public string Icon = "";
    public string ChangeCount = "";
    public string ServerType = "";
    public string ClientType = "";
    public string SequenceNumber = "";
    public string Description = "";
    public string WinWidth = "";
    public string WinHeight = "";
    public string WinColor = "";
    public string WinType = "";
    public string WinScale = "";
    public string SoundTypeMinimum = "";
    public string SoundType = "";
    public string VideoTypeMinimum = "";
    public string VideoType = "";
    public string EncryptionMinimum = "";
    public string Encryption = "";
    public string AppOnDesktop = "";
    public string AppInStartmenu = "";
    public string StartmenuFolder = "";
    public string StartMenuRoot = "";
    public string PublisherName = "";
    public string SSLEnabled = "";
    public string RemoteAccessEnabled = "";
    public string ContentAddress = "";
    public FileType[] fileTypes = null;
    public User[] users = null;
    public bool fileExtentionExists(string ext)
    {

        for (int i = 0; fileTypes != null && i < this.fileTypes.Length; i++)
            for (int j = 0; j < this.fileTypes[i].FileExtension.Length; j++)
                if (this.fileTypes[i].FileExtension[j] == ext)
                    return true;
        return false;
    }
}

[Serializable]
[XmlTypeAttribute(Namespace = NamespaceConstants.v1)]
public class FileType
{
    public string FileTypeName = "";
    public bool IsDefault =  false;
    public bool Overwrite = false;
    public string [] FileExtension = null;
    public string []MimeType = null;
    public string Parameters = "";

}

[Serializable]
[XmlTypeAttribute(Namespace = NamespaceConstants.v1)]
public class User
{
    public string Name = "";
    public string DomainName = "";
    public string DomainType = "";
}