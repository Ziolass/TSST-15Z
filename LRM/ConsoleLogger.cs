using ConectionController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRM
{
    public class ConsoleLogg
    {
        private static void Log(string data)
        {
            Console.WriteLine(data);
        }
        public static void PrintLocaLTopology(Dictionary<string,VirtualNode> register)
        {
            Dictionary<string, List<VirtualNode>> nodesForDomain = new Dictionary<string,List<VirtualNode>>();

            foreach (VirtualNode node in register.Values)
            {
                string domain = node.DomiansHierarchy[0] != null ? node.DomiansHierarchy[0] : "??";
                if (!nodesForDomain.ContainsKey(domain))
                {
                    nodesForDomain.Add(domain, new List<VirtualNode>());
                }
                nodesForDomain[domain].Add(node);
            }

            foreach (string domian in nodesForDomain.Keys)
            {
                Log("LRM at " + domian);
                Log("LOCAL TOPOLOGY ");
                Log(TextUtils.Dash);
                foreach (VirtualNode node in nodesForDomain[domian])
                {
                    Log("Node: " + node.Name );
                    Console.Write("    ports :");
                    foreach (int port in node.Destinations.Keys)
                    {
                        Console.Write(" " + port);
                    }
                    Log("");
                }
            }

            Log("");
        }
    }
}
