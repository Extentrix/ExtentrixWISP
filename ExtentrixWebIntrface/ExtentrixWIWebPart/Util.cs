using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;
using Microsoft.SharePoint;
using System.Xml.Serialization;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart
{
    class Util
    {
        public static string XML_ELEMENT_APPLICATIONS = "Applications";
        public static string XML_ELEMENT_APP_ITEM = "Application";

        private static XmlElement CreateXMLApplicaitonItem(XmlDocument xmlDoc, ApplicationItem item, string farmName)
        {
            XmlElement app = xmlDoc.CreateElement(XML_ELEMENT_APP_ITEM);
            app.SetAttribute("ID", item.InternalName);

            //add Application In start menu
            XmlElement appInStartmenu = xmlDoc.CreateElement("AppInStartmenu");
            appInStartmenu.InnerText = item.AppInStartmenu;
            app.AppendChild(appInStartmenu);

            XmlElement appOnDesktop = xmlDoc.CreateElement("AppOnDesktop");
            appOnDesktop.InnerText = item.AppOnDesktop;
            app.AppendChild(appOnDesktop);

            XmlElement changeCount = xmlDoc.CreateElement("ChangeCount");
            changeCount.InnerText = item.ChangeCount;
            app.AppendChild(changeCount);

            XmlElement clientType = xmlDoc.CreateElement("ClientType");
            clientType.InnerText = item.ClientType;
            app.AppendChild(clientType);

            XmlElement contentAddress = xmlDoc.CreateElement("ContentAddress");
            contentAddress.InnerText = item.ContentAddress;
            app.AppendChild(contentAddress);

            XmlElement description = xmlDoc.CreateElement("Description");
            description.InnerText = item.Description;
            app.AppendChild(description);

            XmlElement encryption = xmlDoc.CreateElement("Encryption");
            encryption.InnerText = item.Encryption;
            app.AppendChild(encryption);

            XmlElement encryptionMinimum = xmlDoc.CreateElement("EncryptionMinimum");
            encryptionMinimum.InnerText = item.EncryptionMinimum;
            app.AppendChild(encryptionMinimum);

            FileType[] fileTypes = item.fileTypes;
            XmlElement filetypeRoot = xmlDoc.CreateElement("FileTypes");
            if (fileTypes != null)
            {
                foreach (FileType ft in fileTypes)
                {
                    XmlElement fileType = xmlDoc.CreateElement("FileType");
                    XmlElement fileName = xmlDoc.CreateElement("FileTypeName");
                    fileName.InnerXml = ft.FileTypeName;
                    fileType.AppendChild(fileName);

                    XmlElement isDef = xmlDoc.CreateElement("IsDefault");
                    isDef.InnerXml = ft.IsDefault.ToString();
                    fileType.AppendChild(isDef);

                    XmlElement ow = xmlDoc.CreateElement("Overwrite");
                    ow.InnerXml = ft.Overwrite.ToString();
                    fileType.AppendChild(ow);

                    XmlElement exts = xmlDoc.CreateElement("FileExtentions");
                    if (ft.FileExtension != null)
                    {
                        foreach (string ext in ft.FileExtension)
                        {
                            XmlElement ee = xmlDoc.CreateElement("Extention");
                            ee.InnerXml = ext;
                            exts.AppendChild(ee);
                        }
                    }
                    fileType.AppendChild(exts);

                    XmlElement mms = xmlDoc.CreateElement("MimeTypes");
                    if (ft.MimeType != null)
                    {
                        foreach (string mm in ft.MimeType)
                        {
                            XmlElement me = xmlDoc.CreateElement("mime");
                            me.InnerXml = mm;
                            mms.AppendChild(me);
                        }
                    }
                    fileType.AppendChild(mms);

                    XmlElement param = xmlDoc.CreateElement("Parameters");
                    param.InnerXml = ft.Parameters;
                    fileType.AppendChild(param);

                    filetypeRoot.AppendChild(fileType);
                }
            }
            app.AppendChild(filetypeRoot);

            XmlElement folderName = xmlDoc.CreateElement("FolderName");
            folderName.InnerText = item.FolderName;
            app.AppendChild(folderName);

            XmlElement freindlyName = xmlDoc.CreateElement("FreindlyName");
            freindlyName.InnerText = item.FreindlyName;
            app.AppendChild(freindlyName);

            XmlElement icon = xmlDoc.CreateElement("Icon");
            icon.InnerText = item.Icon;
            app.AppendChild(icon);

            XmlElement internalName = xmlDoc.CreateElement("InternalName");
            internalName.InnerText = item.InternalName;
            app.AppendChild(internalName);

            XmlElement publisherName = xmlDoc.CreateElement("PublisherName");
            publisherName.InnerText = item.PublisherName;
            app.AppendChild(publisherName);

            XmlElement remoteAccessEnabled = xmlDoc.CreateElement("RemoteAccessEnabled");
            remoteAccessEnabled.InnerText = item.RemoteAccessEnabled;
            app.AppendChild(remoteAccessEnabled);

            XmlElement serverType = xmlDoc.CreateElement("ServerType");
            serverType.InnerText = item.ServerType;
            app.AppendChild(serverType);

            XmlElement soundType = xmlDoc.CreateElement("SoundType");
            soundType.InnerText = item.SoundType;
            app.AppendChild(soundType);

            XmlElement soundTypeMinimum = xmlDoc.CreateElement("SoundTypeMinimum");
            soundTypeMinimum.InnerText = item.SoundTypeMinimum;
            app.AppendChild(soundTypeMinimum);

            XmlElement SSLEnabled = xmlDoc.CreateElement("SSLEnabled");
            SSLEnabled.InnerText = item.SSLEnabled;
            app.AppendChild(SSLEnabled);

            XmlElement startmenuFolder = xmlDoc.CreateElement("StartmenuFolder");
            startmenuFolder.InnerText = item.StartmenuFolder;
            app.AppendChild(startmenuFolder);

            XmlElement startMenuRoot = xmlDoc.CreateElement("StartMenuRoot");
            startMenuRoot.InnerText = item.StartMenuRoot;
            app.AppendChild(startMenuRoot);

            User[] users = item.users;
            XmlElement usrs = xmlDoc.CreateElement("Users");
            if (users != null)
            {
                foreach (User usr in users)
                {
                    XmlElement ue = xmlDoc.CreateElement("User");
                    XmlElement uname = xmlDoc.CreateElement("UserName");
                    uname.InnerXml = usr.Name;
                    ue.AppendChild(uname);

                    XmlElement udomain = xmlDoc.CreateElement("UserDomain");
                    udomain.InnerXml = usr.DomainName;
                    ue.AppendChild(udomain);

                    XmlElement udt = xmlDoc.CreateElement("UserDomainType");
                    udt.InnerXml = usr.DomainType;
                    ue.AppendChild(udt);
                    usrs.AppendChild(ue);
                }
            }
            app.AppendChild(usrs);


            XmlElement videoType = xmlDoc.CreateElement("VideoType");
            videoType.InnerText = item.VideoType;
            app.AppendChild(videoType);

            XmlElement videoTypeMinimum = xmlDoc.CreateElement("VideoTypeMinimum");
            videoTypeMinimum.InnerText = item.VideoTypeMinimum;
            app.AppendChild(videoTypeMinimum);

            XmlElement winColor = xmlDoc.CreateElement("WinColor");
            winColor.InnerText = item.WinColor;
            app.AppendChild(winColor);

            XmlElement winHeight = xmlDoc.CreateElement("WinHeight");
            winHeight.InnerText = item.WinHeight;
            app.AppendChild(winHeight);

            XmlElement winScale = xmlDoc.CreateElement("WinScale");
            winScale.InnerText = item.WinScale;
            app.AppendChild(winScale);

            XmlElement winType = xmlDoc.CreateElement("WinType");
            winType.InnerText = item.WinType;
            app.AppendChild(winType);

            XmlElement winWidth = xmlDoc.CreateElement("WinWidth");
            winWidth.InnerText = item.WinWidth;
            app.AppendChild(winWidth);

            XmlElement farm = xmlDoc.CreateElement("FarmName");
            farm.InnerText = farmName;
            app.AppendChild(farm);

            //parent.AppendChild(app);

            return app;

        }

        private static string getXMLPath(XmlElement el)
        {
            string path = "";
            XmlNode parent = el.ParentNode;
            while (parent != null)
            {
                path = parent.Name + "\\" + path;
                parent = parent.ParentNode;
            }
            return path;
        }

        public static XmlElement getXMLElementByIdAttribute(XmlDocument xmlDoc, string tagName, string id)
        {
            XmlElement res = null;

            XmlNodeList tags = xmlDoc.GetElementsByTagName(tagName);
            IEnumerator ie = tags.GetEnumerator();

            while (ie.MoveNext())
            {
                if (((XmlElement)ie.Current).GetAttribute("ID").Equals(id))
                {
                    res = (XmlElement)ie.Current;
                }
            }


            return res;
        }

        private static XmlElement getXMLParentElement(XmlDocument xmlDoc, string folderName)
        {

            string[] folders = folderName.Split('\\');

            //XmlElement xmlFolder = xmlDoc.GetElementById(folderName);
            XmlElement xmlFolder = getXMLElementByIdAttribute(xmlDoc, "Folder", folderName);
            if (xmlFolder != null)
            {
                return xmlFolder;
            }
            else
            {
                if (folders.Length == 1)
                {
                    XmlElement xe = xmlDoc.CreateElement("Folder");
                    xe.SetAttribute("ID", folderName);
                    getXMLElementByIdAttribute(xmlDoc, "Applications", "root").AppendChild(xe);
                    return xe;
                }
                else
                {
                    string parentName = folderName.Substring(0, folderName.IndexOf(folders[folders.Length - 1]) - "\\".Length);
                    XmlElement pe =  getXMLParentElement(xmlDoc, parentName);
                    XmlElement xe = xmlDoc.CreateElement("Folder");
                    xe.SetAttribute("ID", folders[folders.Length - 1]);
                    pe.AppendChild(xe);
                    return xe;
                }
            }

        }

        public static string createXML_AllicationItem(PublishedApplication[] arrApplicatioItem)
        {
            string res_xml = "";

            #region commented applivcation attibutes
            //appInStartmenu = arrApplicatioItem[0].AppInStartmenu
            //appOnDesktop = arrApplicatioItem[0].AppOnDesktop
            //changeCount = arrApplicatioItem[0].ChangeCount;
            //clientType = arrApplicatioItem[0].ClientType
            //contentAddress = arrApplicatioItem[0].ContentAddress
            //description = arrApplicatioItem[0].Description
            //encryption = arrApplicatioItem[0].Encryption
            //encryptionMinimum = arrApplicatioItem[0].EncryptionMinimum
            //fileTypes = arrApplicatioItem[0].fileTypes   array!!!!
            //folderName = arrApplicatioItem[0].FolderName
            //freindlyName = arrApplicatioItem[0].FreindlyName
            //icon = arrApplicatioItem[0].Icon
            //internalName = arrApplicatioItem[0].InternalName
            //publisherName = arrApplicatioItem[0].PublisherName
            //remoteAccessEnabled = arrApplicatioItem[0].RemoteAccessEnabled
            //serverType = arrApplicatioItem[0].ServerType
            //soundType= arrApplicatioItem[0].SoundType
            //soundTypeMinimum = arrApplicatioItem[0].SoundTypeMinimum
            //SSLEnabled = arrApplicatioItem[0].SSLEnabled
            //startmenuFolder = arrApplicatioItem[0].StartmenuFolder
            //startMenuRoot = arrApplicatioItem[0].StartMenuRoot
            //users = arrApplicatioItem[0].users    array !!!!
            //videoType = arrApplicatioItem[0].VideoType
            //videoTypeMinimum = arrApplicatioItem[0].VideoTypeMinimum
            //winColor = arrApplicatioItem[0].WinColor
            //winHeight = arrApplicatioItem[0].WinHeight
            //winScale = arrApplicatioItem[0].WinScale
            //winType = arrApplicatioItem[0].WinType
            //winWidth = arrApplicatioItem[0].WinWidth
#endregion

            XmlDocument xmlDoc = new XmlDocument();

            //create xml document
            XmlElement root = xmlDoc.CreateElement(XML_ELEMENT_APPLICATIONS);
            root.SetAttribute("ID", "root");

            xmlDoc.AppendChild(root);

            int size = arrApplicatioItem.Length;

            foreach (PublishedApplication item in arrApplicatioItem)
            {

                //XmlElement app = xmlDoc.CreateElement(XML_ELEMENT_APP_ITEM);
                ApplicationItem appItem = item.Item;
                XmlElement app = CreateXMLApplicaitonItem(xmlDoc, appItem, item.FarmName);

                XmlElement p = null;
                if (!appItem.FolderName.Equals(String.Empty))
                {
                    p = getXMLParentElement(xmlDoc, appItem.FolderName);
                    p.AppendChild(app);
                }
                else
                {
                    root.AppendChild(app);
                }


                #region create xml item
                //    //add Application In start menu
                //    XmlElement appInStartmenu = xmlDoc.CreateElement("AppInStartmenu");
                //    appInStartmenu.InnerText = item.AppInStartmenu;
                //    app.AppendChild(appInStartmenu);

                //    XmlElement appOnDesktop = xmlDoc.CreateElement("AppOnDesktop");
                //    appOnDesktop.InnerText = item.AppOnDesktop;
                //    app.AppendChild(appOnDesktop);

                //    XmlElement changeCount = xmlDoc.CreateElement("ChangeCount");
                //    changeCount.InnerText = item.ChangeCount;
                //    app.AppendChild(changeCount);

                //    XmlElement clientType = xmlDoc.CreateElement("ClientType");
                //    clientType.InnerText = item.ClientType;
                //    app.AppendChild(clientType);

                //    XmlElement contentAddress = xmlDoc.CreateElement("ContentAddress");
                //    contentAddress.InnerText = item.ContentAddress;
                //    app.AppendChild(contentAddress);

                //    XmlElement description = xmlDoc.CreateElement("Description");
                //    description.InnerText = item.Description;
                //    app.AppendChild(description);

                //    XmlElement encryption = xmlDoc.CreateElement("Encryption");
                //    encryption.InnerText = item.Encryption;
                //    app.AppendChild(encryption);

                //    XmlElement encryptionMinimum = xmlDoc.CreateElement("EncryptionMinimum");
                //    encryptionMinimum.InnerText = item.EncryptionMinimum;
                //    app.AppendChild(encryptionMinimum);

                //    FileType[] fileTypes = item.fileTypes;
                //    XmlElement filetypeRoot = xmlDoc.CreateElement("FileTypes");
                //    if (fileTypes != null)
                //    {
                //        foreach (FileType ft in fileTypes)
                //        {
                //            XmlElement fileType = xmlDoc.CreateElement("FileType");
                //            XmlElement fileName = xmlDoc.CreateElement("FileTypeName");
                //            fileName.InnerXml = ft.FileTypeName;
                //            fileType.AppendChild(fileName);

                //            XmlElement isDef = xmlDoc.CreateElement("IsDefault");
                //            isDef.InnerXml = ft.IsDefault.ToString();
                //            fileType.AppendChild(isDef);

                //            XmlElement ow = xmlDoc.CreateElement("Overwrite");
                //            ow.InnerXml = ft.Overwrite.ToString();
                //            fileType.AppendChild(ow);

                //            XmlElement exts = xmlDoc.CreateElement("FileExtentions");
                //            if (ft.FileExtension != null)
                //            {
                //                foreach (string ext in ft.FileExtension)
                //                {
                //                    XmlElement ee = xmlDoc.CreateElement("Extention");
                //                    ee.InnerXml = ext;
                //                    exts.AppendChild(ee);
                //                }
                //            }
                //            fileType.AppendChild(exts);

                //            XmlElement mms = xmlDoc.CreateElement("MimeTypes");
                //            if (ft.MimeType != null)
                //            {
                //                foreach (string mm in ft.MimeType)
                //                {
                //                    XmlElement me = xmlDoc.CreateElement("mime");
                //                    me.InnerXml = mm;
                //                    mms.AppendChild(me);
                //                }
                //            }
                //            fileType.AppendChild(mms);

                //            XmlElement param = xmlDoc.CreateElement("Parameters");
                //            param.InnerXml = ft.Parameters;
                //            fileType.AppendChild(param);

                //            filetypeRoot.AppendChild(fileType);
                //        }
                //    }
                //    app.AppendChild(filetypeRoot);

                //    XmlElement folderName = xmlDoc.CreateElement("FolderName");
                //    folderName.InnerText = item.FolderName;
                //    app.AppendChild(folderName);

                //    XmlElement freindlyName = xmlDoc.CreateElement("FreindlyName");
                //    freindlyName.InnerText = item.FreindlyName;
                //    app.AppendChild(freindlyName);

                //    XmlElement icon = xmlDoc.CreateElement("Icon");
                //    icon.InnerText = item.Icon;
                //    app.AppendChild(icon);

                //    XmlElement internalName = xmlDoc.CreateElement("InternalName");
                //    internalName.InnerText = item.InternalName;
                //    app.AppendChild(internalName);

                //    XmlElement publisherName = xmlDoc.CreateElement("PublisherName");
                //    publisherName.InnerText = item.PublisherName;
                //    app.AppendChild(publisherName);

                //    XmlElement remoteAccessEnabled = xmlDoc.CreateElement("RemoteAccessEnabled");
                //    remoteAccessEnabled.InnerText = item.RemoteAccessEnabled;
                //    app.AppendChild(remoteAccessEnabled);

                //    XmlElement serverType = xmlDoc.CreateElement("ServerType");
                //    serverType.InnerText = item.ServerType;
                //    app.AppendChild(serverType);

                //    XmlElement soundType = xmlDoc.CreateElement("SoundType");
                //    soundType.InnerText = item.SoundType;
                //    app.AppendChild(soundType);

                //    XmlElement soundTypeMinimum = xmlDoc.CreateElement("SoundTypeMinimum");
                //    soundTypeMinimum.InnerText = item.SoundTypeMinimum;
                //    app.AppendChild(soundTypeMinimum);

                //    XmlElement SSLEnabled = xmlDoc.CreateElement("SSLEnabled");
                //    SSLEnabled.InnerText = item.SSLEnabled;
                //    app.AppendChild(SSLEnabled);

                //    XmlElement startmenuFolder = xmlDoc.CreateElement("StartmenuFolder");
                //    startmenuFolder.InnerText = item.StartmenuFolder;
                //    app.AppendChild(startmenuFolder);

                //    XmlElement startMenuRoot = xmlDoc.CreateElement("StartMenuRoot");
                //    startMenuRoot.InnerText = item.StartMenuRoot;
                //    app.AppendChild(startMenuRoot);

                //    User[] users = item.users;
                //    XmlElement usrs = xmlDoc.CreateElement("Users");
                //    if (users != null)
                //    {
                //        foreach (User usr in users)
                //        {
                //            XmlElement ue = xmlDoc.CreateElement("User");
                //            XmlElement uname = xmlDoc.CreateElement("UserName");
                //            uname.InnerXml = usr.Name;
                //            ue.AppendChild(uname);

                //            XmlElement udomain = xmlDoc.CreateElement("UserDomain");
                //            udomain.InnerXml = usr.DomainName;
                //            ue.AppendChild(udomain);

                //            XmlElement udt = xmlDoc.CreateElement("UserDomainType");
                //            udt.InnerXml = usr.DomainType;
                //            ue.AppendChild(udt);
                //            usrs.AppendChild(ue);
                //        }
                //    }
                //    app.AppendChild(usrs);


                //    XmlElement videoType = xmlDoc.CreateElement("VideoType");
                //    videoType.InnerText = item.VideoType;
                //    app.AppendChild(videoType);

                //    XmlElement videoTypeMinimum = xmlDoc.CreateElement("VideoTypeMinimum");
                //    videoTypeMinimum.InnerText = item.VideoTypeMinimum;
                //    app.AppendChild(videoTypeMinimum);

                //    XmlElement winColor = xmlDoc.CreateElement("WinColor");
                //    winColor.InnerText = item.WinColor;
                //    app.AppendChild(winColor);

                //    XmlElement winHeight = xmlDoc.CreateElement("WinHeight");
                //    winHeight.InnerText = item.WinHeight;
                //    app.AppendChild(winHeight);

                //    XmlElement winScale = xmlDoc.CreateElement("WinScale");
                //    winScale.InnerText = item.WinScale;
                //    app.AppendChild(winScale);

                //    XmlElement winType = xmlDoc.CreateElement("WinType");
                //    winType.InnerText = item.WinType;
                //    app.AppendChild(winType);

                //    XmlElement winWidth = xmlDoc.CreateElement("WinWidth");
                //    winWidth.InnerText = item.WinWidth;
                //    app.AppendChild(winWidth);

                //    root.AppendChild(app);

                ////            xmlDoc.AppendChild(root);
                #endregion
            }
            res_xml = xmlDoc.OuterXml;

            return res_xml;
        }

        public static XmlAttribute getAttibuteByName(XmlNode node, string name)
        {
            XmlAttribute res = null;
            IEnumerator ie = node.Attributes.GetEnumerator();
            while (ie.MoveNext())
            {
                XmlAttribute attr = (XmlAttribute)ie.Current;
                if (attr.Name.Equals(name))
                {
                    res = attr;
                    break;
                }

            }

            return res;
        }
        public static List<PublishedApplication> GetPublishedApplications()
        {
            List<PublishedApplication> apps = new List<PublishedApplication>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(SPContext.Current.Web.Url + "/_layouts/Images/ExtentrixWebIntrface/Applications.xml");

            foreach (XmlNode node in xmlDoc.SelectNodes("//Application"))
            {
                PublishedApplication app = new PublishedApplication(node);
                apps.Add(app);
            }
            
            //string rs = Util.createXML_AllicationItem(Apps);

            return apps;
        }
    }
}
