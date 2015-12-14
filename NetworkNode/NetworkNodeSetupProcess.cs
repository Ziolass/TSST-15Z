using NetworkNode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkNode
{
    class NetworkNodeSetupProcess
    {
        private String configurationFilePath;

        public NetworkNodeSetupProcess(string configurationFileName)
        {
            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            StringBuilder sb = new StringBuilder();
            sb.Append(defaultDirectoryPath);
            sb.Append("\\");
            sb.Append(configurationFileName);

            this.configurationFilePath = sb.ToString();
        } 

        public NetworkNode  startNodeProcess () {
            if (!File.Exists(configurationFilePath))
            {
                Console.WriteLine("Check your configuration file ");
                Console.WriteLine("your default config file path is {0} ", configurationFilePath);
                return null;
            }
            ElementConfigurator configManager = new ElementConfigurator(configurationFilePath);
            return configManager.configureNode();
        }
    }
}
