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
/// Summary description for ICASession
/// </summary>
[Serializable]
public class ICASession
{
	public ICASession()
	{
		
	}
    public string InternalName = "";
    public string DataType     = "";
    public string ServerType   = "";
    public string ClientType   = "";
    public int ID = 0;
}
