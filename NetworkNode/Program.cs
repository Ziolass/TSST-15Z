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

            FrameBuilder fmb = new Frame.FrameBuilder();

            Frame.Frame frame = new Frame.Frame();
            VirtualContainer newVC = new Frame.VirtualContainer(Frame.VirtualContainerLevel.VC3);
            newVC.Content = new Frame.Container("test");
            frame.SetVirtualContainer(ContainerLevel.TUG3, 0, newVC);
            frame.SetVirtualContainer(ContainerLevel.TUG3, 1, newVC);
            frame.SetVirtualContainer(ContainerLevel.TUG3, 2, newVC);

            String var = fmb.BuildLiteral(frame);
            //

            String id = "0";
            if (args.Length == 0)
            {
                Console.WriteLine("Input parameter missing");

            }
            NetworkNodeSetupProcess setUpProcess = new NetworkNodeSetupProcess("nodeConfig" + id + ".xml");
            NetworkNode node = setUpProcess.startNodeProcess();



            List<ForwardingRecord> records = new List<ForwardingRecord>();
            records.Add(new ForwardingRecord(4000, 5000, ContainerLevel.TUG3, 0, 0));
            records.Add(new ForwardingRecord(4000, 5000, ContainerLevel.TUG3, 1, 1));
            records.Add(new ForwardingRecord(4000, 5000, ContainerLevel.TUG3, 2, 2));

            foreach (ForwardingRecord record in records)
            {
                node.AddForwardingRecord(record);
            }

            Console.WriteLine("Start emulation");
            //node.emulateManagement("sub-conection-HPC|1002-2003#|");

        }
    }
}
