﻿using NetworkNode.SDHFrame;
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
            Frame frame = new Frame(STMLevel.STM4);
            frame.Msoh = new Header("test", "test2", "test");
            frame.Rsoh = new Header("test", "test2", "test");
            frame.SetVirtualContainer(VirtualContainerLevel.VC12, 1, new VirtualContainer(SDHFrame.VirtualContainerLevel.VC12));
            frame.SetVirtualContainer(VirtualContainerLevel.VC21, 0, new VirtualContainer(SDHFrame.VirtualContainerLevel.VC21));
            frame.SetVirtualContainer(VirtualContainerLevel.VC21, 1, new VirtualContainer(SDHFrame.VirtualContainerLevel.VC21));
            frame.SetVirtualContainer(VirtualContainerLevel.VC21, 1, new VirtualContainer(SDHFrame.VirtualContainerLevel.VC32));

            FrameBuilder fmb = new SDHFrame.FrameBuilder();
            string var = fmb.BuildLiteral(frame);
            
            frame = (SDHFrame.Frame)fmb.BuildFrame(var);

            String id = "0";
            if (args.Length == 0)
            {
                Console.WriteLine("Input parameter missing");
            }
            NetworkNodeSetupProcess setUpProcess = new NetworkNodeSetupProcess("nodeConfig" + id + ".xml");
            NetworkNode node = setUpProcess.startNodeProcess();

            List<ForwardingRecord> records = new List<ForwardingRecord>();
            records.Add(new ForwardingRecord(4000, 5000, VirtualContainerLevel.VC32, 0, 0));
            records.Add(new ForwardingRecord(4000, 5000, VirtualContainerLevel.VC32, 1, 1));
            records.Add(new ForwardingRecord(4000, 5000, VirtualContainerLevel.VC32, 2, 2));

            foreach (ForwardingRecord record in records)
            {
                node.AddForwardingRecord(record);
            }

            Console.WriteLine("Start emulation");
            //node.emulateManagement("sub-conection-HPC|1002-2003#|");

        }
    }
}
