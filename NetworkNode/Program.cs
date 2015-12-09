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
            Frame.Frame frame = new Frame.Frame();
            frame.Msoh = new Header("test", "test2");
            frame.Rsoh = new Header("test", "test2");
            frame.SetVirtualContainer(ContainerLevel.TUG12, 1, new VirtualContainer(Frame.VirtualContainerLevel.VC12));
            frame.SetVirtualContainer(ContainerLevel.TUG2, 0, new VirtualContainer(Frame.VirtualContainerLevel.VC2));
            frame.SetVirtualContainer(ContainerLevel.TUG2, 1, new VirtualContainer(Frame.VirtualContainerLevel.VC2));
            frame.SetVirtualContainer(ContainerLevel.TUG3, 1, new VirtualContainer(Frame.VirtualContainerLevel.VC3));

            FrameBuilder fmb = new Frame.FrameBuilder();
            string var = fmb.BuildLiteral(frame);

            frame = (Frame.Frame)fmb.BuildFrame(var);

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
