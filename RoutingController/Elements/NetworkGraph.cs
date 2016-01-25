using RoutingController.Interfaces;
using System.Collections.Generic;

namespace RoutingController.Elements
{
    /// <summary>
    /// Represent graph of network
    /// </summary>
    public class NetworkGraph
    {
        public int NetworkLevel { get; set; }
        public string NetworkId { get; set; }
        private Dictionary<string, Dictionary<ILink, int>> Graph { get; set; }

        public NetworkGraph()
        {
            this.Graph = new Dictionary<string, Dictionary<ILink, int>>();
        }

        public NetworkGraph(ITopology topology)
        {
            //TODO 
            this.NetworkId = "mydomain1";
            this.Graph = new Dictionary<string, Dictionary<ILink, int>>();
            UpdateGraph(topology);
        }

        /// <summary>
        /// Updates the graph.
        /// </summary>
        /// <param name="topology">The link list.</param>
        public void UpdateGraph(ITopology topology)
        {
            foreach (ILink link in topology.LinkList)
            {
                //string nodeId = topology.Node + ":" + link.Port;
                string nodeId = topology.Node;
                if (!Graph.ContainsKey(nodeId)) //Graph doesn't have this link - add
                {
                    AddVertex(nodeId, link);
                }
                else //Graph have this link - update
                {
                    UpdateVertexConnection(nodeId, link);
                }
            }
        }

        /// <summary>
        /// Adds the vertex to graph.
        /// https://github.com/mburst/dijkstras-algorithm/blob/master/dijkstras.cs
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="edges">The edges.</param>
        public void AddVertex(string name, Dictionary<ILink, int> edges)
        {
            Graph[name] = edges;
        }

        /// <summary>
        /// Adds the vertex.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="link">The link.</param>
        private void AddVertex(string nodeId, ILink link)
        {
            Dictionary<ILink, int> edge = new Dictionary<ILink, int>();
            //TODO: zmiana weight
            edge.Add(link, 1);
            AddVertex(nodeId, edge);
        }

        /// <summary>
        /// Updates the vertex connection.
        /// TODO:Zrobiæ usuwanie nieaktualnych linków
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="link">The link.</param>
        private void UpdateVertexConnection(string nodeId, ILink link)
        {
            Dictionary<ILink, int> edges = Graph[nodeId];
            if (!edges.ContainsKey(link))
            {
                //TODO: zmiana weight
                edges.Add(link, 1);
            }
            else
            {
                //TODO: zmiana weight
                edges[link] = 1;
            }
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
            var previous2 = new Dictionary<string, string>();
            var distances = new Dictionary<string, int>();
            var distances2 = new Dictionary<string, int>();
            var nodes = new List<string>();
            var nodes2 = new List<Node>();

            List<SNPP> path2 = null;
            List<string> path = null;

            foreach (var vertex in Graph)
            {
                if (vertex.Key == start)
                {
                    distances[vertex.Key] = 0;
                    distances2[vertex.Key] = 0;
                }
                else
                {
                    distances[vertex.Key] = int.MaxValue;
                    distances2[vertex.Key] = int.MaxValue;
                }

                List<int> portsList = new List<int>();
                foreach (var link in vertex.Value)
                {
                    portsList.Add(link.Key.Port);
                }
                List<Link> portsList2 = new List<Link>();
                foreach (var link in vertex.Value)
                {
                    portsList2.Add((Link)link.Key);
                }

                nodes2.Add(new Node(vertex.Key, portsList2));
                nodes.Add(vertex.Key);

            }

            while (nodes.Count != 0)
            {
                nodes2.Sort((x, y) => distances[x.Name] - distances[y.Name]);
                nodes.Sort((x, y) => distances[x] - distances[y]);

                Node smallest2 = nodes2[0];
                string smallest = nodes[0];
                nodes.Remove(smallest);
                nodes2.Remove(smallest2);

                if (smallest == finish)
                {
                    path2 = new List<SNPP>();

                    path = new List<string>();
                    while (previous.ContainsKey(smallest2.Name))
                    {
                        path2.Add(new SNPP(smallest2.Name));
                        smallest2.Name = previous[smallest2.Name];
                    }

                    path2.Add(new SNPP(start));
                    for (int i = path2.Count - 1; i >= 0; i-=2)
                    {
                        
                        foreach (var vertex in Graph[path2[i].NodeName])
                        {
                            if (i - 1 > 0 && vertex.Key.Destination.Node == path2[i-1].NodeName)
                            {
                                path2[i].Port = vertex.Key.Port;
                                path2[i - 1].Port = vertex.Key.Destination.Port;
                            }
                            else if (i + 1 < path2.Count && vertex.Key.Destination.Node == path2[i + 1].NodeName)
                            {
                                path2[i].Port = vertex.Key.Port;
                                path2[i + 1].Port = vertex.Key.Destination.Port;
                            }
                        }
                    }


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
                if (distances2[smallest2.Name] == int.MaxValue)
                {
                    break;
                }

                foreach (var neighbor in Graph[smallest])
                {
                    var alt = distances[smallest] + neighbor.Value;

                    if (alt < distances[neighbor.Key.Destination.Node])
                    {
                        distances[neighbor.Key.Destination.Node] = alt;
                        previous[neighbor.Key.Destination.Node] = smallest;
                    }
                }
                foreach (var neighbor2 in Graph[smallest2.Name])
                {
                    var alt2 = distances2[smallest2.Name] + neighbor2.Value;
                    if (alt2 < distances2[neighbor2.Key.Destination.Node])
                    {
                        distances2[neighbor2.Key.Destination.Node] = alt2;
                        previous2[neighbor2.Key.Destination.Node] = smallest2.Name;
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
            List<SNPP> returnPath2 = null;
            if (path2 != null)
            {
                returnPath2 = new List<SNPP>();
                for (int i = path2.Count - 1; i >= 0; i--)
                {
                    returnPath2.Add(path2[i]);
                }
            }
            return returnPath;
        }
    }
}