using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.LRM.Communication
{
    public class LrmToken
    {
        public string Tag { get; set; }
        public string SenderPort { get; set; }
        public LrmDestination Reciver { get; set; }
    }
}
