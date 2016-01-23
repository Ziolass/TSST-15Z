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
        public int[,] Graph { get; private set; }

        public NetworkGraph()
        {
        }

        /// <summary>
        /// Updates the graph.
        /// </summary>
        /// <param name="topology">The link list.</param>
        /*public void UpdateGraph(ITopology topology)
        {
            if (this.Graph == null)
            {                
                int size = CountDistinguish(topology.LinkList);
                this.Graph = new int[size, size];
            }
            else if (this.Graph.GetLength(0) >= CountDistinguish(topology.LinkList)) //Add information to current graph
            {

            }
            else //Resize graph
            {

            }

            //Make graph
            int[,] graph = new int[topology.LinkList.Count, topology.LinkList.Count];




            //Create SNPP and attach 
            for (int i = 0; i < topology.NodeCount; i++)
            {
                Console.WriteLine("SNPP add: " + topology.NodeList[i].Id);
                snppList.Add(new SNPP(i, topology.NodeList[i].Id));
            }

            foreach (ILink link in topology.LinkList)
            {
                int srcNumber = -1;
                int dstNumber = -1;

                foreach (SNPP snpp in snppList)
                {
                    if (link.SourceId == snpp.NodeId)
                    {
                        srcNumber = snpp.Id;
                    }
                    if (link.DestinationId == snpp.NodeId)
                    {
                        dstNumber = snpp.Id;
                    }
                }
                if (srcNumber != -1 && dstNumber != -1)
                    graph[srcNumber, dstNumber] = 1;
                else
                {

                }
            }

            for (int i = 0; i < graph.GetLength(0); i++)
            {
                for (int j = 0; j < graph.GetLength(1); j++)
                {
                    if (graph[i, j] == 0)
                        graph[i, j] = Dijkstra.Inf;
                }
            }
            return graph;
        }
          */  

        /// <summary>
        /// Counts distinguish items in list.
        /// </summary>
        /// <param name="linkList">The link list.</param>
        /// <returns></returns>
        private static int CountDistinguish(List<ILink> linkList)
        {
            List<ILink> distinguishLinks = new List<ILink>();
            foreach (ILink link in linkList)
            {
                if (distinguishLinks.Count == 0)
                {
                    distinguishLinks.Add(link);
                }
                else if (!distinguishLinks.Contains(link))
                {
                    distinguishLinks.Add(link);
                }
                else continue;
            }

            return distinguishLinks.Count;
        }

        Dictionary<char, Dictionary<char, int>> vertices = new Dictionary<char, Dictionary<char, int>>();

        public void AddVertex(char name, Dictionary<char, int> edges)
        {
            vertices[name] = edges;
        }

        public List<char> ShortestPath(char start, char finish)
        {
            var previous = new Dictionary<char, char>();
            var distances = new Dictionary<char, int>();
            var nodes = new List<char>();

            List<char> path = null;

            foreach (var vertex in vertices)
            {
                if (vertex.Key == start)
                {
                    distances[vertex.Key] = 0;
                }
                else
                {
                    distances[vertex.Key] = int.MaxValue;
                }

                nodes.Add(vertex.Key);
            }

            while (nodes.Count != 0)
            {
                nodes.Sort((x, y) => distances[x] - distances[y]);

                var smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest == finish)
                {
                    path = new List<char>();
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

                foreach (var neighbor in vertices[smallest])
                {
                    var alt = distances[smallest] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = smallest;
                    }
                }
            }

            return path;
        }
    }
}
