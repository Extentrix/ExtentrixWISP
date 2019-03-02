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
/// Summary description for ApplicationItemEx
/// </summary>
[Serializable]
public class ApplicationItemEx :ApplicationItem 
{
	public Groups[] groups;
    public IconInfo[] availableIcons;
    public ApplicationItemEx()
	{
        
	}
    
}

[Serializable]
public class Groups
{
    public string[] GroupNames;
    public string DomainName;
    public string DomainType;
}

[Serializable]
public struct IconInfo
{
    public int size;
    public int depth;
    public string format;
}