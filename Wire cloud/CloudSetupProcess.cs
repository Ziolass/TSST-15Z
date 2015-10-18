using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WireCloud
{
    class CloudSetupProcess
    {
        private String configurationFilePath;

        private string setupDirectoryPath(string path, string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(path);
            sb.Append("\\");
            sb.Append(file);
            return sb.ToString();
        }
        public CloudSetupProcess(string configurationFileName)
        {
            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            this.configurationFilePath = setupDirectoryPath(defaultDirectoryPath,configurationFileName);
        }

        public CloudSetupProcess(string configurationFileName, string directoryPath)
        {
            this.configurationFilePath = setupDirectoryPath(directoryPath, configurationFileName);
        }  

        public void  StartCloudProcess () {
            if (!File.Exists(configurationFilePath))
            {
                Console.WriteLine("Check your configuration file ");
                Console.WriteLine("your default config file path is {0} ", configurationFilePath);
                return;
            }
            Manager configManager = new Manager(configurationFilePath);
            List<Link> links = configManager.createLinks();
            List<Thread> threadList = wrapInThreads(links);
            foreach (Thread thread in threadList)
            {
                thread.Start();
            }
        }
        private List<Thread> wrapInThreads(List<Link> links)
        {
            List<Thread> threadList = new List<Thread>();
            foreach (Link link in links)
            {
                Thread linkThread = new Thread(new ThreadStart(link.startListening));
                threadList.Add(linkThread);
            }
            return threadList;
        }
     
    }
}
