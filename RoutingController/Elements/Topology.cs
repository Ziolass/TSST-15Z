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
        public List<Link> LinkList { get; private set; }

        public TopologyRequest()
        {
            this.LinkList = new List<Link>();
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