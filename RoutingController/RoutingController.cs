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
        public SNPP RouteTableResponse(string source, string destination)
        {
            //What domain name?
            string destinationDomainName = SearchDomainName(destination);
            string sourceDomainName = SearchDomainName(source);
            if (destinationDomainName == sourceDomainName)
            {
                NetworkGraph tempNetworkGraph = this.GetNetworkGraph(destinationDomainName);
                if (tempNetworkGraph != null)
                {
                    List<string> returnList = tempNetworkGraph.ShortestPath(source, destination);
                    if (returnList != null)
                    {
                        SNPP returnSNPP = new SNPP(returnList);
                        return returnSNPP;
                    }
                    else throw new Exception("Error RouteTableResponse: Graph is uncomplete (ShortestPath)! ");
                }
                else throw new Exception("Error RouteTableResponse: NetworkGraph not found! ");
            }
            else throw new Exception("Error RouteTableResponse: Two different domains! start: " + sourceDomainName + " destination: " + destinationDomainName );
        }

        /// <summary>
        /// Searches name of the domain.
        /// </summary>
        /// <param name="nodeName">Name of the node. Source or destination</param>
        /// <returns></returns>
        private string SearchDomainName(string nodeName)
        {
            string domainName = string.Empty;
            foreach (NetworkGraph networkGraph in NetworkGraphs)
            {
                domainName = networkGraph.GetDomainName(nodeName);
                if (domainName != null)
                    break;
            }
            return domainName;
        }

        /// <summary>
        /// Updates the network graph. RouteController action for LocalTopology() from LRM
        /// </summary>
        /// <param name="topology">The topology.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Error UpdateNetworkGraph: No domain name!</exception>
        public bool UpdateNetworkGraph(ITopology topology)
        {
            TopologyRequest topologyRequest = (TopologyRequest)topology;
            List<string> domainNames = topologyRequest.GetDomains();
            if (domainNames.Count != 0)
            {
                foreach (string domainName in domainNames)
                {
                    NetworkGraph tempNetworkGraph = this.GetNetworkGraph(domainName);
                    if (tempNetworkGraph != null)
                    {
                        tempNetworkGraph.UpdateGraph(topology);
                    }
                    else
                    {
                        tempNetworkGraph = new NetworkGraph(topology, domainName);
                        tempNetworkGraph.UpdateGraph(topology);
                        NetworkGraphs.Add(tempNetworkGraph);
                    }
                }
                return true;
            }
            else throw new Exception("Error UpdateNetworkGraph: No domain name!");
            
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
                if (networkGraph.NetworkName == networkName)
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
