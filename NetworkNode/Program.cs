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
            Frame frame = new Frame(StmLevel.STM256);
            frame.Msoh = new Header("test", "test2", "test");
            frame.Rsoh = new Header("test", "test2", "test");

            frame.SetVirtualContainer(VirtualContainerLevel.VC32, 0, new VirtualContainer(VirtualContainerLevel.VC32));
            frame.SetVirtualContainer(VirtualContainerLevel.VC21, 10, new VirtualContainer(VirtualContainerLevel.VC21));


            FrameBuilder fmb = new SDHFrame.FrameBuilder();
            string var = fmb.BuildLiteral(frame);

            
            if (args.Length == 0)
            {
                Console.WriteLine("Input parameter missing");
            }
            else
            {
                String id = args[0];
                NetworkNodeSetupProcess setUpProcess = new NetworkNodeSetupProcess("../../../Configs/nodeConfig" + id + ".xml");

                //Add forwarding records
                ForwardingRecord record = new ForwardingRecord(4000, 6000, StmLevel.STM1, VirtualContainerLevel.VC32, 0, 1);
                List<ForwardingRecord> records = new List<ForwardingRecord>();
                records.Add(record);
                NetworkNode node = setUpProcess.startNodeProcess();
                node.AddForwardingRecords(records);

                Console.WriteLine("Start emulation");
            }
        }

    }
}