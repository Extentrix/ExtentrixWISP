using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.SharePoint;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
    [Serializable]
    public class Farm
    {
        public string Name
        {
            get;
            set;
        }
        public string Port
        {
            get;
            set;
        }       
        public string Url
        {
            get;
            set;
        }
    }
    [Serializable]
    public class Farms : IEnumerable<Farm>
    {
        //Dictionary<string, Farm> _farms;
        //public Farm this[int index]
        //{
        //    get
        //    {
        //        return _farms[index];
        //    }
        //}

        Dictionary<string, Farm> _farms;
        public Farm this[string farmName]
        {
            get
            {
                return _farms[farmName];
            }
            
        }

        public Farms()
        {
            //_farms = new Dictionary<int, Farm>();
            _farms = new Dictionary<string, Farm>();
            LoadFarms();
        }

        private void LoadFarms()
        {

            SPWeb web = SPContext.Current.Web.Site.RootWeb;
            SPList farmsList = web.Lists.TryGetList(Constants.FarmsList);

            foreach (SPListItem item in farmsList.Items)
            {
                Farm farm = new Farm();
                farm.Name = (string)item["Title"];
                
                farm.Url = (string)item["Url"];
                string tmp = (string)item["Port"];
                farm.Port = !string.IsNullOrEmpty(tmp) ? tmp.ToString() : "80";
               
                _farms.Add(farm.Name, farm);
            }           

        }
                
        public IEnumerator<Farm> GetEnumerator()
        {
            return _farms.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
