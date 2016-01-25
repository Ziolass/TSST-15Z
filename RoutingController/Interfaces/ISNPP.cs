using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public interface ISNPP
    {
        string NodeName { get; }
        int Port { get; }
    }
}
