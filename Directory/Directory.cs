using Directory.FileUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directory
{
    class Directory
    {
        private Dictionary<string, string> hostnames_ports_dictionary;
        private int localPort;
        
     
        private void setDict(int port,Dictionary<string, string> dict)
        {
            hostnames_ports_dictionary = dict;
            localPort = port;
        }
        public void setUp()
        {
            try {
                Tuple<string,int, Dictionary<string, string>> t = ConfigReader.readEntriesFromConfig("directoryConfig.xml");
                string network_name = t.Item1;

                Console.WriteLine("Identyfikator sieci: " + network_name);
                setDict(t.Item2,t.Item3);
            }catch(Exception e)
            {
                Console.WriteLine("Blad! Sprawdz poprawnosc pliku konfiguracyjnego");
            }
        }
        public string checkForEntries(string query)
        {
            if (hostnames_ports_dictionary.ContainsKey(query))
            {
                return hostnames_ports_dictionary[query];
            }
            
            return "no-such-entry|";
        }
        public int getLocalPort()
        {
            return localPort;
        }

    }
}
