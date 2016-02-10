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
        public String NetworkName { get; set; }
        public ILinkResourceMenager LRM { get; private set; }
        public List<NetworkGraph> NetworkGraphs { get; private set; }
        private Dictionary<NodeElement, string> Gateways { get; set; }
        private Dictionary<string, List<string>> ExternalClients { get; set; }

        public RoutingController()
        {
            this.NetworkGraphs = new List<NetworkGraph>();
            this.Gateways = new Dictionary<NodeElement, string>();
            this.ExternalClients = new Dictionary<string, List<string>>();
        }

        public RoutingController(String networkId)
        {
            this.NetworkGraphs = new List<NetworkGraph>();
            this.Gateways = new Dictionary<NodeElement, string>();
            this.ExternalClients = new Dictionary<string, List<string>>();
            this.NetworkName = networkId;
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
        public RouteResponse RouteTableResponse(QueryRequest queryRequest)
        {
            NodeElement source = queryRequest.Ends[0];
            NodeElement destination = queryRequest.Ends[1];
            //What domain name?
            string destinationDomainName = SearchDomainName(destination.GetNodeId());
            string sourceDomainName = SearchDomainName(source.GetNodeId());

            Ends sourceEnd = new Ends(null, source.Name, source.Port);
            Ends destinationEnd;
            List<Ends> endsList = new List<Ends>();

            if (destinationDomainName == sourceDomainName)
            {
                destinationEnd = new Ends(null, destination.Name, destination.Port);
                NetworkGraph tempNetworkGraph = this.GetNetworkGraph(destinationDomainName);
                if (tempNetworkGraph != null)
                {
                    List<NodeElement> tempList = tempNetworkGraph.ShortestPath(source.GetNodeId(), destination.GetNodeId());
                    if (tempList != null)
                    {
                        List<NodeElement> returnList = new List<NodeElement>(); //Add source to steps
                        returnList.Add(source);
                        returnList.AddRange(tempList);

                        RouteResponse returnResponse = new RouteResponse();
                        returnResponse.AddNodes(returnList);
                        endsList.Add(sourceEnd);
                        endsList.Add(destinationEnd);
                        returnResponse.Ends = endsList;

                        return returnResponse;
                    }
                    else throw new Exception("Error RouteTableResponse: Graph is uncomplete (ShortestPath)! ");
                }
                else throw new Exception("Error RouteTableResponse: NetworkGraph not found! ");
            }
            else if (!String.IsNullOrEmpty(sourceDomainName) && !String.IsNullOrEmpty(destinationDomainName))
            {
                foreach (var item in this.ExternalClients)
                {
                    if (item.Value.Contains(destination.Name))
                    {
                        destinationDomainName = item.Key;
                        break;
                    }
                    else if (item.Value.Contains(source.Name))
                    {
                        sourceDomainName = item.Key;
                    }
                    else continue;
                }
                foreach (var item in Gateways)
                {
                    if (item.Value == destinationDomainName)
                    {
                        destination = item.Key;
                    }
                }

                destinationEnd = new Ends(destinationDomainName, destination.Name, destination.Port);
                NetworkGraph tempNetworkGraph = this.GetNetworkGraph(sourceDomainName);
                if (tempNetworkGraph != null)
                {
                    List<NodeElement> tempList = tempNetworkGraph.ShortestPath(source.GetNodeId(), destination.GetNodeId());
                    if (tempList != null)
                    {
                        List<NodeElement> returnList = new List<NodeElement>(); //Add source to steps
                        returnList.Add(source);
                        returnList.AddRange(tempList);

                        //Add external gateway
                        NodeElement externalGateway = GetExternalGateway(returnList[returnList.Count - 1], sourceDomainName, destinationDomainName);
                        returnList.Add(externalGateway);
                        RouteResponse returnResponse = new RouteResponse();
                        returnResponse.AddNodes(returnList);
                        endsList.Add(sourceEnd);
                        endsList.Add(destinationEnd);
                        returnResponse.Ends = endsList;

                        return returnResponse;
                    }
                    else throw new Exception("Error RouteTableResponse: Graph is uncomplete (ShortestPath)! ");
                }
                else throw new Exception("Error RouteTableResponse: NetworkGraph not found! ");
            }
            else throw new Exception("Error RoutTableResponse: Node not found!");
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
            Dictionary<string, int> domainHierarchy = new Dictionary<string, int>(this.GetHierarchyOfDomain(topology));

            foreach (TopologyNode topologyNode in topology.Nodes)
            {
                foreach (var domain in domainHierarchy)
                {
                    if (topologyNode.Domains.Contains(domain.Key))
                    {
                        UpdateGraph(domain.Key, topologyNode, 0);
                    }
                }
            }
            foreach (var item in domainHierarchy)
            {
                if (domainHierarchy.ContainsValue(item.Value + 1))
                {
                    foreach (var lowerDomain in domainHierarchy)
                    {
                        if (lowerDomain.Value == item.Value + 1)
                        {
                            //Add metroNode aka metroVertex
                            NetworkGraph tempNetworkGraph = GetNetworkGraph(item.Key);
                            tempNetworkGraph.MakeMetroVertex(GetNetworkGraph(lowerDomain.Key));
                        }
                    }
                    //Add connections between lowerDomains in higherDomain
                    var items = from pair in domainHierarchy where pair.Value == item.Value + 1 select pair;
                    Dictionary<string, int> lowerDomainElements = new Dictionary<string, int>();
                    foreach (KeyValuePair<string, int> pair in items)
                    {
                        lowerDomainElements.Add(pair.Key, pair.Value);
                    }
                    if (lowerDomainElements.Count > 1) //
                    {
                        for (int x = 0; x < lowerDomainElements.Count; ++x)
                        {
                            for (int y = x + 1; y < lowerDomainElements.Count; ++y)
                            {
                                var xElement = lowerDomainElements.ElementAt(x);
                                var yElement = lowerDomainElements.ElementAt(y);

                                NetworkGraph tempNetworkGraph = GetNetworkGraph(item.Key);
                                tempNetworkGraph.MakeDomainConnection(GetNetworkGraph(xElement.Key), GetNetworkGraph(yElement.Key));
                            }
                        }
                    }
                }
                else continue;
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
            //Node nale�y do tej domeny ale nie jest to domena najni�sza (podstawowa)
            if (topologyNode.Domains.Count > 1 && topologyNode.Domains[0] == domainName)
            {
                this.AddTopologyNodeToGraph(topologyNode, domainName);
            }
            else if (topologyNode.Domains.Count > 1)
            {
                ++domainLevel;
                string lowerDomainName = topologyNode.Domains[topologyNode.Domains.Count - 1 - domainLevel];
                UpdateGraph(lowerDomainName, topologyNode, domainLevel);

                /*List<string> domainList = new List<string>();
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
                 */

            }
            //Node nale�y do tej domeny i jest to jego domena najni�sza (podstawowa)
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
        private Dictionary<string, int> GetHierarchyOfDomain(LocalTopologyRequest topology)
        {
            Dictionary<string, int> domainDictionary = new Dictionary<string, int>();
            foreach (TopologyNode topologyNode in topology.Nodes)
            {
                for (int i = 0; i < topologyNode.Domains.Count; i++)
                {
                    if (!domainDictionary.ContainsKey(topologyNode.Domains[i]))
                    {
                        domainDictionary.Add(topologyNode.Domains[i], topologyNode.Domains.Count - i);
                    }
                }
            }
            Dictionary<string, int> hierarchyDomain = new Dictionary<string, int>();
            var items = from pair in domainDictionary orderby pair.Value ascending select pair;
            foreach (KeyValuePair<string, int> pair in items)
            {
                hierarchyDomain.Add(pair.Key, pair.Value);
            }
            return hierarchyDomain;
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

        public NodeElement GetExternalGateway(NodeElement ourGatewayNode, string sourceDomainName, string externalDomainName)
        {
            NetworkGraph tempNetworkGraph = this.GetNetworkGraph(sourceDomainName);
            NodeElement returnNode = null;
            var vertex = tempNetworkGraph.GetVertex(ourGatewayNode.GetNodeId());

            tempNetworkGraph = this.GetNetworkGraph(externalDomainName);
            foreach (var item in tempNetworkGraph.Graph)
            {
                foreach (var itemRoutes in item.Value)
                {
                    if (itemRoutes.Key.Destination.Name == ourGatewayNode.Name && itemRoutes.Key.Destination.Port == ourGatewayNode.Port)
                    {
                        returnNode = item.Key;
                    }
                }
            }
            return returnNode;
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
        /// <summary>
        /// Shows the external clients.
        /// </summary>
        /// <returns></returns>
        public string ShowExternalClients()
        {
            string returnString = null;
            foreach (var item in this.ExternalClients)
            {
                returnString += "Clients in other domains \n";
                returnString += item.Key + "\n";
                returnString += "---------------------------\n";
                foreach (var clients in item.Value)
                {
                    returnString += clients + "\n";
                }
            }
            return returnString;

        }

        public bool ClearNetworkGraph()
        {
            NetworkGraphs = new List<NetworkGraph>();
            return true;
        }

        /// <summary>
        /// Networks the request response.
        /// </summary>
        /// <returns></returns>
        public NetworkRequest NetworkRequestResponse()
        {
            List<NodeElement> clients = new List<NodeElement>();
            foreach (NetworkGraph item in this.NetworkGraphs)
            {
                Dictionary<NodeElement, Dictionary<ILink, int>> vertexes = item.GetNearVertexes("client");
                foreach (var clientElement in vertexes)
                {
                    clients.Add(clientElement.Key);
                }
            }
            List<NodeElement> gateways = new List<NodeElement>();
            if (this.NetworkGraphs.Count > 1)
            {
                foreach (NetworkGraph networkGraph in this.NetworkGraphs)
                {
                    Dictionary<NodeElement, Dictionary<ILink, int>> vertexes = networkGraph.GetNearVertexes("");
                    foreach (var item in vertexes)
                    {
                        foreach (var itemDestination in item.Value)
                        {
                            foreach (NetworkGraph networkGraphSearch in this.NetworkGraphs)
                            {
                                if (networkGraph != networkGraphSearch && !itemDestination.Key.Destination.Name.Contains("client"))
                                {
                                    Dictionary<NodeElement, Dictionary<ILink, int>> vertexesOther = networkGraphSearch.GetNearVertexes(itemDestination.Key.Destination.Name);
                                    Dictionary<NodeElement, Dictionary<ILink, int>> vertexesOur = networkGraph.GetNearVertexes(itemDestination.Key.Destination.Name);

                                    if (vertexesOther.Count == 1 && vertexesOur.Count == 0)
                                    {
                                        if (this.Gateways.ContainsKey(item.Key))
                                        {
                                            this.Gateways[item.Key] = networkGraphSearch.DomainName;
                                        }
                                        else this.Gateways.Add(item.Key, networkGraphSearch.DomainName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else gateways = null;
            NetworkRequest networkRequest = new NetworkRequest(this.NetworkName, null, clients);
            return networkRequest;
        }

        //DODAJEMY sobie dictionary domena -> list client�w
        /// <summary>
        /// Updates the network topology.
        /// </summary>
        /// <param name="queryRequest">The query request.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void UpdateNetworkTopology(NetworkRequest queryRequest)
        {
            List<string> clientsList = new List<string>();

            if (queryRequest.OtherDomains != null)
            {
                foreach (var externalClient in queryRequest.OtherDomains)
                {
                    clientsList.Add(externalClient);
                }
                if (this.ExternalClients.ContainsKey(queryRequest.NetworkName))
                {
                    this.ExternalClients[queryRequest.NetworkName] = clientsList;
                }
                else this.ExternalClients.Add(queryRequest.NetworkName, clientsList);
            }

            if (queryRequest.Clients != null)
            {
                foreach (var externalClient in queryRequest.Clients)
                {
                    clientsList.Add(externalClient.Name);
                }
                if (this.ExternalClients.ContainsKey(queryRequest.NetworkName))
                {
                    this.ExternalClients[queryRequest.NetworkName] = clientsList;
                }
                else this.ExternalClients.Add(queryRequest.NetworkName, clientsList);
            }

        }
    }
}
