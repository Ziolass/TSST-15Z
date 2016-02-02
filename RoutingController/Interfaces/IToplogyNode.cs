using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Elements;

namespace RoutingController.Interfaces
{
    public interface ITopologyNode
    {
        string Node { get; } //Which node send this info
        List<string> Domains { get; }
        List<Link> Data { get; }

    }
}
