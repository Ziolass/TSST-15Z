using NetworkNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cc
{
    class Program
    {
        static void Main(string[] args)
        {
            String id = args[0];
            ElementConfigurator configurator = new ElementConfigurator("..\\..\\..\\Configs\\CC\\ccConfig" + id + ".xml");
            ConnectionController cc = configurator.configureController();
            Console.WriteLine("Start emulation");
        }
    }
}
