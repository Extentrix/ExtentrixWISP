using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExtentrixWebIntrface.EWS;
using System.Xml;
using System.Collections;
using ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils;
using Microsoft.SharePoint;


namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
    public class PublishedApplicationEx
    {
        public ApplicationItemEx applicationItem;
        public int farmIndex;
    }

    [Serializable]
    public class PublishedApplication
    {
        private ApplicationItem _applicationItem;
        public ApplicationItem Item
        {
            get
            {
                return _applicationItem;
            }
            set
            {
                _applicationItem = value;
            }
        }
        public int FarmIndex
        {
            get;
            set;
        }

        public string AppName
        {
            get;
            set;
        }


        public string SmallIcon
        {
            get;
            set;
        }

        public string IconFileName
        {
            get
            {
                string fileName = "icon_" + _applicationItem.InternalName.Replace(' ', '_') + ".ico";
                return fileName;
            }
            
        }
        public string LaunchAppUri
        {
            get
            {
                return string.Format("{0}?app={1}&farmIndx={2}", SPContext.Current.Web.Url, Uri.EscapeDataString(AppName), FarmIndex);                 
            }
            
        }
        

        public PublishedApplication()
        { }

        public PublishedApplication(XmlNode node)
        {
            _applicationItem = new ApplicationItem();
            FillItem(node);
        }
        private void FillItem(XmlNode node)
        {
            if (node.Name.Equals("Application"))
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {                    
                    switch (childNode.Name)
                    {                            
                        case Constants.AppInStartmenu:
                            _applicationItem.AppInStartmenu = childNode.InnerText;
                            break;
                        case Constants.AppOnDesktop:
                           _applicationItem.AppOnDesktop = childNode.InnerText;
                            break;
                        case Constants.ChangeCount:
                           _applicationItem.ChangeCount = childNode.InnerText;
                            break;
                        case Constants.ClientType:
                            _applicationItem.ClientType = childNode.InnerText;
                            break;
                        case Constants.ContentAddress:
                            _applicationItem.ContentAddress = childNode.InnerText;
                            break;
                        case Constants.Description:
                            _applicationItem.Description = childNode.InnerText;
                            break;
                        case Constants.Encryption:
                            _applicationItem.Encryption = childNode.InnerText;
                            break;
                        case Constants.EncryptionMinimum:
                            _applicationItem.EncryptionMinimum = childNode.InnerText;
                            break;
                        case Constants.FarmIndex:
                            FarmIndex = int.Parse(childNode.InnerText);
                            break;
                        case Constants.FileTypes:
                            //todo 
                            if (childNode.HasChildNodes)
                            {
                                FileType[] fileTypes = new FileType[childNode.ChildNodes.Count];
                                int i = 0;
                                foreach (XmlNode fileTypeNode in childNode.ChildNodes)
                                {
                                    FileType fileType = new FileType();                                    
                                    i++;                                                                    
                                }
                                _applicationItem.fileTypes = fileTypes;
                            }
                            break;
                        case Constants.FolderName:
                            string folder = childNode.InnerText;
                            _applicationItem.FolderName = folder;                           
                            break;
                        case Constants.FreindlyName:
                            _applicationItem.FreindlyName = childNode.InnerText;
                            break;
                        case Constants.Icon:
                            _applicationItem.Icon = childNode.InnerText;
                            break;
                        case Constants.SmallIcon:
                            SmallIcon = childNode.InnerText;
                            break;
                        case Constants.InternalName:
                            _applicationItem.InternalName = childNode.InnerText;
                            AppName = childNode.InnerText;
                            break;
                        case Constants.PublisherName:
                            _applicationItem.PublisherName = childNode.InnerText;
                            break;
                        case Constants.RemoteAccessEnabled:
                            _applicationItem.RemoteAccessEnabled = childNode.InnerText;
                            break;
                        case Constants.ServerType:
                            _applicationItem.ServerType = childNode.InnerText;
                            break;
                        case Constants.SoundType:
                            _applicationItem.SoundType = childNode.InnerText;
                            break;
                        case Constants.SoundTypeMinimum:
                            _applicationItem.SoundTypeMinimum = childNode.InnerText;
                            break;
                        case Constants.SSLEnabled:
                            _applicationItem.SSLEnabled = childNode.InnerText;
                            break;
                        case Constants.StartmenuFolder:
                            _applicationItem.StartmenuFolder = childNode.InnerText;
                            break;
                        case Constants.StartMenuRoot:
                            _applicationItem.StartMenuRoot = childNode.InnerText;
                            break;
                        case Constants.Users:
                            //todo 
                            if (childNode.HasChildNodes)
                            {
                                User[] users = new User[childNode.ChildNodes.Count];
                                int i = 0;
                                foreach (XmlNode userNode in childNode.ChildNodes)
                                {
                                    User fileType = new User();
                                    i++;
                                }
                                _applicationItem.users = users;
                            }                            
                            break;
                        case Constants.VideoType:
                            _applicationItem.VideoType = childNode.InnerText;
                            break;
                        case Constants.VideoTypeMinimum:
                            _applicationItem.VideoTypeMinimum = childNode.InnerText;
                            break;
                        case Constants.WinColor:
                            _applicationItem.WinColor = childNode.InnerText;
                            break;
                        case Constants.WinHeight:
                            _applicationItem.WinHeight = childNode.InnerText;
                            break;
                        case Constants.WinScale:
                            _applicationItem.WinScale = childNode.InnerText;
                            break;
                        case Constants.WinType:
                            _applicationItem.WinType = childNode.InnerText;
                            break;
                        case Constants.WinWidth:
                            _applicationItem.WinWidth = childNode.InnerText;
                            break;
                        default:
                            break;
                    }
                }


            }
        }
    }
}
