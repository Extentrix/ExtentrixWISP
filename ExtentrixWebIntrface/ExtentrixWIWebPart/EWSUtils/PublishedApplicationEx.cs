using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtentrixWebIntrface.ExtentrixWIWebPart.EWSUtils
{
    [Serializable]
    public class PublishedApplicationEx
    {
        private ApplicationItemEx _applicationItem;
        public ApplicationItemEx Item
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
        public PublishedApplicationEx()
        {
        }
    }
}
