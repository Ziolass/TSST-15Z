using NetworkNode.HPC;
using NetworkNode.MenagmentModule;
using NetworkNode.Ports;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NetworkNode
{
    class ElementConfigurator
    {
        private XmlReader configReader;

        public ElementConfigurator(string cofigFilePath)
        {
            configReader = XmlReader.Create(cofigFilePath);
        }

        public NetworkNode configureNode()
        {
            List<Input> inputs = new List<Input>();
            Dictionary<int, Output> outputs = new Dictionary<int, Output>();
            
            //IoSlot managementInterface = null;
            int nodeNumber = -1;
            String nodeType = null;
            while (configReader.Read())
            {
                if (configReader.IsStartElement())
                {
                    if (configReader.NodeType == XmlNodeType.Element)
                    {
                        if (configReader.Name == "port")
                        {
                            string portType = configReader.GetAttribute("portType");
                            int portNumber = int.Parse(configReader.GetAttribute("portNumber"));

                            if (configReader.Name.Equals("input"))
                            {
                                inputs.Add(new Input(portNumber));
                            }

                            if (configReader.Name.Equals("output"))
                            {
                                outputs.Add(portNumber, new Output(portNumber));
                            }

                        }
                        else if (configReader.Name == "node" && configReader.IsStartElement())
                        {
                            nodeNumber = int.Parse(configReader.GetAttribute("number"));
                            nodeType = configReader.GetAttribute("nodeType");
                        }
                    }

                }
            }

            SynchronousPhysicalInterface spi = new SynchronousPhysicalInterface(inputs, outputs);
            TransportTerminalFunction ttf = new TransportTerminalFunction(spi, getMode(nodeType));
            HigherOrderPathConnection hpc = new HigherOrderPathConnection(ttf);
            
            //ManagementCenter managementCenter = new ManagementCenter(managementInterface, ttf, hpc, nodeNumber);

            return new NetworkNode(null, hpc);
        }

        private NodeMode getMode(string mode)
        {
            NodeMode result;
            switch (mode)
            {
                case "mux":
                {
                    result = NodeMode.MULTIPLEXER;
                    break;
                }
                case "reg":
                {
                    result = NodeMode.REGENERATOR;
                    break;
                }
                default:
                {
                    result = NodeMode.MULTIPLEXER;
                    break;
                }
            }

            return result;
        }
    }
}
