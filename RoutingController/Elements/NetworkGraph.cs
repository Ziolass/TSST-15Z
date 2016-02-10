using RoutingController.Interfaces;
using System;
using System.Collections.Generic;
using RoutingController.Requests;

namespace RoutingController.Elements
{
    /// <summary>
    /// Represent graph of network and it's logic
    /// </summary>
    public class NetworkGraph
    {
        private static int InternalLinkWeight = 1;
        private static int ExternalLinkWeight = 2;
        public int NetworkLevel { get; set; }
        public string DomainName { get; set; }
        public string Log { get; set; }
        public Dictionary<NodeElement, Dictionary<ILink, int>> Graph { get; private set; }

        public NetworkGraph()
        {
            this.Graph = new Dictionary<NodeElement, Dictionary<ILink, int>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkGraph"/> class.
        /// </summary>
        /// <param name="topology">The topology.</param>
        /// <param name="networkName">Name of the network.</param>
        public NetworkGraph(ITopologyNode topology, string networkName)
        {
            //TODO 
            this.DomainName = networkName;
            this.Graph = new Dictionary<NodeElement, Dictionary<ILink, int>>();
        }

        /// <summary>
        /// Updates the graph.
        /// </summary>
        /// <param name="topology">The link list.</param>
        public void UpdateGraph(ITopologyNode topology)
        {
            //Create new graph
            Dictionary<NodeElement, Dictionary<ILink, int>> newGraph = new Dictionary<NodeElement, Dictionary<ILink, int>>();
            Dictionary<NodeElement, Dictionary<ILink, int>> clientGraph = new Dictionary<NodeElement, Dictionary<ILink, int>>();
            foreach (ILink link in topology.Data)
            {
                if (link.Status == NodeStatus.FREE)
                {
                    NodeElement newNode = new NodeElement(topology.Node, link.Port, link.Destination.Scope);
                    AddVertex(newGraph, newNode, link); //Add exp Node1:1 (id node : port of this node)
                }
                else Log += topology.Node + ":" + link.Port + " -> " + link.Destination.Name + ":" + link.Destination.Port + "\n"; ;
            }

            //Complete graph
            newGraph = CompleteGraph(newGraph, this.DomainName);

            //Compere to local graph (remove old or not sent routes and update if link changed)
            if (Graph.Count == 0)
            {
                Graph = newGraph;
            }
            else
            {
                Graph = new Dictionary<NodeElement, Dictionary<ILink, int>>(CompereGraph(Graph, newGraph));
            }
        }

        /// <summary>
        /// Adds the vertex to graph
        /// https://github.com/mburst/dijkstras-algorithm/blob/master/dijkstras.cs
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="link">The link.</param>
        private static void AddVertex(Dictionary<NodeElement, Dictionary<ILink, int>> graph, NodeElement node, ILink link)
        {
            Dictionary<ILink, int> edge = new Dictionary<ILink, int>();
            edge.Add(link, ExternalLinkWeight);
            graph[node] = edge;
        }

        public void AddLink(NodeElement node, ILink link)
        {
            bool add = true;
            foreach (var item in Graph[node])
            {
                if (item.Key.Destination.Name == link.Destination.Name && item.Key.Destination.Port == link.Destination.Port)
                {
                    add = false;
                }
            }
            if (add)
            {
                Graph[node].Add(link, ExternalLinkWeight);
            }
        }

        public void MakeMetroVertex(NetworkGraph otherGraph)
        {
            foreach (var thisItem in this.Graph)
            {
                foreach (var otherItem in otherGraph.Graph)
                {
                    foreach (var otherItemConnections in otherItem.Value)
                    {
                        if (thisItem.Key.Name == otherItemConnections.Key.Destination.Name && thisItem.Key.Port == otherItemConnections.Key.Destination.Port)
                        {
                            //Remove connection from my domain to other domenain node
                            Dictionary<NodeElement,Link> linkToRemove = new Dictionary<NodeElement,Link>();
                            foreach (var itemValue in thisItem.Value)
                            {
                                if (itemValue.Key.Destination.Name == otherItem.Key.Name && itemValue.Key.Destination.Port == otherItem.Key.Port)
                                {
                                    linkToRemove.Add(thisItem.Key, (Link)itemValue.Key);
                                }
                            }
                            foreach (var link in linkToRemove)
                            {                                
                                    RemoveLink(link.Key, link.Value);
                            }

                            List<string> domains = new List<string>();
                            domains.Add(this.DomainName);
                            Destination destination = new Destination();
                            List<Link> linkData = new List<Link>();

                            //Dodaj linki od node (domena tego graphu) do metro Node

                            //Numeracja wejść do metroNode
                            int newMetroPort = 1 + this.GetVertexes(otherGraph.DomainName).Count;

                            destination = new Destination(otherItem.Key.GetNodeId(), thisItem.Key.Name, thisItem.Key.Port, DestinationType.DOMAIN);

                            linkData.Add(new Link(newMetroPort.ToString(), domains, destination, otherItemConnections.Key.Status));

                            if (this.GetVertex(otherGraph.DomainName).Key != null) //Czy metroNode istnieje?
                            {
                                foreach (var existingLinks in this.GetVertex(otherGraph.DomainName).Value)
                                {
                                    linkData.Add((Link)existingLinks.Key);
                                }
                            }

                            TopologyNode newTopologyNode = new TopologyNode(otherGraph.DomainName, domains, linkData);
                            this.UpdateGraph(newTopologyNode);

                            destination = new Destination(otherItem.Key.GetNodeId(), otherGraph.DomainName, newMetroPort.ToString(), DestinationType.DOMAIN);
                            Link tempLink = new Link(thisItem.Key.Port, domains, destination, otherItemConnections.Key.Status);
                            this.Graph[thisItem.Key].Add(tempLink, ExternalLinkWeight);

                        }
                    }
                }
            }
        }
        private void RemoveLink(NodeElement item, Link link)
        {
            this.Graph[item].Remove(link);
        }

        public void MakeDomainConnection(NetworkGraph firstGraph, NetworkGraph secondGraph)
        {
            foreach (var fristItem in firstGraph.Graph)
            {
                foreach (var secondItem in secondGraph.Graph)
                {
                    foreach (var secondItemConnections in secondItem.Value)
                    {
                        if (fristItem.Key.Name == secondItemConnections.Key.Destination.Name && fristItem.Key.Port == secondItemConnections.Key.Destination.Port)
                        {
                           //does metro node exists in thid higher domain for lower domains
                            if (this.GetVertexes(firstGraph.DomainName).Count >= 1 && this.GetVertexes(secondGraph.DomainName).Count >= 1)
                            {
                                int newFirstMetroPort = 1 + this.GetVertexes(firstGraph.DomainName).Count;
                                int newSecondMetroPort = 1 + this.GetVertexes(secondGraph.DomainName).Count;

                                List<string> domains = new List<string>();
                                domains.Add(this.DomainName);
                                Destination destination = new Destination();
                                List<Link> linkData = new List<Link>();

                                //B -> C
                                destination = new Destination(secondItem.Key.GetNodeId(), secondGraph.DomainName, newSecondMetroPort.ToString(), DestinationType.DOMAIN);
                                linkData.Add(new Link(newFirstMetroPort.ToString(), domains, destination, secondItemConnections.Key.Status));

                                if (this.GetVertex(firstGraph.DomainName).Key != null) //Czy metroNode juz istnieje?
                                {
                                    foreach (var existingLinks in this.GetVertex(firstGraph.DomainName).Value)
                                    {
                                        linkData.Add((Link)existingLinks.Key);
                                    }
                                }

                                TopologyNode newTopologyNode = new TopologyNode(firstGraph.DomainName, domains, linkData);
                                this.UpdateGraph(newTopologyNode);


                                //B <- C
                                destination = new Destination(fristItem.Key.GetNodeId(), firstGraph.DomainName, newFirstMetroPort.ToString(), DestinationType.DOMAIN);
                                linkData = new List<Link>();
                                linkData.Add(new Link(newSecondMetroPort.ToString(), domains, destination, secondItemConnections.Key.Status));

                                if (this.GetVertex(secondGraph.DomainName).Key != null) //Czy metroNode juz istnieje?
                                {
                                    foreach (var existingLinks in this.GetVertex(secondGraph.DomainName).Value)
                                    {
                                        linkData.Add((Link)existingLinks.Key);
                                    }
                                }

                                newTopologyNode = new TopologyNode(secondGraph.DomainName, domains, linkData);
                                this.UpdateGraph(newTopologyNode);

                                /**
                                destination = new Destination(secondItem.Key.GetNodeId(), secondGraph.DomainName, newSecondMetroPort.ToString(), DestinationType.DOMAIN);
                                Link tempLink = new Link(newFirstMetroPort.ToString(), domains, destination, secondItemConnections.Key.Status);
                                this.Graph[this.GetVertex(firstGraph.DomainName + ":" + newFirstMetroPort.ToString()).Key].Add(tempLink, ExternalLinkWeight);
                                **/
                                /*
                                destination = new Destination(fristItem.Key.GetNodeId(), firstGraph.DomainName, newFirstMetroPort.ToString(), DestinationType.DOMAIN);
                                tempLink = new Link(newSecondMetroPort.ToString(), domains, destination, secondItemConnections.Key.Status);
                                this.Graph[this.GetVertex(secondGraph.DomainName + ":" + newSecondMetroPort.ToString()).Key].Add(tempLink, ExternalLinkWeight);
                            
                                 */
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Complete the graph. Graf zupe�ny
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="networkName">The network identifier.</param>
        private static Dictionary<NodeElement, Dictionary<ILink, int>> CompleteGraph(Dictionary<NodeElement, Dictionary<ILink, int>> graph, string networkName)
        {
            foreach (var currentVertex in graph)
            {
                foreach (var otherVertex in graph)
                {
                    if (currentVertex.Key != otherVertex.Key)
                    {
                        Destination destination = new Destination(null, otherVertex.Key.Name, otherVertex.Key.Port);
                        List<string> domains = new List<string>();
                        domains.Add(networkName);
                        Link newLink = new Link(currentVertex.Key.Port, domains, destination, NodeStatus.FREE); //WERYFIKACJA !!!
                        currentVertex.Value.Add(newLink, InternalLinkWeight);
                    }
                }
            }
            return new Dictionary<NodeElement, Dictionary<ILink, int>>(graph);
        }

        /// <summary>
        /// Comperes the graph.
        /// Add new links
        /// Remove old links
        /// Update existing links
        /// </summary>
        /// <param name="currentGraph">The current graph.</param>
        /// <param name="newGraph">The new graph.</param>
        private static Dictionary<NodeElement, Dictionary<ILink, int>> CompereGraph(Dictionary<NodeElement, Dictionary<ILink, int>> currentGraph, Dictionary<NodeElement, Dictionary<ILink, int>> newGraph)
        {
            //Compere OLD -> NEW (remove old links, update links)
            Dictionary<NodeElement, Dictionary<ILink, int>> tempGraph = new Dictionary<NodeElement, Dictionary<ILink, int>>(currentGraph);
            foreach (var oldVertex in currentGraph)
            {
                bool remove = true;
                Dictionary<ILink, int> routes = null;
                foreach (var newVertex in newGraph)
                {
                    if (oldVertex.Key.Name != newVertex.Key.Name)
                    {
                        remove = false;
                        break;
                    }
                    else if (oldVertex.Key.Equals(newVertex.Key))
                    {
                        remove = false;
                        routes = newVertex.Value;
                    }
                }
                if (remove)
                {
                    tempGraph.Remove(oldVertex.Key);
                }
                else if (routes != null)
                {
                    tempGraph[oldVertex.Key] = routes;
                }
                else continue;
            }
            currentGraph = new Dictionary<NodeElement, Dictionary<ILink, int>>(tempGraph);
            //Compere NEW -> OLD (add new links)
            foreach (var newVertex in newGraph)
            {
                bool add = true;
                foreach (var oldVertex in currentGraph)
                {
                    if (oldVertex.Key.Equals(newVertex.Key))
                    {
                        add = false;
                    }
                }
                if (add)
                {
                    tempGraph.Add(newVertex.Key, newVertex.Value);
                }
                else continue;
            }
            return tempGraph;
        }

        /// <summary>
        /// Calculate the shortests path using Dijkstra algorithm
        /// https://github.com/mburst/dijkstras-algorithm/blob/master/dijkstras.cs
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="finish">The finish.</param>
        /// <returns></returns>
        public List<NodeElement> ShortestPath(string start, string finish)
        {
            var previous = new Dictionary<NodeElement, NodeElement>();
            var distances = new Dictionary<NodeElement, int>();
            var nodes = new List<NodeElement>();

            List<NodeElement> path = null;

            foreach (var vertex in Graph)
            {
                NodeElement vertexId = vertex.Key;
                if (vertexId.GetNodeId() == start)
                {
                    distances[vertexId] = 0;
                }
                else
                {
                    distances[vertexId] = int.MaxValue;
                }
                nodes.Add(vertexId);
            }

            while (nodes.Count != 0)
            {
                nodes.Sort((x, y) => distances[x] - distances[y]);

                NodeElement smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest.GetNodeId() == finish)
                {
                    path = new List<NodeElement>();
                    while (previous.ContainsKey(smallest))
                    {
                        path.Add(smallest);
                        smallest = previous[smallest];
                    }
                    break;
                }

                if (distances[smallest] == int.MaxValue)
                {
                    break;
                }
                NodeElement smallestNode = null;
                foreach (var vertex in Graph)
                {
                    if (vertex.Key.Equals(new NodeElement(smallest)))
                    {
                        smallestNode = vertex.Key;
                        break;
                    }
                }

                foreach (var neighbor in Graph[smallestNode])
                {
                    var alt = distances[smallest] + neighbor.Value;

                    var nodeNeighbor = GetVertex(((Destination)neighbor.Key.Destination).NodeId()).Key;

                    if (nodeNeighbor != null && distances.ContainsKey(nodeNeighbor) && alt < distances[nodeNeighbor])
                    {
                        distances[nodeNeighbor] = alt;
                        previous[nodeNeighbor] = smallest;
                   }
                }
            }
            //Change order
            List<NodeElement> returnPath = null;
            if (path != null)
            {
                returnPath = new List<NodeElement>();
                for (int i = path.Count - 1; i >= 0; i--)
                {
                    returnPath.Add(path[i]);
                }
            }
            return returnPath;
        }


        /// <summary>
        /// Gets the vertex.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        public KeyValuePair<NodeElement, Dictionary<ILink, int>> GetVertex(string nodeName)
        {
            foreach (var vertex in Graph)
            {
                if (vertex.Key.Name == nodeName || vertex.Key.GetNodeId() == nodeName)
                {
                    return vertex;
                }
            }
            return new KeyValuePair<NodeElement, Dictionary<ILink, int>>();
        }
        public Dictionary<NodeElement, Dictionary<ILink, int>> GetVertexes(string nodeName)
        {
            Dictionary<NodeElement, Dictionary<ILink, int>> returnDictionary = new Dictionary<NodeElement, Dictionary<ILink, int>>();
            foreach (var vertex in Graph)
            {
                if (vertex.Key.Name == nodeName || vertex.Key.GetNodeId() == nodeName)
                {
                    returnDictionary.Add(vertex.Key, vertex.Value);
                }
            }
            return returnDictionary;
        }
        public Dictionary<NodeElement, Dictionary<ILink, int>> GetNearVertexes(string nodeName)
        {
            Dictionary<NodeElement, Dictionary<ILink, int>> returnDictionary = new Dictionary<NodeElement, Dictionary<ILink, int>>();
            foreach (var vertex in Graph)
            {
                if (vertex.Key.Name.Contains(nodeName))
                {
                    returnDictionary.Add(vertex.Key, vertex.Value);
                }
            }
            return returnDictionary;
        }


        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetDomainName(string source)
        {
            foreach (var vertex in Graph)
            {
                if (vertex.Key.GetNodeId() == source)
                {
                    foreach (var links in vertex.Value)
                    {
                        return links.Key.Domains[0];
                    }
                }
            }
            return null;
        }

        public Dictionary<NodeElement, NodeElement> GetRoutes()
        {
            Dictionary<NodeElement, NodeElement> returnDictionary = new Dictionary<NodeElement, NodeElement>();
            foreach (var vertex in Graph)
            {
                foreach (var item in vertex.Value)
                {
                    if (item.Key.Destination.Name != vertex.Key.Name)
                    {
                        returnDictionary.Add(new NodeElement(vertex.Key.Name, vertex.Key.Port), new NodeElement(item.Key.Destination.Name, item.Key.Destination.Port));
                    }
                }
            }
            return returnDictionary;
        }
    }
}