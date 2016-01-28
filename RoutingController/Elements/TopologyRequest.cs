using Newtonsoft.Json;
using RoutingController.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RoutingController.Elements
{
    public class TopologyRequest : ITopology
    {
        public string Node { get; private set; }
        public List<ILink> Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopologyRequest"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="linkList">The link list.</param>
        [JsonConstructor]
        public TopologyRequest(string node, List<ILink> linkList)
        {
            this.Node = node;
            this.Data = linkList;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TopologyRequest"/> class.
        /// </summary>
        public TopologyRequest()
        {
            this.Data = new List<ILink>();
        }

        /// <summary>
        /// Gets all domains.
        /// </summary>
        /// <returns></returns>
        public List<string> GetDomains()
        {
            List<string> returnList = new List<string>();
            foreach (Link link in Data)
            {
                foreach (string linkDomain in link.Domains)
                {
                    if (!returnList.Contains(linkDomain))
                        returnList.Add(linkDomain);
                    else continue;
                }
            }
            return returnList;
        }

        /// <summary>
        /// Reads from XML.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static TopologyRequest ReadFromXML(string path)
        {
            try
            {
                TopologyRequest returnTopology = new TopologyRequest();
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TopologyRequest));
                FileStream myFileStream = new FileStream(ConvertPath(path), FileMode.Open);
                returnTopology = (TopologyRequest)xmlSerializer.Deserialize(myFileStream);
                return returnTopology;
            }
            catch (Exception exp)
            {
                Console.WriteLine("Error Topology.SaveToXML: " + exp.Message);
                return null;
            }
        }

        /// <summary>
        /// Saves to XML.
        /// </summary>
        /// <param name="path">The path.</param>
        private void SaveToXML(string path)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TopologyRequest));
                StringWriter stringWriter = new StringWriter();
                XmlWriter xmlWriter = XmlWriter.Create(stringWriter);
                xmlSerializer.Serialize(xmlWriter, this);
                String xml = stringWriter.ToString();
                File.WriteAllText(ConvertPath(path), xml);
            }
            catch (Exception exp)
            {
                Console.WriteLine("Error Topology.SaveToXML: " + exp.Message);
            }
        }

        /// <summary>
        /// Converts the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static String ConvertPath(String path)
        {
            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            StringBuilder sb = new StringBuilder();
            sb.Append(defaultDirectoryPath);
            sb.Append("\\");
            sb.Append(path);
            return sb.ToString();
        }
    }
}