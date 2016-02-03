using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Elements
{
    public class NeighbourRoutingController
    {
        public int Port { get; set; }
        public string NetworkName { get; set; }
        public NeighbourRoutingController(int port, string networkName)
        {
            this.Port = port;
            this.NetworkName = networkName;
        }
    }
}
