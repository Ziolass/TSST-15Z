using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.LRM.Communication
{
    public enum LrmRespStatus
    {
        ACK, ERROR
    }
    public class LrmResp
    {
        public string Type { get; set; }
        public string Status { get; set; }
        public List<int> Ports { get; set; }
    }
}
