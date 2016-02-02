using RoutingController.Interfaces;
using System;
using System.Collections.Generic;

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
        private Dictionary<NodeElement, Dictionary<ILink, int>> Graph { get; set; }

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
                    NodeElement newNode = new NodeElement(topology.Node, link.Port);
                    AddVertex(newGraph, newNode, link); //Add exp Node1:1 (id node : port of this node)
                }
                else Log += topology.Node + ":" + link.Port + " -> " + link.Destination.Node + ":" + link.Destination.Port + "\n"; ;
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
                if (item.Key.Destination.Node == link.Destination.Node && item.Key.Destination.Port == link.Destination.Port)
                {
                    add = false;
                }
            }
            if (add)
            {
                Graph[node].Add(link, ExternalLinkWeight);
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
                        Destination destination = new Destination(null, otherVertex.Key.Node, otherVertex.Key.Port);
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
                    if (oldVertex.Key.Node != newVertex.Key.Node)
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
        public List<string> ShortestPath(string start, string finish)
        {
            var previous = new Dictionary<string, string>();
            var distances = new Dictionary<string, int>();
            var nodes = new List<string>();

            List<string> path = null;

            foreach (var vertex in Graph)
            {
                string vertexId = vertex.Key.GetNodeId();
                if (vertexId == start)
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

                string smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest == finish)
                {
                    path = new List<string>();
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

                    if (distances.ContainsKey(((Destination)neighbor.Key.Destination).NodeId()) && alt < distances[((Destination)neighbor.Key.Destination).NodeId()])
                    {
                        distances[((Destination)neighbor.Key.Destination).NodeId()] = alt;
                        previous[((Destination)neighbor.Key.Destination).NodeId()] = smallest;
                    }
                }
            }
            //Change order
            List<string> returnPath = null;
            if (path != null)
            {
                returnPath = new List<string>();
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
                if (vertex.Key.Node == nodeName || vertex.Key.GetNodeId() == nodeName)
                {
                    return vertex;
                }
                //else return new KeyValuePair<NodeElement, Dictionary<ILink, int>>();
            }
            return new KeyValuePair<NodeElement, Dictionary<ILink, int>>();
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
                    if (item.Key.Destination.Node != vertex.Key.Node)
                    {
                        returnDictionary.Add(new NodeElement(vertex.Key.Node, vertex.Key.Port), new NodeElement(item.Key.Destination.Node, item.Key.Destination.Port));
                    }
                }
            }
            return returnDictionary;
        }
    }
}