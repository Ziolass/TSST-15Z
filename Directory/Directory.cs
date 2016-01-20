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
        private Dictionary<string, int> hostnames_ports_dictionary;
        
        public Directory()
        {
        
        }
        private void setDict(Dictionary<string, int> dict)
        {
            hostnames_ports_dictionary = dict;
        }
        public void setUp()
        {
            try {
                Tuple<string, Dictionary<string, int>> t = ConfigReader.readEntriesFromConfig("directoryConfig.xml");
                string network_name = t.Item1;
                Console.WriteLine("Identyfikator sieci: " + network_name);
                setDict(t.Item2);
            }catch(Exception e)
            {
                Console.WriteLine("Blad! Sprawdz poprawnosc pliku konfiguracyjnego");
            }
        }

    }
}
