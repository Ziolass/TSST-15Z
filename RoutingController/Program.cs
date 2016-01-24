using RoutingController.Interfaces;
using RoutingController.Elements;
using System;
using System.Collections.Generic;
using RoutingController.Service;

namespace RoutingController
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                ElementConfigurator configurator = new ElementConfigurator("../../../Configs/RoutingController/routingControllerConfig.xml");
                RoutingControllerCenter RC = configurator.ConfigureRoutingController();
                Console.WriteLine("Routing Controller Console");
                RC.StartListening();
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }

            /*
            RoutingController RC = new RoutingController();

            Topology topo = new Topology();
            List<ILink> linkList = new List<ILink>();
            linkList.Add(new Link("Router 1", "Router 2", 1));
            linkList.Add(new Link("Router 1", "Router 3", 2));
            topo.LinkList = linkList;
            topo.NetworkId = "AS100";
            topo.NetworkLevel = 0;
            RC.UpdateNetworkGraph(topo);
            List<string> test;
            try
            {
                test = RC.RouteTableResponse("Router 2", "Router 5", 0);
                test.ForEach(x => Console.WriteLine(x));
            }
            catch (Exception e) { }

            linkList.Add(new Link("Router 2", "Router 1", 1));
            linkList.Add(new Link("Router 2", "Router 3", 5));
            linkList.Add(new Link("Router 2", "Router 4", 12));
            linkList.Add(new Link("Router 3", "Router 1", 2));
            linkList.Add(new Link("Router 3", "Router 2", 5));
            linkList.Add(new Link("Router 3", "Router 4", 1));
            linkList.Add(new Link("Router 4", "Router 3", 1));
            linkList.Add(new Link("Router 4", "Router 2", 12));
            linkList.Add(new Link("Router 4", "Router 5", 5));
            linkList.Add(new Link("Router 5", "Router 4", 5));
            topo.LinkList = linkList;
            RC.UpdateNetworkGraph(topo);

            test = RC.RouteTableResponse("Router 2", "Router 5", 0);
            test.ForEach(x => Console.WriteLine(x));
            Console.WriteLine("-----------");

            linkList = new List<ILink>();
            linkList.Add(new Link("Router 1", "Router 2", 1));
            linkList.Add(new Link("Router 1", "Router 3", 2));
            linkList.Add(new Link("Router 2", "Router 1", 1));
            linkList.Add(new Link("Router 2", "Router 3", 5));
            linkList.Add(new Link("Router 2", "Router 4", 12));
            linkList.Add(new Link("Router 3", "Router 1", 2));
            linkList.Add(new Link("Router 3", "Router 2", 5));
            linkList.Add(new Link("Router 3", "Router 4", 1));
            linkList.Add(new Link("Router 4", "Router 3", 1));
            linkList.Add(new Link("Router 4", "Router 2", 12));

            topo.LinkList = linkList;
            RC.UpdateNetworkGraph(topo);

            test = RC.RouteTableResponse("Router 2", "Router 5", 0);
            test.ForEach(x => Console.WriteLine(x));

             */
            Console.ReadLine();
        }
    }
}