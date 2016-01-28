using NetworkNode.HPC;
using NetworkNode.LRM;
using NetworkNode.MenagmentModule;
using NetworkNode.Ports;
using NetworkNode.SDHFrame;
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
    public class ElementConfigurator
    {
        private XmlReader configReader;

        public ElementConfigurator(string cofigFilePath)
        {
            configReader = XmlReader.Create(cofigFilePath);
        }

        public NetworkNode configureNode()
        {
            List<NodeInput> ports = new List<NodeInput>();
            int rcPort = 0;
            string nodeName = null;
            string nodeType = null;
            string domain = null;
            NodeCcInput ccServer = null;
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
                            int portNumber = int.Parse(configReader.GetAttribute("local"));
                            int tcp = int.Parse(configReader.GetAttribute("tcp"));
                            string stm = configReader.GetAttribute("stm");

                            ports.Add(new NodeInput(tcp, portNumber, StmLevelExt.GetContainer(stm))); 
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
                        else if (configReader.Name == "lrm")
                        {
                            domain = configReader.GetAttribute("domain");
                            int ccTcp = int.Parse(configReader.GetAttribute("cc-tcp"));
                            rcPort = int.Parse(configReader.GetAttribute("rc-tcp"));
                            ccServer = new NodeCcInput(ccTcp);
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
            LinkResourceManager lrm = new LinkResourceManager(hpc, domain, nodeName, ccServer, rcPort);
            ccServer.Lrm = lrm;

            NetworkNode node = new NetworkNode(hpc, ttf, nodeName);
            
            ManagementCenter managementCenter = new ManagementCenter(managementPort,node);
            managementPort.SetManagementCenter(managementCenter);
            managementPort.StartListening();
            
            foreach (NodeInput input in ports)
            {
                input.SetUpServer(10000, 10);
                input.StartListening();
            }

            ccServer.SetUpServer(10000, 10);
            ccServer.StartListening();
            
            lrm.Strat();
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
