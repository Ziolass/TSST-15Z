using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public interface ILink
    {
        string SourceId { get; set; }
        string DestinationId { get; set; }
        double Weight { get; set; }
    }
}
