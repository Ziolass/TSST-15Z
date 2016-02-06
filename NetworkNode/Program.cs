using NetworkNode.SDHFrame;
using System;
using System.Threading;

namespace NetworkNode
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            String id = args[0];
            NetworkNodeSetupProcess setUpProcess = new NetworkNodeSetupProcess("..\\..\\..\\Configs\\NetworkNode\\nodeConfig" + id + ".xml");

            Console.WriteLine("Start emulation");
            NetworkNode node = setUpProcess.startNodeProcess();
            new Thread(delegate() { startReadingCommands(node); });
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