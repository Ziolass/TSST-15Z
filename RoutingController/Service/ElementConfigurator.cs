using RoutingController.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RoutingController.Service
{
    public class ElementConfigurator
    {
        private XmlReader ConfigReader;

        public ElementConfigurator(string cofigFilePath)
        {
            ConfigReader = XmlReader.Create(cofigFilePath);
        }

        public RoutingControllerCenter ConfigureRoutingController()
        {
            int servicePort = 0;
            string networkName = string.Empty;
            List<NeighbourRoutingController> neighboursList = new List<NeighbourRoutingController>();
            while (ConfigReader.Read())
            {
                if (ConfigReader.IsStartElement())
                {
                    if (ConfigReader.NodeType == XmlNodeType.Element)
                    {
                        if (ConfigReader.Name == "port")
                        {
                            if (!int.TryParse(ConfigReader.GetAttribute("value"), out servicePort))
                                throw new Exception("Error ElementConfigurator: Port value failed to parse!");
                        }   
                        //TODO: Dodanie możliwości startowania RC z NetworkGraph
                        else if (ConfigReader.Name == "neighbour-rc")
                        {
                            int neighbourPort = -1;
                            string neighbourDomainName = string.Empty;
                            if (!int.TryParse(ConfigReader.GetAttribute("port"), out neighbourPort))
                                throw new Exception("Error ElementConfigurator: Port value failed to parse!");

                            neighbourDomainName = ConfigReader.GetAttribute("domain-name");

                            neighboursList.Add(new NeighbourRoutingController(neighbourPort, neighbourDomainName));
                        }
                        else if (ConfigReader.Name == "network-name")
                        {
                            networkName = ConfigReader.GetAttribute("value");
                        }
                    }
                }
            }

            RoutingControllerCenter routingControllCenter = new RoutingControllerCenter(servicePort, neighboursList, networkName);
            return routingControllCenter;
        }
    }
}
