using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public interface IDestination
    {
        string Scope { get; } //Name of other subnetwork
        string Node { get; } //Node ID
        int Port { get; } //Node Port

        string NodeId();
    }
}
