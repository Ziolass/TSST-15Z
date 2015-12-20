using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireCloud;
using WireCloudTerminal.CloudLogic;

namespace WireCloudTerminal
{
    class LinkViewModel : INotifyPropertyChanged
    {
        private Link Link;

        public LinkViewModel(Link link) {
            this.Link = link;
            Link.LinkActive += new LinkStateChangedHandler(riseChangesToView);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public bool Active
        {
            get
            {
                return Link.IsLinkActive;
            }
            set
            {
                Link.IsLinkActive = value;
                riseChangesToView();
            }
        }

        public String Id
        {
            get
            {
                return "[Link " + Link. + " : " + Link.Destination + "] : " + (Link.IsLinkActive ? "Włączony" : "Wyłączony");
            }
            set 
            {
                
            }
        }

        public void riseChangesToView() 
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Active"));
                PropertyChanged(this, new PropertyChangedEventArgs("Id"));
            }
        }

        public void Stop()
        {
            
        }

        public Link GetModel()
        {
            return Link;
        }
        
    }
}
