using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireCloud;

namespace WireCloudTerminal
{
    class LinkViewModel : INotifyPropertyChanged
    {
        private Link link;

        public LinkViewModel(Link link) {
            this.link = link;
            link.LinkActive += new LinkStateChangedHandler(riseChangesToView);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public bool Active
        {
            get
            {
                return link.IsLinkActive;
            }
            set
            {
                link.IsLinkActive = value;
                riseChangesToView();
            }
        }

        public String Id
        {
            get
            {
                return "[Link " + link.Source + " : " + link.Destination + "] : " + (link.IsLinkActive ? "Włączony" : "Wyłączony");
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
            return link;
        }
        
    }
}
