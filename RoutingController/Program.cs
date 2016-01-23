using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Interfaces;
using RoutingController.RoutingControllElements;

namespace RoutingController
{
    class Program
    {
        static void Main(string[] args)
        {
            RoutingController RC = new RoutingController();

            Topology topo = new Topology();
            List<ILink> linkList = new List<ILink>();
            linkList.Add(new Link(1, 2, 2));
            linkList.Add(new Link(2, 3, 10));
            linkList.Add(new Link(3, 4, 1));
            linkList.Add(new Link(3, 1, 2));
            topo.LinkList = linkList;


            NetworkGraph netGraph = new NetworkGraph();
            netGraph.AddVertex('A', new Dictionary<char, int>() { { 'B', 7 }, { 'C', 8 } });
            netGraph.AddVertex('B', new Dictionary<char, int>() { { 'A', 7 }, { 'F', 2 } });
            netGraph.AddVertex('C', new Dictionary<char, int>() { { 'A', 8 }, { 'F', 6 }, { 'G', 4 } });
            netGraph.AddVertex('D', new Dictionary<char, int>() { { 'F', 8 } });

            netGraph.ShortestPath('A', 'H').ForEach(x => Console.WriteLine(x));

            Console.ReadLine();

        }
    }
}
