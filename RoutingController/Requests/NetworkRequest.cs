using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Elements;

namespace RoutingController.Requests
{
    public class NetworkRequest
    {
        public string Protocol { get; private set; }
        public string NetworkName { get; set; }
        public List<string> OtherDomains { get; set; }
        public List<NodeElement> Clients { get; set; }

        public NetworkRequest(string networkName, List<string> otherDomains, List<NodeElement> clients)
        {

            this.Protocol = "network";
            this.NetworkName = networkName;
            this.OtherDomains = otherDomains;
            this.Clients = clients;
        }
    }
}
