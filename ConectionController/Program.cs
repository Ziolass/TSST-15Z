using CcConfig;
using System;

namespace Cc
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            String id = args[0];
            ElementConfigurator configurator = new ElementConfigurator("..\\..\\..\\Configs\\CC\\ccConfig" + id + ".xml");
            ConnectionController cc = configurator.configureController();
            Console.ReadLine();

            Console.ReadLine();
        }
    }
}