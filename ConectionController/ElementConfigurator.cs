using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Cc;
using Cc.Communication;

namespace NetworkNode
{
    public class ElementConfigurator
    {
        private XmlReader configReader;

        public ElementConfigurator(string cofigFilePath)
        {
            configReader = XmlReader.Create(cofigFilePath);
        }

        public ConnectionController configureController()
        {
            int rcPort = -1;
            int nccPort = -1;
            Dictionary<string, int> lrmPorts = new Dictionary<string,int>();
            Dictionary<string, string> gateways = new Dictionary<string, string>();
            while (configReader.Read())
            {
                if (configReader.IsStartElement())
                {
                    if (configReader.NodeType == XmlNodeType.Element)
                    {
                        if (configReader.Name == "gateway")
                        {
                            string domian = configReader.GetAttribute("domian");
                            string gate = configReader.GetAttribute("gate");
                            gateways.Add(domian, gate);
                        }
                        else if (configReader.Name == "ncc")
                        {
                            nccPort = int.Parse(configReader.GetAttribute("tcp"));
                        }
                        else if (configReader.Name == "rc")
                        {
                            rcPort = int.Parse(configReader.GetAttribute("tcp"));
                        }
                        else if (configReader.Name == "lrm")
                        {
                            string nodeName = configReader.GetAttribute("node");
                            int tcp = int.Parse(configReader.GetAttribute("tcp"));
                            lrmPorts.Add(nodeName, tcp);
                            
                        }
                    }

                }
            }

            ConnectionController cc = new ConnectionController(rcPort,lrmPorts,gateways);
            NccServer nccServ = new NccServer(nccPort);

            nccServ.SetUpServer(10000, 10);
            nccServ.StartListening();

            return cc;
        }

    }
}
