using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Interfaces;

namespace RoutingController.RoutingControllElements
{
    public class Dijkstra
    {
        public static int Inf = 1000000;
        public int[,] Graph { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dijkstra"/> class.
        /// </summary>
        /// <param name="graph">The graph.</param>
        public Dijkstra(int[,] graph)
        {
            Graph = graph;
        }

        /// <summary>
        /// Gets the size of the graph.
        /// </summary>
        /// <returns></returns>
        public int GetGraphSize()
        {
            return Graph.GetLength(0);
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The dest.</param>
        /// <returns></returns>
        public int[] GetPath(int source, int destination)
        {
            int graphSize = Graph.GetLength(0);
            int[] dist = new int[graphSize];
            int[] prev = new int[graphSize];
            int[] nodes = new int[graphSize];

            for (int i = 0; i < dist.Length; i++)
            {
                dist[i] = prev[i] = Inf;
                nodes[i] = i;
            }

            dist[source] = 0;
            do
            {
                int smallest = nodes[0];
                int smallestIndex = 0;
                for (int i = 1; i < graphSize; i++)
                {
                    if (dist[nodes[i]] < dist[smallest])
                    {
                        smallest = nodes[i];
                        smallestIndex = i;
                    }
                }
                graphSize--;
                nodes[smallestIndex] = nodes[graphSize];

                if (dist[smallest] == Inf || smallest == destination)
                    break;

                for (int i = 0; i < graphSize; i++)
                {
                    int v = nodes[i];
                    int newDist = dist[smallest] + Graph[smallest, v];
                    if (newDist < dist[v])
                    {
                        dist[v] = newDist;
                        prev[v] = smallest;
                    }
                }
            } while (graphSize > 0);

            return ReconstructPath(prev, source, destination);
        }

        /// <summary>
        /// Reconstructs the path.
        /// </summary>
        /// <param name="prev">The previous.</param>
        /// <param name="source">The source.</param>
        /// <param name="destination">The dest.</param>
        /// <returns></returns>
        public int[] ReconstructPath(int[] prev, int source, int destination)
        {
            int[] ret = new int[prev.Length];
            int currentNode = 0;
            ret[currentNode] = destination;
            while (ret[currentNode] != Inf && ret[currentNode] != source)
            {
                ret[currentNode + 1] = prev[ret[currentNode]];
                currentNode++;
            }
            if (ret[currentNode] != source)
            {
                return null;
            }

            int[] reversed = new int[currentNode + 1];
            for (int i = currentNode; i >= 0; i--)
                reversed[currentNode - i] = ret[i];

            return reversed;
        }        
    }

}