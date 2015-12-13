using NetworkNode.SDHFrame;
using NetworkNode.HPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    class Program
    {
        static void Main(string[] args)
        {
            String id = args[0];
            if (args.Length == 0)
            {
                Console.WriteLine("Input parameter missing");
            }
            NetworkNodeSetupProcess setUpProcess = new NetworkNodeSetupProcess("nodeConfig" + id + ".xml");
            NetworkNode node = setUpProcess.startNodeProcess();
            Console.WriteLine("Start emulation");
        }
    }
}