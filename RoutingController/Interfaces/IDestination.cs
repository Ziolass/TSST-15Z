using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public enum DestinationType { DOMAIN, LOCAL}
    public interface IDestination
    {
        string Scope { get; } //Name of other subnetwork
        string Node { get; } //Node ID
        string Port { get; } //Node Port
        DestinationType Type { get; }


    }
}
