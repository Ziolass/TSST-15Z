using RoutingController.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace RoutingController.Elements
{
    public class Node : INode
    {
        public List<string> Domains { get; private set; }
        public int Port { get; private set; }
        public NodeType Type { get; private set; }
        public List<Link> LinkList { get; private set; }
        public NodeStatus Status { get; private set; }

        /*public Node()
        {
            this.Domains = new List<string>();
            this.Type = NodeType.UNDEF;
            this.Destination = new List<IDestination>();
            this.Status = NodeStatus.UNDEF;
        }*/

        public Node(List<string> domains, int port, NodeType type, List<Link> linkList, NodeStatus status)
        {
            this.Domains = domains;
            this.Port = port;
            this.Type = type;
            this.LinkList = linkList;
            this.Status = status;
        }
    }
}