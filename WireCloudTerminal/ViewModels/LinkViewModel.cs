using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireCloud;
using WireCloud.CloudLogic;

namespace WireCloud.ViewModels
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
                return "Link [" + Link.ToString()+ "]";// + (Link.IsLinkActive ? "Włączony" : "Wyłączony");
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
