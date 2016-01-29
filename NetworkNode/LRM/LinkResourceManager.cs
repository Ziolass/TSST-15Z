using NetworkNode.HPC;
using NetworkNode.Ports;
using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WireCloud;

namespace NetworkNode.LRM
{
    public class DestinationInfo
    {
        public string Type { get; set; }
        public string Domain { get; set; }
        public string Port { get; set; }
        public string Node { get; set; }
        public DestinationInfo(string type, string domain, string port, string node)
        {
            Type = type;
            Domain = domain;
            Port = port;
            Node = node;
        }
    }
    public class LrmDataCollector
    {
        public Thread Action { get; private set; }
        private Action<int> CollectDataFn;
        private int Port;
        public LrmDataCollector(Action<int> collectDataFn, int port)
        {
            Port = port;
            CollectDataFn = collectDataFn;
            Action = new Thread(new ThreadStart(CollectData));
        }

        public void CollectData()
        {
            CollectDataFn(Port);
        }

    }

    public class LinkResourceManager
    {
        private HigherOrderPathConnection Hpc;
        private string Domain;
        private string NetworkNodeName;
        private Dictionary<int, bool?> AvailabilityList;
        private Dictionary<int, DestinationInfo> DestinationsInfo;
        private Thread InitiationThread;

        private NodeCcInput CcServer;
        private NetworkNodeSender RcClient;

        private const int REPETITIONS = 10;
        private const int INTERVAL = 2000;

        private object initilaLoc = new object();

        public LinkResourceManager(HigherOrderPathConnection hpc, string domain, string networkNodeName, NodeCcInput ccServer, int rcPort)
        {
            Hpc = hpc;
            Hpc.HandleLrmInfo += new HandleLrmInfo(HandleLrmData);
            Hpc.LinkResourceExhausted += new LinkResourceExhausted(SetExhausted);
            Hpc.LinkResourceFreed += new LinkResourceFreed(SetFree);
            Domain = domain;
            NetworkNodeName = networkNodeName;
            AvailabilityList = new Dictionary<int, bool?>();
            DestinationsInfo = new Dictionary<int, DestinationInfo>();
            InitiationThread = new Thread(new ThreadStart(InitLrm));
            CcServer = ccServer;
            RcClient = new NetworkNodeSender(rcPort);
        }

        public void Strat()
        {
            InitiationThread.Start();
        }

        public string HandleCcData(string data)
        {
            string[] protocolData = data.Split('|');
            List<string> endpoints = new List<string>();

            endpoints.Add(protocolData[1]);
            endpoints.Add(protocolData[2]);

            switch (protocolData[0])
            {
                case "ADD":
                    {
                        return Allocate(endpoints);
                    }
                case "REMOVE":
                    {
                        return FreeEndpoints(endpoints);
                    }
            }

            return "ERROR";
        }

        private string FreeEndpoints(List<string> endpoints)
        {
            Dictionary<int, int> ports = new Dictionary<int, int>();
            foreach (string endpoint in endpoints)
            {
                string[] portData = endpoint.Split(':');
                ports.Add(int.Parse(portData[0]), int.Parse(portData[1]));
            }

            return "OK";
        }


        private string Allocate(List<string> endpoints)
        {

            Dictionary<int, int?> portsWithIndexes = new Dictionary<int, int?>();
            foreach (string endpoint in endpoints)
            {
                string[] endpointDetails = endpoint.Split(':');
                if (endpointDetails.Length == 1)
                {
                    portsWithIndexes.Add(int.Parse(endpointDetails[0]), null);
                }
                else
                {
                    portsWithIndexes.Add(int.Parse(endpointDetails[0]), int.Parse(endpointDetails[1]));
                }
            }

            Dictionary<int, int> allocatedResources = Hpc.Allocate(portsWithIndexes);

            StringBuilder builder = new StringBuilder();
            builder.Append(NetworkNodeName);
            builder.Append("|");
            int index = 0;
            foreach (int port in portsWithIndexes.Keys)
            {
                builder.Append(port);
                builder.Append(':');
                builder.Append(allocatedResources[port]);
                builder.Append('#');
                builder.Append(DestinationsInfo[port].Node);
                builder.Append('#');
                builder.Append(DestinationsInfo[port].Port);
                if (index < portsWithIndexes.Count)
                {
                    builder.Append('|');
                }
                index++;
            }

            return builder.ToString();
        }

