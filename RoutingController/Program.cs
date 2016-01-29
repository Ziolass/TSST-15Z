using RoutingController.Interfaces;
using RoutingController.Elements;
using System;
using System.Collections.Generic;
using RoutingController.Service;
using System.Threading;

namespace RoutingController
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                ElementConfigurator configurator = new ElementConfigurator("../../../Configs/RoutingController/routingControllerConfig.xml");
                RoutingControllerCenter RC = configurator.ConfigureRoutingController();

                Console.WriteLine("Routing Controller Console");
                Console.WriteLine();

                new Thread(delegate()
                {
                    RC.StartListening();
                }).Start();
                new Thread(delegate()
                {
                    startReadingCommands(RC);
                }).Start();
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }
        }
        private static void startReadingCommands(RoutingControllerCenter rc)
        {
            while (true)
            {

                string command = Console.ReadLine();

                if (command.Equals("show"))
                {
                    Console.WriteLine(rc.ShowRoutes());
                }
                else if (command.Equals("clear"))
                {
                    Console.WriteLine(rc.ClearRoutes());
                }
                else
                {
                    Console.WriteLine("Nie odnaleziono polecenia");
                    continue;
                }
            }
        }
    }
}