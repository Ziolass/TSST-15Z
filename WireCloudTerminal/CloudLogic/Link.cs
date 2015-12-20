using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireCloud.CloudLogic
{
    public delegate void LinkStateChangedHandler();
    public class Link
    {
        private Dictionary<AbstractAddress, NetworkNodeSender> Ports;

        public event LinkStateChangedHandler LinkActive;

        private bool active;
        public bool IsLinkActive
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
                if (LinkActive != null)
                {
                    LinkActive();
                }
            }
        }

        public Link(Dictionary<AbstractAddress, NetworkNodeSender> Ports)
        {
            this.Ports = Ports;
            this.IsLinkActive = true;
        }
        public Link(Link link)
        {
            this.Ports = link.Ports;
            this.IsLinkActive = link.IsLinkActive;
        }

        public bool Contains(AbstractAddress address)
        {
            return Ports.ContainsKey(address);
        }

        public void SendData(string data, AbstractAddress address)
        {
            Ports[address].SendContent(data);
        }
        public String ToString()
        {
            String returnValue = String.Empty;
            foreach (var item in Ports)
            {
                returnValue += item.Key.NodeId + ": " + item.Key.Port + " " ;
            }
            return returnValue;
        }
        public bool Equals(Link link)
        {
            bool abbstract = false;
            bool node = false;
            foreach (var item in Ports)
            {
                foreach (var linkItem in link.Ports)
                {
                    if (item.Key.Equals(linkItem.Key)) //AbstractAddress
                    {
                        abbstract = true;
                    }
                    else return false;
                    if (item.Value.Equals(linkItem.Value)) //Node
                    {
                        node = true;
                    }
                    else return false;
                }
            }
            return node && abbstract;
        }
    }
}
