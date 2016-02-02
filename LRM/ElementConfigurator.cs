using LRM;
using NetworkNode.LRM;
using NetworkNode.LRM.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace NetworkNode
{
    public class ElementConfigurator
    {
        private XmlReader configReader;

        public ElementConfigurator(string cofigFilePath)
        {
            configReader = XmlReader.Create(cofigFilePath);
        }

        public LinkResourceManager configureNode()
        {
            string highestDomian = null;
            int lrmServerPort = 0;
            int rcPort = 0;
            int ccPort = 0;
            while (configReader.Read())
            {
                if (configReader.IsStartElement())
                {
                    if (configReader.NodeType == XmlNodeType.Element)
                    {
                        if (configReader.Name == "lrm")
                        {
                            lrmServerPort = int.Parse(configReader.GetAttribute("server-tcp"));
                            rcPort = int.Parse(configReader.GetAttribute("rc-tcp"));
                            ccPort = int.Parse(configReader.GetAttribute("cc-tcp"));
                            highestDomian = configReader.GetAttribute("domian-scope"); 
                        }
                    }

                }
            }

            LinkResourceManager lrm = new LinkResourceManager(lrmServerPort, rcPort, ccPort, highestDomian);
            lrm.RunServer();
            return lrm;
        }
    }
}
