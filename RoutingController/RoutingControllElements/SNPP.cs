using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.RoutingControllElements
{
    public class SNPP : ISNPP
    {
        public int Id { get; private set; }
        public int NodeId { get; private set; }

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
