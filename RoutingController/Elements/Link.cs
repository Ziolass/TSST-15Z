using RoutingController.Interfaces;
using System.Collections.Generic;

namespace RoutingController.Elements
{
    public class Link : ILink
    {
        public int Port { get; private set; }
        public List<string> Domains { get; private set; }
        public NodeType Type { get; private set; }
        public IDestination Destination { get; private set; }
        public NodeStatus Status { get; private set; }

        /*public Node()
        {
            this.Domains = new List<string>();
            this.Type = NodeType.UNDEF;
            this.Destination = new List<IDestination>();
            this.Status = NodeStatus.UNDEF;
        }*/

        public Link(int port, List<string> domains, NodeType type, Destination destination, NodeStatus status)
        {
            this.Port = port;
            this.Domains = domains;
            this.Type = type;
            this.Destination = destination;
            this.Status = status;
        }
    }
}