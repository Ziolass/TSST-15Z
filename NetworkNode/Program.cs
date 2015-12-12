using NetworkNode.Frame;
using NetworkNode.HPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkNode.Frame;

namespace NetworkNode
{
    class Program
    {
        static void Main(string[] args)
        {
           

            String id = "0";
            if (args.Length == 0)
            {
                Console.WriteLine("Input parameter missing");
            }
            NetworkNodeSetupProcess setUpProcess = new NetworkNodeSetupProcess("nodeConfig" + id + ".xml");
            NetworkNode node = setUpProcess.startNodeProcess();

            Console.WriteLine("Start emulation");
            //node.emulateManagement("sub-conection-HPC|1002-2003#|");

        }
    }
}
