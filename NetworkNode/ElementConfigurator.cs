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
            int nodeNumber = -1;
            String nodeType = null;
            ManagementPort managementPort = null;
            while (configReader.Read())
            {
                if (configReader.IsStartElement())
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
                        else if (configReader.Name == "management-port")
                        {
                            int portNumber = int.Parse(configReader.GetAttribute("number"));
                            managementPort = new ManagementPort(portNumber);
                        } 
                        else if (configReader.Name == "node" && configReader.IsStartElement())
                        {
                            nodeNumber = int.Parse(configReader.GetAttribute("number"));
                            nodeType = configReader.GetAttribute("type");
                        }
                    }

                }
            }

            SynchronousPhysicalInterface spi = new SynchronousPhysicalInterface(inputs, outputs);
            TransportTerminalFunction ttf = new TransportTerminalFunction(spi, getMode(nodeType));
            HigherOrderPathConnection hpc = new HigherOrderPathConnection(ttf);
            NetworkNode node = new NetworkNode(hpc, spi);
            
            ManagementCenter managementCenter = new ManagementCenter(managementPort,node);
            managementPort.SetManagementCenter(managementCenter);
            
            return node;
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
