using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCallController
{
    class Program
    {
        static void Main(string[] args)
        {
            string id = "0";
            Console.WriteLine("╔═╗     NCC     ╔═╗");
            Console.WriteLine("╚═╝             ╚═╝");
            Console.WriteLine("NCC starting...");
            if (args.Length > 0)
            {
                id = args[0];
            }
            NetworkCallController ncc = new NetworkCallController(id);
            
            //Console.Read();
        }
    }
}
