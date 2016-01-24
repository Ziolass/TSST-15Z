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
    public interface INode
    {
        List<string> Domains { get; }
        int Port { get; }
        NodeType Type { get; }
        List<Link> LinkList { get; }
        NodeStatus Status { get; }
    }

}
