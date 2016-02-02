using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Interfaces;
using RoutingController.Elements;
using RoutingController.Requests;

namespace RoutingController.Service
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
        public SNPP RouteTableResponse(QueryRequest queryRequest)
        {
            string source = queryRequest.Ends[0].GetNodeId();
            string destination = queryRequest.Ends[1].GetNodeId();
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
            else {
                NetworkGraph tempNetworkGraph = this.GetNetworkGraph(sourceDomainName);
                if (tempNetworkGraph != null)                
                {
                    string newDestination = tempNetworkGraph.GetVertex(destinationDomainName).Key.GetNodeId();
                    List<string> returnList = tempNetworkGraph.ShortestPath(source, newDestination);
                    SNPP returnSNPP = null;
                    if (returnList != null)
                    {
                        returnSNPP = new SNPP(returnList);
                    }
                    else throw new Exception("Error RouteTableResponse: Graph is uncomplete (ShortestPath)! ");
                    return returnSNPP;
                }
                else throw new Exception("Error RouteTableResponse: NetworkGraph not found! ");
            }
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
                    var vertex = networkGraph.GetVertex(nodeName);
                    if (vertex.Key != null)
                        return networkGraph.DomainName;
                
            }
            return domainName;
        }

        /// <summary>
        /// Updates the network graph. RouteController action for LocalTopology() from LRM
        /// </summary>
        /// <param name="topology">The topology.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Error UpdateNetworkGraph: No domain name!</exception>
        public bool UpdateNetworkGraph(LocalTopologyRequest topology)
        {

            foreach (TopologyNode topologyNode in topology.Nodes)
            {
                UpdateGraph(this.GetMainDomain(topology), topologyNode, 0);
            }



            return true;

        }
        /// <summary>
        /// Updates the graph.
        /// </summary>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="topologyNode">The topology node.</param>
        /// <param name="domainLevel">The domain level.</param>
        private void UpdateGraph(string domainName, TopologyNode topologyNode, int domainLevel)
        {
            //Node nale¿y do tej domeny ale nie jest to domena najni¿sza (podstawowa)
            if (topologyNode.Domains.Count > 1 && topologyNode.Domains[0] == domainName)
            {
                this.AddTopologyNodeToGraph(topologyNode, domainName);
            }
            else if (topologyNode.Domains.Count > 1)
            {
                ++domainLevel;
                string lowerDomainName = topologyNode.Domains[topologyNode.Domains.Count - 1 - domainLevel];
                UpdateGraph(lowerDomainName, topologyNode, domainLevel);

                List<string> domainList = new List<string>();
                domainList.Add(domainName);
                List<Link> linkList = new List<Link>();

                NetworkGraph tempNetworkGraph = this.GetNetworkGraph(domainName);
                Dictionary<NodeElement, Dictionary<ILink, int>> vertex = new Dictionary<NodeElement, Dictionary<ILink, int>>();
                Link currentLink = null;
                foreach (var item in topologyNode.Data)
                {
                    var test = tempNetworkGraph.GetVertex(item.Destination.Node + ":" + item.Destination.Port);
                    if (test.Key != null)
                    {
                        currentLink = item;
                        vertex.Add(test.Key, test.Value);
                        break;
                    }
                }
                foreach (var item in vertex)
                {
                    linkList.Add(new Link(currentLink.Port, domainList, new Destination(null, item.Key.Node, item.Key.Port), currentLink.Status));
                    TopologyNode domainNode = new TopologyNode(lowerDomainName, domainList, linkList);
                    this.AddTopologyNodeToGraph(domainNode, domainName);

                    
                    //Upgrade link in node gateway
                    tempNetworkGraph.AddLink(item.Key, new Link(item.Key.Port, domainList, new Destination(null, lowerDomainName, currentLink.Port), currentLink.Status));
                }

            }
            //Node nale¿y do tej domeny i jest to jego domena najni¿sza (podstawowa)
            else if (GetLastDomain(topologyNode.Domains) == domainName && topologyNode.Domains.Count == 1)
            {
                this.AddTopologyNodeToGraph(topologyNode, domainName);
            }
        }

        private string GetLastDomain(List<string> domains)
        {
            return domains[domains.Count - 1];
        }

        private string GetMainDomain(LocalTopologyRequest topology)
        {
            Dictionary<string, int> domainDictionary = new Dictionary<string, int>();
            //Create all nodes
            foreach (TopologyNode topologyNode in topology.Nodes)
            {
                if (!domainDictionary.ContainsKey(topologyNode.Domains[topologyNode.Domains.Count - 1]))
                {
                    domainDictionary.Add(topologyNode.Domains[topologyNode.Domains.Count - 1], topologyNode.Domains.Count);
                }
            }
            //Get main domainName
            string mainDomain = string.Empty;
            int maxLevel = 0;
            foreach (var item in domainDictionary)
            {
                if (item.Value > maxLevel)
                {
                    maxLevel = item.Value;
                    mainDomain = item.Key;
                }
            }
            return mainDomain;
        }

        private void AddTopologyNodeToGraph(TopologyNode topologyNode, string domainName)
        {
            NetworkGraph tempNetworkGraph = this.GetNetworkGraph(domainName);
            if (tempNetworkGraph != null)
            {
                tempNetworkGraph.UpdateGraph(topologyNode);
            }
            else
            {
                tempNetworkGraph = new NetworkGraph(topologyNode, domainName);
                tempNetworkGraph.UpdateGraph(topologyNode);
                NetworkGraphs.Add(tempNetworkGraph);
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
                if (networkGraph.DomainName == networkName)
                    return networkGraph;
            }
            return null;
        }


        public string ShowRoutes()
        {
            string returnString = null;
            foreach (NetworkGraph item in NetworkGraphs)
            {

                Dictionary<NodeElement, NodeElement> routes = new Dictionary<NodeElement, NodeElement>();
                routes = routes.Concat(item.GetRoutes()).ToDictionary(x => x.Key, x => x.Value);

                Dictionary<NodeElement, NodeElement> returnRoutes = new Dictionary<NodeElement, NodeElement>(routes);


                for (int i = 0; i < returnRoutes.Count; i++)
                {
                    NodeElement nodeTemp = returnRoutes.ElementAt(i).Key;
                    for (int x = 0; x < returnRoutes.Count; x++)
                    {
                        NodeElement itemOther = returnRoutes.ElementAt(x).Key;
                        NodeElement itemOtherValue = returnRoutes.ElementAt(x).Value;
                        if (nodeTemp.GetNodeId() != itemOther.GetNodeId() && nodeTemp.GetNodeId() == itemOtherValue.GetNodeId())
                        {
                            returnRoutes.Remove(itemOther);
                            break;
                        }
                    }

                }
                returnString += item.DomainName + "\n";
                returnString += "---------------------------\n";
                foreach (var route in returnRoutes)
                {
                    returnString += route.Key.GetNodeId() + " <-> " + route.Value.GetNodeId() + "\n";
                }
                returnString += "\n";
            }
            return returnString;
        }

        public bool ClearNetworkGraph()
        {
            NetworkGraphs = new List<NetworkGraph>();
            return true;
        }
    }
}
