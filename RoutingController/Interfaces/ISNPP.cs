using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Elements;

namespace RoutingController.Interfaces
{
    public interface ISNPP
    {
        List<SNP> Steps { get; }
    }
}
