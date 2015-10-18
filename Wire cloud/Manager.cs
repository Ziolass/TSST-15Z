using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WireCloud
{
    class Manager
    {
       private XmlReader configReader;

        public Manager(string cofigFilePath)
        {
            configReader = XmlReader.Create(cofigFilePath);
        }

        public List<Link> createLinks()
        {
            List<Link>  links = new List<Link>();    
            while (configReader.Read())
            {
                    if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "basic-network-link"))
                    {
                        if (configReader.HasAttributes)
                        {
                            Link way1 = new Link(int.Parse(configReader.GetAttribute("src")), int.Parse(configReader.GetAttribute("dst")));
                            links.Add(way1);
                        }
                    }
            }

            return links;
        }
    }
}
