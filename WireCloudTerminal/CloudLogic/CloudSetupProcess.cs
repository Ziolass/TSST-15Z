using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WireCloud.CloudLogic
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
        private String ConfigurationFilePath;
        private Dictionary<Link, Thread> linksThreads;
        private List<Link> usedPorts;
        private const int CLOSING_TIME = 1000;

        public event LinksCreatedHandler LinksCreated;
        public event LinkCreatedHandler LinkCreated;

        public ProcessMonitor ProcessMonitor { get; private set; }
        public ElementConfigurator ElementConfigurator { get; private set; }


        public List<Link> Links
        {
            get { return this.ProcessMonitor.Links; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CloudSetupProcess" /> class.
        /// </summary>
        /// <param name="configurationFilePath">The configuration file path.</param>
        public CloudSetupProcess(string configurationFilePath)
        {
            this.ConfigurationFilePath = configurationFilePath;
            linksThreads = new Dictionary<Link, Thread>();
            usedPorts = new List<Link>();
        }

        public void StartCloudProcess()
        {
            if (!File.Exists(ConfigurationFilePath))
            {
                throw new Exception("Missing configuration file or wrong directory");
            }

            this.ElementConfigurator = new ElementConfigurator(this.ConfigurationFilePath);
            this.ProcessMonitor = this.ElementConfigurator.SetUpCloud();
            this.ProcessMonitor.StartAction();
            usedPorts = this.ProcessMonitor.Links;
            if (this.ProcessMonitor.Links.Count != 0)
            {
                if (LinksCreated != null)
                {
                    LinksCreated();
                }
            }
        }
        public void CloseLink(Link link)
        {
            disposeLink(link, CLOSING_TIME);
            usedPorts.Remove(link);
            linksThreads.Remove(link);
        }

        private void disposeLink(Link link, int executionTime)
        {
            //if (!Links.Contains(link))
            //{
            //    return;
            //}

            //link.DestroyLink();

            Thread linkThread = linksThreads[link];
            linkThread.Join(executionTime);
            if (linkThread.IsAlive)
            {
                try
                {
                    linkThread.Abort();
                }
                catch (Exception ex) { 
                
                }
            }
        }

        public void CloseAll()
        {
            foreach (Link link in linksThreads.Keys)
            {
                disposeLink(link, 10);
            }
        }

        public List<Link> TryAddLink(Link src)
        {
            List<Link> occupiedPorts = new List<Link>();
            validatePort(occupiedPorts, src);
            if (occupiedPorts.Count == 0)
            {
                Link link = new Link(src);
                Links.Add(link);
                if (LinkCreated != null)
                {
                    LinkCreated(new LinkCreatedArgs(link));
                }
            }
            return occupiedPorts;
        }
        private void validatePort(List<Link> invalidPorts, Link port)
        {
            if (usedPorts.Contains(port))
            {
                invalidPorts.Add(port);
            }
        }

    }
}
