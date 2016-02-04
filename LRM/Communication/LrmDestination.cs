using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.LRM.Communication
{
    public class LrmDestination
    {
        public string Node { get; set; }
        public string Port { get; set; }
    }

    public class LrmSnp : LrmDestination
    {
        public string Index { get; set; }
    }
}
