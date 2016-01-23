using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public interface INode
    {
        int Id { get; set; }
        String NetworkId { get; set; }
    }
}
