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
/// Summary description for Validator
/// </summary>
public class Validator
{
    static public bool validateAddress(ref string ipAddress)
    {
        if (String.IsNullOrEmpty(ipAddress))
        {
            ipAddress = WSConstants.defaultIP;
            return true;
        }

        string[] digits = ipAddress.Split(".".ToCharArray());
        if (digits == null && digits.Length < 4)
            return false;
        else
        {
            int digit;
            for (int i = 0; i < digits.Length; i++)
            {
                if (!Int32.TryParse(digits[i], out digit))
                    return false;
                if (digit < 0 || digit > 255)
                    return false;
            }
            return true;
        }
    }

    static public bool validateServerTypes(ref string[] serverTypes)
    {
        if (serverTypes == null || serverTypes.Length == 0 || (serverTypes.Length == 1 && serverTypes[0].Trim() == string.Empty))
        {
            //
            // If the user did not specify any server type, applications published under all server types
            // will be returned
            //
            serverTypes = new string[] { WSConstants.defaultServerType };
            return true;
        }
        return validateAgainstRange(serverTypes, WSConstants.allowedServerTypes);
    }

    static public bool validateClientTypes(ref string[] clientTypes)
    {
        if (clientTypes == null || clientTypes.Length == 0 || (clientTypes.Length == 1 && clientTypes[0].Trim()==string.Empty))
        {
            //
            // If no client types are specified the default is all client types
            //
            clientTypes = new string[] { WSConstants.defaultClientType };
            return true;
        }
        return validateAgainstRange(clientTypes, WSConstants.allowedClientTypes);
    }

    static public bool validateDesiredDetails(ref string[] desiredDetails)
    {
        if (desiredDetails == null || desiredDetails.Length == 0 || (desiredDetails.Length == 1 && desiredDetails[0].Trim() == string.Empty))
        {
            desiredDetails = new string[] { WSConstants.defaultDetails };
            return true;
        }
        return validateAgainstRange(desiredDetails, WSConstants.allowedDetails);
    }

    internal static bool validateIDMethod(int idMethod)
    {
        if (idMethod != 0 && idMethod != 1 && idMethod != -1)
            return false;
        return true;
    }

    internal static bool validateAgainstRange(string[] dataset,string [] range)
    {
        bool[] matches = new bool[dataset.Length];
        for (int i = 0; i < matches.Length;i++ )
            matches[i] = false;
        for (int i = 0; i < dataset.Length; i++)
            for (int j = 0; j < range.Length; j++)
                if (dataset[i] == range[j])
                {
                    matches[i] = true;
                    break;
                }
        foreach (bool member in matches)
            if (!member)
                return member;
        return true;
    }


    internal static bool validateAgainstRange(int dataset, int[] range)
    {

        for (int i = 0; i < range.Length; i++)
        {
            if (dataset == range[i])
                return true;
        }

            return false;
    }

}
