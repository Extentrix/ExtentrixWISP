using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
    class FolderComparsion : IComparer<Folder>
    {
        public int Compare(Folder x, Folder y)
        {
            if (x.Depth > y.Depth)
            {
                return 1;
            }
            if (x.Depth < y.Depth)
            {
                return -1;
            }
            if (x.Depth == y.Depth)
            {
                return 0;
            }
            return 0;
        }
    }
    class Folder
    {

        List<Folder> _folders;

        public int Depth
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public string Path
        {
            get;
            set;
        }
        public string ParentPath
        {
            get;
            set;
        }
        public bool IsRoot
        {
            get
            {
                return string.IsNullOrEmpty(Name);
            }            
        }
        public List<Folder> Folders
        {
            get
            {
                if (_folders == null)
                {
                    return new List<Folder>();
                }
                return _folders;
            }            
        }
        public void AddFolder(Folder folder)
        {
            _folders.Add(folder);
        }
        public Folder(string path, bool leaf)
        {
            Path = path;
            if (!leaf)
            {                
                _folders = new List<Folder>();
            }
        }
        
    }
}
