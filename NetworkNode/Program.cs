using NetworkNode.SDHFrame;
using System;

namespace NetworkNode
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Frame frameTest = new Frame(StmLevel.STM4);

            frameTest.SetVirtualContainer(VirtualContainerLevel.VC32, 0, 1, new VirtualContainer(VirtualContainerLevel.VC32, new Container("TEST")));

            var response = frameTest.IsFrameOccupied(VirtualContainerLevel.VC32);
            //index 0, 0
            var response2 = frameTest.SetNextAvalible(VirtualContainerLevel.VC32, new VirtualContainer(VirtualContainerLevel.VC32, new Container("TEST2")));
            //index 0, 2
            response2 = frameTest.SetNextAvalible(VirtualContainerLevel.VC32, new VirtualContainer(VirtualContainerLevel.VC32, new Container("TEST2")));
            //Doda do drugiego VC4 czyli indexy 1, 0
            response2 = frameTest.SetNextAvalible(VirtualContainerLevel.VC32, new VirtualContainer(VirtualContainerLevel.VC32, new Container("TEST2")));
            //Że jest to VC4 to zwroci 2, null;
            response2 = frameTest.SetNextAvalible(VirtualContainerLevel.VC4, new VirtualContainer(VirtualContainerLevel.VC4, new Container("TEST2")));

            String id = args[0];
            NetworkNodeSetupProcess setUpProcess = new NetworkNodeSetupProcess("..\\..\\..\\Configs\\NetworkNode\\nodeConfig" + id + ".xml");
            NetworkNode node = setUpProcess.startNodeProcess();
            Console.WriteLine("Start emulation");
            startReadingCommands(node);
        }

        private static void startReadingCommands(NetworkNode node)
        {
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
            Console.WriteLine("Wprowad� dane");
            return Console.ReadLine();
        }
    }
}