        private void InitLrm()
        {
            List<Thread> startedCollectors = new List<Thread>();
            foreach (int port in Hpc.GetPortsForLrm())
            {
                AvailabilityList.Add(port, null);
                LrmDataCollector collector = new LrmDataCollector(SendLrmMessage, port);
                startedCollectors.Add(collector.Action);
            }

            foreach (Thread collector in startedCollectors)
            {
                Thread.Sleep(200);
                collector.Start();
            }

            foreach (Thread collector in startedCollectors)
            {
                collector.Join();
            }

            ReportTopology();
        }


        private void SendLrmMessage(int portNumber)
        {
            for (int i = 0; i < REPETITIONS; i++)
            {
                SendId(portNumber);

                Thread.Sleep(INTERVAL);

                if (AvailabilityList[portNumber] != null)
                {
                    break;
                }
            }

            Console.WriteLine("LRM: COLLECTOR END " + portNumber);

        }
        private void SendId(int portNumber)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Domain);
            builder.Append("%");
            builder.Append(NetworkNodeName);
            builder.Append("%");
            builder.Append(portNumber);

            lock (Hpc)
            {
                Hpc.SendLrmData(portNumber, builder.ToString());
            }
        }

        private void HandleLrmData(object sender, InputLrmArgs args)
        {

            string protocolData = args.Data;
            string[] protocolComponents = protocolData.Split('%');
            string domain = protocolComponents[0];
            string type = domain.Equals(Domain) ? "LOCAL" : "NETWORK";
            string name = protocolComponents[1];
            string port = protocolComponents[2];

            lock (initilaLoc)
            {
                if (!DestinationsInfo.ContainsKey(args.PortNumber))
                {
                    DestinationsInfo.Add(args.PortNumber, new DestinationInfo(type, domain, port, name));
                    Console.WriteLine("LRM -> ADD: " + domain + " " + type + " " + name + " " + port + " ");
                }
                else
                {
                    Console.WriteLine("LRM -> SET PREV: " + DestinationsInfo[args.PortNumber].Domain
                        + " " + DestinationsInfo[args.PortNumber].Type
                        + " " + DestinationsInfo[args.PortNumber].Node
                        + " " + DestinationsInfo[args.PortNumber].Port + " ");
                    DestinationsInfo[args.PortNumber] = new DestinationInfo(type, domain, port, name);
                    Console.WriteLine("LRM -> SET NEXT: " + domain + " " + type + " " + name + " " + port + " ");
                }


                AvailabilityList[args.PortNumber] = true;
            }
        }

        private void ReportTopology()
        {
            TopologyReports nodeTopology = new TopologyReports();
            nodeTopology.Protocol = "resources";
            nodeTopology.Node = NetworkNodeName;
            List<TopologyData> topologyData = new List<TopologyData>();
            Console.WriteLine("Avalible Ports for : " + NetworkNodeName);
            foreach (int port in AvailabilityList.Keys)
            {

                if (AvailabilityList[port] == null)
                {
                    continue;
                }

                DestinationInfo portInfo = DestinationsInfo[port];

                TopologyData data = new TopologyData();

                List<string> domains = new List<string>();
                domains.Add(Domain);
                data.Domains = domains;
                data.Port = portInfo.Port;

                Destination dst = new Destination();

                if (Domain.Equals(portInfo.Domain))
                {
                    dst.Node = portInfo.Node;
                }
                else
                {
                    dst.Scope = portInfo.Domain;
                }

                dst.Port = portInfo.Port;

                data.Destination = dst;

                data.Type = portInfo.Type;

                data.Status = (bool)AvailabilityList[port] ? "FREE" : "OCCUPIED";

                topologyData.Add(data);
                Console.WriteLine("LRM: on port " + port + " = " + portInfo.Domain + "|" + portInfo.Node + "|" + portInfo.Port + "|" + portInfo.Type);
            }

            nodeTopology.Data = topologyData;

            RcClient.SendContent(JsonConvert.SerializeObject(nodeTopology));
        }

        private void SetExhausted(object sender, LinkOccupiedArgs args)
        {
            foreach (int exhaustedResource in args.Ports)
            {
                AvailabilityList[exhaustedResource] = false;
            }
            ReportTopology();
        }

        private void SetFree(object sender, LinkFreeArgs args)
        {
            foreach (int exhaustedResource in args.Ports)
            {
                AvailabilityList[exhaustedResource] = true;
            }
            ReportTopology();
        }



    }
}
