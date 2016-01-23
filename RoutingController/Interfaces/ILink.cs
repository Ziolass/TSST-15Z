using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public interface ILink
    {
        int SourceId { get; set; }
        int DestinationId { get; set; }
        int Weight { get; set; }
    }
}
