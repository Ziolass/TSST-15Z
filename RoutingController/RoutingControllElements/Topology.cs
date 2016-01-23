using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using RoutingController.Interfaces;

namespace RoutingController.RoutingControllElements
{
    public class Topology : ITopology
    {
        public int NodeCount { get; set; }
        public List<INode> NodeList { get; set; }

        public int NetworkLevel { get; set; }
        public int NetworkId { get; set; }
        public List<ILink> LinkList { get; set; }

        public Topology()
        {
            this.LinkList = new List<ILink>();
            this.NodeList = new List<INode>();
        }

        /// <summary>
        /// Reads from XML.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static Topology ReadFromXML(string path){
            try
            {
                Topology returnTopology = new Topology();
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Topology));                
                FileStream myFileStream = new FileStream(ConvertPath(path), FileMode.Open);
                returnTopology = (Topology)xmlSerializer.Deserialize(myFileStream);
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
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Topology));
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
