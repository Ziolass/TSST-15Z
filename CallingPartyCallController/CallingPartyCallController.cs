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
        private Dictionary<string, string> connectedClientsList;

        public CallingPartyCallController(string id)
        {
            connectedClientsList = new Dictionary<string, string>();
            readConfig(id);
            chandler = new ConnectionHandler(localPort, this);
        }
        public void addToConnectedClients(string calling, string called)
        {
            connectedClientsList.Add(calling, called);
        }
        public bool checkIfConnectionExist()
        {
            // todo 
            return false;
        }
        internal void deleteRecord(string v)
        {
            throw new NotImplementedException();
        }
        public string callRequest(string callingName, string calledName)
        {
            string response = "error";
            response = chandler.sendCommand("call-request|" + callingName.ToUpper() + "|" + calledName.ToUpper(), nccPort);
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
            Tuple<string, int, int> t = ConfigReader.readConfig("cpccConfig" + id + ".xml");
            localPort = t.Item2;
            ASname = t.Item1;
            Console.WriteLine("Identyfikator podsieci: " + ASname);
            nccPort = t.Item3;
        }


    }
}
