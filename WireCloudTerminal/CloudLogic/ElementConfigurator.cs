
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WireCloud.CloudLogic
{
    public class ElementConfigurator
    {
        private XmlReader ConfigReader;
        public ElementConfigurator(string cofigFilePath)
        {
            ConfigReader = XmlReader.Create(cofigFilePath);
        }

        public ProcessMonitor SetUpCloud()
        {
            List<Link> links = new List<Link>();
            CloudServer server = null;
            while (ConfigReader.Read())
            {
                if (ConfigReader.IsStartElement())
                {
                    if (ConfigReader.NodeType == XmlNodeType.Element)
                    {
                        if (ConfigReader.Name == "link")
                        {
                            links.Add(createLink(ConfigReader.ReadSubtree()));
                        }
                        else if (ConfigReader.Name == "server" && ConfigReader.IsStartElement())
                        {
                            int port = int.Parse(ConfigReader.GetAttribute("tcp"));
                            int buffer = int.Parse(ConfigReader.GetAttribute("buffer"));
                            int clientsNumber = int.Parse(ConfigReader.GetAttribute("clientsNumber"));
                            server = new CloudServer(port);
                            server.SetUpServer(buffer, clientsNumber);

                        }
                    }
                }
            }
            return new ProcessMonitor(server, links);
        }

        private Link createLink(XmlReader configReader)
        {
            List<NetworkNodeSender> senders = new List<NetworkNodeSender>();
            List<AbstractAddress> addresses = new List<AbstractAddress>();
            while (configReader.Read())
            {

                if (configReader.NodeType == XmlNodeType.Element)
                {
                    if (configReader.Name.Equals("port"))
                    {
                        int tcpPort = int.Parse(configReader.GetAttribute("tcp"));
                        int localPort = int.Parse(configReader.GetAttribute("local"));
                        string nodeId = configReader.GetAttribute("node");
                        addresses.Add(new AbstractAddress(localPort, nodeId));
                        senders.Add(new NetworkNodeSender(tcpPort));
                    }
                }
            }

            if (senders.Count != 2)
            {
                throw new Exception("Error in config file : wrong number of ports in link");
            }

            Dictionary<AbstractAddress, NetworkNodeSender> linkConnections = new Dictionary<AbstractAddress, NetworkNodeSender>();
            linkConnections.Add(addresses[0], senders[1]);
            linkConnections.Add(addresses[1], senders[0]);           

            return new Link(linkConnections);
        }

    }
}