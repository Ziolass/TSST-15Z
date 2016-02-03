using Cc.Communication;
using ConectionController;
using ConectionController.Communication.ReqResp;
using LRM;
using LRM.Communication;
using NetworkNode.LRM.Communication;
using Newtonsoft.Json;
using RoutingController.Elements;
using RoutingController.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cc
{
    public enum TerminationType
    {
        CLIENT, GATEWAY
    }
    public class Termination
    {
        public string Node { get; set; }
        public string Port { get; set; }
        public string Index { get; set; }
        public string Type { get; set; }
        public string Domian { get; set; }
    }
 
    public class NetworkConnection
    {
        public Termination End1 { get; set; }
        public Termination End2 { get; set; }
        public string Id { get; set; }
        public List<SNP> AllSteps { get; set; }
        public ConnectionRequest ActualLevelConnection { get; set; }
        public Dictionary<SNP, SNP> ConnectionRequests { get; set; }
        public Termination DstGateway { get; set; }
        public Termination SrcGateway { get; set; }
    }
    public class ConnectionController
    {
        private Dictionary<string,NetworkConnection> Connections;
        private List<string> Domains;
        private string Domain;
        //private CcClient parentCc;
        private Dictionary<string, CcClient> LowerClients;

        private NetworkNodeSender RcSender;
        private LrmClient LrmClient;

        private NetworkConnection Actual;
        public ConnectionController(int rcPort, Dictionary<string, int> lrmPorts, Dictionary<string, string> gateways)
        {
            int bufferSize = 6000;
            RcSender = new NetworkNodeSender(rcPort, bufferSize);
            Connections = new Dictionary<string, NetworkConnection>();
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
            NetworkConnection  actual = new NetworkConnection
            {
                End1 = new Termination {
                    Node = arguments[0].Split(':')[0],
                    Port = arguments[0].Split(':')[1]
                },
                End2 = new Termination
                {
                    Node = arguments[1].Split(':')[0],
                    Port = arguments[1].Split(':')[1]
                },
                AllSteps = new List<SNP>(),
                ConnectionRequests = new Dictionary<SNP,SNP>()
            };

            string id = actual.End1.Node + actual.End1.Port + actual.End2.Node + actual.End2.Port;
            Connections.Add(id, actual);

            SimpleConnection sc = new SimpleConnection
            {
                Protocol = "query",
                Source = arguments[0],
                Destination = arguments[1]
            };

            RcSender.SendContent(JsonConvert.SerializeObject(sc), HandleRoutingData);

        }

        private ConnectionRequest GetMyConnection(NetworkConnection Actual)
        {
            throw new NotImplementedException();
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
                if ((conn.End1.Equals(arguments[0]) && conn.End2.Equals(arguments[1])) ||
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

        private NetworkConnection GetActualConnection(RouteResponse snpp)
        {
            
            foreach (NetworkConnection conn in Connections)
            {
                bool first = false;
                bool second = false;

                first = (snpp.Ends[0].Node == conn.End1.Node && snpp.Ends[0].Port == conn.End1.Port)
                    || (snpp.Ends[0].Node == conn.End2.Node && snpp.Ends[0].Port == conn.End2.Port);

                second = (snpp.Ends[1].Node == conn.End1.Node && snpp.Ends[1].Port == conn.End1.Port)
                    || (snpp.Ends[1].Node == conn.End2.Node && snpp.Ends[1].Port == conn.End2.Port);

                if (first && second)
                {
                    return conn;
                }
            }

            return null;

        }

        private void HandleRoutingData(string data)
        {

            RouteResponse snpp = JsonConvert.DeserializeObject<RouteResponse>(data);
            NetworkConnection actualNetworkConn = GetActualConnection(snpp);

            ConnectionRequest conn = GetMyConnection(snpp, actualNetworkConn);
            actualNetworkConn.ActualLevelConnection = conn;

            SendConnectionReq(conn);
        }

        private void SendConnectionReq(ConnectionRequest request)
        {
            LrmClient.SendToLrm(JsonConvert.SerializeObject(request));
        }

        

        private ConnectionRequest GetMyConnection(RouteResponse snpp, NetworkConnection actualConn)
        {
            List<ConnectionStep> steps = new List<ConnectionStep>();

            for (int i = 0 ; i < snpp.Steps.Count; i++)
            {
                SNP previous = i ==0? null:snpp.Steps[i-1];
                 SNP actual = snpp.Steps[i];
                
                if(actual.Domain != null){
                    //Przypadek gdy ostatni punkt jest gatewayem
                    if(!Domains.Contains(actual.Domain)) {
                        actualConn.DstGateway = PrepareGateWay(actual);
                        if(previous.Domain == null) {
                            actualConn.DstGateway = PrepareGateWay(previous);
                        }
                    }
                    //Wejścia do niższych domen
                    if(i != 0 && previous.Domain != null && previous.Domain.Equals(actual.Domain)){  
                        actualConn.ConnectionRequests[previous] = actual;
                    } 
                    else 
                    {
                        actualConn.ConnectionRequests.Add(actual, null);
                    }

                    continue;
                }
                List<LrmPort> lrmPorts = new List<LrmPort>();
                foreach(string port in actual.Ports) {
                    lrmPorts.Add(new LrmPort {
                        Number = port
                    });
                }


                ConnectionStep step = new ConnectionStep
                {
                    Node = actual.Node,
                    Ports = lrmPorts
                };
                steps.Add(step);
            }

            actualConn.Id = actualConn.End1.Node + actualConn.End1.Port + actualConn.End2.Node + actualConn.End2.Port;

            return new ConnectionRequest
            {
                Steps=steps,
                Type = ReqType.CONNECTION_REQUEST.ToString(),
                Id = actualConn.Id 
            };
            
        }

        private Termination PrepareGateWay(SNP gatewaySnp)
        {
            return new Termination
            {
                Node = gatewaySnp.Node,
                Domian = gatewaySnp.Domain,
                Port = gatewaySnp.Ports[0],
                Type = TerminationType.GATEWAY.ToString()
            };
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

        private void ReportStep(NodeStep ns)
        {
            Console.WriteLine("Node: " + ns.NodeName + " Port: [" + ns.Port1 + " " + ns.Index1 + "]");
        }


        private void HandleLrmData(string data, AsyncCommunication async) {
            
            if (data.Contains(ReqType.CONNECTION_REQUEST.ToString()))
            {
                HandleConnectionAns(data, async);
            }
        }

        private void HandleConnectionAns(string data, AsyncCommunication async)
        {
            ConnectionRequest reqResp = JsonConvert.DeserializeObject<ConnectionRequest>(data);
            string connectionId = reqResp.Id;
            NetworkConnection actual = Connections[connectionId];
            actual.ActualLevelConnection = reqResp;
            if (actual.ConnectionRequests.Count > 0)
            {
                foreach(SNP snp in snp.Keys)
            }
        }

    }
}
