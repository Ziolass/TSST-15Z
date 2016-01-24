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
        private int ccPort;
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
        public int getCCPort()
        {
            return ccPort;
        }
        private int policyPort;
        private string ASforeignName;
        private ConnectionHandler chandler;
        public NetworkCallController(string id)
        {
            readConfig(id);
            chandler = new ConnectionHandler(localPort,this);
            
        }


        private void readConfig(string id)
        {

            Tuple<string, int,string,int,int,int,int> t = ConfigReader.readConfig("nccConfig"+id+".xml");
            localPort = t.Item2;
            ASname = t.Item1;
            ASforeignName = t.Item3;
            Console.WriteLine("Identyfikator podsieci: " + ASname);
            foreignPort = t.Item4;
            directoryPort = t.Item5;
            policyPort = t.Item6;
            ccPort = t.Item7;
        }
    }
    
}
