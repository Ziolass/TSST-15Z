using RoutingController.Interfaces;

namespace RoutingController.RoutingControllElements
{
    public class Link : ILink
    {
        public string SourceId { get; set; }
        public string DestinationId { get; set; }
        public int Weight { get; set; }

        public Link(string sourceId, string destinationId, int weight)
        {
            this.SourceId = sourceId;
            this.DestinationId = destinationId;
            this.Weight = weight;
        }
    }
}