using Newtonsoft.Json;
using RoutingController.Interfaces;
using System.Collections.Generic;

namespace RoutingController.Elements
{
    public class Link : ILink
    {
        public string Port { get; private set; }
        public List<string> Domains { get; private set; }
        public IDestination Destination { get; private set; }
        public NodeStatus Status { get; private set; }

        public Link()
        {
            this.Domains = new List<string>();
            this.Destination = new Destination();
            this.Status = NodeStatus.UNDEF;
        }

        [JsonConstructor]
        public Link(string port, List<string> domains, Destination destination, NodeStatus status)
        {
            this.Port = port;
            this.Domains = domains;
            this.Destination = destination;
            this.Status = status;
        }
    }
}