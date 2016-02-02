using RoutingController.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Requests
{
    public class Ends
    {
        public string OuterDomain {get; private set;}
        public string Node { get; set; }
        public string Port {get; private set;}

        public Ends(string outerDomain, string node, string port)
        {
            this.OuterDomain = outerDomain;
            this.Node = node;
            this.Port = port;

        }
    }
    public class RouteResponse
    {
        public string Protocol { get; private set; }
        public List<Ends> Ends { get; set; }
        public List<SNP> Steps { get; set; }

        public RouteResponse(List<Ends> ends, List<SNP> steps)
        {
            this.Protocol = "route";
            this.Ends = new List<Ends>(ends);
            this.Steps = new List<SNP>(steps);
        }
        public RouteResponse()
        {
            this.Protocol = "route";
            this.Ends = new List<Ends>();
            this.Steps = new List<SNP>();
        }

        public void AddNodes(List<NodeElement> nodes)
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
        }
    }
}
