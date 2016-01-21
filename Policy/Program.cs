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
            Console.Out.WriteLine("Policy starting...");
            Policy policy = new Policy();
            ConnectionHandler chandler = new ConnectionHandler(policy);
            chandler.StartListening();


        }
    }
}
