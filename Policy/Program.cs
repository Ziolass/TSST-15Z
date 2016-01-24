using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Policy
{
    class Program
    {
        static void Main(string[] args)
        {
            string id = "0";
            Console.WriteLine("╒════════╕");
            Console.WriteLine("│ POLICY │");
            Console.WriteLine("╘════════╛");
            Console.Out.WriteLine("Policy starting...");
            if (args.Length > 0)
            {
                id = args[0];
            }
            Policy policy = new Policy(id);
            ConnectionHandler chandler = new ConnectionHandler(policy);
            chandler.StartListening();


        }
    }
}
