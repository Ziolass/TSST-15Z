using RoutingController.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RoutingController.Elements
{
    public class SNPP : ISNPP
    {
        public List<SNP> Nodes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SNPP" /> class.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="port">The port.</param>
        [JsonConstructor]
        public SNPP(List<SNP> nodes)
        {
            this.Nodes = new List<SNP>(nodes);
        }

        public SNPP(List<string> nodes)
        {
            this.Nodes = new List<SNP>();
            for (int i = 0; i < nodes.Count; i+=2)
            {
                if (i + 1 < nodes.Count)
                {
                    NodeElement firstNode = new NodeElement(nodes[i]);
                    NodeElement secondNode = new NodeElement(nodes[i + 1]);
                    List<string> portList = new List<string>();
                    portList.Add(firstNode.Port);
                    portList.Add(secondNode.Port);
                    this.Nodes.Add(new SNP(firstNode.Node, portList));
                }
            }
            /*foreach (string firstNodeId in nodes)
            {
                Node firstNode = new Node(firstNodeId);
                Node secondNode = null;
                foreach (string secondNodeId in nodes)
                {
                    secondNode = new Node(secondNodeId);
                    if (firstNode.Name == secondNode.Name && firstNode.Port != secondNode.Port)
                    {
                        List<string> portList = new List<string>();
                        portList.Add(firstNode.Port);
                        portList.Add(secondNode.Port);
                        this.Nodes.Add(new SNP(firstNode.Name, portList));
                    }
                }
            }*/
        }

        public override string ToString()
        {
            string returnString = string.Empty;
            foreach (var item in Nodes)
            {
                returnString += item.ToString();
            }
            return returnString;
        }

    }
}