using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.LRM.Communication
{
    public class ResourceLocation
    {
        public string Type { get; set; }
        public List<LrmPort> AllocatedPorts { get; set; }
    }
}
