using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.RoutingControllElements
{
    public interface ILink
    {
        public int SourceId { get; set; }
        public int DestinationId { get; set; }
        public int Weight { get; set; }
    }
}
