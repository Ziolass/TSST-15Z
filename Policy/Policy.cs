using Policy.FileUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Policy
{
    class Policy
    {
        private int localPort;
        private string ASidentifier;

        public Policy(string id)
        {
            Tuple<string,int,Dictionary<string,string>> t = ConfigReader.readEntriesFromConfig("policyConfig"+id+".xml");
            ASidentifier = t.Item1;
            Console.WriteLine("Identyfikator podsieci: " + ASidentifier);
            localPort = t.Item2;
        }
        public int getLocalPort()
        {
            return localPort;
        }
        public bool verify(string query)
        {
            return true;
        }
    }
}
