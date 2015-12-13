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
            Frame frame = new Frame(StmLevel.STM4);
            frame.Msoh = new Header("test", "test2", "test");
            frame.Rsoh = new Header("test", "test2", "test");
           

            frame.SetVirtualContainer(VirtualContainerLevel.VC4, 0, new VirtualContainer(VirtualContainerLevel.VC4));
            frame.SetVirtualContainer(VirtualContainerLevel.VC4, 1, new VirtualContainer(VirtualContainerLevel.VC4));
            frame.SetVirtualContainer(VirtualContainerLevel.VC4, 2, new VirtualContainer(VirtualContainerLevel.VC4));
            frame.SetVirtualContainer(VirtualContainerLevel.VC32, 9, new VirtualContainer(VirtualContainerLevel.VC32));
            frame.SetVirtualContainer(VirtualContainerLevel.VC21, 82, new VirtualContainer(VirtualContainerLevel.VC21));
            frame.SetVirtualContainer(VirtualContainerLevel.VC12, 250, new VirtualContainer(VirtualContainerLevel.VC12));


            FrameBuilder fmb = new SDHFrame.FrameBuilder();
            string var = fmb.BuildLiteral(frame);
            
            frame = (SDHFrame.Frame)fmb.BuildFrame(var);

            Console.WriteLine(frame.ToString());

            String id = "0";
            if (args.Length == 0)
            {
                Console.WriteLine("Input parameter missing");
            }
            NetworkNodeSetupProcess setUpProcess = new NetworkNodeSetupProcess("nodeConfig" + id);
            NetworkNode node = setUpProcess.startNodeProcess();

            Console.WriteLine("Start emulation");
            //node.emulateManagement("sub-conection-HPC|1002-2003#|");

        }
    }
}
