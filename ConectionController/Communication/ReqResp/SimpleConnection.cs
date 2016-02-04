using NetworkNode.LRM.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectionController.Communication.ReqResp
{
    class SimpleConnection
    {
        public string Id { get; set; }
        public string Protocol { get; set; }
        public List<LrmDestination> Ends { get; set; }
        public string Domian { get; set; }
    }
}
