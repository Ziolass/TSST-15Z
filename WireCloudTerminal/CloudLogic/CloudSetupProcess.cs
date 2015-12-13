using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WireCloud
{
    public delegate void LinksCreatedHandler();
    public delegate void LinkCreatedHandler(LinkCreatedArgs args);
    
    public class LinkCreatedArgs : EventArgs
    {
        public LinkCreatedArgs(Link link)
        {
            Link = link;
        }
        public Link Link { get; private set; }
    } 

    public class CloudSetupProcess
    {
        private String configurationFilePath;
        private Dictionary<Link, Thread> linksThreads;
        private List<int> usedPorts;
        private const int CLOSING_TIME = 1000;
        
        public event LinksCreatedHandler LinksCreated;
        public event LinkCreatedHandler LinkCreated;
        
        public List<Link> Links { get; private set; } 

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
            Links = new List<Link>();
            linksThreads = new Dictionary<Link, Thread>();
            usedPorts = new List<int>();
            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            this.configurationFilePath = setupDirectoryPath(defaultDirectoryPath,configurationFileName);
        }

        public CloudSetupProcess(string configurationFileName, string directoryPath)
        {
            Links = new List<Link>();
            linksThreads = new Dictionary<Link, Thread>();
            usedPorts = new List<int>();
            this.configurationFilePath = setupDirectoryPath(directoryPath, configurationFileName);
        }  

        public void  StartCloudProcess () {
            if (!File.Exists(configurationFilePath))
            {
                throw new Exception("Missing configuration file or wrong directory");
            }
            
            createLinks();

            foreach (Link link in Links)
            {
                wrapInThread(link);
            }
        }

        private void createLinks() {
            Manager configManager = new Manager(configurationFilePath);
            Links = configManager.createLinks();
            if (LinksCreated != null)
            {
                LinksCreated();
            }
        }

        private void wrapInThread(Link link)
        {
            Thread linkThread = new Thread(new ThreadStart(link.StartListening));
            linksThreads.Add(link, linkThread);
            usedPorts.Add(link.Source);
            usedPorts.Add(link.Destination);
            linkThread.Start();
        }

        public void CloseLink(Link link)
        {
            disposeLink(link, CLOSING_TIME);
            usedPorts.Remove(link.Destination);
            usedPorts.Remove(link.Source);
            linksThreads.Remove(link);
        }

        private void disposeLink(Link link, int executionTime)
        {
            if (!Links.Contains(link))
            {
                return;
            }

            link.DestroyLink();
            Thread linkThread = linksThreads[link];
            linkThread.Join(executionTime);
            if (linkThread.IsAlive)
            {
                try
                {
                    linkThread.Abort();
                }
                catch (Exception ex){}
            }
        }

        public void CloseAll()
        {
            foreach (Link link in linksThreads.Keys)
            {
                disposeLink(link, 10);
            }
        }

        public List<int> TryAddLink(int src, int dst)
        {   
            List<int> occupiedPorts = new List<int>();
            validatePort(occupiedPorts,src);
            validatePort(occupiedPorts, dst);
            if (occupiedPorts.Count == 0)
            {
                Link link = new Link(src,dst);
                Links.Add(link);
                wrapInThread(link);
                if (LinkCreated != null)
                {
                    LinkCreated(new LinkCreatedArgs(link));
                }
            }
            return occupiedPorts;
        }
        private void validatePort(List<int> invalidPorts, int port)
        {
            if (usedPorts.Contains(port))
            {
                invalidPorts.Add(port);
            }
        }
     
    }
}
