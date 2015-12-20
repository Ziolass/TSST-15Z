using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireCloudTerminal.CloudLogic;

namespace WireCloud
{
     public class Link
    {
        private Dictionary<AbstractAddress, NetworkNodeSender> Ports;

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


        public static LinkStateChangedHandler LinkActive { get; set; }
    }
}
