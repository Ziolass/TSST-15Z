using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Interfaces;
using RoutingController.RoutingControllElements;

namespace RoutingController
{
    public class RoutingController
    {
        private List<NetworkGraph> NetworkGraphs { get; set; }
        public String NetworkId { get; set; }

        public ILinkResourceMenager LRM { get; private set; }

        public RoutingController()
        {
            NetworkGraphs = new List<NetworkGraph>();
        }

        public RoutingController(String networkId)
        {
            NetworkGraphs = new List<NetworkGraph>();
            this.NetworkId = networkId;
        }

        public ISNPP[] RouteTableQueryResponse(string source, string destination)
        {
            //Get LRM local topology
            //Dijkstra dijkstra = new Dijkstra(Dijkstra.MakeGraph(LRM.LocalTopology()));


            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the network graph. RouteController action for LocalTopology() from LRM
        /// </summary>
        /// <param name="topology">The topology.</param>
        /// <returns></returns>
        public bool UpdateNetworkGraph(ITopology topology)
        {
            if (IsNetworkLevelValid(topology.NetworkLevel))
            {
                NetworkGraph tempNetworkGraph = this.GetNetworkGraph(topology.NetworkLevel);
                if (tempNetworkGraph != null)
                {
                    tempNetworkGraph.UpdateGraph(topology);
                    return true;
                }
                else return false;
            }
            else return false;
        }

        /// <summary>
        /// Determine if routing controller has knowledge of this network level.
        /// </summary>
        /// <param name="networkLevel">The network level.</param>
        /// <returns></returns>
        private bool IsNetworkLevelValid(int networkLevel)
        {
            foreach (NetworkGraph networkGraph in NetworkGraphs)
            {
                if (networkGraph.NetworkLevel == networkLevel)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the network graph.
        /// </summary>
        /// <param name="networkLevel">The network level.</param>
        /// <returns></returns>
        private NetworkGraph GetNetworkGraph(int networkLevel)
        {
            foreach (NetworkGraph networkGraph in NetworkGraphs)
            {
                if (networkGraph.NetworkLevel == networkLevel)
                    return networkGraph;
            }
            return null;
        }

        ISNPP NetworkTopology()
        {
            throw new NotImplementedException();
        }
    }
}
