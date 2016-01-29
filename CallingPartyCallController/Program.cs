using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallingPartyCallController
{
    class Program
    {
        static void Main(string[] args)
        {
            string id = "0";
            if (args.Length > 0)
            {
                id = args[0];
            }
            Console.WriteLine("╔═╗     CPCC     ╔═╗");
            Console.WriteLine("╚═╝              ╚═╝");
            Console.WriteLine("CPCC starting...");
            CallingPartyCallController ncc = new CallingPartyCallController(id);
            while (true)
            {
                string a = Console.ReadLine().ToString();

                switch (a)
                {
                    case "a":
                        Console.WriteLine(ncc.callRequest("jan1", "jan2"));
                        break;
                    case "b":
                        Console.WriteLine(ncc.callRequest("jan1", "monika"));
                        break;
                    case "c":
                        Console.WriteLine(ncc.callRequest("jan2", "jan1"));


                        break;
                }

            }
        }
    }
}
