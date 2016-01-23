using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Interfaces;
using RoutingController.RoutingControllElements;

namespace RoutingController
{
    public class RoutingController : IRoutingController
    {
        public List<NetworkGraph> NetworkGraph { get; set; }
        public String NetworkId { get; set; }

        public ILinkResourceMenager LRM { get; private set; }

        public RoutingController()
        {

        }
        public RoutingController(String networkId)
        {
            this.NetworkId = networkId;
        }

        public ISNPP[] RouteTableQuery(string source, string destination)
        {
            //Get LRM local topology
            //Dijkstra dijkstra = new Dijkstra(Dijkstra.MakeGraph(LRM.LocalTopology()));


            throw new NotImplementedException();
        }

        ISNPP IRoutingController.NetworkTopology()
        {
            throw new NotImplementedException();
        }
    }
}
