using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public interface ITopology
    {
        int NetworkLevel { get; set; }
        string NetworkId { get; set; }
        List<ILink> LinkList { get; set; }
    }
}
