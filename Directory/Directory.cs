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
        public void setUp(string id)
        {
            try {
                Tuple<string,int, Dictionary<string, string>> t = ConfigReader.readEntriesFromConfig("directoryConfig"+id+".xml");
                string network_name = t.Item1;

                Console.WriteLine("Identyfikator sieci: " + network_name);
                setDict(t.Item2,t.Item3);
            }catch(Exception e)
            {
                Console.WriteLine("Blad! Sprawdz poprawnosc pliku konfiguracyjnego");
            }
        }
        private string checkForEntries(string query)
        {
            if (hostnames_ports_dictionary.ContainsKey(query))
            {
                Console.WriteLine("Znaleziono wpis: " + query);
                return hostnames_ports_dictionary[query];
            }
            
            return "no-such-entry";
        }
        private string addEntry(string name,string address)
        {
            hostnames_ports_dictionary.Add(name, address);
            Console.WriteLine("Wpis: " + name + "/ " + address +" dodano pomyslnie.");
            return "entry-added|";
        }
        public string commandHandle(string query)
        {
            string[] temp = query.Split('|');
            switch (temp[0])
            {
                case "get-address":
                    return checkForEntries(temp[1]);
                case "add-entry":
                    return addEntry(temp[1], temp[2]);
                default:
                    return "error|";
            }
        }
        public int getLocalPort()
        {
            return localPort;
        }

    }
}
