using NetworkNode.LRM.Communication;
using System.Collections.Generic;

namespace ConectionController.Communication.ReqResp
{
    public class SimpleConnection
    {
        public string Id { get; set; }
        public string Protocol { get; set; }
        public List<LrmDestination> Ends { get; set; }
        public string Domain { get; set; }
    }
}