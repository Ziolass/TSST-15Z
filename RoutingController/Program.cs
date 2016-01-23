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
            linkList.Add(new Link("Router 1", "Router 2", 2));
            linkList.Add(new Link("Router 2", "Router 3", 10));
            linkList.Add(new Link("Router 3", "Router 4", 1));
            linkList.Add(new Link("Router 3", "Router 2", 2));
            topo.LinkList = linkList;
            topo.NetworkId = "AS100";
            topo.NetworkLevel = 0;

            RC.UpdateNetworkGraph(topo);

            RC.RouteTableResponse();


            Console.ReadLine();

        }

    }

}