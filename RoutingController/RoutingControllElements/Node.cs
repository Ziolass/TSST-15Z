using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
