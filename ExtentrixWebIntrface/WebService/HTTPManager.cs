using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Services.Protocols;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;

/// <summary>
/// Summary description for HTTPManger
/// </summary>
public class HTTPManager
{
    static public string doHttpRequest(string url, string postData)
    {
        
        //set Expect flag to  false to avoid expect exception
        ServicePointManager.Expect100Continue = false;

        // *** Establish the request 
        HttpWebRequest loHttp =  (HttpWebRequest)WebRequest.Create(url);
        // set headers
        loHttp.ContentType = "text/xml";
        loHttp.Method = "POST";
        loHttp.KeepAlive = false;

        // set post data
        StreamWriter loPostData = new StreamWriter(loHttp.GetRequestStream());
        loPostData.Write(postData);
        loPostData.Close();
        string xx = "";
        try
        {
            // get response
            HttpWebResponse webResponse = (HttpWebResponse)loHttp.GetResponse();
            
            StringBuilder sbuilder = new StringBuilder();
            StringBuilder responseBuilder = new StringBuilder();
            int dataSize;
            
            char[] buffer;
            Encoding enc;
            try
            {
                enc = Encoding.GetEncoding(webResponse.ContentEncoding);
            }
            catch
            {
                enc = Encoding.GetEncoding(1252);//windows-1252 code page (code page value 1252)
            }
            StreamReader responseStreamReader = new StreamReader(webResponse.GetResponseStream(),enc);
            // Read response chuncks
            //bool readSize = false;
            if (webResponse.Headers["Transfer-Coding"] == "chunked")
            {
                while (!responseStreamReader.EndOfStream)
                {

                   // get the string representing chunck size
                    char readChar = (char)responseStreamReader.Read();
                    
                    if (readChar != '\r')
                        sbuilder.Append(readChar);
                    else
                    {
                       
                        // calculate chunck size

                        xx = sbuilder.ToString();
                        dataSize = new Hex(sbuilder.ToString()).ToInt();
                        sbuilder.Remove(0, sbuilder.Length);
                        if (dataSize != 0)
                        {
                            if (WSConstants.LOGDETINFO)
                                EventLogger.logEvent("doHttpRequest: Read chunck with size:" + dataSize, 1);
                            // skip the '\n' character
                            responseStreamReader.Read();
                            //Intialize the buffer that will contain the chunck data

                            buffer = new char[dataSize];
                            // read the chunck 
                            responseStreamReader.ReadBlock(buffer, 0, dataSize);
                            if (WSConstants.LOGDETINFO)
                                EventLogger.logEvent("doHttpRequest: Read chunck (with size:" + dataSize +"), Chunck Data"+ new string(buffer), 1);
                            //append the read block to the whole response
                            responseBuilder.Append(buffer);
                            GC.Collect();
                            // skip the trailing '\r' '\n' characters
                            responseStreamReader.Read();
                            responseStreamReader.Read();
                        }
                        else
                            break;
                    }
              }
                //write chunke to response bulider 
                if (responseBuilder.Length == 0 && sbuilder.Length > 0)
                {
                    responseBuilder.Append(sbuilder.ToString().ToCharArray());
                }
           }
           else
           {
              responseBuilder.Append(responseStreamReader.ReadToEnd());
           }
            webResponse.Close();
            responseStreamReader.Close();
            return System.Text.Encoding.UTF8.GetString(System.Text.Encoding.GetEncoding(1252).GetBytes(responseBuilder.ToString()) ) ;
        }
        catch (Exception ex)
        {
            throw new SoapException(ex.Message + "String :"+xx+ex.StackTrace, SoapException.ServerFaultCode);
        }

    }

    public static string GetHttpResponse(string url, string postData)
    {
        string[] urls = url.Split(",".ToCharArray());
        string response = "";
        for (int i = 0; i < urls.Length; i++)
        {
            try {
                if (WSConstants.LOGINFO)
                    EventLogger.logEvent("contact CPS server: " + "http://" + urls[i] + ":" + WSConstants.CTXXMLPort + "/scripts/WPnBr.dll", 1);
                response = doHttpRequest("http://" + urls[i] + ":" + WSConstants.CTXXMLPort + "/scripts/WPnBr.dll", postData);
                return response;
            }catch(Exception exc){
                
                if (WSConstants.LOGERRORS)
                    EventLogger.logEvent("Error contact CPS server: " + "http://" + urls[i] + ":" + WSConstants.CTXXMLPort + "/scripts/WPnBr.dll\n" + exc.Message, 0);
                if(WSConstants.LOGDETINFO)
                    EventLogger.logEvent(exc.StackTrace, 0);
               
            }
        }


      //  string s = "</Details> <SeqNo>1220271912</SeqNo> <ServerType>win32</ServerType> <ClientType>ica30</ClientType> </AppData> <AppData> <InName>SMP-NG</InName> <FName>SMP-NG</FName> <Details> <Settings appisdisabled=\"false\" appisdesktop=\"false\"> <Folder/> <Description>SMP-NG</Description> <WinWidth>1024</WinWidth> <WinHeight>768</WinHeight> <WinColor>4</WinColor> <WinType>pixels</WinType> <WinScale>75</WinScale> <SoundType minimum=\"false\">none</SoundType> <VideoType minimum=\"false\">none</VideoType> <Encryption minimum=\"false\">basic</Encryption> <AppOnDesktop value=\"true\"/> <AppInStartmenu value=\"false\"/> <PublisherName>STAHL-S</PublisherName> <SSLEnabled>false</SSLEnabled> <RemoteAccessEnabled>false</RemoteAccessEnabled> </Settings> </Details> <SeqNo>1220271923</SeqNo> <ServerType>win32</ServerType> <ClientType>ica30</ClientType> </AppData> <AppData> <InName>SMP-NG&#32;mit&#32;AnmeldungB2F9</InName> <FName>SMP-NG&#32;mit&#32;Anmeldung</FName> <Details> <Settings appisdisabled=\"false\" appisdesktop=\"false\"> <Folder/> <Description>SMP-NG-english&#32;mit&#32;Anmeldung</Description> <WinWidth>640</WinWidth> <WinHeight>480</WinHeight> <WinColor>4</WinColor> <WinType>pixels</WinType> <WinScale>75</WinScale> <SoundType minimum=\"false\">none</SoundType>";
        return response;
    
    }
}
