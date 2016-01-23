using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.RoutingControllElements;
using RoutingController.Interfaces;

namespace RoutingController
{
    public interface IRoutingController
    {
        ISNPP[] RouteTableQuery(string source, string destination);
        ISNPP NetworkTopology();
    }
}
