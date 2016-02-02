using NetworkNode.LRM.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRM.Communication
{
    public class ConnectionRequest
    {
        public string Id { get; set; }
        public List<ConnectionStep> Steps { get; set; }
    }

    public class ConnectionStep
    {
        public string Node { get; set; }
        public List<LrmPort> Ports{ get; set; }
    }


}
