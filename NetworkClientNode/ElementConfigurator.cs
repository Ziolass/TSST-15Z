using NetworkClientNode.Adaptation;
using NetworkClientNode.Menagment;
using NetworkNode.LRM.Communication;
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
            VirtualContainerLevel networkDefaultLevel = 0;
            List<string> domians = null;
            int lrmPort = 0;
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
                        else if (configReader.Name == "lrm-client")
                        {
                            lrmPort = int.Parse(configReader.GetAttribute("tcp"));
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
                        }
                    }

                }
            }

            SynchronousPhysicalInterface spi = new SynchronousPhysicalInterface(ports, sender, nodeName);
            TransportTerminalFunction ttf = new TransportTerminalFunction(spi, NodeMode.CLIENT);
            AdaptationFunction adpt = new AdaptationFunction(ttf, new LrmIntroduce
            {
                Domians = domians,
                Node = nodeName
            }
            , lrmPort, networkDefaultLevel);
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


            Thread.Sleep(2000);
            lock (adpt)
            {
                adpt.ConnectClient();
            }
            Thread.Sleep(100);
            lock (adpt)
            {
                adpt.IntroduceToLrm();
            }
            /*
            new Thread(delegate()
            {
                try
                {

                    adpt.ConnectClient();
                    new Thread(delegate()
                    {
                        try
                        {
                            adpt.IntroduceToLrm();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Indroduce " + e.Message);
                        }
                    }).Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Client " + e.Message);
                }

            }).Start();*/

            /*new Thread(delegate()
            {
                adpt.ConnectClient();
                adpt.IntroduceToLrm();
            }).Start();
            */
            return node;
        }
        public string GetNodeName(){
            string nodeName = null;
            while (configReader.Read())
            {
                if (configReader.IsStartElement())
                {
                    if (configReader.NodeType == XmlNodeType.Element)
                    {
                        if (configReader.Name == "node" && configReader.IsStartElement())
                        {
                            nodeName = configReader.GetAttribute("name");
                        }
                    }

                }
            }
            return nodeName;
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

        
    }
}
