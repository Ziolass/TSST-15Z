using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkNode.LRM.Communication;

namespace ConectionController.Communication.ReqResp
{
    public class HigherLevelConnectionRequest
    {
        public string Id { get; set; }
        public String Type { get; set; } 
        public LrmSnp  Src {get;set;}
        public LrmSnp Dst { get; set; }
    }
}
