using NetworkNode.HPC;
using NetworkNode.LRM;
using NetworkNode.LRM.Communication;
using NetworkNode.MenagmentModule;
using NetworkNode.Ports;
using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            VirtualContainerLevel networkDefaultLevel = 0;
            List<string> domians = null;
            int lrmPort = 0;
            int lrmPort2 = 0;
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
                        else if (configReader.Name == "lrm-client")
                        {
                            lrmPort = int.Parse(configReader.GetAttribute("tcp"));
                        }
                        else if (configReader.Name == "lrm-client2")
                        {
                            lrmPort2 = int.Parse(configReader.GetAttribute("tcp"));
                        }
                        else if (configReader.Name == "domians")
                        {
                            int domiansNumber = int.Parse(configReader.GetAttribute("number"));
                            domians = CreateDomainsHierarchy(configReader.ReadSubtree(), domiansNumber);

                        }
                        else if (configReader.Name == "network")
                        {
                            string levelTxt = configReader.GetAttribute("level");
                            Type enumType = typeof(VirtualContainerLevel);
                            networkDefaultLevel = (VirtualContainerLevel)Enum.Parse(enumType, levelTxt);
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
            HigherOrderPathConnection hpc = new HigherOrderPathConnection(ttf, networkDefaultLevel);
            LrmIntroduce lrmIntroduce = new LrmIntroduce
            {
                Domians = domians,
                Node = nodeName
            };
            LrmIntroduce lrmIntroduceGateway = null;
            if (lrmPort2 != 0)
            {

                lrmIntroduceGateway = new LrmIntroduce
                {
                    Domians = domians,
                    Node = nodeName
                };
            }

            NetworkNode node = new NetworkNode(hpc, ttf, lrmIntroduce, lrmPort, lrmIntroduceGateway, lrmPort2);

            ManagementCenter managementCenter = new ManagementCenter(managementPort, node);
            managementPort.SetManagementCenter(managementCenter);
            managementPort.StartListening();

            foreach (NodeInput input in ports)
            {
                input.SetUpServer(10000, 10);
                input.StartListening();
            }
            

            //Thread.Sleep(100);
            new Thread(delegate()
            {
                node.StartLrmClient();
                node.IntroduceToLrm();
            }).Start();

            if (lrmPort2 != 0)
            {
                new Thread(delegate()
                {
                    node.StartLrmGatewayClient();
                    node.IntroduceToLrmGateway();
                }).Start();
            }

            return node;
        }

        private List<string> CreateDomainsHierarchy(XmlReader reader, int domiansNumber)
        {
            string[] domians = new string[domiansNumber];
            while (reader.Read())
            {
                if (reader.Name == "domian")
                {
                    int index = int.Parse(reader.GetAttribute("index"));
                    string domian = reader.GetAttribute("name");
                    domians[index] = domian;
                }
            }
            return new List<string>(domians);
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
