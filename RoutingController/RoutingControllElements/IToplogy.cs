using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.RoutingControllElements
{
    public interface ITopology
    {
        public int Id { get; set; }
        public String NetworkId { get; set; }
        public List<ILink> LinkList { get; set; }
    }
}
