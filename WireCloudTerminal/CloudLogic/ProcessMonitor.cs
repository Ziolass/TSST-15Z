using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WireCloud.CloudLogic
{
    public class ProcessMonitor
    {
        private CloudServer Server;
        public List<Link> Links { get; private set; }

        private Thread ProccessMonitorThread { get; set; }

        public ProcessMonitor(CloudServer Server, List<Link> Links)
        {
            this.Server = Server;
            this.Links = Links;
            this.Server.HandleDataIncom += new HandleDataIncom(PullData);
            ProccessMonitorThread = new Thread(new ThreadStart(Server.StartListening));
        }

        private void PullData(IncomeDataArgs incomeData)
        {
            foreach (Link link in Links)
            {
                if (link.Contains(incomeData.Address))
                {
                    link.SendData(incomeData.Data, incomeData.Address);
                    break;
                }
            }
        }

        public void StartAction()
        {
            ProccessMonitorThread.Start();
            //Server.StartListening();
        }
        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            ProccessMonitorThread.Suspend();
            Server.StopServerThread();
        }

        /// <summary>
        /// Deletes the link from Links
        /// </summary>
        /// <param name="link">The link to delete</param>
        public void DeleteLink(Link link)
        {
            this.Links.Remove(link);
        }
    }
}
