using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public interface IDestination
    {
        string Scope { get; private set; } //Name of other subnetwork
        string Node { get; private set; } //Node ID
        int Port { get; private set; } //Node Port
    }
}
