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
        private int foreignPort;
        private string ASname;
        private int directoryPort;
        public int getDirectoryPort()
        {
            return directoryPort;
        }
        public int getPolicyPort()
        {
            return policyPort;
        }
        public int getForeingPort()
        {
            return foreignPort;
        }
        private int policyPort;
        private string ASforeignName;
        private ConnectionHandler chandler;
        public NetworkCallController()
        {
            readConfig();
            chandler = new ConnectionHandler(localPort,this);
            
        }


        private void readConfig()
        {
            Tuple<string, int,string,int,int,int> t = ConfigReader.readConfig("nccConfig.xml");
            localPort = t.Item2;
            ASname = t.Item1;
            ASforeignName = t.Item3;
            foreignPort = t.Item4;
            directoryPort = t.Item5;
            policyPort = t.Item6;
        }
    }
    
}
