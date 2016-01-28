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
        public string Node { get; set; }
        public List<TopologyData> Data { get; set; }

    }

    public class TopologyData
    {
        public List<string> Domains { get; set; }
        public string Port { get; set; }
        public string Type { get; set; }
        public Destination Destination { get; set; }
        public string Status { get; set; }
    }

    public class Destination
    {
        public string Scope { get; set; }
        public string Node { get; set; }
        public string Port { get; set; }

    }
}
