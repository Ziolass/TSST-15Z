using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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



        }
    }
}
