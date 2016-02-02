using Newtonsoft.Json;
using RoutingController.Elements;
using RoutingController.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RoutingController.Requests
{
    public class TopologyNode : ITopologyNode
    {
        public string Node { get; private set; }
        public List<string> Domains { get; private set; }
        public List<Link> Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopologyNode"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="linkList">The link list.</param>
        [JsonConstructor]
        public TopologyNode(string node, List<string> domains, List<Link> data)
        {
            this.Node = node;
            this.Domains = new List<string>(domains);
            this.Data = new List<Link>(data);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TopologyNode"/> class.
        /// </summary>
        public TopologyNode()
        {
            this.Domains = new List<string>();
            this.Data = new List<Link>();
        }

        /// <summary>
        /// Gets all domains.
        /// </summary>
        /// <returns></returns>
        public List<string> GetDomains()
        {
            List<string> returnList = new List<string>();
            if (this.Domains != null)
            {
                foreach (string domainName in this.Domains)
                {
                    returnList.Add(domainName);
                }
            }
            return returnList;
        }

        public override string ToString()
        {
            string returnString = string.Empty;
            returnString += this.Node + ", [";
            this.Domains.ForEach(x => returnString += x.ToString() + ", ");
            returnString = returnString.Remove(returnString.LastIndexOf(','));
            returnString += "], [";
            this.Domains.ForEach(x => returnString += x.ToString() + ", ");
            returnString = returnString.Remove(returnString.LastIndexOf(','));
            returnString += "]";
            return returnString;
        }
    }
}