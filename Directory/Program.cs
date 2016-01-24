using Directory.FileUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Directory
{
    class Program
    {

        static void Main(string[] args)
        {
            string id = args[0];
            if (args.Length > 0)
            {
                id = args[0];
            }
            Directory directory = new Directory();
            Console.WriteLine("Start Directory...");
            directory.setUp(id);
            ConnectionHandler chandler = new ConnectionHandler(directory);
            chandler.StartListening();
            
        }

        
    }
}
