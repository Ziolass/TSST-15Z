using NetworkNode.LRM.Communication;
using System;

namespace ConectionController.Communication.ReqResp
{
    public class HigherLevelConnectionRequest
    {
        public string Id { get; set; }
        public String Type { get; set; }
        public LrmSnp Src { get; set; }
        public LrmSnp Dst { get; set; }
    }
}