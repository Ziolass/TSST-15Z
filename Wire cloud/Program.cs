using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WireCloud.Test;

namespace WireCloud
{
    class Program
    {
        static void Main(string[] args)
        {
            CloudSetupProcess setupProcess = new CloudSetupProcess("configFile.xml");
            setupProcess.StartCloudProcess();

            Console.Write("Naciśnij aby rozpocząć test");
            Console.ReadLine();
            //TODO: przygotowac modół testów
            testCloud();
        }

        private static void testCloud()
        {
            IThreadRepresentable testingServer = new TestServer(5000);
            IThreadRepresentable testingClient = new TestClient(4000);
            testingServer.GetThread().Start();
            Console.Write("Czekam na inicjalizację");
            Thread.Sleep(5000);
            Console.Write("Po inicjalizacji");
            testingClient.GetThread().Start();
            
        }
    }
}
