using NetworkNode.HPC;
using NetworkNode.MenagmentModule;
using NetworkNode.Ports;
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

namespace Client
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
            List<Input> inputs = new List<Input>();
            Dictionary<int, Output> outputs = new Dictionary<int, Output>();
            int manag = 0;
            while (configReader.Read())
            {
                //if (configReader.IsStartElement())
                {
                    if (configReader.NodeType == XmlNodeType.Element)
                    {
                        if (configReader.Name == "port")
                        {
                            string portType = configReader.GetAttribute("type");
                            int portNumber = int.Parse(configReader.GetAttribute("number"));

                            switch (portType)
                            {
                                case "input":
                                    {
                                        Input input = new Input(portNumber);
                                        inputs.Add(input);
                                        input.TurnOn();
                                        break;
                                    }
                                case "output":
                                    {
                                        outputs.Add(portNumber, new Output(portNumber));
                                        break;
                                    }
                            }



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

            if (manag!= 0 )
                return new LinkCollection(inputs, outputs, name, new Input(manag), new Output(manag));
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
        public List<Input> input_ports;
        public Dictionary<int, Output> output_ports;
        public string name;
        public Input management_in;
        public Output management_out;
        public LinkCollection(List<Input> inp, Dictionary<int, Output> outp,string name,Input manag_in,Output manag_out)
        {
            input_ports = inp;
            output_ports = outp;
            this.name = name;
            this.management_in = manag_in;
            this.management_out = manag_out;
        }
        //TODO: POLA -> PRIVATE, DOSTĘP PRZEZ PROPERTY
    }
}
