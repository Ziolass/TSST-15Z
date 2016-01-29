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
            String id = "0";//args[0];
            ElementConfigurator configurator = new ElementConfigurator("..\\..\\..\\Configs\\CC\\ccConfig" + id + ".xml");
            ConnectionController cc = configurator.configureController();
            Console.ReadLine();
            cc.HandleNccData("connection-request|node1:2|node3:2");
            Console.ReadLine();
        }
    }
}
