using Cc.Communication;
using ConectionController.Communication.ReqResp;
using Newtonsoft.Json;
using RoutingController.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cc
{
    public class NodeStep
    {
        public string NodeName { get; set; }

        public string Port1 { get; set; }
        public string Index1 { get; set; }
        public string Dest1 { get; set; }
        public string DestPort1 { get; set; }

        public string Dest2 { get; set; }
        public string Port2 { get; set; }
        public string Index2 { get; set; }
        public string DestPort2 { get; set; }

    }

    public class NetworkConnection
    {
        public string End1 { get; set; }
        public string End2 { get; set; }
        public List<NodeStep> Steps { get; set; }
    }
    public class ConnectionController
    {
        private List<NetworkConnection> Connections;

        private Dictionary<string, string> Gateways;

        private NetworkNodeSender RcSender;
        private Dictionary<string, NetworkNodeSender> LrmSenders;

        private NetworkConnection Actual;
        public ConnectionController(int rcPort, Dictionary<string, int> lrmPorts, Dictionary<string, string> gateways)
        {
            int bufferSize = 6000;
            RcSender = new NetworkNodeSender(rcPort, bufferSize);
            LrmSenders = new Dictionary<string, NetworkNodeSender>();
            Gateways = gateways;
            Connections = new List<NetworkConnection>();

            foreach (KeyValuePair<string, int> lrmPort in lrmPorts)
            {
                LrmSenders.Add(lrmPort.Key,new NetworkNodeSender(lrmPort.Value, bufferSize));
            }
            
        }
        public string HandleNccData(string data)
        {
            string[] protocolDetails = data.Split('|');
            List<string> arguments = new List<string>();
            for (int i = 1; i < protocolDetails.Length; i++)
            {
                arguments.Add(protocolDetails[i]);
            }
            try
            {
                switch (protocolDetails[0])
                {
                    case "connection-request":
                        {
                            ConnectionRequest(arguments);
                            return "OK|";
                        }
                    case "inter-connection-request":
                        {
                            InterConnectionRequest(arguments);
                            return "OK|";
                        }
                    case "call-teardown":
                        {
                            CallTeardown(arguments);
                            return "OK|";
                        }
                    case "inter-call-teardown":
                        {
                            InterCallTeardown(arguments);
                            return "OK|";
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }
            return "ERROR|";
        }

        private void ConnectionRequest(List<string> arguments)
        {
            Actual = new NetworkConnection
            {
                End1 = arguments[0],
                End2 = arguments[1],
                Steps = new List<NodeStep>()
            };

            Connections.Add(Actual);

            SimpleConnection sc = new SimpleConnection
            {
                Protocol = "query",
                Source = arguments[0],
                Destination = arguments[1]
            };

            RcSender.SendContent(JsonConvert.SerializeObject(sc), HandleRoutingData);

        }

        private void InterConnectionRequest(List<string> arguments)
        {
            string domian = arguments[0];
            string gateway = Gateways[domian];
            List<string> localArguments = new List<string>();
            localArguments.Add(arguments[1]);
            localArguments.Add(gateway);

            ConnectionRequest(localArguments);
        }

        private void CallTeardown(List<string> arguments)
        {
            NetworkConnection connection = null;
            foreach (NetworkConnection conn in Connections)
            {
                if((conn.End1.Equals(arguments[0]) && conn.End2.Equals(arguments[1])) ||
                   (conn.End1.Equals(arguments[1]) && conn.End2.Equals(arguments[0]))) 
                {
                     connection = conn;
                     break;
                }
            }
            
            TearDownConnection(connection);
            
        }

        private void TearDownConnection(NetworkConnection conn)
        {
            try
            {
                foreach (NodeStep step in conn.Steps)
                {
                    NetworkNodeSender sender = LrmSenders[step.NodeName];
                    StringBuilder builder = new StringBuilder();
                    builder.Append("REMOVE|");
                    builder.Append(step.Port1);
                    builder.Append(":");
                    builder.Append(step.Index1);
                    builder.Append("|");
                    builder.Append(step.Port2);
                    builder.Append(":");
                    builder.Append(step.Index2);
                    sender.SendContent(builder.ToString(), CheckTearDown);
                }
                Connections.Remove(conn);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot remove Connection");
            }
        }

        private void CheckTearDown(string data)
        {
            if (data.Equals("ERROR"))
            {
                throw new Exception("Cannot remove connection");
            }
        }

        private void InterCallTeardown(List<string> arguments)
        {
            string domian = arguments[0];
            string gateway = Gateways[domian];
            List<string> localArguments = new List<string>();
            localArguments.Add(arguments[1]);
            localArguments.Add(gateway);

            CallTeardown(localArguments);
        }

        private void HandleRoutingData(string data)
        {
            
            SNPP snpp = JsonConvert.DeserializeObject<SNPP>(data);
            foreach (SNP snp in snpp.Nodes)
            {

                string nodeName = snp.Name;
                NetworkNodeSender lrmSender = LrmSenders[nodeName];

                StringBuilder builder = new StringBuilder();
                builder.Append("ADD|");

                if (Actual.Steps.Count > 0)
                {
                    NodeStep lastStep = Actual.Steps[Actual.Steps.Count - 1];

                    if (lastStep.Dest1.Equals(nodeName))
                    {
                        builder.Append(lastStep.DestPort1);
                        builder.Append(':');
                        builder.Append(lastStep.Index1);
                        builder.Append("|");
                        builder.Append(snp.Ports[0].Equals(lastStep.DestPort1) ? snp.Ports[1] : snp.Ports[0]);
                    }
                    else
                    {
                        builder.Append(lastStep.DestPort2);
                        builder.Append(':');
                        builder.Append(lastStep.Index2);
                        builder.Append("|");
                        builder.Append(snp.Ports[0].Equals(lastStep.DestPort2) ? snp.Ports[1] : snp.Ports[0]);
                    }

                }
                else
                {
                    builder.Append(snp.Ports[0]);
                    builder.Append("|");
                    builder.Append(snp.Ports[1]);
                }
                Console.WriteLine(builder.ToString());
                lrmSender.SendContent(builder.ToString(), HandleLrmData);
            }
        }

        private void HandleLrmData(string data)
        {
            string[] allocationDetails = data.Split('|');
            string networkNode = allocationDetails[0];

            string[] port1Details = allocationDetails[1].Split('#');
            string[] port2Details = allocationDetails[2].Split('#');

            NodeStep ns = new NodeStep
            {
                NodeName = networkNode,
                Port1 = port1Details[0].Split(':')[0],
                Index1 = port1Details[0].Split(':')[1],
                Dest1 = port1Details[1],
                DestPort1 = port1Details[2],
                Port2 = port2Details[0].Split(':')[0],
                Index2 = port2Details[0].Split(':')[1],
                Dest2 = port2Details[1],
                DestPort2 = port2Details[2]
            };

            Actual.Steps.Add(ns);
        }

    }
}
