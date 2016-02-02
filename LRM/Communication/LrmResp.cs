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
        public string Msg { get; set; }
        public string Id { get; set; }
    }
}
