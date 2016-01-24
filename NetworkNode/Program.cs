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