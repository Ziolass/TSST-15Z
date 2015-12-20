using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireCloud
{
    class Program
    {
        static void Main(string[] args)
        {
            ElementConfigurator config = new ElementConfigurator("config.xml");
            ProcessMonitor pm = config.SetUpCloud();
            pm.StartAction();
            Console.WriteLine("ProcessMonitor started");
        }
    }
}
