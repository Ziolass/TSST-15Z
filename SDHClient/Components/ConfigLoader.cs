using NetworkNode.HPC;
using NetworkNode.MenagmentModule;
using NetworkNode.TTF;
using NetworkNode;
using WireCloud;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Specialized;

namespace SDHClient
{
   public class ConfigLoader
    {
        private XmlReader configReader;
        string name = "";
        public ConfigLoader(string cofigFilePath)
        {
            
            configReader = XmlReader.Create(cofigFilePath);
        
        }

        public LinkCollection getLinks()
        {
            List<IOPort> ports = new List<IOPort>();
            //List<Input> inputs = new List<Input>();
            //Dictionary<int, Output> outputs = new Dictionary<int, Output>();
            int manag = 0;
            while (configReader.Read())
            {
                //if (configReader.IsStartElement())
                {
                    if (configReader.NodeType == XmlNodeType.Element)
                    {
                        if (configReader.Name == "port")
                        {
                            //string portType = configReader.GetAttribute("type");
                            int portNumberTo = 0;

                                bool b = int.TryParse(configReader.GetAttribute("to"),out portNumberTo);
                            int portNumberLs = 0;

                                bool c = int.TryParse(configReader.GetAttribute("listen"),out portNumberLs);
                            if (b == false || c == false) throw new FormatException("Nieprawidłowy plik konfiguracyjny");                       ////  switch (portType)
                           // {
                               // case "input":
                                //    {
                                        IOPort io = new IOPort(portNumberTo, portNumberLs);
                                          ports.Add(io);
                                       // Input input = new Input(portNumber);
                                       // inputs.Add(input);
                                        //input.TurnOn();
                                       // break;
                                 //   }
                               // case "output":
                                 //   {
                                       // outputs.Add(portNumber, new Output(portNumber));
                                     //   break;
                                 //   }
                           // }

                        }
                        else if (configReader.Name == "name")
                        {
                            name = configReader.GetAttribute("value");
                        }
                        else if (configReader.Name == "management_port")
                            manag = Int32.Parse(configReader.GetAttribute("number"));
                        
                    }
                   // resource - location |{ port_we}#{port_wy}#{poziom_z1}#{poziom_do1}#{typ_konteneru1}

                }
            }

            if (manag!= 0)
                return new LinkCollection( name,ports,manag);
            else
                throw new NotImplementedException("Nie odczytano wartosci portow management");
            
        }
        public string  Name
        {
            get
            {
                return name;
            }
        }

    }
    public class LinkCollection
    {
        public List<IOPort> ports;
       // public Dictionary<int, Output> output_ports;
        public string name;
        //public Input management_in;
        //public Output management_out;
        public int management_port;
        public LinkCollection(string name,List<IOPort> ports,int manag)
        {
            this.ports = ports;
            this.name = name;
            this.management_port = manag;
        }
        //TODO: POLA -> PRIVATE, DOSTĘP PRZEZ PROPERTY
    }
}
