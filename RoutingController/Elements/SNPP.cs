using RoutingController.Interfaces;

namespace RoutingController.Elements
{
    public class SNPP : ISNPP
    {
        public int Id { get; set; }
        public int NodeId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SNPP"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="nodeid">The nodeid.</param>
        public SNPP(int id, int nodeid)
        {
            this.Id = id;
            this.NodeId = nodeid;
        }
    }
}