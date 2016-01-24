using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Interfaces
{
    public interface IRequest
    {
        string Protocol { get; set; }
        string Node { get; set; }
        List<INode> Data { get; set; }
    }
}
