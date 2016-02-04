using Cc;
using System.Collections.Generic;
using System.Xml;

namespace CcConfig
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
            int lrmPort = -1;
            int peerCoordinationPort = -1;
            int? notifierPort = null;
            List<string> domians = null;

            Dictionary<string, int> subnetworksCc = new Dictionary<string, int>();
            Dictionary<string, int> peers = new Dictionary<string, int>();

            string domian = null;

            while (configReader.Read())
            {
                if (configReader.IsStartElement())
                {
                    if (configReader.NodeType == XmlNodeType.Element)
                    {
                        if (configReader.Name == "cc")
                        {
                            peerCoordinationPort = int.Parse(configReader.GetAttribute("peer-coordination"));
                            nccPort = int.Parse(configReader.GetAttribute("ncc-tcp"));
                            rcPort = int.Parse(configReader.GetAttribute("rc-tcp"));
                            lrmPort = int.Parse(configReader.GetAttribute("lrm-tcp"));
                            string notifier = configReader.GetAttribute("notifier");
                            notifierPort = notifier == "" ? null : (int?)int.Parse(notifier);
                        }
                        else if (configReader.Name == "domians")
                        {
                            int domiansNumber = int.Parse(configReader.GetAttribute("number"));
                            domians = CreateDomainsHierarchy(configReader.ReadSubtree(), domiansNumber);
                        }
                        else if (configReader.Name == "sub-networks")
                        {
                            subnetworksCc = CreateSubnetworkCc(configReader.ReadSubtree(), "sub-network");
                        }
                        else if (configReader.Name == "peers")
                        {
                            peers = CreateSubnetworkCc(configReader.ReadSubtree(), "peer");
                        }
                    }
                }
            }

            ConnectionController cc = new ConnectionController(domian,
                rcPort,
                subnetworksCc,
                peers,
                peerCoordinationPort,
                nccPort,
                lrmPort,
                notifierPort,
                domians);

            cc.Start();

            return cc;
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

        private Dictionary<string, int> CreateSubnetworkCc(XmlReader reader, string tag)
        {
            Dictionary<string, int> subnetworkCc = new Dictionary<string, int>();
            while (reader.Read())
            {
                if (reader.Name.Equals(tag))
                {
                    string domian = reader.GetAttribute("domian");
                    int port = int.Parse(reader.GetAttribute("tcp"));
                    subnetworkCc.Add(domian, port);
                }
            }
            return subnetworkCc;
        }
    }
}