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
        public string Id { get; set; }
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
            for (int i = 0; i < nodes.Count; i++)
            {
                if (i + 1 < nodes.Count)
                {
                    NodeElement firstNode = nodes[i];
                    NodeElement secondNode = nodes[i + 1];
                    List<string> portList = new List<string>();

                    SNP newSNP = null;

                    if (firstNode.Name == secondNode.Name)
                    {
                        if (firstNode.Scope != null)
                        {
                            firstNode = new NodeElement(firstNode.Scope);
                            portList.Add(firstNode.Port);
                            portList.Add(null);
                            newSNP = new SNP(firstNode.Name, nodes[i].Name, portList);
                            this.Steps.Add(newSNP);

                            portList = new List<string>();
                            secondNode = new NodeElement(secondNode.Scope);
                            portList.Add(secondNode.Port);
                            portList.Add(null);
                            newSNP = new SNP(secondNode.Name, nodes[i].Name, portList);
                            this.Steps.Add(newSNP);
                        }
                        else
                        {
                            portList.Add(firstNode.Port);
                            portList.Add(secondNode.Port);
                            newSNP = new SNP(firstNode.Name, null, portList);
                            this.Steps.Add(newSNP);
                        }
                        i++;
                    }
                    else
                    {
                        portList = new List<string>();
                        portList.Add(firstNode.Port);
                        portList.Add(null);
                        this.Steps.Add(new SNP(firstNode.Name, null, portList));
                    }
                }
                else
                {
                    NodeElement firstNode = nodes[i];
                    List<string> portList = new List<string>();
                    portList.Add(firstNode.Port);
                    portList.Add(null);
                    this.Steps.Add(new SNP(firstNode.Name, null, portList));
                }
            }
        }
        public override string ToString()
        {
            string returnString = string.Empty;
            returnString += "------------------- \n";
            returnString += "Ends\n";
            returnString += "------------------- \n";
            foreach (var item in this.Ends)
            {
                if (String.IsNullOrEmpty(item.OuterDomain))
                {
                    returnString += "Node: " + item.Node + " : " + item.Port + "\n";
                }
                else returnString += "Node: " + item.Node + " : " + item.Port + " domain: " + item.OuterDomain + "\n";
            }
            returnString += "------------------- \n";
            returnString += "Steps \n";
            returnString += "------------------- \n";
            foreach (var item in this.Steps)
            {
                if (String.IsNullOrEmpty(item.Domain))
                {
                    returnString += "Node: " + item.Node + "\n";
                }
                else returnString += "Node: " + item.Node + " domain: " + item.Domain + "\n";
                returnString += "Ports: ";
                foreach (var itemPorts in item.Ports)
                {
                    returnString += itemPorts + " ";
                }
                returnString += "\n";
            }
            returnString += "------------------- \n";
            return returnString;
        }
    }
}
