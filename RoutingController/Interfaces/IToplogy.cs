using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public interface ITopology
    {
        string Node { get; }
        List<ILink> LinkList { get; }
    }
}
