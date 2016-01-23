using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.RoutingControllElements;

namespace RoutingController
{
    public interface IRoutingController
    {
        public ISNPP[] RouteTableQuery(string source, string destination);
        public ISNPP NetworkTopology();
    }
}
