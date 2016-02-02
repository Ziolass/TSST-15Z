using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.LRM.Communication
{
    public enum ReqType
    {
        ALLOC, DELLOC
    }

    public class LrmPort
    {
        public string Number { get; set; }
        public string Index { get; set; }
    }
    public class LrmReq
    {
        public string Id { get; set; }
        public string ReqType { get; set; }
        public List<LrmPort> Ports { get; set; }
    }
}
