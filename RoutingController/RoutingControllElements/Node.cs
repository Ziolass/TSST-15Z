using RoutingController.Interfaces;
using System;

namespace RoutingController.RoutingControllElements
{
    public class Node : INode
    {
        public int Id { get; set; }
        public String NetworkId { get; set; }

        public Node(int id, String networkId)
        {
            this.Id = id;
            this.NetworkId = networkId;
        }
    }
}