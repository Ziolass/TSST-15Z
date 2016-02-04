using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCallController.Adapters
{
    public enum CommunicationType
    {
        CC_COMMUNICATION
    }

    class CcResponse
    {
        public string Type { get; set; }
        public string Response { get; set; }

        public CcResponse()
        {
            Type = CommunicationType.CC_COMMUNICATION.ToString();
        }
    }
}
