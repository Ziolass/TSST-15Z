using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.RoutingControllElements
{
    public interface ILinkResourceMenager
    {
        public ITopology LocalTopology();
    }
}
