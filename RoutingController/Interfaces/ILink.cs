using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Elements;

namespace RoutingController.Interfaces
{
    public enum NodeType { LOCAL, NETWORK, CLIENT, UNDEF }
    public enum NodeStatus { FREE, OCCUPIED, UNDEF }

    /// <summary>
    /// Represent Node
    /// </summary>
    public interface ILink
    {
        string Node { get; }
        int Port { get; }
        List<string> Domains { get; }
        NodeType Type { get; }
        IDestination Destination { get; }
        NodeStatus Status { get; }


        string NodeId();

    }
}
