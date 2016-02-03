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
        public string MySubconnectionId { get; set; }
        public List<SNP> AllSteps { get; set; }
        public ConnectionRequest ActualLevelConnection { get; set; }
        public Dictionary<string, Tuple<LrmSnp, LrmSnp>> SubConnections { get; set; }
        public Dictionary<string, string> SubConnectionsDomians { get; set; }
        public Dictionary<string, bool> SubConnectionsAvability { get; set; }
        public Termination DstGateway { get; set; }
        public Termination SrcGateway { get; set; }
        public NetworkConnection()
        {
            SubConnections = new Dictionary<string, Tuple<LrmSnp, LrmSnp>>();
            SubConnectionsDomians = new Dictionary<string, string>();
            SubConnectionsAvability = new Dictionary<string, bool>();
        }
        public void AddSubconnection(string id, LrmSnp src, LrmSnp dst, string domian)
        {
            SubConnections.Add(id, new Tuple<LrmSnp, LrmSnp>(src, dst));
            SubConnectionsAvability.Add(id, false);
            SubConnectionsDomians.Add(id, domian);
        }
    }
    public class ConnectionController
    {
        private Dictionary<string, NetworkConnection> Connections;
        private List<string> Domains;
        private string Domain;
        
        private Dictionary<string, CcClient> LowerClients;

        private RcClinet RcSender;
        private NccServer NccServer;
        private LrmClient LrmClient;

        private NetworkConnection Actual;
        public ConnectionController(int rcPort, Dictionary<string, int> ccPorts, int nccPort)
        {
            int bufferSize = 6000;
            NccServer = new NccServer(nccPort,HandleNccData);
            RcSender = new RcClinet(rcPort, HandleRoutingData);
            Connections = new Dictionary<string, NetworkConnection>();
            LowerClients
        }
        public void HandleNccData(string data, AsyncCommunication async)
        {
            HigherLevelConnectionRequest request = JsonConvert.DeserializeObject<HigherLevelConnectionRequest>(data);

            try
            {
                switch (request.Type)
                {
                    case "connection-request":
                        {
                            ConnectionRequest(request);
                            return;
                        }
                    case "call-teardown":
                        {
                            CallTeardown(request);
                            return;

                        }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }
        }

        public void HandleCcData(string data, AsyncCommunication async)
        {
            CcResponse response = JsonConvert.DeserializeObject<CcResponse>(data);

            if (response.Response.Contains("call-mallfunction") || response.Response.Equals("ERROR"))
            {

                return;
            }
            string[] splitedResponseTag = response.Response.Split('|');
            string ConnectionId = splitedResponseTag[1];
            string SubconnectionId = splitedResponseTag[2];

            ReqType type = (ReqType)Enum.Parse(typeof(ReqType), splitedResponseTag[3]);
 
            
            Connections[ConnectionId]
                .SubConnectionsAvability[SubconnectionId] = type == ReqType.CONNECTION_REQUEST;
            
            bool allDone = true;

            foreach (bool subConnectionDone in Connections[ConnectionId].SubConnectionsAvability.Values)
            {
                if (type == ReqType.CONNECTION_REQUEST && !subConnectionDone)
                {
                    allDone = false;
                }

                if (type == ReqType.DISCONNECTION_REQUEST && subConnectionDone)
                {
                    allDone = false;
                }
            }

            if (allDone)
            {
                CcResponse resp = new CcResponse
                {
                    Response = "OK|" + ConnectionId + "|" + Connections[ConnectionId].MySubconnectionId + "|"  +type.ToString()
                };
                NccServer.Send(JsonConvert.SerializeObject(resp));
            }

        }

        private void ConnectionRequest(HigherLevelConnectionRequest request)
        {
            NetworkConnection actual = new NetworkConnection
            {
                End1 = new Termination
                {
                    Node = request.Src.Name,
                    Port = request.Src.Port
                },
                End2 = new Termination
                {
                    Node = request.Dst.Name,
                    Port = request.Dst.Port
                },
                AllSteps = new List<SNP>()
            };

            if (request.Id != null)
            {
                string[] reqParts = request.Id.Split('|');
                actual.Id = reqParts[0];
                actual.MySubconnectionId = reqParts[1];
            }
            else
            {
                actual.Id = GenerateConnectionId(request);
            }

            Connections.Add(actual.Id, actual);

            SimpleConnection sc = new SimpleConnection
            {
                Id = actual.Id,
                Protocol = "query",
                Source = request.Src.Name + ":" + request.Src.Port,
                Destination = request.Dst.Name + ":" + request.Dst.Port
            };

            RcSender.SendToRc(JsonConvert.SerializeObject(sc));

        }

        private string GenerateConnectionId(HigherLevelConnectionRequest request)
        {
            return request.Src.Name + request.Src.Port + request.Dst.Name + request.Dst.Port;
        }


        private void CallTeardown(HigherLevelConnectionRequest request)
        {
            string id = request.Id == null ? GenerateConnectionId(request) : request.Id.Split('|')[0];
            
            if (!Connections.ContainsKey(id)) {
                LrmSnp tmp = request.Src;
                request.Src = request.Dst;
                request.Dst = tmp;

                id = GenerateConnectionId(request);
            }

            NetworkConnection actual = Connections[id];
            SendConnectionReq(actual.ActualLevelConnection, ReqType.DISCONNECTION_REQUEST);
        }

        private void HandleRoutingData(string data, AsyncCommunication async)
        {

            RouteResponse snpp = JsonConvert.DeserializeObject<RouteResponse>(data);
            NetworkConnection actualNetworkConn = Connections[snpp.Id];

            ConnectionRequest conn = GetMyConnection(snpp, actualNetworkConn);
            actualNetworkConn.ActualLevelConnection = conn;

            SendConnectionReq(conn, ReqType.CONNECTION_REQUEST);
        }

        private void SendConnectionReq(ConnectionRequest request, ReqType type)
        {
            request.Type = type.ToString();
            LrmClient.SendToLrm(JsonConvert.SerializeObject(request));
        }

        private ConnectionRequest GetMyConnection(RouteResponse snpp, NetworkConnection actualConn)
        {
            List<ConnectionStep> steps = new List<ConnectionStep>();

            for (int i = 0; i < snpp.Steps.Count; i++)
            {
                SNP previous = i == 0 ? null : snpp.Steps[i - 1];
                SNP actual = snpp.Steps[i];

                if (actual.Domain != null)
                {
                    //Przypadek gdy ostatni punkt jest gatewayem
                    if (!Domains.Contains(actual.Domain))
                    {
                        actualConn.DstGateway = PrepareGateWay(actual);
                        if (previous.Domain == null)
                        {
                            actualConn.DstGateway = PrepareGateWay(previous);
                        }
                    }
                    //Wejścia do niższych domen
                    if (!(i != 0 && previous.Domain != null && previous.Domain.Equals(actual.Domain)))
                    {
                        string id = previous.Node + previous.Ports[0] + actual.Node + actual.Ports[0];

                        actualConn.AddSubconnection(id, new LrmSnp
                        {
                            Name = previous.Node,
                            Port = previous.Ports[0]
                        },
                        new LrmSnp
                        {
                            Name = actual.Node,
                            Port = actual.Ports[0]
                        }, previous.Domain);
                    }

                    continue;
                }
                List<LrmPort> lrmPorts = new List<LrmPort>();
                foreach (string port in actual.Ports)
                {
                    lrmPorts.Add(new LrmPort
                    {
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
                Steps = steps,
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

        private void HandleLrmData(string data, AsyncCommunication async)
        {

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

            if (actual.SubConnections.Count == 0)
            {
                CcResponse resp = new CcResponse
                {
                    Response = "OK|" + actual.Id + "|" + actual.MySubconnectionId + "|" + reqResp.Type
                };
                NccServer.Send(JsonConvert.SerializeObject(resp));
                return;
            }



            foreach (string subconnectionId in actual.SubConnections.Keys)
            {
                Tuple<LrmSnp, LrmSnp> edges = actual.SubConnections[subconnectionId];
                
                if (reqResp.Type.Equals(ReqType.CONNECTION_REQUEST.ToString()))
                {
                    UpdateEdgeSnp(reqResp, edges, actual.AllSteps);
                }

                string domian = actual.SubConnectionsDomians[subconnectionId];
                //Sprawdzać kolejność
                HigherLevelConnectionRequest request = new HigherLevelConnectionRequest
                {
                    Src = edges.Item1,
                    Dst = edges.Item2,
                    Id = connectionId + "|" + subconnectionId,
                    Type = reqResp.Type
                };

                LowerClients[domian].SendToCc(JsonConvert.SerializeObject(request));
            }

        }

        private void UpdateEdgeSnp(ConnectionRequest actual, Tuple<LrmSnp, LrmSnp> edges, List<SNP> allSteps)
        {
            LrmSnp first = null;
            LrmSnp second = null;
            //TODO indexof może nie działać
            int firstIndex = FindSnpIndex(allSteps, edges.Item1);
            int secondIndex = FindSnpIndex(allSteps, edges.Item2);
            if (firstIndex < secondIndex)
            {
                first = edges.Item1;
                second = edges.Item2;
            }
            else
            {
                first = edges.Item2;
                second = edges.Item1;
            }

            SNP backward = firstIndex - 1 > 0 ? allSteps[firstIndex - 1] : null;
            SNP forward = secondIndex + 1 < allSteps.Count ? allSteps[secondIndex + 1] : null;
            List<ConnectionStep> steps = actual.Steps;
            if (backward != null)
            {
                ConnectionStep backwardStep = FindStep(steps, backward);
                first.Index = backwardStep.Ports[1].Index;
            }

            if (forward != null)
            {
                ConnectionStep forwardStep = FindStep(steps, forward);
                second.Index = forwardStep.Ports[0].Index;
            }
        }

        private int FindSnpIndex(List<SNP> allSteps, LrmSnp lrmSnp)
        {
            int index = 0;
            foreach (SNP snp in allSteps)
            {
                if (snp.Node == lrmSnp.Name && snp.Ports[0] == lrmSnp.Port)
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        private ConnectionStep FindStep(List<ConnectionStep> steps, SNP snp)
        {

            foreach (ConnectionStep step in steps)
            {
                if (step.Node == snp.Node)
                {
                    return step;
                }
            }

            return null;
        }

    }
}
