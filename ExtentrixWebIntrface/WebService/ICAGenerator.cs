using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Web.Services.Protocols;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;

/// <summary>
/// Summary description for ICAGenerator
/// </summary>
public class ICAGenerator
{
	public ICAGenerator()
	{
	   
	}
    static public string getTicket(Credentials credentials, string clientName, string ipAddress, string appName, ref string serverAddress)
    {
       
        string errorMessage = "";
        string ticketTag    = "";
        
        WebService.GetApplicationsByCredentials(credentials, clientName, ipAddress, null, null, null);

        string postData = "<?xml version='1.0' encoding='utf-8'?>"
                              + "<!DOCTYPE NFuseProtocol>"
                              + "<NFuseProtocol version='4.2'>"
                              + "<RequestAddress>"
                              + "<Name>"
                              + "<AppName>" + HttpUtility.HtmlAttributeEncode(appName) +"</AppName>"
                              + "</Name>"
                              + "<ServerAddress addresstype='dot'/>"
                              + (credentials == null ? Assembler.assembleAllUser(): credentials.ToXML(true))
                              + "</RequestAddress>"
                              + "</NFuseProtocol>";
        string responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);

        if (Parser.checkErrorResponse(responseText, out errorMessage))
        {
            throw new SoapException(errorMessage, SoapException.ServerFaultCode);
        }
        else
            ticketTag = Parser.parseAddressResponse(responseText, ref serverAddress);

        if (WSConstants.LOGDETINFO)
            EventLogger.logEvent("getTicket: ticketTag:  " + ticketTag, 1);

