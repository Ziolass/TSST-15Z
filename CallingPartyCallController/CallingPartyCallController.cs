using CallingPartyCallController.FileUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallingPartyCallController
{
    class CallingPartyCallController
    {
        private int localPort;
        private int nccPort;
        private ConnectionHandler chandler;
        private string ASname;
        
        public CallingPartyCallController(string id)
        {
            readConfig(id);
            chandler = new ConnectionHandler(localPort, this);
        }
        public string callRequest(string callingName, string calledName)
        {
            string response = "error";
            response = chandler.sendCommand("call-request|"+callingName.ToUpper()+"|"+calledName.ToUpper(), nccPort);
            return response;
        }
        public int getLocalPort()
        {
            return localPort;
        }
        public int getNCCPort()
        {
            return nccPort;
        }
        private void readConfig(string id)
        {
            Tuple<string, int,  int> t = ConfigReader.readConfig("cpccConfig"+id+".xml");
            localPort = t.Item2;
            ASname = t.Item1;
            Console.WriteLine("Identyfikator podsieci: " + ASname);
            nccPort = t.Item3;
        }
    }
}
