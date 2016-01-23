using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Interfaces;

namespace RoutingController.RoutingControllElements
{
    public class NetworkGraph
    {
        public int NetworkLevel { get; set; }
        public int NetworkId { get; set; }
        private Dictionary<string, Dictionary<string, double>> Graph {get; set;}

        public NetworkGraph()
        {
            this.Graph = new Dictionary<string, Dictionary<string, double>>();
        }

        /// <summary>
        /// Updates the graph.
        /// </summary>
        /// <param name="topology">The link list.</param>
        public void UpdateGraph(ITopology topology)
        {
            foreach (ILink link in topology.LinkList)
            {
                if (!Graph.ContainsKey(link.SourceId)) //Graph doesn't have this link - add
                {
                    AddVertex(link);
                }
                else //Graph have this link - update
                {
                    UpdateVertexConnection(link);
                }
            }
        }

        /// <summary>
        /// Adds the vertex to graph.
        /// https://github.com/mburst/dijkstras-algorithm/blob/master/dijkstras.cs
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="edges">The edges.</param>
        public void AddVertex(string name, Dictionary<string, double> edges)
        {
            Graph[name] = edges;
        }

        /// <summary>
        /// Adds the vertex.
        /// </summary>
        /// <param name="link">The link.</param>
        private void AddVertex(ILink link)
        {
            Dictionary<string, double> edge = new Dictionary<string,double>();
            edge.Add(link.DestinationId, link.Weight);
            Graph[link.SourceId] = edge;
        }

        /// <summary>
        /// Updates the vertex connection.
        /// </summary>
        /// <param name="link">The link.</param>
        private void UpdateVertexConnection(ILink link)
        {
            Dictionary<string, double> edges = Graph[link.SourceId];
            if (!edges.ContainsKey(link.DestinationId))
            {
                edges.Add(link.DestinationId, link.Weight);
            }
            else
            {
                edges[link.DestinationId] = link.Weight;
            }
        }

        /// <summary>
        /// Calculate the shortests path.
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
                if (vertex.Key == start)
                {
                    distances[vertex.Key] = 0;
                }
                else
                {
                    distances[vertex.Key] = int.MaxValue;
                }

                nodes.Add(vertex.Key.ToString());
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

                foreach (var neighbor in Graph[smallest])
                {
                    var alt = distances[smallest] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = smallest;
                    }
                }
            }
            //Change order
            List<string> returnPath = new List<string>();            
            for (int i = path.Count -1; i >= 0; i--)
            {
                returnPath.Add(path[i]);
            }
            return returnPath;
        }
    }
}
