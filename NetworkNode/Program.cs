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
            ForwardingRecord record = new ForwardingRecord(4000, 6000, StmLevel.STM1, VirtualContainerLevel.VC32, 0, 1);
            List<ForwardingRecord> records = new List<ForwardingRecord>();
            records.Add(record);
            node.AddForwardingRecords(records);
            Console.WriteLine("Start emulation");
            startReadingCommands(node);
            //node.emulateManagement("sub-conection-HPC|1002-2003#|");

        }

        private static void startReadingCommands(NetworkNode node) {
            while (true) 
            {
                Console.WriteLine("Oczekiwanie na polecenie");
                Console.WriteLine("R - kanał rozmówny rsoh");
                Console.WriteLine("M - kanał rozmówny msoh");

                string command = Console.ReadLine();
                
                if (command.Equals("R"))
                {
                    string args = takeArgs();
                    node.AddRsohContent(args);

                } 
                else if (command.Equals("M"))
                {
                    string args = takeArgs();
                    node.AddMsohContent(args);
                }
                else
                {
                    Console.WriteLine("Nie odnaleziono polecenia");
                    continue;
                }
            }
        }

        private static string takeArgs()
        {
            Console.WriteLine("Wprowadź dane");
            return Console.ReadLine();
        }
    }
}
