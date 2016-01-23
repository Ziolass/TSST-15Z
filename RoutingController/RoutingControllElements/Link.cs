using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.RoutingControllElements
{
    public class Link : ILink
    {
        public int SourceId { get; set; }
        public int DestinationId { get; set; }
        public int Weight { get; set; }

        public Link(int sourceId, int destinationId, int weight)
        {
            this.SourceId = sourceId;
            this.DestinationId = destinationId;
            this.Weight = weight;
        }
    }
}
