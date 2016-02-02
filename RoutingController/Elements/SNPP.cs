using RoutingController.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RoutingController.Elements
{
    public class SNPP : ISNPP
    {
        public List<SNP> Steps { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SNPP" /> class.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="port">The port.</param>
        [JsonConstructor]
        public SNPP(List<SNP> nodes)
        {
            this.Steps = new List<SNP>(nodes);
        }

        public SNPP(List<NodeElement> nodes)
        {
            this.Steps = new List<SNP>();
            for (int i = 0; i < nodes.Count; i += 2)
            {

                if (i + 1 < nodes.Count)
                {
                    NodeElement firstNode = nodes[i];
                    NodeElement secondNode = nodes[i + 1];
                    List<string> portList = new List<string>();

                    SNP newSNP = null;
                    if (firstNode.Scope != null)
                    {
                        firstNode = new NodeElement(firstNode.Scope);
                        portList.Add(firstNode.Port);
                        portList.Add(null);
                        newSNP = new SNP(firstNode.Node, nodes[i].Node, portList);
                        this.Steps.Add(newSNP);

                        portList = new List<string>();
                        secondNode = new NodeElement(secondNode.Scope);
                        portList.Add(secondNode.Port);
                        portList.Add(null);
                        newSNP = new SNP(secondNode.Node, nodes[i].Node, portList);
                        this.Steps.Add(newSNP);
                    }
                    else
                    {
                        portList.Add(firstNode.Port);
                        portList.Add(secondNode.Port);
                        newSNP = new SNP(firstNode.Node, null, portList);
                        this.Steps.Add(newSNP);
                    }
                }
                else
                {
                    NodeElement firstNode = nodes[i];
                    List<string> portList = new List<string>();
                    portList.Add(firstNode.Port);
                    portList.Add(null);
                    this.Steps.Add(new SNP(firstNode.Node, null, portList));
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
        public void AddRange(List<string> nodes)
        {
            this.Steps = new List<SNP>();
            for (int i = 0; i < nodes.Count; i += 2)
            {
                if (i + 1 < nodes.Count)
                {
                    NodeElement firstNode = new NodeElement(nodes[i]);
                    NodeElement secondNode = new NodeElement(nodes[i + 1]);
                    List<string> portList = new List<string>();
                    portList.Add(firstNode.Port);
                    portList.Add(secondNode.Port);
                    this.Steps.Add(new SNP(firstNode.Node, null, portList));
                }
            }
        }

        public override string ToString()
        {
            string returnString = string.Empty;
            foreach (var item in Steps)
            {
                returnString += item.ToString();
            }
            return returnString;
        }

    }
}