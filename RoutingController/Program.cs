using RoutingController.Interfaces;
using RoutingController.Elements;
using System;
using System.Collections.Generic;
using RoutingController.Service;
using System.Threading;

namespace RoutingController
{
    public class Program
    {

        public static void Main(String[] args)
        {
            try
            {
                ElementConfigurator configurator = new ElementConfigurator("..\\..\\..\\Configs\\RoutingController\\routingControllerConfig" + args[0].ToString() + ".xml");
                RoutingControllerCenter RC = configurator.ConfigureRoutingController();

                Console.WriteLine("Routing Controller Console");
                Console.WriteLine("Main domain: {0}", RC.NetworkName);
                Console.WriteLine("==========================\n");

                new Thread(delegate()
                {
                    RC.StartListening();
                }).Start();

                startReadingCommands(RC);

                Console.ReadLine();
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
        private static void startReadingCommands(RoutingControllerCenter rc)
        {
            while (true)
            {

                string command = Console.ReadLine();

                if (command.Equals("show routes"))
                {
                    Console.WriteLine(rc.ShowRoutes());
                }
                else if (command.Equals("show external"))
                {
                    Console.WriteLine(rc.ShowExternalClients());
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