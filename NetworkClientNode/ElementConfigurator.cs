using NetworkClientNode.Adaptation;
using NetworkClientNode.Menagment;
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

namespace NetworkClientNode
{
    public class ElementConfigurator
    {
        private XmlReader configReader;

        public ElementConfigurator(string cofigFilePath)
        {
            configReader = XmlReader.Create(cofigFilePath);
        }

        public NetworkClNode ConfigureNode()
        {
            List<NodeInput> ports = new List<NodeInput>();
            string nodeName = null;
            ManagementClientPort managementPort = null;
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
                            managementPort = new ManagementClientPort(portNumber);
                        }
                        else if (configReader.Name == "node" && configReader.IsStartElement())
                        {
                            nodeName = configReader.GetAttribute("name");
                        }
                    }

                }
            }

            SynchronousPhysicalInterface spi = new SynchronousPhysicalInterface(ports, sender, nodeName);
            TransportTerminalFunction ttf = new TransportTerminalFunction(spi, NodeMode.CLIENT);
            AdaptationFunction adpt = new AdaptationFunction(ttf);
            NetworkClNode node = new NetworkClNode(adpt, nodeName);
            
            //TODO
            List<StreamData> records = new List<StreamData>();
            records.Add(new StreamData(1,StmLevel.STM1, VirtualContainerLevel.VC32, 0, 0));
            //node.AddStreamData(records);
            ManagementCenter managementCenter = new ManagementCenter(managementPort, node);
            managementPort.SetManagementCenter(managementCenter);
            managementPort.StartListening();
            foreach (NodeInput input in ports)
            {
                input.SetUpServer(10000, 10);
                input.StartListening();
            }
            return node;
        }
        
    }
}
