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
using System.Threading;

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
        public AsyncCommunication PeerCoordination { get; set; }
        public List<ConnectionStep> AllSteps { get; set; }
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
            PeerCoordination = null;
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

        private Dictionary<string, CcClient> SubnetworkCc;

        private Dictionary<string, CcClient> PeerCoordinators;

        private RcClinet RcSender;
        private NccServer NccServer;
        private CcServer PeerCoordinationServer;

        private LrmClient LrmClient;
        private LrmClient SecretNccNotifier;

        private NetworkConnection Actual;

        public ConnectionController(string domain,
            int rcPort,
            Dictionary<string, int> ccPorts,
            Dictionary<string, int> peers,
            int peerCoordinationServer,
            int nccPort,
            int lrmPort,
            int? notifier,
            List<string> domains)
        {
            this.Domain = domain;
            if (nccPort != -1)
            {
                NccServer = new NccServer(nccPort, HandleNccData);
            }
            else NccServer = null;

            RcSender = new RcClinet(rcPort, HandleRoutingData);
            LrmClient = new LrmClient(lrmPort, HandleLrmData);
            Connections = new Dictionary<string, NetworkConnection>();
            SubnetworkCc = new Dictionary<string, CcClient>();
            PeerCoordinationServer = new CcServer(peerCoordinationServer, HandlePeerData);
            PeerCoordinators = new Dictionary<string, CcClient>();

            if (notifier != null)
            {
                SecretNccNotifier = new LrmClient(notifier.Value, (string data, AsyncCommunication async) =>
                {
                    Console.WriteLine(data);
                });
            }

            foreach (string ccDomain in ccPorts.Keys)
            {
                SubnetworkCc.Add(ccDomain, new CcClient(ccPorts[ccDomain], HandleCcData));
            }

            foreach (string ccDomain in peers.Keys)
            {
                PeerCoordinators.Add(ccDomain, new CcClient(peers[ccDomain], HandlePeerAns));
            }

            if (domains == null)
                this.Domains = new List<string>();
            else this.Domains = domains;
        }

        public void Start()
        {
            Console.WriteLine("INITIALIZATION");
            Console.WriteLine(TextUtils.Dash);
            if (NccServer != null)
            {

                new Thread(delegate() { NccServer.Start(); }).Start();
                Console.WriteLine("CONNECTION REQUEST IN - RUNNING");
            }
            new Thread(delegate() { PeerCoordinationServer.Start(); }).Start();
            Console.WriteLine("PEER COORDINATION IN - RUNNING");
            RcSender.ConnectToRc();
            Console.WriteLine("ROUTE TABLE QUERY OUT - RUNNING");
            LrmClient.ConnectToLrm();

            Console.WriteLine("LINK CONNECTION REQUEST OUT - RUNNING");
            Console.WriteLine("LINK CONNECTION DEALLOCATION  OUT - RUNNING");
            Thread.Sleep(10000);
            foreach (CcClient cc in SubnetworkCc.Values)
            {
                cc.ConnectToCc();
            }
            Console.WriteLine("CONNECTION REQUEST OUT - RUNNING");
            Thread.Sleep(10000);

            foreach (CcClient peer in PeerCoordinators.Values)
            {
                peer.ConnectToCc();
            }
            Console.WriteLine("PEER COORDINATION OUT - RUNNING");
        }

        public void HandleNccData(string data, AsyncCommunication async)
        {
            Console.WriteLine(data);
            HigherLevelConnectionRequest request = JsonConvert.DeserializeObject<HigherLevelConnectionRequest>(data);

            try
            {
                switch (request.Type)
                {
                    case "connection-request":
                        {
                            if (LrmClient == null)
                            {

                                return;
                            }
                            else
                            {
                                ConnectionRequest(request, null);
                                return;
                            }
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

        public void HandlePeerData(string data, AsyncCommunication async)
        {
            HigherLevelConnectionRequest request = JsonConvert.DeserializeObject<HigherLevelConnectionRequest>(data);
            ConnectionRequest(request, async);
        }

        public void HandlePeerAns(string data, AsyncCommunication async)
        {
            if (NccServer != null)
            {
                NccServer.Send(data);
            }
        }

        public void HandleCcData(string data, AsyncCommunication async)
        {
            CcResponse response = JsonConvert.DeserializeObject<CcResponse>(data);
            Console.WriteLine(response.Type + " " + response.Response);
            if (response.Response.Contains("call-mallfunction") || response.Response.Equals("ERROR"))
            {
                if (SecretNccNotifier != null)
                {
                    SecretNccNotifier.SendToLrm(data);
                }

                return;
            }

            string[] splitedResponseTag = response.Response.Split('|');
            string ConnectionId = splitedResponseTag[1];
            string SubconnectionId = splitedResponseTag[2];

            ReqType type = (ReqType)Enum.Parse(typeof(ReqType), splitedResponseTag[3]);
            /*NetworkConnection nc = null;
            if (Connections.ContainsKey(Conn))
            {

            }*/
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
                    Response = "OK|" + ConnectionId + "|" + Connections[ConnectionId].MySubconnectionId + "|" + type.ToString()
                };
                if (Connections[ConnectionId].PeerCoordination != null)
                {
                    Connections[ConnectionId].PeerCoordination.Send(JsonConvert.SerializeObject(resp));
                }
                else
                {
                    if (Connections[ConnectionId].DstGateway == null && NccServer != null)
                    {
                        NccServer.Send(JsonConvert.SerializeObject(resp));
                    }
                    else
                    {
                        HigherLevelConnectionRequest request = PreparePeerRequest(Connections[ConnectionId]);
                        string domain = Connections[ConnectionId].DstGateway.Domian;
                        PeerCoordinators[domain].SendToCc(JsonConvert.SerializeObject(request));
                    }
                }
            }
        }

        private HigherLevelConnectionRequest PreparePeerRequest(NetworkConnection nc)
        {
            Termination peerGateway = nc.DstGateway;
            List<LrmPort> ports = nc.AllSteps[nc.AllSteps.Count - 2].Ports;
            string lastIndex = null;
            if (ports.Count == 1)
            {
                lastIndex = ports[0].Index;
            }
            else
            {
                lastIndex = ports[1].Index;
            }

            LrmSnp beginning = new LrmSnp
            {
                Index = lastIndex,
                Name = peerGateway.Node,
                Port = peerGateway.Port
            };

            LrmSnp end = new LrmSnp
            {
                Name = nc.End2.Node,
                Port = nc.End2.Port
            };

            return new HigherLevelConnectionRequest
            {
                Dst = end,
                Src = beginning,
                Id = nc.Id,
                Type = nc.ActualLevelConnection.Type
            };
        }

        private void ConnectionRequest(HigherLevelConnectionRequest request, AsyncCommunication async)
        {
            ConsoleLogger.PrintConnectionRequest(request);
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
                AllSteps = new List<ConnectionStep>()
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

            if (async != null)
            {
                actual.PeerCoordination = async;
            }

            List<LrmDestination> ends = new List<LrmDestination>();

            ends.Add(new LrmDestination
            {
                Name = request.Src.Name,
                Port = request.Src.Port
            });

            ends.Add(new LrmDestination
            {
                Name = request.Dst.Name,
                Port = request.Dst.Port
            });

            SimpleConnection sc = new SimpleConnection
            {
                Id = actual.Id,
                Protocol = "route",
                Ends = ends,
                Domain = this.Domain
            };
            ConsoleLogger.PrintRouteTableQuery(sc);
            RcSender.SendToRc(JsonConvert.SerializeObject(sc));
        }

        private string GenerateConnectionId(HigherLevelConnectionRequest request)
        {
            return request.Src.Name + request.Src.Port + request.Dst.Name + request.Dst.Port;
        }

        private void CallTeardown(HigherLevelConnectionRequest request)
        {
            string id = request.Id == null ? GenerateConnectionId(request) : request.Id.Split('|')[0];

            if (!Connections.ContainsKey(id))
            {
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
            if (data != "ERROR")
            {
                RouteResponse snpp = JsonConvert.DeserializeObject<RouteResponse>(data);
                NetworkConnection actualNetworkConn = Connections[snpp.Id];

                ConnectionRequest conn = GetMyConnection(snpp, actualNetworkConn);
                actualNetworkConn.ActualLevelConnection = conn;

                SendConnectionReq(conn, ReqType.CONNECTION_REQUEST);
            }
            else
            {
                Console.WriteLine(data);
            }
        }

        private void SendConnectionReq(ConnectionRequest request, ReqType type)
        {
            request.Type = type.ToString();
            Console.WriteLine();
            Console.WriteLine("Link Connection Request");
            LrmClient.SendToLrm(JsonConvert.SerializeObject(request));
        }

        private ConnectionRequest GetMyConnection(RouteResponse snpp, NetworkConnection actualConn)
        {
            List<ConnectionStep> steps = new List<ConnectionStep>();
            Console.WriteLine();
            Console.WriteLine("SNPP :");
            Console.WriteLine(TextUtils.Dash);
            ConsoleLogger.PrintSNPP(snpp.Steps);

            for (int i = 0; i < snpp.Steps.Count; i++)
            {
                SNP previous = i == 0 ? null : snpp.Steps[i - 1];
                SNP actual = snpp.Steps[i];

                if (actual.Ports[0] == null)
                {
                    actual.Ports.RemoveAt(0);
                }

                if (actual.Ports[1] == null)
                {
                    actual.Ports.RemoveAt(1);
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

                actualConn.AllSteps.Add(step);

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
                    if (i != 0 && previous.Domain != null && previous.Domain.Equals(actual.Domain))
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

                steps.Add(step);
            }

            if (actualConn.Id == null)
            {
                actualConn.Id = actualConn.End1.Node + actualConn.End1.Port + actualConn.End2.Node + actualConn.End2.Port;
            }

            ConnectionRequest req = new ConnectionRequest
            {
                Steps = steps,
                Id = actualConn.Id
            };
            Console.WriteLine();
            Console.WriteLine("My domain snpp : ");
            Console.WriteLine(TextUtils.Dash);

            ConsoleLogger.PrintConnection(req, false);
            return req;
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
            ConnectionRequest reqResp = JsonConvert.DeserializeObject<ConnectionRequest>(data);
            string connectionId = reqResp.Id;
            NetworkConnection actual = null;
            if (Connections.ContainsKey(connectionId))
            {
                actual = Connections[connectionId];

            }
            else
            {
                foreach (NetworkConnection conn in Connections.Values)
                {
                    if (conn.MySubconnectionId.Equals(connectionId))
                    {
                        actual = conn;
                    }
                }
            }
            actual.ActualLevelConnection = reqResp;

            Console.WriteLine();
            Console.WriteLine("Allocated snp");
            Console.WriteLine(TextUtils.Dash);
            ConsoleLogger.PrintConnection(reqResp, true);

            if (actual.SubConnections.Count == 0 && actual.DstGateway == null)
            {
                CcResponse resp = new CcResponse
                {
                    Response = "OK|" + actual.Id + "|" + actual.MySubconnectionId + "|" + reqResp.Type
                };

                if (actual.PeerCoordination != null)
                {
                    actual.PeerCoordination.Send(JsonConvert.SerializeObject(resp));
                }
                else if (NccServer != null)
                {
                    NccServer.Send(JsonConvert.SerializeObject(resp));
                }

                return;
            }

            if (actual.SubConnections.Count > 0)
            {
                foreach (string subconnectionId in actual.SubConnections.Keys)
                {
                    Tuple<LrmSnp, LrmSnp> edges = actual.SubConnections[subconnectionId];

                    if (reqResp.Type.Equals(ReqType.CONNECTION_REQUEST.ToString()))
                    {
                        UpdateEdgeSnp(actual, edges);
                    }

                    string domian = actual.SubConnectionsDomians[subconnectionId];
                    //Sprawdzać kolejność
                    HigherLevelConnectionRequest request = new HigherLevelConnectionRequest
                    {
                        Src = edges.Item1,
                        Dst = edges.Item2,
                        Id = connectionId + "|" + subconnectionId,
                        Type = TransformRequestType(reqResp.Type)
                    };



                    SubnetworkCc[domian].SendToCc(JsonConvert.SerializeObject(request));
                    Console.WriteLine();
                    Console.WriteLine("Connection Request to CC at " + domian);
                    Console.WriteLine(TextUtils.Dash);
                    ConsoleLogger.PrintConnectionRequest(request);
                }
            }
            else
            {
                HigherLevelConnectionRequest request = PreparePeerRequest(actual);
                string domain = actual.DstGateway.Domian;
                PeerCoordinators[domain].SendToCc(JsonConvert.SerializeObject(request));
            }
        }

        private string TransformRequestType(string type)
        {
            string reqType = null;

            if (type.Equals(ReqType.DISCONNECTION_REQUEST.ToString()))
            {
                reqType = "call-teardown";
            }
            else if (type.Equals(ReqType.CONNECTION_REQUEST.ToString()))
            {
                reqType = "connection-request";
            }

            return reqType;
        }

        private void UpdateEdgeSnp(NetworkConnection conn, Tuple<LrmSnp, LrmSnp> edges)
        {
            LrmSnp first = null;
            LrmSnp second = null;
            List<ConnectionStep> allSteps = conn.AllSteps;
            ConnectionRequest actual = conn.ActualLevelConnection;

            //TODO indexof może nie działać
            int firstIndex = FindEdgeSnpIndex(allSteps, edges.Item1);
            int secondIndex = FindEdgeSnpIndex(allSteps, edges.Item2);
            if (firstIndex < secondIndex)
            {
                first = edges.Item1;
                second = edges.Item2;
            }
            else
            {
                first = edges.Item2;
                second = edges.Item1;
                int tmpIndex = firstIndex;
                firstIndex = secondIndex;
                secondIndex = tmpIndex;
            }

            ConnectionStep backward = firstIndex - 1 > 0 ? allSteps[firstIndex - 1] : null;
            backward = FindActualStep(backward, actual);

            ConnectionStep forward = secondIndex + 1 < allSteps.Count ? allSteps[secondIndex + 1] : null;
            forward = FindActualStep(forward, actual);

            List<ConnectionStep> steps = actual.Steps;

            if (backward != null)
            {
                first.Index = backward.Ports[1].Index;
                allSteps[firstIndex].Ports[0].Index = backward.Ports[1].Index;
            }

            if (forward != null)
            {
                second.Index = forward.Ports[0].Index;
                allSteps[secondIndex].Ports[0].Index = forward.Ports[0].Index;
            }
        }

        private ConnectionStep FindActualStep(ConnectionStep step, ConnectionRequest actual)
        {
            foreach (ConnectionStep actualStep in actual.Steps)
            {
                if (step.Node.Equals(actualStep.Node) && step.Ports[0].Number.Equals(actualStep.Ports[0].Number))
                {
                    if (step.Ports.Count > 1)
                    {
                        if (actualStep.Ports.Count > 1 && actualStep.Ports[1].Number.Equals(step.Ports[1].Number))
                        {
                            return actualStep;
                        }
                    }
                    else
                    {
                        return actualStep;
                    }
                }
            }

            return null;
        }

        private int FindEdgeSnpIndex(List<ConnectionStep> allSteps, LrmSnp lrmSnp)
        {
            int index = 0;
            foreach (ConnectionStep snp in allSteps)
            {
                //Edge has only one value as port
                if (snp.Node == lrmSnp.Name && snp.Ports[0].Number == lrmSnp.Port)
                {
                    return index;
                }
                index++;
            }

            return -1;
        }
    }
}