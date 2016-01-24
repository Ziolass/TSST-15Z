using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Elements;

namespace RoutingController.Interfaces
{
    public interface ITopology
    {
        List<Link> LinkList { get; }
        string Node { get; } //Which node send this info

    }
}
