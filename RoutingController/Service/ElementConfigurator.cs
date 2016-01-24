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
                    }
                }
            }

            RoutingControllerCenter routingControllCenter = new RoutingControllerCenter(servicePort);
            return routingControllCenter;
        }
    }
}
