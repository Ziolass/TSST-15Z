using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public enum NodeType { LOCAL, NETWORK, CLIENT, UNDEF }
    public enum NodeStatus { FREE, OCCUPIED, UNDEF }

    /// <summary>
    /// Represent Node
    /// </summary>
    public interface INode
    {
        List<string> Domains { get; private set; }
        int Port { get; private set; }
        NodeType Type { get; private set; }
        List<IDestination> Destination { get; private set; }
        NodeStatus Status { get; private set; }
    }

}
