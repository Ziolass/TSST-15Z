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

            /*Frame frame = new Frame();

            VirtualContainer vcTest = new VirtualContainer(VirtualContainerLevel.VC4);
            vcTest.SetContent("TEST");
            //frame.SetVirtualContainer(VirtualContainerLevel.VC4, 0, vcTest);
            //frame.SetVirtualContainer(VirtualContainerLevel.VC4, 0, null, new VirtualContainer(VirtualContainerLevel.VC4, new Container("test")));

                frame.SetVirtualContainer(VirtualContainerLevel.VC32, 0, 0, new VirtualContainer(VirtualContainerLevel.VC32, new Container("test")));
                frame.SetVirtualContainer(VirtualContainerLevel.VC32, 0, 1, new VirtualContainer(VirtualContainerLevel.VC32, new Container("test2")));
                frame.SetVirtualContainer(VirtualContainerLevel.VC32, 0, 1, new VirtualContainer(VirtualContainerLevel.VC32, new Container("mnie nie ma")));

                //frame.SetVirtualContainer(VirtualContainerLevel.VC12, 42, new VirtualContainer(VirtualContainerLevel.VC12, new Container("test3 ja sie nie mieszcze")));
                //frame.SetVirtualContainer(VirtualContainerLevel.VC21, 15, new VirtualContainer(VirtualContainerLevel.VC21, new Container("testVC21 ja sie nie mieszcze")));

                var test = frame.GetVirtualContainer(VirtualContainerLevel.VC32, 0, 1);
                test = frame.GetVirtualContainer(VirtualContainerLevel.VC32, 1);

            FrameBuilder fmb = new FrameBuilder();
            var literal = fmb.BuildLiteral(frame);

            frame = (Frame)fmb.BuildFrame(literal);

            frame.ToString();

            Console.ReadLine();

            /*
            if (args.Length == 0)
            {
                Console.WriteLine("Input parameter missing");
            }*/

			String id = "0";//args[0];
            //String id = "1";
            NetworkNodeSetupProcess setUpProcess = new NetworkNodeSetupProcess("..\\..\\..\\Configs\\NetworkNode\\nodeConfig" + id + ".xml");


            NetworkNode node = setUpProcess.startNodeProcess();
            //ForwardingRecord record = new ForwardingRecord(1, 2, StmLevel.STM1, VirtualContainerLevel.VC32, 0, 1);
            //List<ForwardingRecord> records = new List<ForwardingRecord>();
            //records.Add(record);
            //node.AddForwardingRecords(records);
            Console.WriteLine("Start emulation");
            startReadingCommands(node);
            //node.emulateManagement("sub-conection-HPC|1002-2003#|");
            
        }

        private static void startReadingCommands(NetworkNode node)
        {
            while (true)
            {
                Console.WriteLine("Oczekiwanie na polecenie");
                Console.WriteLine("R - kana� rozm�wny rsoh");
                Console.WriteLine("M - kana� rozm�wny msoh");

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
            Console.WriteLine("Wprowad� dane");
            return Console.ReadLine();
        }
    }

}