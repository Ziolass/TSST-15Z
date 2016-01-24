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
        private ConnectionHandler chandler;
        private string ASname;
        public int getLocalPort()
        {
            return localPort;
        }
        public int getNCCPort()
        {
            return nccPort;
        }
        public CallingPartyCallController()
        {
            readConfig();
            chandler = new ConnectionHandler(localPort, this);
        }
        public void callRequest()
        {

        }
        private void readConfig()
        {
            Tuple<string, int, int> t = ConfigReader.readConfig("cpccConfig.xml");
            localPort = t.Item2;
            ASname = t.Item1;
            nccPort = t.Item3;
        }
    }
}
