using NetworkNode.LRM.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.LRM
{

    public class TopologyReports
    {
        public string Protocol { get; set; }
        public List<TopologyData> Nodes { get; set; }

    }

    public class TopologyData
    {
        public string Node { get; set; }
        public List<string> Domains { get; set; }
        public List<TopologyConnections> Data { get; set; }
    }

    public class TopologyConnections {
        public string Port { get; set; }
        public LrmDestination Destination { get; set; }
        public string Status { get; set; }
    }

}
