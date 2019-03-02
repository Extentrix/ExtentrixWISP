using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
    class FolderStructure
    {
        public string Path
        {
            get;
            set;
        }
        public string Parent
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public bool Leaf
        {
            get;
            set;
        }
    }
    class ApplicationsTree
    {
        List<Folder> _folders;
        Folder _rootFolder;
        List<PublishedApplication> _apps;

        public Folder Root
        {
            get
            {
                return _rootFolder;
            }
        }
        public ApplicationsTree(PublishedApplication[] apps)
        {
            BuildTree(apps);
        }
        public Folder GetFolder(string path)
        {
            Folder folder = null;
            foreach (var item in _folders)
            {
                if (item.Path == path)
                {
                    folder = item;
                    break;
                }
            }
            return folder;
        }

        public List<PublishedApplication> GetFolderApplications(Folder folder)
        {
            List<PublishedApplication> apps = new List<PublishedApplication>();
            foreach (var app in _apps)
            {
                if (app.Item.FolderName == folder.Path)
                {
                    apps.Add(app);
                }
            }
            return apps;
        }

        public PublishedApplication GetApplication(string appName)
        {
            foreach (var app in _apps)
            {
                if (app.AppName == appName)
                {
                    return app;
                }
            }
            return null;
        }

       
        private void BuildTree(PublishedApplication[] apps)
        {
            _rootFolder = new Folder(string.Empty, false);
            _folders = new List<Folder>();
            _apps = new List<PublishedApplication>();
            List<Folder> rootFolders = new List<Folder>();
            List<FolderStructure> folderStrucures = new List<FolderStructure>();
            foreach (PublishedApplication app in apps)
            {
								
								string folderName = !string.IsNullOrEmpty(app.Item.FolderName) ? app.Item.FolderName.Substring(app.Item.FolderName.LastIndexOf('\\') + 1) : string.Empty;

                _apps.Add(app);

                if (folderName != string.Empty)
                {
                    FolderStructure structure = new FolderStructure();
                    structure.Path = app.Item.FolderName;
                    structure.Name = folderName;
                    if (app.Item.FolderName.Contains("\\"))
                    {
                       // int length = app.Item.FolderName.LastIndexOf("\\") - 1;
                        structure.Parent = app.Item.FolderName.Substring(0, app.Item.FolderName.LastIndexOf("\\"));
                    }
                    else
                    {
                        structure.Parent = string.Empty;
                    }
                    folderStrucures.Add(structure);
                }
            }

            for (int i = 0; i < folderStrucures.Count; i++)
            {
                string path = folderStrucures[i].Path;
                bool isLeaf = true;
                foreach (var item in folderStrucures)
                {
                    if (item.Path.Contains(path) && (item.Path != path))
                    {
                        isLeaf = false;
                        break;
                    }
                }
                folderStrucures[i].Leaf = isLeaf;
            }

            Dictionary<string, FolderStructure> tempFolderStructures = new Dictionary<string, FolderStructure>();
            foreach (FolderStructure structure in folderStrucures)
            {
                if (!tempFolderStructures.ContainsKey(structure.Path))
                {
                    tempFolderStructures.Add(structure.Path, structure);
                }
            }


            foreach (FolderStructure structure in folderStrucures)
            {
                string tempPath = structure.Parent;
                if (!tempPath.Contains("\\"))
                {
                    if (!tempFolderStructures.ContainsKey(tempPath) && !string.IsNullOrEmpty(tempPath))
                    {
                        FolderStructure newStructure = new FolderStructure();
                        newStructure.Path = tempPath;
                        newStructure.Name = tempPath;
                        newStructure.Parent = string.Empty;
                        newStructure.Leaf = false;

                        tempFolderStructures.Add(tempPath, newStructure);
                    }
                }
                else
                {
                    while (tempPath.Contains("\\"))
                    {
                        int lastIndex = tempPath.LastIndexOf("\\");
                        FolderStructure newStructure = new FolderStructure();
                        newStructure.Path = tempPath;
                        newStructure.Name = tempPath.Substring(lastIndex + 1);
                        newStructure.Parent = tempPath.Substring(0, lastIndex);//tempPath.Substring(0, lastIndex - 1);
                        newStructure.Leaf = false;

                        tempPath = tempPath.Substring(0, lastIndex);//, lastIndex - 1);

                        if (!tempFolderStructures.ContainsKey(newStructure.Path))
                        {
                            tempFolderStructures.Add(newStructure.Path, newStructure);
                        }
                    }
                }
            }

            folderStrucures.Clear();
            folderStrucures.AddRange(tempFolderStructures.Values);

           // List<Folder> tempFolders = new List<Folder>();
            foreach (var item in folderStrucures)
            {
                Folder folder = new Folder(item.Path, item.Leaf);
                folder.Name = item.Name;
                folder.ParentPath = item.Parent;
                if (item.Parent == string.Empty)
                {
                    folder.Depth = 1;
                }
                else
                {
                    int i = 0;
                    foreach (var charItem in item.Path.ToCharArray())
                    {
                        if (charItem == '\\')
                        {
                            i++;
                        }
                    }
                    //i /= 2;
                    folder.Depth = i + 1;
                }
                _folders.Add(folder);
            }
            _folders.Sort(new FolderComparsion());
            for (int i = 0; i < _folders.Count; i++)
            {
                if (_folders[i].Name == _folders[i].Path)
                {
                    _rootFolder.AddFolder(_folders[i]);
                }
                else
                {
                    Folder parent = null;
                    foreach (var item in _folders)
                    {
                        if (item.Path == _folders[i].ParentPath)
                        {
                            parent = item;
                            break;
                        }
                    }
                    //Folder parent = FindFolder(tempFolders, tempFolders[i].ParentPath);
										if (parent != null)
										{
											parent.AddFolder(_folders[i]);
										}
                }
            }
        }


        public List<PublishedApplication> FindApplications(string search)
        {
            List<PublishedApplication> searchResults = new List<PublishedApplication>();

            foreach (var app in _apps)
            {
                if (app.AppName.ToLower().Contains(search.ToLower()))
                {
                    searchResults.Add(app);
                }
            }

            return searchResults;
        }
    }
}