       postData = "<?xml version='1.0' encoding='utf-8'?>"
                       + "<!DOCTYPE NFuseProtocol>"
                       + "<NFuseProtocol version='4.2'>"
                       + "<RequestTicket>"
                       + (credentials == null ? Assembler.assembleAllUser() : credentials.ToXML(true))
                       + "<TicketType>CtxLogon</TicketType>"
                       + "<TicketTag>"+ticketTag+"</TicketTag>"
                       + "<TimetoLive>3600</TimetoLive>"
                       + "</RequestTicket>"
                       + "</NFuseProtocol>";
       responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
       if (Parser.checkErrorResponse(responseText, out errorMessage))
       {
           if (WSConstants.LOGDETINFO)
               EventLogger.logEvent("getTicket: error getting ticket: " + errorMessage, 0);
           return "";//throw new SoapException(errorMessage, SoapException.ServerFaultCode);
       }
       else
       {
           string result = Parser.parseTicketResponse(responseText);
           if (WSConstants.LOGDETINFO)
               EventLogger.logEvent("getTicket, succeed: " + result, 1);

           return result;
       }
 
       
               
    }



    internal static string generateICAContent(string ticket, string serverAddress, string appName, Credentials credentials, string clientName)
    {
       

        string CredStr = "";
       
        string postData = "<?xml version='1.0' encoding='utf-8'?>"
                              + "<!DOCTYPE NFuseProtocol>"
                              + "<NFuseProtocol version='4.2'>"
                              + "<RequestServerFarmData>"
                              + "<Nil/>"
                              + "</RequestServerFarmData>"
                              + "</NFuseProtocol>";
        string errorMessage = "";
        string farmName     = "";
        string responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
        if (Parser.checkErrorResponse(responseText, out errorMessage))
            throw new SoapException(errorMessage, SoapException.ServerFaultCode);
        else
        {
            farmName = Parser.parseFarmDataResponse(responseText);
            if (WSConstants.LOGDETINFO)
                EventLogger.logEvent("generateICAContent, Farm Name: " + farmName, 1);
        }

        if (credentials != null)
        {
            CredStr = "SessionsharingKey=4-basic-basic-" + credentials.Domain + "-" + credentials.UserName + "-" + farmName;// +"\n"
            if (!(credentials is WindowsIdentity))
            {
                CredStr = CredStr + "Username=" + credentials.UserName + "\n";
            }
            if (!string.IsNullOrEmpty(ticket))
            {
                CredStr += "LogonTicket=" + ticket + "\n"
                    + "Domain=\\" + ticket.Substring(14) + "\n"
                    + "ClearPassword=" + ticket.Substring(0, 14) + "\n";
            }
            if(credentials is WindowsIdentity)
                CredStr += "UseLocalUserAndPassword=On\n"
                         + "EnableSSOnThruICAFile=Yes\n";
        
        }
       
        
        string icaTemplate  = "[Encoding]\n"
                            + "InputEncoding=UTF-8\n"
                            +"[WFClient]\n"
                            +"ClientName="+clientName+"\n"
                            +"ProxyFavorIEConnectionSetting=Yes\n"
                            +"ProxyTimeout=30000\n"
                            +"ProxyType=Auto\n"
                            +"ProxyUseFQDN=Off\n"
                            +"RemoveICAFile=Yes\n"
                            +"TransportReconnectEnabled=On\n"
                            +"Version=2\n"
                            +"VirtualCOMPortEmulation=On\n"
                            +"[ApplicationServers]\n"
                            +appName+"=\n"
                            +"["+appName+"]\n"
                            +"Address="+serverAddress+":"+WSConstants.ICAPORT+"\n"
                            +"AudioBandwidthLimit=2\n"
                            +"BrowserProtocol=HTTPonTCP\n"
                            +"AutologonAllowed=ON\n"
                            //+"CGPAddress=*:2598\n"
                            +"ClientAudio=On\n"
                            +"DesiredColor=8\n"
                            +"DesiredHRES=1024\n"
                            +"DesiredVRES=768\n"
                            +"DoNotUseDefaultCSL=On\n"
                            +"EnableIPCSessionControl=TRUE\n"
                            +"InitialProgram=#" + appName + "\n"
                            +"Launcher=PNAgent\n"
                            +"LocHttpBrowserAddress=!\n"
                            +CredStr
                            +"LogonTicketType=CTXS1\n"
                            +"LongCommandLine=\n"
                            +"ProxyTimeout=30000\n"
                            +"ProxyType=Auto\n"
                            +"SSLEnable=Off\n"
                            
                            +"TWIMode=ON\n"
                            +"TransportDriver=TCP/IP\n"
                           
                            +"WinStationDriver=ICA 3.0\n"
                            +"[EncRC5-0]\n"
                            +"DriverNameWin32=pdc0n.dll\n"
                            +"[EncRC5-128]\n"
                            +"DriverNameWin32=pdc128n.dll\n"
                            +"[EncRC5-40]\n"
                            +"DriverNameWin32=pdc40n.dll\n"
                            +"[EncRC5-56]\n"
                            +"DriverNameWin32=pdc56n.dll\n";
        return icaTemplate;
       }


    internal static string generateICAContentWithOption(string ticket, string serverAddress, string appName, Credentials credentials, string clientName, ICAConnectionOptions option)
    {
        
        string postData = "<?xml version='1.0' encoding='utf-8'?>"
                              + "<!DOCTYPE NFuseProtocol>"
                              + "<NFuseProtocol version='4.2'>"
                              + "<RequestServerFarmData>"
                              + "<Nil/>"
                              + "</RequestServerFarmData>"
                              + "</NFuseProtocol>";
        string errorMessage = "";
        string farmName = "";
        string responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
        if (Parser.checkErrorResponse(responseText, out errorMessage))
            throw new SoapException(errorMessage, SoapException.ServerFaultCode);
        else
        {
            farmName = Parser.parseFarmDataResponse(responseText);
            if (WSConstants.LOGDETINFO)
                EventLogger.logEvent("generateICAContentWithOption, Farm Name: " + farmName, 1);
        }



        ICAFields ica = new ICAFields();

        if (credentials != null)
        {
            ica.LogonTicket = ticket;
            ica.SessionsharingKey = "4-basic-basic-" + credentials.Domain + "-" + credentials.UserName + "-" + farmName;
            ica.Username = (credentials == null ? "" : credentials.UserName);
            if (!string.IsNullOrEmpty(ticket))
            {
                ica.Domain = "\\" + ticket.Substring(14);
                ica.ClearPassword = ticket.Substring(0, 14);
            }
        }

       

       
        if (option != null)
        {
            if (WSConstants.LOGDETINFO)
            {
                string optionsStr = "connectionSpeed=" + option.connectionSpeed + "\nEnablePDADeviceSupport=" + option.EnablePDADeviceSupport + "\nEnablePrinterMapping=" + option.EnablePrinterMapping + "\nEnableSessionReliability=" + option.EnableSessionReliability
                    + "\nEnableSound="
                    + option.EnableSound + "\nencryptionLevel=" + option.encryptionLevel + "\nkeyboard=" + option.keyboard + "\nQueueMouseMovementsAndKeystrokes=" + option.QueueMouseMovementsAndKeystrokes + "\nUseDataCompression="
                    +option.screenHight +"\n"+option.screenSizePercent+"\n"+option.screenWidth+"\n"+option.soundQuality + "\n"
                    + option.UseDataCompression + "\nuseDiskCacheForBitmaps=" + option.useDiskCacheForBitmaps + "\nwindowColor=" + option.windowColor + "\nwindowSize=" + option.windowSize;

                EventLogger.logEvent("generateICAContentWithOption, input options: " + optionsStr, 1);
            }
            ica.DoNotUseDefaultCSL = "On";

            #region encrypted ICA sessions
           
            switch (option.encryptionLevel) {
                case EncryptionLevel._128bit:
                    ica.EncryptionLevelSession = "EncRC5-128";
                    break;

                case EncryptionLevel._128bitForLogonOnly:
                    ica.EncryptionLevelSession = "EncRC5-0";
                    break;

                case EncryptionLevel._40bit:
                    ica.EncryptionLevelSession = "EncRC5-40";
                    break;

                case EncryptionLevel._56bit:
                    ica.EncryptionLevelSession = "EncRC5-56";
                    break;

                case EncryptionLevel._basic:
                    ica.EncryptionLevelSession = "1";
                    break;
                case EncryptionLevel._defaultLevel:
                    ica.UseDefaultEncryption = "On";
                    break;
            }

            #endregion
           


            #region windows size
            
            switch (option.windowSize) {
                case WindowSize._1024x768:
                    ica.DesiredHRES = "1024";
                    ica.DesiredVRES = "768";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "On";
                    break;

                case WindowSize._1280x1024:
                    ica.DesiredHRES = "1280";
                    ica.DesiredVRES = "1024";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "On";
                   
                    break;
                case WindowSize._1600x1200:
                    ica.DesiredHRES = "1600";
                    ica.DesiredVRES = "1200";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "On";

                  
                    break;
                case WindowSize._640x480:
                   
                    ica.DesiredHRES = "640";
                    ica.DesiredVRES = "480";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "Off";
                    break;

                case WindowSize._800x600:
                    ica.DesiredHRES = "800";
                    ica.DesiredVRES = "600";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "Off";
                    break;

                case WindowSize._custom:
                    ica.DesiredHRES = "" + (option.screenWidth == 0 ? 1024 : option.screenWidth);
                    ica.DesiredVRES = "" + (option.screenHight == 0 ? 768 : option.screenHight); 
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "Off";
                    break;


                case WindowSize._fullScreen:
                    ica.DesiredHRES = "4294967295";
                    ica.DesiredVRES = "4294967295";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "Off";
                    ica.DoNotUseDefaultCSL = "On";
                    break;

                case WindowSize._PercentOfScreenSize:
                    ica.ScreenPercent = "" + (option.screenSizePercent==0?50: option.screenSizePercent);
                    ica.TWIMode = "Off";
                    break;

                case WindowSize._seamlessWindow:
                    ica.DesiredHRES = "1024";
                    ica.DesiredVRES = "768";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "On";
                    break;

            }
            #endregion

            #region Windows Colors
            //
            //DesiredColor=1 | 2 | 4 | 8
            //DesiredColor sets the color palette depth. Use 1 for 16-color and 2 for 256-color. The default is 2 (256-color). Use 4 for High Color. For True Color, use 8.
            //
          
            switch (option.windowColor) {
                case WindowColor._16color:
                    ica.DesiredColor = "1";
                    break;

                case WindowColor._256colors:
                    ica.DesiredColor = "2";    
                    break;

                case WindowColor._defaultColors:
                    ica.DesiredColor = "2";
                    break;

                case WindowColor._highColor:
                    ica.DesiredColor = "4";
                    break;

                case WindowColor._trueColor:
                    ica.DesiredColor = "8";
                    break;
            }

            #endregion

           
            #region KeyBoard
            //Apply Windows key combinations (for example, ALT+TAB)
            // when Window size is seamless , then this option is disabled
            if (!(option.windowSize == WindowSize._seamlessWindow))
            {
                switch (option.keyboard)
                {
                    case TransparentKeyPassthrough.OnFullScreenOnly:
                        ica.TransparentKeyPassthrough = "FullScreenOnly";
                        break;

                    case TransparentKeyPassthrough.OnLocalComputer:
                        ica.TransparentKeyPassthrough = "Local";
                        break;

                    case TransparentKeyPassthrough.OnRemoteComputer:
                        ica.TransparentKeyPassthrough = "Remote";
                        break;
                }
            }

            #endregion

           
            #region Allow PDA
            //Enable PDA device support 
            //COMAllowed: Enables or disables client COM port mapping. Specify Yes to enable, No to disable. This should be written in the application server section of the Template.ica file. 
            //COMAllowed: Controls the use of the COM port mapping virtual channel
            if (option.EnablePDADeviceSupport)
            {
                ica.COMAllowed = "On";
                ica.VirtualCOMPortEmulation = "On";
            }
            else {
                ica.COMAllowed = "Off";
                ica.VirtualCOMPortEmulation = "Off";
            }
                //pdaSupport = "COMAllowed=On\n"
                  //          + "VirtualCOMPortEmulation=On\n";
            //COMAllowed=On
            //VirtualCOMPortEmulation=On
            //NRWD=16
            #endregion

           
            #region Connection Speed
            
            switch (option.connectionSpeed) { 
                case ConnectionSpeed.Custom:
                    ica.VSLAllowed = "Off";
                    ica.LPWD = "16";
                    ica.NRWD = "16";
                    ica.TRWD = "0";
                   break;
                case ConnectionSpeed.High:
                    ica.VSLAllowed = "On";
                    ica.AudioBandwidthLimit = "0";
                    ica.ClientAudio = "On";
                    ica.DesiredColor = "8";
                    ica.LPWD = "31";
                    ica.NRWD = "0";
                    ica.TRWD = "16";
                    break;

                case ConnectionSpeed.Low:
                    ica.COMAllowed = "Off";
                    ica.KeyboardTimer = "50";
                    ica.MouseTimer = "200";
                    ica.OutBufCountClient = "118";
                    ica.OutBufCountClient2 = "118";
                    ica.OutBufCountHost = "118";
                    ica.OutBufCountHost2 = "118";
                    ica.OutBufLength = "512";
                    ica.PersistentCacheEnabled = "On";
                    ica.NRWD = "0";
                    ica.TRWD = "0";
                    ica.ZLKeyboardMode = "1";
                    ica.ZLMouseMode = "1";
                    break;

                case ConnectionSpeed.Medium:
                    ica.VSLAllowed = "Off";
                    ica.LPWD = "16";
                    ica.NRWD = "16";
                    ica.TRWD = "0";
                    break;

                case ConnectionSpeed.MediumHigh:
                    ica.ClientAudio = "Off";
                    ica.DesiredColor = "4";
                    ica.LPWD = "15";
                    ica.NRWD = "15";
                    ica.TRWD = "0";
                    break;
            }
            #endregion

          
            #region Sound Quality
            //ClientAudio: Controls the use of the audio virtual channel
            if (option.connectionSpeed == ConnectionSpeed.Custom)
            {
                ica.ClientAudio = "On";
                switch (option.soundQuality)
                {
                    case SoundQuality.HighsoundQuality:
                        ica.AudioBandwidthLimit = "0";
                        break;
                    case SoundQuality.LowSoundQuality:
                        ica.AudioBandwidthLimit = "2";
                        break;
                    case SoundQuality.MediumSoundQuality:
                        ica.AudioBandwidthLimit = "1";
                        break;

                }
                if (option.EnableSound)
                    ica.ClientAudio = "On";
            }


            #endregion


            #region Copression
            
            //Compress=On
            //This vale determines whether or not to compress ICA.
            /*  Data compression increases the bandwidth efficiency of the ICA client. Furthermore, using maximum data
                compression maximizes this efficiency, though at a cost of slightly more memory resource used on the MetaFrame
                server. Maximum data compression is enabled through the following ICA file entry.
             */
            if (option.UseDataCompression)
                ica.MaximumCompression = "On";
            else
                ica.MaximumCompression = "Off";

            #endregion


            #region Enable the persistent cache
            /*
                Enable the persistent cache
                Enabling the persistent cache decreases logon time and improves the performance of graphics operations during
                an ICA session. Persistent cache is not supported in all ICA clients. For example, the ICA Client for Windows CE
                version does not support this due to typical resource constraints on the devices limiting memory allocation
                available for caching.
                To enable use of the persistent cache, set PersistentCacheEnabled=On.             
             */
            if (option.useDiskCacheForBitmaps)
                ica.PersistentCacheEnabled = "On";
            else 
                ica.PersistentCacheEnabled = "Off";

            #endregion


            #region  Enable mouse movement and keystroke queuing
            /*
                Enable mouse movement and keystroke queuing
                Enabling these parameters reduces the number of small mouse and keyboard packets sent to the server.
                Intermediate mouse packets are discarded and a number of keystroke packets are coalesced into a single larger
                packet.
                The following ICA file settings enable mouse movement and keystroke queuing for the period defined below in
                milliseconds
             */

            //MouseTimer: 200 Setting can be varied but increasing this value too much could degrade interactive
            //response regardless of Speedscreen 3 being enabled.
            
            //KeyboardTimer 50 Setting can be varied but increasing this value too much could degrade interactive
            //response regardless of Speedscreen 3 being enabled.
            if (option.QueueMouseMovementsAndKeystrokes) {
                ica.MouseTimer = "100";
                ica.KeyboardTimer = "100";
            }

            #endregion

            
            if (option.EnablePrinterMapping)
            {
                //this option is enabled just when the connection speed is custom
                //VSLAllowed: Controls the use of the print spooler virtual channel
                //CPMAllowed: Controls the use of the Client Printer Mapping virtual channel
                if (option.connectionSpeed == ConnectionSpeed.Custom)
                {
                    ica.VSLAllowed = "On";
                    ica.CPMAllowed = "On";
                    ica.LPWD = "46";
                    ica.NRWD = "16";
                }
               
            }

        }



        #region Create ICA file
        string icaTemplate = "[Encoding]\n"
                    + "InputEncoding=UTF-8\n"

                    +"[WFClient]\n"
                    +"COMAllowed="+ica.COMAllowed+"\n"
                    +"CPMAllowed="+ica.CPMAllowed+"\n"
                    +"ClientName=" +clientName + "\n"
                    +"ProxyFavorIEConnectionSetting=Yes\n"
                    +"ProxyTimeout=30000\n"
                    +"ProxyType=Auto\n"
                    +"ProxyUseFQDN=Off\n"
                    +"RemoveICAFile=yes\n";
        
        if (!string.IsNullOrEmpty(ica.PersistentCacheEnabled))
            icaTemplate += "PersistentCacheEnabled=" + ica.PersistentCacheEnabled + "\n";
        
        if (!string.IsNullOrEmpty(ica.TransparentKeyPassthrough))
            icaTemplate += "TransparentKeyPassthrough=" + ica.TransparentKeyPassthrough + "\n";
        
        //if (!string.IsNullOrEmpty(ica.TransportReconnectEnabled))
        //    icaTemplate += "TransportReconnectEnabled=" + ica.TransportReconnectEnabled + "\n";
         
        if (!string.IsNullOrEmpty(ica.MaximumCompression)){
            icaTemplate += "MaximumCompression=" + ica.MaximumCompression + "\n"
                + "Compress=" + ica.MaximumCompression + "\n";
            }
        

       

        icaTemplate += "VSLAllowed=" + ica.VSLAllowed + "\n"
                    + "Version=2\n";
        if (!string.IsNullOrEmpty(ica.VirtualCOMPortEmulation))
            icaTemplate += "VirtualCOMPortEmulation=" + ica.VirtualCOMPortEmulation + "\n";


        icaTemplate += "[ApplicationServers]\n"
                   + appName + "=\n"

                   + "[" + appName + "]\n"
                   + "Address=" + serverAddress + ":" + WSConstants.ICAPORT + "\n"
                   + "AudioBandwidthLimit=" + ica.AudioBandwidthLimit + "\n"
                   + "AutologonAllowed=ON\n";
                   
                    if (!string.IsNullOrEmpty(ica.EncryptionLevelSession))
                       icaTemplate += "EncryptionLevelSession=" + ica.EncryptionLevelSession+"\n";
                    if (!string.IsNullOrEmpty(ica.KeyboardTimer))
                       icaTemplate += "KeyboardTimer=" + ica.KeyboardTimer+"\n";
                    if (!string.IsNullOrEmpty(ica.MouseTimer))
                       icaTemplate += "MouseTimer=" + ica.MouseTimer+"\n";
        
                   icaTemplate += "BrowserProtocol=HTTPonTCP\n"
                   + "CGPAddress=*:2598\n"
                  
                   + "ClientAudio=" + ica.ClientAudio + "\n"
                   + "DesiredColor=" + ica.DesiredColor + "\n"
                   + "DesiredHRES=" + ica.DesiredHRES + "\n"
                   + "DesiredVRES=" + ica.DesiredVRES + "\n";

                    if (!string.IsNullOrEmpty(ica.DoNotUseDefaultCSL) )
                        icaTemplate += "DoNotUseDefaultCSL=" + ica.DoNotUseDefaultCSL + "\n";
                    
                    if(!string.IsNullOrEmpty(ticket)){
                        icaTemplate += "Domain=\\" + ica.Domain + "\n"
                             + "ClearPassword=" + ica.ClearPassword + "\n"
                             + "LogonTicket=" + ica.LogonTicket + "\n";
                    }

                    icaTemplate += "InitialProgram=#" + appName + "\n"
                    + "LPWD=" + ica.LPWD + "\n"
                    + "Launcher=WI\n"
                    + "LocHttpBrowserAddress=!\n"

                    + "LogonTicketType=CTXS1\n"
                    + "LongCommandLine=\n"
                    + "NRWD=" + ica.NRWD + "\n"
                    + "ProxyTimeout=30000\n"
                    + "ProxyType=Auto\n"
                    + "SSLEnable=Off\n"
                    + "SessionsharingKey=" + ica.SessionsharingKey + "\n"
                    + "StartIFDCD=1220507579638\n"
                    + "StartSCD=1220507579638\n"
                    + "TRWD=" + ica.TRWD + "\n"
                    + "TWIMode=" + ica.TWIMode + "\n"
                    + "TransportDriver=TCP/IP\n"
                    + "UILocale=en\n";
                    if (!(credentials is WindowsIdentity))
                        icaTemplate += "Username=" + ica.Username + "\n";
                 
                   icaTemplate += "WinStationDriver=ICA 3.0\n";
                    if (credentials is WindowsIdentity)
                        icaTemplate += "UseLocalUserAndPassword=On\n";
            icaTemplate += "[Compress]\n"
                   + "DriverNameWin16=pdcompw.dll\n"
                   + "DriverNameWin32=pdcompn.dll\n"

                   + "[EncRC5-0]\n"
                   + "DriverNameWin16=pdc0w.dll\n"
                   + "DriverNameWin32=pdc0n.dll\n"

                   + "[EncRC5-128]\n"
                   + "DriverNameWin16=pdc128w.dll\n"
                   + "DriverNameWin32=pdc128n.dll\n"

                   + "[EncRC5-40]\n"
                   + "DriverNameWin16=pdc40w.dll\n"
                   + "DriverNameWin32=pdc40n.dll\n"

                   + "[EncRC5-56]\n"
                   + "DriverNameWin16=pdc56w.dll\n"
                   + "DriverNameWin32=pdc56n.dll\n";
        #endregion


                    return icaTemplate;
    }



    internal static string generateICAContentWithOptionAndCAG(string ticket, string serverAddress, string appName, Credentials credentials, string clientName, ICAConnectionOptions option)
    {

        string postData = "<?xml version='1.0' encoding='utf-8'?>"
                              + "<!DOCTYPE NFuseProtocol>"
                              + "<NFuseProtocol version='4.2'>"
                              + "<RequestServerFarmData>"
                              + "<Nil/>"
                              + "</RequestServerFarmData>"
                              + "</NFuseProtocol>";
        string errorMessage = "";
        string farmName = "";
        string responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);
        if (Parser.checkErrorResponse(responseText, out errorMessage))
            throw new SoapException(errorMessage, SoapException.ServerFaultCode);
        else
            farmName = Parser.parseFarmDataResponse(responseText);


        if (WSConstants.LOGDETINFO)
            EventLogger.logEvent("generateICAContentWithOptionAndCAG: Farm Name" + farmName, 1);
       
        #region Get SAT ticket


        string STARequest = "";


        try
        {
            STARequest = "<?xml version=\"1.0\"?>\n" +
                           "<!DOCTYPE CtxSTAProtocol SYSTEM \"CtxSTA.dtd\">\n" +
                           "<CtxSTAProtocol version=\"4.0\">\n" +
                           "<RequestTicket>\n" +
                           "<AllowedTicketType>STA-v4</AllowedTicketType>\n" +
                           "<AllowedAuthorityIDType>STA-v1</AllowedAuthorityIDType>\n" +
                           "<TicketData>\n" +

                            "<Value name=\"ICAAddress\">" + serverAddress + ":" + WSConstants.ICAPORT + "</Value>\n" +
                            "<Value name=\"XData\">\n" +
                           "&lt;?xml version=&quot;1.0&quot;?&gt;&lt;!--DOCTYPE CtxConnInfoProtocol SYSTEM &quot;CtxConnInfo.dtd&quot;--&gt;&lt;CtxConnInfo version=&quot;1.0&quot;&gt;&lt;ServerAddress&gt;172.19.0.49:1494&lt;/ServerAddress&gt;&lt;UserName&gt;administrator&lt;/UserName&gt;&lt;UserDomain&gt;xendesktop&lt;/UserDomain&gt;&lt;ApplicationName&gt;Notepad&lt;/ApplicationName&gt;&lt;Protocol&gt;ICA&lt;/Protocol&gt;&lt;/CtxConnInfo&gt; </Value>\n" +
                           "</TicketData>\n" +
                           "<Control>\n" +
                            "<Refreshable>true</Refreshable>\n" +
                           "</Control>\n" +
                           "</RequestTicket>\n" +
                           "</CtxSTAProtocol> ";

            responseText = GetStaResponse(STARequest); //HTTPManager.doHttpRequest(lastUsedStaUrl, STARequest);


        }
        catch (System.Net.WebException webExc)
        {
            if (WSConstants.LOGERRORS)
                EventLogger.logEvent("generateICAContentWithOptionAndCAG:" + webExc.Message, 0);
        }
        catch (System.Web.Services.Protocols.SoapException soapExc)
        {
            if (WSConstants.LOGERRORS)
                EventLogger.logEvent("generateICAContentWithOptionAndCAG:" + soapExc.Message, 0);
        }


        if (responseText.Equals(""))
        {
            if (WSConstants.LOGERRORS)
                EventLogger.logEvent("generateICAContentWithOptionAndCAG:" + WSConstants.STA_INVALID_RESPONSE, 0);

            throw new SoapException(WSConstants.STA_INVALID_RESPONSE, SoapException.ServerFaultCode);
        }



        string authID = "";
        string staTicket = "";
        string version = "";
        Parser.parseSTATicketResponse(responseText, ref authID, ref staTicket, ref version);
        if (WSConstants.LOGDETINFO)
            EventLogger.logEvent("generateICAContentWithOptionAndCAG: STA Ticket: " + staTicket, 1);


        #endregion



        ICAFields ica = new ICAFields();

        if (credentials != null)
        {
            ica.LogonTicket = ticket;
            ica.SessionsharingKey = "4-basic-basic-" + credentials.Domain + "-" + credentials.UserName + "-" + farmName;
            ica.Username = (credentials == null ? "" : credentials.UserName);
            ica.Domain = "\\" + ticket.Substring(14);
            ica.ClearPassword = ticket.Substring(0, 14);

        }

        if (option != null)
        {
            ica.DoNotUseDefaultCSL = "On";
            #region encrypted ICA sessions

            switch (option.encryptionLevel)
            {
                case EncryptionLevel._128bit:
                    ica.EncryptionLevelSession = "EncRC5-128";
                    break;

                case EncryptionLevel._128bitForLogonOnly:
                    ica.EncryptionLevelSession = "EncRC5-0";
                    break;

                case EncryptionLevel._40bit:
                    ica.EncryptionLevelSession = "EncRC5-40";
                    break;

                case EncryptionLevel._56bit:
                    ica.EncryptionLevelSession = "EncRC5-56";
                    break;

                case EncryptionLevel._basic:
                    ica.EncryptionLevelSession = "1";
                    break;
                case EncryptionLevel._defaultLevel:
                    ica.UseDefaultEncryption = "On";
                    break;
            }

            #endregion



            #region windows size

            switch (option.windowSize)
            {
                case WindowSize._1024x768:
                    ica.DesiredHRES = "1024";
                    ica.DesiredVRES = "768";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "On";
                    break;

                case WindowSize._1280x1024:
                    ica.DesiredHRES = "1280";
                    ica.DesiredVRES = "1024";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "On";

                    break;
                case WindowSize._1600x1200:
                    ica.DesiredHRES = "1600";
                    ica.DesiredVRES = "1200";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "On";


                    break;
                case WindowSize._640x480:

                    ica.DesiredHRES = "640";
                    ica.DesiredVRES = "480";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "Off";
                    break;

                case WindowSize._800x600:
                    ica.DesiredHRES = "800";
                    ica.DesiredVRES = "600";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "Off";
                    break;

                case WindowSize._custom:
                    ica.DesiredHRES = "" + (option.screenWidth == 0 ? 1024 : option.screenWidth);
                    ica.DesiredVRES = "" + (option.screenHight == 0 ? 768 : option.screenHight);
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "Off";
                    break;


                case WindowSize._fullScreen:
                    ica.DesiredHRES = "4294967295";
                    ica.DesiredVRES = "4294967295";
                    ica.ScreenPercent = "";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "Off";
                    ica.DoNotUseDefaultCSL = "On";
                    break;

                case WindowSize._PercentOfScreenSize:
                    ica.ScreenPercent = "" + (option.screenSizePercent == 0 ? 50 : option.screenSizePercent);
                    ica.TWIMode = "Off";
                    break;

                case WindowSize._seamlessWindow:
                    ica.DesiredHRES = "1024";
                    ica.DesiredVRES = "768";
                    ica.UseDefaultWinSize = "Off";
                    ica.TWIMode = "On";
                    break;

            }
            #endregion

            #region Windows Colors
            //
            //DesiredColor=1 | 2 | 4 | 8
            //DesiredColor sets the color palette depth. Use 1 for 16-color and 2 for 256-color. The default is 2 (256-color). Use 4 for High Color. For True Color, use 8.
            //

            switch (option.windowColor)
            {
                case WindowColor._16color:
                    ica.DesiredColor = "1";
                    break;

                case WindowColor._256colors:
                    ica.DesiredColor = "2";
                    break;

                case WindowColor._defaultColors:
                    ica.DesiredColor = "2";
                    break;

                case WindowColor._highColor:
                    ica.DesiredColor = "4";
                    break;

                case WindowColor._trueColor:
                    ica.DesiredColor = "8";
                    break;
            }

            #endregion


            #region KeyBoard
            if (!(option.windowSize == WindowSize._seamlessWindow))
            {
                switch (option.keyboard)
                {
                    case TransparentKeyPassthrough.OnFullScreenOnly:
                        ica.TransparentKeyPassthrough = "FullScreenOnly";
                        break;

                    case TransparentKeyPassthrough.OnLocalComputer:
                        ica.TransparentKeyPassthrough = "Local";
                        break;

                    case TransparentKeyPassthrough.OnRemoteComputer:
                        ica.TransparentKeyPassthrough = "Remote";
                        break;
                }
            }

            #endregion


            #region Allow PDA
            //COMAllowed: Controls the use of the COM port mapping virtual channel
            if (option.EnablePDADeviceSupport)
            {
                ica.COMAllowed = "On";
                ica.VirtualCOMPortEmulation = "On";
            }
            else
            {
                ica.COMAllowed = "Off";
                ica.VirtualCOMPortEmulation = "Off";
            }
            //pdaSupport = "COMAllowed=On\n"
            //          + "VirtualCOMPortEmulation=On\n";
            //COMAllowed=On
            //VirtualCOMPortEmulation=On
            //NRWD=16
            #endregion


            #region Connection Speed

            switch (option.connectionSpeed)
            {
                case ConnectionSpeed.Custom:
                    ica.VSLAllowed = "Off";
                    ica.LPWD = "16";
                    ica.NRWD = "16";
                    ica.TRWD = "0";
                    break;
                case ConnectionSpeed.High:
                    ica.VSLAllowed = "On";
                    ica.AudioBandwidthLimit = "0";
                    ica.ClientAudio = "On";
                    ica.DesiredColor = "8";
                    ica.LPWD = "31";
                    ica.NRWD = "0";
                    ica.TRWD = "16";
                    break;

                case ConnectionSpeed.Low:
                    ica.COMAllowed = "Off";
                    ica.KeyboardTimer = "50";
                    ica.MouseTimer = "200";
                    ica.OutBufCountClient = "118";
                    ica.OutBufCountClient2 = "118";
                    ica.OutBufCountHost = "118";
                    ica.OutBufCountHost2 = "118";
                    ica.OutBufLength = "512";
                    ica.PersistentCacheEnabled = "On";
                    ica.NRWD = "0";
                    ica.TRWD = "0";
                    ica.ZLKeyboardMode = "1";
                    ica.ZLMouseMode = "1";
                    break;

                case ConnectionSpeed.Medium:
                    ica.VSLAllowed = "Off";
                    ica.LPWD = "16";
                    ica.NRWD = "16";
                    ica.TRWD = "0";
                    break;

                case ConnectionSpeed.MediumHigh:
                    ica.ClientAudio = "Off";
                    ica.DesiredColor = "4";
                    ica.LPWD = "15";
                    ica.NRWD = "15";
                    ica.TRWD = "0";
                    break;
            }
            #endregion


            #region Sound Quality

            //ClientAudio:Controls the use of the audio virtual channel
            if (option.connectionSpeed == ConnectionSpeed.Custom)
            {
                ica.ClientAudio = "On";
                switch (option.soundQuality)
                {
                    case SoundQuality.HighsoundQuality:
                        ica.AudioBandwidthLimit = "0";
                        break;
                    case SoundQuality.LowSoundQuality:
                        ica.AudioBandwidthLimit = "2";
                        break;
                    case SoundQuality.MediumSoundQuality:
                        ica.AudioBandwidthLimit = "1";
                        break;

                }
                if (option.EnableSound)
                    ica.ClientAudio = "On";
            }


            #endregion


            #region Compression
            //Compress=On
            //This vale determines whether or not to compress ICA.

            //Compress=On
            //This vale determines whether or not to compress ICA.
            /*  Data compression increases the bandwidth efficiency of the ICA client. Furthermore, using maximum data
                compression maximizes this efficiency, though at a cost of slightly more memory resource used on the MetaFrame
                server. Maximum data compression is enabled through the following ICA file entry.
             */
            if (option.UseDataCompression)
                ica.MaximumCompression = "On";
            else
                ica.MaximumCompression = "Off";

            #endregion

            #region Enable the persistent cache
            /*
                Enable the persistent cache
                Enabling the persistent cache decreases logon time and improves the performance of graphics operations during
                an ICA session. Persistent cache is not supported in all ICA clients. For example, the ICA Client for Windows CE
                version does not support this due to typical resource constraints on the devices limiting memory allocation
                available for caching.
                To enable use of the persistent cache, set PersistentCacheEnabled=On.
             */

            if (option.useDiskCacheForBitmaps)
                ica.PersistentCacheEnabled = "On";
            else ica.PersistentCacheEnabled = "Off";
            #endregion


            #region Enable mouse movement and keystroke queuing
            /*
                Enable mouse movement and keystroke queuing
                Enabling these parameters reduces the number of small mouse and keyboard packets sent to the server.
                Intermediate mouse packets are discarded and a number of keystroke packets are coalesced into a single larger
                packet.
                The following ICA file settings enable mouse movement and keystroke queuing for the period defined below in
                milliseconds
             */

            //MouseTimer: 200 Setting can be varied but increasing this value too much could degrade interactive
            //response regardless of Speedscreen 3 being enabled.

            //KeyboardTimer 50 Setting can be varied but increasing this value too much could degrade interactive
            //response regardless of Speedscreen 3 being enabled.
            if (option.QueueMouseMovementsAndKeystrokes)
            {
                ica.MouseTimer = "100";
                ica.KeyboardTimer = "100";
            }
            #endregion


            if (option.EnablePrinterMapping)
            {
                //VSLAllowed: Controls the use of the print spooler virtual channel                
                //CPMAllowed: Controls the use of the Client Printer Mapping virtual channel

                if (option.connectionSpeed == ConnectionSpeed.Custom)
                {
                    ica.VSLAllowed = "On";
                    ica.CPMAllowed = "On";
                    ica.LPWD = "46";
                    ica.NRWD = "16";
                }

            }

        }


        #region Create ICA file
        string icaTemplate = "[Encoding]\n"
                    + "InputEncoding=UTF-8\n"

                    + "[WFClient]\n"
                    + "COMAllowed=" + ica.COMAllowed + "\n"
                    + "CPMAllowed=" + ica.CPMAllowed + "\n"
                    + "ClientName=" + clientName + "\n"
                    + "ProxyFavorIEConnectionSetting=Yes\n"
                    + "ProxyTimeout=30000\n"
                    + "ProxyType=Auto\n"
                    + "ProxyUseFQDN=Off\n"
                    + "RemoveICAFile=yes\n";

        if (!string.IsNullOrEmpty(ica.PersistentCacheEnabled))
            icaTemplate += "PersistentCacheEnabled=" + ica.PersistentCacheEnabled + "\n";

        if (!string.IsNullOrEmpty(ica.TransparentKeyPassthrough))
            icaTemplate += "TransparentKeyPassthrough=" + ica.TransparentKeyPassthrough + "\n";

        //if (!string.IsNullOrEmpty(ica.TransportReconnectEnabled))
        //    icaTemplate += "TransportReconnectEnabled=" + ica.TransportReconnectEnabled + "\n";

        if (!string.IsNullOrEmpty(ica.MaximumCompression))
        {
            icaTemplate += "MaximumCompression=" + ica.MaximumCompression + "\n"
                + "Compress=" + ica.MaximumCompression + "\n";
        }




        icaTemplate += "VSLAllowed=" + ica.VSLAllowed + "\n"
                    + "Version=2\n";
        if (!string.IsNullOrEmpty(ica.VirtualCOMPortEmulation))
            icaTemplate += "VirtualCOMPortEmulation=" + ica.VirtualCOMPortEmulation + "\n";


        icaTemplate += "[ApplicationServers]\n"
                   + appName + "=\n"

                   + "[" + appName + "]\n"
                   +  "Address=;" + version + ";" + authID + ";" + staTicket + "\n"
                   + "AudioBandwidthLimit=" + ica.AudioBandwidthLimit + "\n"
                   + "AutologonAllowed=ON\n";

        if (!string.IsNullOrEmpty(ica.EncryptionLevelSession))
            icaTemplate += "EncryptionLevelSession=" + ica.EncryptionLevelSession + "\n";
        if (!string.IsNullOrEmpty(ica.KeyboardTimer))
            icaTemplate += "KeyboardTimer=" + ica.KeyboardTimer + "\n";
        if (!string.IsNullOrEmpty(ica.MouseTimer))
            icaTemplate += "MouseTimer=" + ica.MouseTimer + "\n";

        icaTemplate += "BrowserProtocol=HTTPonTCP\n"
        + "CGPAddress=*:2598\n"
        + "ClearPassword=" + ica.ClearPassword + "\n"
        + "ClientAudio=" + ica.ClientAudio + "\n"
        + "DesiredColor=" + ica.DesiredColor + "\n"
        + "DesiredHRES=" + ica.DesiredHRES + "\n"
        + "DesiredVRES=" + ica.DesiredVRES + "\n";

        if (!string.IsNullOrEmpty(ica.DoNotUseDefaultCSL))
            icaTemplate += "DoNotUseDefaultCSL=" + ica.DoNotUseDefaultCSL + "\n";

        icaTemplate += "Domain=\\" + ica.Domain + "\n"
       + "InitialProgram=#" + appName + "\n"
       + "LPWD=" + ica.LPWD + "\n"
       + "Launcher=WI\n"
       + "LocHttpBrowserAddress=!\n"
       + "LogonTicket=" + ica.LogonTicket + "\n"
       + "LogonTicketType=CTXS1\n"
       + "LongCommandLine=\n"
       + "NRWD=" + ica.NRWD + "\n"
       + "ProxyTimeout=30000\n"
       + "ProxyType=Auto\n"
       + "SSLCiphers=all\n"
       + "SSLEnable=On\n"
       + "SSLProxyHost=" + WSConstants.CAG_FQDN + ":" + WSConstants.CAG_Port + "\n"
       + "SecureChannelProtocol=Detect\n"
       + "SessionsharingKey=" + ica.SessionsharingKey + "\n"
       + "StartIFDCD=1220507579638\n"
       + "StartSCD=1220507579638\n"
       + "TRWD=" + ica.TRWD + "\n"
       + "TWIMode=" + ica.TWIMode + "\n"
       + "TransportDriver=TCP/IP\n"
       + "UILocale=en\n";
        if (!(credentials is WindowsIdentity))
            icaTemplate += "Username=" + ica.Username + "\n";

        icaTemplate += "WinStationDriver=ICA 3.0\n"
       + "[Compress]\n"
       + "DriverNameWin16=pdcompw.dll\n"
       + "DriverNameWin32=pdcompn.dll\n"

       + "[EncRC5-0]\n"
       + "DriverNameWin16=pdc0w.dll\n"
       + "DriverNameWin32=pdc0n.dll\n"

       + "[EncRC5-128]\n"
       + "DriverNameWin16=pdc128w.dll\n"
       + "DriverNameWin32=pdc128n.dll\n"

       + "[EncRC5-40]\n"
       + "DriverNameWin16=pdc40w.dll\n"
       + "DriverNameWin32=pdc40n.dll\n"

       + "[EncRC5-56]\n"
       + "DriverNameWin16=pdc56w.dll\n"
       + "DriverNameWin32=pdc56n.dll\n";
        #endregion


        return icaTemplate;
    }




    internal static string generateICAContentWithCAG(string ticket, string serverAddress, string appName, Credentials credentials, string clientName)
    {

        string CredStr = "";

        string postData = "<?xml version='1.0' encoding='utf-8'?>"
                              + "<!DOCTYPE NFuseProtocol>"
                              + "<NFuseProtocol version='4.2'>"
                              + "<RequestServerFarmData>"
                              + "<Nil/>"
                              + "</RequestServerFarmData>"
                              + "</NFuseProtocol>";
        string errorMessage = "";
        string farmName = "";
        string responseText = HTTPManager.GetHttpResponse(WSConstants.CTX_XML, postData);

        if (Parser.checkErrorResponse(responseText, out errorMessage))
            throw new SoapException(errorMessage, SoapException.ServerFaultCode);
        else
        {
            farmName = Parser.parseFarmDataResponse(responseText);
            if (WSConstants.LOGDETINFO)
                EventLogger.logEvent("generateICAContentWithCAG: farm Name: " + farmName, 1);
           
        }

        if (credentials != null)
        {
            CredStr = "LogonTicket=" + ticket + "\n"
                                 + "SessionsharingKey=8-basic-basic-" + credentials.Domain + "-" + credentials.UserName + "-" + farmName + "\n"
                                 + "Username=" + (credentials == null ? "" : credentials.UserName) + "\n"
                                 + "Domain=\\" + ticket.Substring(14) + "\n"
                                 + "ClearPassword=" + ticket.Substring(0, 14) + "\n";
        }

       
             string STARequest = "";
            
                      
                try
                {
                    STARequest = "<?xml version=\"1.0\"?>\n" +
                                   "<!DOCTYPE CtxSTAProtocol SYSTEM \"CtxSTA.dtd\">\n" +
                                   "<CtxSTAProtocol version=\"4.0\">\n" +
                                   "<RequestTicket>\n" +
                                   "<AllowedTicketType>STA-v4</AllowedTicketType>\n" +
                                   "<AllowedAuthorityIDType>STA-v1</AllowedAuthorityIDType>\n" +
                                   "<TicketData>\n" +
                        
                                    "<Value name=\"ICAAddress\">" + serverAddress + ":" + WSConstants.ICAPORT + "</Value>\n" +
                                    "<Value name=\"XData\">\n" +
                                   "&lt;?xml version=&quot;1.0&quot;?&gt;&lt;!--DOCTYPE CtxConnInfoProtocol SYSTEM &quot;CtxConnInfo.dtd&quot;--&gt;&lt;CtxConnInfo version=&quot;1.0&quot;&gt;&lt;ServerAddress&gt;172.19.0.49:1494&lt;/ServerAddress&gt;&lt;UserName&gt;administrator&lt;/UserName&gt;&lt;UserDomain&gt;xendesktop&lt;/UserDomain&gt;&lt;ApplicationName&gt;Notepad&lt;/ApplicationName&gt;&lt;Protocol&gt;ICA&lt;/Protocol&gt;&lt;/CtxConnInfo&gt; </Value>\n" +
                                   "</TicketData>\n" +
                                   "<Control>\n" +
                                    "<Refreshable>true</Refreshable>\n" +
                                   "</Control>\n" +
                                   "</RequestTicket>\n" +
                                   "</CtxSTAProtocol> ";
                    
                    responseText = GetStaResponse( STARequest); //HTTPManager.doHttpRequest(lastUsedStaUrl, STARequest);
                    
                   
                }
                catch (System.Net.WebException webExc)
                {
                    if (WSConstants.LOGERRORS)
                        EventLogger.logEvent(webExc.Message, 0);
                }
                catch (System.Web.Services.Protocols.SoapException soapExc)
                {
                    if (WSConstants.LOGERRORS)
                        EventLogger.logEvent(soapExc.Message, 0);
                }


                if (responseText.Equals(""))
                {
                   if (WSConstants.LOGERRORS)
                        EventLogger.logEvent(WSConstants.STA_INVALID_RESPONSE, 0);
                   
                    throw new SoapException(WSConstants.STA_INVALID_RESPONSE, SoapException.ServerFaultCode);
                }



         string authID = "";
         string staTicket = "";
         string version = "";
         Parser.parseSTATicketResponse(responseText, ref authID, ref staTicket, ref version);
       
        if (WSConstants.LOGDETINFO)
             EventLogger.logEvent("STA Ticket: " + staTicket, 1);

     string icaTem = 
                    "[Encoding]\n"+
                    "InputEncoding=UTF-8\n"+

                    "[WFClient]\n"+
                    "CPMAllowed=On\n"+
                     "ClientName=" + clientName + "\n"+
                    "ProxyFavorIEConnectionSetting=Yes\n"+
                    "ProxyTimeout=30000\n"+
                    "ProxyType=Auto\n"+
                    "ProxyUseFQDN=Off\n"+
                    "RemoveICAFile=yes\n"+
                    "TransparentKeyPassthrough=Local\n"+
                    "TransportReconnectEnabled=Off\n"+
                    "VSLAllowed=On\n"+
                    "Version=2\n"+
                    "VirtualCOMPortEmulation=Off\n"+

                    "[ApplicationServers]\n"+
                     appName + "=\n"+
                    "[" + appName + "]\n"+
                    "Address=;" + version + ";" + authID + ";" + staTicket + "\n"+
                    "AudioBandwidthLimit=2\n"+
                    "AutologonAllowed=ON\n"+
                    "BrowserProtocol=HTTPonTCP\n"+
                    //"ClearPassword=" + ticket.Substring(0, 14) + "\n" +
                    "ClientAudio=On\n"+
                    "DesiredColor=8\n"+
                    "DesiredHRES=1024\n"+
                    "DesiredVRES=768\n"+
                    "DoNotUseDefaultCSL=On\n"+
                   // "Domain=\\" + ticket.Substring(14) + "\n" +
                    "HTTPBrowserAddress=!\n"+
                     "InitialProgram=#" + appName + "\n"+
                    "LPWD=141\n"+
                    "Launcher=PNAgent\n" +
                    "LocHttpBrowserAddress=!\n"+
                   // "LogonTicket=" + ticket + "\n" +
                    "LogonTicketType=CTXS1\n"+
                    "LongCommandLine=\n"+
                    "NRWD=31\n"+
                    "ProxyTimeout=30000\n"+
                    "ProxyType=Auto\n"+
                    "SSLCiphers=all\n"+
                    "SSLEnable=On\n"+
                    "SSLProxyHost="+WSConstants.CAG_FQDN+":"+WSConstants.CAG_Port+"\n"+
                    "SecureChannelProtocol=Detect\n"+
                    //"SessionsharingKey=8-basic-basic-" + credentials.Domain + "-" + credentials.UserName + "-" + farmName + "\n" +
                    //"StartIFDCD=1218175949419\n"+
                    //"StartSCD=1218175949419\n"+
                    CredStr+
                    "TRWD=16\n"+
                    "TWIMode=On\n"+
                    "TransportDriver=TCP/IP\n"+
                    "UILocale=en\n"+
                    //"Username=" + (credentials == null ? "" : credentials.UserName) + "\n" +
                    "WinStationDriver=ICA 3.0\n"+

                    "[Compress]\n"+
                    "DriverNameWin16=pdcompw.dll\n"+
                    "DriverNameWin32=pdcompn.dll\n"+

                    "[EncRC5-0]\n"+
                    "DriverNameWin16=pdc0w.dll\n"+
                    "DriverNameWin32=pdc0n.dll\n"+

                    "[EncRC5-128]\n"+
                    "DriverNameWin16=pdc128w.dll\n"+
                    "DriverNameWin32=pdc128n.dll\n"+

                    "[EncRC5-40]\n"+
                    "DriverNameWin16=pdc40w.dll\n"+
                    "DriverNameWin32=pdc40n.dll\n"+

                    "[EncRC5-56]\n"+
                    "DriverNameWin16=pdc56w.dll\n"+
                    "DriverNameWin32=pdc56n.dll\n";

        return icaTem;
    }




    private static string GetStaResponse( string STARequest) {
        string[] urls = WSConstants.STA_URL.Split(",".ToCharArray());
        for (int i = 0; i < urls.Length; i++ )
        {
            string url = GetLastUsedSTAURL();
            try
            {
                if (WSConstants.LOGINFO)
                    EventLogger.logEvent("Contact STA URL: " + url, 1);

                return HTTPManager.doHttpRequest(url, STARequest);
                
            }
            catch {
                if (WSConstants.LOGERRORS)
                    EventLogger.logEvent("Error Contact STA URL: " + url, 0);

                if (!WebService.failedSTAUrls.ContainsKey(url))
                    WebService.failedSTAUrls.Add(url, System.DateTime.Now);
            }
        }
        return "";

    }


    private static string GetLastUsedSTAURL()
    {

        string[] staUrls = WSConstants.STA_URL.Split(",".ToCharArray());
        
        if (staUrls == null) return "";

        if (WebService.LastSTAServer.Equals(""))
        {
            string lastURL = getUnFailedSta(staUrls, -1);
            WebService.LastSTAServer = lastURL;
            if (WSConstants.LOGINFO)
                EventLogger.logEvent("Last used STA URL: " + WebService.LastSTAServer, 1);
            return lastURL;
        }
        else {
                for (int i = 0; i < staUrls.Length; i++)
                {
                    if (WebService.LastSTAServer.Equals(staUrls[i]))
                    {
                        string lastURL = getUnFailedSta(staUrls , i);
                        WebService.LastSTAServer = lastURL;

                        if (WSConstants.LOGINFO)
                            EventLogger.logEvent("Last used STA URL: " + WebService.LastSTAServer, 1);
                        return lastURL;
                    }

                }

                if (WSConstants.LOGINFO)
                    EventLogger.logEvent("Last used STA URL: " + WebService.LastSTAServer,1);
                return WebService.LastSTAServer;
            }
        
        }

    
    
    
    private static string getUnFailedSta(string [] staUrls, int currentIndex) {
        
        int i = 0;
        if (currentIndex == (staUrls.Length - 1)) i = 0;
        else i = currentIndex + 1;
        
        for (int j = 0; j < staUrls.Length; j++ )
        {
            if (WebService.failedSTAUrls.ContainsKey(staUrls[(i + j) % staUrls.Length]))
            {
                try
                {
                    int index = ((i + j) % staUrls.Length) ;
                    string key = staUrls[index];
                    DateTime date = (DateTime)WebService.failedSTAUrls[key];
                    DateTime lastFailerTime = date;// System.DateTime.Parse(date);
                    DateTime now = System.DateTime.Now;
                    TimeSpan diff = now.Subtract(lastFailerTime);
                    if (diff.TotalMinutes >= int.Parse(WSConstants.STA_FailOver))
                        continue;
                    else
                    {
                        if (WSConstants.LOGDETINFO)
                            EventLogger.logEvent("getUnFailedSta: succeed " + staUrls[(i + j) % staUrls.Length], 0);
                        return staUrls[(i + j) % staUrls.Length];
                    }
                }
                catch(Exception exc) {
                    if (WSConstants.LOGDETINFO)
                        EventLogger.logEvent("getUnFailedSta: Failed" + exc.Message, 0);
                    continue;
                }
            }
            else
            {
                if (WSConstants.LOGDETINFO)
                    EventLogger.logEvent("getUnFailedSta, Succeed: " + staUrls[(i + j) % staUrls.Length], 1);
                return staUrls[(i + j) % staUrls.Length ];
            }
            
        }

        if (WSConstants.LOGDETINFO)
            EventLogger.logEvent("getUnFailedSta, Succeed: " + staUrls[currentIndex], 1);
        return staUrls[currentIndex];

    }

}
