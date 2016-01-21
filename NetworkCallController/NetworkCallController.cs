using NetworkCallController.FileUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCallController
{
    class NetworkCallController
    {
        private int localPort;
        private string ASname;
        private ConnectionHandler chandler;
        public NetworkCallController()
        {
            readConfig();
            chandler = new ConnectionHandler(localPort);
            
        }


        private void readConfig()
        {
            Tuple<string, int> t = ConfigReader.readConfig("nccConfig.xml");
            localPort = t.Item2;
            ASname = t.Item1;
        }
    }
    
}
