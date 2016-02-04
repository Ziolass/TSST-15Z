using NetworkClientNode.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClientNode.CPCC
{
    class CallingPartyCallController
    {
        private int localPort;
        private int nccPort;
        private string clientName;
        private ClientViewModel ClientView;
        private ConnectionHandler chandler;
        private string ASname;
        private List<string> connectedClientsList;

        public CallingPartyCallController(string id,ClientViewModel cview)
        {
            connectedClientsList = new List<string>();
            this.ClientView = cview;
           // ConsoleManager.Show();
            readConfig(id);
            chandler = new ConnectionHandler(localPort, this);
        }
        public void updateConsole(string s)
        {
            ClientView.MessageConsoleText += DateTime.Now + ": " + s.ToString()+"\n";
            ClientView.RisePropertyChange(ClientView, "MessageConsoleText");
        }
        public void addToConnectedClients(string called)
        {
            if (checkIfConnectionExist(called))
            {
                return;
            }
            connectedClientsList.Add(called);
        }
        public bool checkIfConnectionExist(string called)
        {

            if (connectedClientsList.Contains(called))
            {
                return true;
            }
            return false;
        }
        internal void deleteRecord(string v)
        {
            connectedClientsList.Remove(v);
        }
        public string callRequest(string calledName)
        {
            if (checkIfConnectionExist(calledName))
            {
                return "error|Takie polaczenie juz istnieje";
            }
            if (calledName.ToLower().Equals(clientName.ToLower()))
            {
                return "error|nie mozesz sam sie do siebie polaczyc";
            }
            string response = "error";
            response = chandler.sendCommand("call-request|" + clientName.ToUpper() + "|" + calledName.ToUpper(), nccPort);
            if (!response.Split('|')[0].Equals("error"))
            {
                connectedClientsList.Add(calledName);
            }
            else
            {
                updateConsole(response.Split('|')[1]);
            }
            return response;
        }
        public string callTeardown(string calledName)
        {
            if (!checkIfConnectionExist(calledName))
            {
                return "error|Takie polaczenie nie istnieje";
            }

            string response = "error";
            response = chandler.sendCommand("call-teardown|" + clientName.ToUpper() + "|" + calledName.ToUpper(), nccPort);
            if (!response.Split('|')[0].Equals("error"))
            {
                deleteRecord(calledName);
            }
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
            Tuple<string, int, int, string> t = ConfigReader.readConfig("cpccConfig" + id + ".xml");
            localPort = t.Item2;
            ASname = t.Item1;
            updateConsole("Identyfikator podsieci: " + ASname);
            nccPort = t.Item3;
            clientName = t.Item4;
            updateConsole("Nazwa klienta: " + clientName);
        }

        internal string getName()
        {
            return clientName;
        }
    }
}
