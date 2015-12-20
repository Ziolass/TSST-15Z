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
        }

        public bool Contains(AbstractAddress address)
        {
            return Ports.ContainsKey(address);
        }

        public void SendData(string data, AbstractAddress address)
        {
            Ports[address].SendContent(data);
        }

    }
}
