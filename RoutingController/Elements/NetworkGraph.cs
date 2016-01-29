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
        public string NetworkName { get; set; }
        public string Log { get; set; }
        private Dictionary<Node, Dictionary<ILink, int>> Graph { get; set; }

        public NetworkGraph()
        {
            this.Graph = new Dictionary<Node, Dictionary<ILink, int>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkGraph"/> class.
        /// </summary>
        /// <param name="topology">The topology.</param>
        /// <param name="networkName">Name of the network.</param>
        public NetworkGraph(ITopology topology, string networkName)
        {
            //TODO 
            this.NetworkName = networkName;
            this.Graph = new Dictionary<Node, Dictionary<ILink, int>>();
        }

        /// <summary>
        /// Updates the graph.
        /// </summary>
        /// <param name="topology">The link list.</param>
        public void UpdateGraph(ITopology topology)
        {
            //Create new graph
            Dictionary<Node, Dictionary<ILink, int>> newGraph = new Dictionary<Node, Dictionary<ILink, int>>();
            Dictionary<Node, Dictionary<ILink, int>> clientGraph = new Dictionary<Node, Dictionary<ILink, int>>();
            foreach (ILink link in topology.Data)
            {
                if (link.Status == NodeStatus.FREE)
                {
                    if (link.Type != NodeType.CLIENT)
                    {
                        Node newNode = new Node(topology.Node, link.Port);
                        AddVertex(newGraph, newNode, link); //Add exp Node1:1 (id node : port of this node)
                    }
                    else
                    {
                        Node newNode = new Node(topology.Node, link.Port);
                        AddVertex(newGraph, newNode, link); //Add exp Node1:1 (id node : port of this node)

                        newNode = new Node(link.Destination.Node, link.Destination.Port);
                        Link revLink = new Link(link.Destination.Port, link.Domains, link.Type, new Destination(null, topology.Node, link.Port), link.Status);
                        AddVertex(clientGraph, newNode, revLink); //Add exp Node1:1 (id node : port of this node)
                    }
                }
                else Log += topology.Node + ":" + link.Port + " -> " + link.Destination.Node + ":" + link.Destination.Port + "\n"; ;
            }

            //Complete graph
            newGraph = CompleteGraph(newGraph, this.NetworkName);
            clientGraph = CompleteGraph(clientGraph, this.NetworkName);
            
            foreach (var item in clientGraph)
            {
                newGraph.Add(item.Key, item.Value);
            }

            //Compere to local graph (remove old or not sent routes and update if link changed)
            if (Graph.Count == 0)
            {
                Graph = newGraph;
            }
            else
            {
                Graph = new Dictionary<Node, Dictionary<ILink, int>>(CompereGraph(Graph, newGraph));
            }
        }

        /// <summary>
        /// Adds the vertex to graph
        /// https://github.com/mburst/dijkstras-algorithm/blob/master/dijkstras.cs
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="link">The link.</param>
        private static void AddVertex(Dictionary<Node, Dictionary<ILink, int>> graph, Node node, ILink link)
        {
            Dictionary<ILink, int> edge = new Dictionary<ILink, int>();
            edge.Add(link, ExternalLinkWeight);
            graph[node] = edge;
        }

        /// <summary>
        /// Complete the graph. Graf zupe³ny
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="networkName">The network identifier.</param>
        private static Dictionary<Node, Dictionary<ILink, int>> CompleteGraph(Dictionary<Node, Dictionary<ILink, int>> graph, string networkName)
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
                        Link newLink = new Link(currentVertex.Key.Port, domains, NodeType.INTERNAL, destination, NodeStatus.FREE); //WERYFIKACJA !!!
                        currentVertex.Value.Add(newLink, InternalLinkWeight);
                    }
                }
            }
            return new Dictionary<Node, Dictionary<ILink, int>>(graph);
        }

        /// <summary>
        /// Comperes the graph.
        /// Add new links
        /// Remove old links
        /// Update existing links
        /// </summary>
        /// <param name="currentGraph">The current graph.</param>
        /// <param name="newGraph">The new graph.</param>
        private static Dictionary<Node, Dictionary<ILink, int>> CompereGraph(Dictionary<Node, Dictionary<ILink, int>> currentGraph, Dictionary<Node, Dictionary<ILink, int>> newGraph)
        {
            //Compere OLD -> NEW (remove old links, update links)
            Dictionary<Node, Dictionary<ILink, int>> tempGraph = new Dictionary<Node, Dictionary<ILink, int>>(currentGraph);
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
            currentGraph = new Dictionary<Node, Dictionary<ILink, int>>(tempGraph);
            //Compere NEW -> OLD (add new links)
            foreach (var newVertex in newGraph)
            {
                bool add = true;
                foreach (var oldVertex in currentGraph)
                {
                    if (oldVertex.Key == newVertex.Key)
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
                Node smallestNode = null;
                foreach (var vertex in Graph)
                {
                    if (vertex.Key.Equals(new Node(smallest)))
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
        public KeyValuePair<Node, Dictionary<ILink, int>> GetVertex(string nodeName)
        {
            foreach (var vertex in Graph)
            {
                if (vertex.Key.Name == nodeName || vertex.Key.GetNodeId() == nodeName)
                {
                    return vertex;
                }
                else return new KeyValuePair<Node, Dictionary<ILink, int>>();
            }
            return new KeyValuePair<Node, Dictionary<ILink, int>>();
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

        public Dictionary<Node, Node> GetRoutes()
        {
            Dictionary<Node, Node> returnDictionary = new Dictionary<Node,Node>();
            foreach (var vertex in Graph)
            {
                foreach (var item in vertex.Value)
                {
                    if (item.Key.Destination.Node != vertex.Key.Name)
                    {
                        returnDictionary.Add(new Node(vertex.Key.Name,vertex.Key.Port), new Node(item.Key.Destination.Node,item.Key.Destination.Port));
                    }
                }
            }
            return returnDictionary;
        }
    }
}