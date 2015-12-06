using NetworkNode.Frame;
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
            String id = "0";
            if (args.Length == 0)
            {
                Console.WriteLine("Input parameter missing");

            }
            NetworkNodeSetupProcess setUpProcess = new NetworkNodeSetupProcess("nodeConfig" + id + ".xml");
            NetworkNode node = setUpProcess.startNodeProcess();


         
            List<ForwardingRecord> records = new List<ForwardingRecord>();
            records.Add(new ForwardingRecord(4000, 5000, ContainerLevel.TUG3, 1, 1));
            records.Add(new ForwardingRecord(4000, 5000, ContainerLevel.TUG3, 2, 2));
            records.Add(new ForwardingRecord(4000, 5000, ContainerLevel.TUG3, 3, 3));
            
            foreach (ForwardingRecord record in records)
            {
                node.AddForwardingRecord(record);
            }

            Console.WriteLine("Start emulation");
            //node.emulateManagement("sub-conection-HPC|1002-2003#|");
        }
    }
}
