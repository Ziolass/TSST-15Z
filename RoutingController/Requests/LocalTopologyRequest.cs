using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Requests
{
    public class LocalTopologyRequest
    {
        public List<TopologyNode> Nodes { get; private set; }
        public LocalTopologyRequest(List<TopologyNode> nodes)
        {
            this.Nodes = new List<TopologyNode>(nodes);
        }
    }
}
