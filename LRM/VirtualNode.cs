using NetworkNode.LRM.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRM
{
    public class VirtualNode
    {
        public string Name { get; set; }
        public List<string> DomiansHierarchy { get; set; }
        public AsyncCommunication Async { get; set; }
        public Dictionary<int, object[]> Resources { get; set; }
        public Dictionary<int, Tuple<LrmDestination,bool>> Destinations { get; set; }

        public VirtualNode()
        {
            Resources = new Dictionary<int, object[]>();
            Destinations = new Dictionary<int, Tuple<LrmDestination, bool>>();
        }
    }
}
