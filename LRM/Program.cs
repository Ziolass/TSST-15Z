using NetworkNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRM
{
    class Program
    {
        static void Main(string[] args)
        {
            String id = args[0];
            ElementConfigurator configurator = new ElementConfigurator("..\\..\\..\\Configs\\LRM\\lrmConfig" + id + ".xml");
            LinkResourceManager lrm = configurator.configureNode();
            Console.ReadLine();
            //cc.HandleNccData("connection-request|node1:2|node3:2");
            Console.ReadLine();
        }
    }
}
