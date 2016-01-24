using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Interfaces;
using RoutingController.Elements;

namespace RoutingController
{
    public class RoutingController
    {
        public String NetworkId { get; set; }
        public ILinkResourceMenager LRM { get; private set; }

        private List<NetworkGraph> NetworkGraphs { get; set; }

        public RoutingController()
        {
            this.NetworkGraphs = new List<NetworkGraph>();
        }

        public RoutingController(String networkId)
        {
            this.NetworkGraphs = new List<NetworkGraph>();
            this.NetworkId = networkId;
        }

        /// <summary>
        /// Routes the table response.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="networkLevel">The network level.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Error RouteTableResponse: NetworkGraph not found! 
        /// or
        /// Error RouteTableResponse: NetworkGraph not found! 
        /// </exception>
        public List<string> RouteTableResponse(string source, string destination)
        {

            //TODO : Zmiana na sprawdzenie w jakiej domenie jest destination
            NetworkGraph tempNetworkGraph = this.GetNetworkGraph("mydomain1");
            if (tempNetworkGraph != null)
            {
                List<string> returnList = tempNetworkGraph.ShortestPath(source, destination);
                if (returnList != null)
                {
                    return returnList;
                }
                else throw new Exception("Error RouteTableResponse: Graph is uncomplete (ShortestPath)! ");
            }
            else throw new Exception("Error RouteTableResponse: NetworkGraph not found! ");
        }

        /// <summary>
        /// Updates the network graph. RouteController action for LocalTopology() from LRM
        /// </summary>
        /// <param name="topology">The topology.</param>
        /// <returns></returns>
        public bool UpdateNetworkGraph(ITopology topology)
        {
            NetworkGraph tempNetworkGraph = this.GetNetworkGraph(topology.LinkList[0].Domains[0]);
            if (tempNetworkGraph != null)
            {
                tempNetworkGraph.UpdateGraph(topology);
                return true;
            }
            else
            {
                //TODO tutaj mega sprytne tworzenie NetworkGraph

                NetworkGraphs.Add(new NetworkGraph(topology));
                return true;
            }
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
        private NetworkGraph GetNetworkGraph(string networkName)
        {
            foreach (NetworkGraph networkGraph in NetworkGraphs)
            {
                if (networkGraph.NetworkId == networkName)
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
