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
using WireCloud;

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
            List<NodeInput> ports = new List<NodeInput>();
            string nodeName = null;
            string nodeType = null;
            ManagementPort managementPort = null;
            NetworkNodeSender sender = null;
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
                            int tcp = int.Parse(configReader.GetAttribute("tcp"));
                            ports.Add(new NodeInput(tcp, portNumber)); 
                        }
                        else if (configReader.Name == "cloud-server")
                        {
                            int tcp = int.Parse(configReader.GetAttribute("tcp"));
                            sender = new NetworkNodeSender(tcp);
                        }
                        else if (configReader.Name == "managment-port")
                        {
                            int portNumber = int.Parse(configReader.GetAttribute("number"));
                            managementPort = new ManagementPort(portNumber);
                        } 
                        else if (configReader.Name == "node" && configReader.IsStartElement())
                        {
                            nodeName = configReader.GetAttribute("name");
                            nodeType = configReader.GetAttribute("type");
                        }
                    }

                }
            }

            SynchronousPhysicalInterface spi = new SynchronousPhysicalInterface(ports, sender, nodeName);
            TransportTerminalFunction ttf = new TransportTerminalFunction(spi, getMode(nodeType));
            HigherOrderPathConnection hpc = new HigherOrderPathConnection(ttf);
            NetworkNode node = new NetworkNode(hpc, ttf, nodeName);
            
            ManagementCenter managementCenter = new ManagementCenter(managementPort,node);
            managementPort.SetManagementCenter(managementCenter);
            managementPort.StartListening();
            
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
