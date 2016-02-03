using LRM.Communication;
using NetworkNode.LRM;
using NetworkNode.LRM.Communication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LRM
{

    public class LinkResourceManager
    {
        private LrmServer LrmServer;
        private LrmRegister LrmRegister;
        private RcClinet RcClinet;
        private string DomianScope;
        private Thread LocalTopologyRaport = null;
        private bool InitPahse;
        private AllocationRegister AllocationRegister;
        private AsyncCommunication HighestCc;


        public LinkResourceManager(int serverPort, int rcPort, int ccPort, string domianScope)
        {
            DomianScope = domianScope;
            LrmRegister = new LrmRegister();
            LrmServer = new LrmServer(serverPort, HandleNodeData, LrmRegister);
            LrmServer.NodeConnected += new NodeConnected(HandleNodeConnection);
            LrmServer.NodeDisconnected += new NodeDisconnected(HandleNodeDisconnection);
            RcClinet = new RcClinet(rcPort, HandleRcData);
            InitPahse = true;
            AllocationRegister = new AllocationRegister();
        }

        public void HandleRcData(string data, AsyncCommunication async)
        {
            Console.WriteLine("RC: " + data);
        }

        public void HandleNodeData(string data, AsyncCommunication async)
        {
            Console.WriteLine(data);
            if (data.Contains(ReqType.ALLOC_RESP.ToString())
                 || data.Contains(ReqType.DELLOC_RESP.ToString()))
            {
                HandleLocationResp(data);
                return;
            }
            else if (data.Contains(ReqType.ALLOC.ToString())
                 || data.Contains(ReqType.DELLOC.ToString()))
            {
                HandleResourceLocationData(data, LrmRegister.FindNodeByConnection(async));
                return;
            }
            else if (data.Contains(ReqType.CONNECTION_REQUEST.ToString()))
            {
                HandleConnectionRequest(data, async, ReqType.ALLOC);
                return;
            }
            else if (data.Contains(ReqType.DISCONNECTION_REQUEST.ToString()))
            {
                HandleConnectionRequest(data, async, ReqType.DELLOC);
                return;
            }
            else if (data.Contains(ReqType.LRM_NEGOTIATION.ToString()))
            {
                HandleLrmNegotiation(data, async);
                return;
            }
            else if (data.Contains(ReqType.LRM_NEGOTIATION_RESP.ToString()))
            {
                HandleLrmNegotiationResp(data);
                return;
            }


            HandleTokenData(data, LrmRegister.FindNodeByConnection(async));
        }

        private void HandleLrmNegotiationResp(string data)
        {
            //TODO ładne wypisywanie
            Console.WriteLine("NEGOTIATION OK");
        }

        private void HandleLrmNegotiation(string data, AsyncCommunication async)
        {
            LrmReq request = JsonConvert.DeserializeObject<LrmReq>(data);
            Console.WriteLine("NODE: " + request.Id + "PORT: " + request.Ports[0].Number + " INDEX: " + request.Ports[1].Index);
            async.Send(JsonConvert.SerializeObject(new LrmResp
            {
                Type = ReqType.LRM_NEGOTIATION_RESP.ToString(),
                Id = request.Id,
                Msg = "@DOMIAN",
                Status = LrmRespStatus.ACK.ToString()
            }));
        }

        private void HandleLocationResp(string data)
        {
            LrmResp response = JsonConvert.DeserializeObject<LrmResp>(data);

            if (AllocationRegister.ConfirmStep(response.ConnectionId, response.Id))
            {
                AsyncCommunication async = AllocationRegister.GetComm(response.ConnectionId);
                async.Send(JsonConvert.SerializeObject(AllocationRegister.GetConnection(response.ConnectionId)));
                AllocationRegister.Remove(response.ConnectionId);
            }
        }



        public void HandleNodeConnection(string NodeName)
        {
            if (LocalTopologyRaport == null)
            {
                LocalTopologyRaport = new Thread(new ThreadStart(SendLocalTopology));
                LocalTopologyRaport.Start();
            }

            AsyncCommunication async = LrmRegister.ConnectedNodes[NodeName].Async;
            LrmToken token = new LrmToken
            {
                Tag = NodeName
            };
            string data = JsonConvert.SerializeObject(token);
            data = WrapWithHeader(LrmCommunicationType.SIGNALLING, LrmHeader.BROADCAST, data);

            async.Send(data);
        }

        public void HandleNodeDisconnection(string NodeName)
        {
            string data = JsonConvert.SerializeObject(new PresenceRaport
            {
                Header = PresenceType.DISCONNECTED.ToString(),
                Node = NodeName
            });

            HighestCc.Send(data);
        }

        public void RunServer()
        {
            LrmServer.Start();
        }

        private string WrapWithHeader(LrmCommunicationType comm, LrmHeader header, string data)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(comm);
            builder.Append("|");
            
            if (!header.Equals(LrmHeader.NONE))
            {
                builder.Append(header);
                builder.Append("#");
            }
            
            builder.Append(data);
            return builder.ToString();
        }

        private void HandleResourceLocationData(string data, VirtualNode node)
        {
            ResourceLocation resource = JsonConvert.DeserializeObject<ResourceLocation>(data);
            List<int> changedPorts = new List<int>();
            foreach (LrmPort port in resource.AllocatedPorts)
            {
                int portNumber = int.Parse(port.Number);
                int resourceIndex = int.Parse(port.Index);
                bool isPortAvalible = node.Destinations[portNumber].Item2;
                object[] resources = node.Resources[portNumber];
                ReqType type = (ReqType)Enum.Parse(typeof(ReqType), resource.Type);
                switch (type)
                {
                    case ReqType.ALLOC:
                        {
                            resources[resourceIndex] = new object();

                            if (Array.TrueForAll(resources, el => el != null) && isPortAvalible)
                            {
                                node.Destinations[portNumber]
                                    = new Tuple<LrmDestination, bool>(node.Destinations[portNumber].Item1, false);
                                changedPorts.Add(portNumber);
                            }

                            break;
                        }
                    case ReqType.DELLOC:
                        {
                            resources[resourceIndex] = null;

                            if (!Array.TrueForAll(resources, el => el != null) && !isPortAvalible)
                            {
                                node.Destinations[portNumber]
                                    = new Tuple<LrmDestination, bool>(node.Destinations[portNumber].Item1, true);
                                changedPorts.Add(portNumber);
                            }

                            break;
                        }
                }
            }

            if (changedPorts.Count > 0)
            {
                LocalTopology();
            }
        }
        //TODO zrobić ogarnianie indeksów
        public void HandleConnectionRequest(string data, AsyncCommunication async, ReqType type)
        {
            ConnectionRequest connection = JsonConvert.DeserializeObject<ConnectionRequest>(data);

            if (HighestCc == null)
            {
                HighestCc = async;
            }

            AllocationRegister.AddConnection(connection.Id, async, connection);
            int stepIndex = 0;
            foreach (ConnectionStep step in connection.Steps)
            {
                foreach (LrmPort port in step.Ports)
                {

                    if (port.Index == null
                        && port.Equals(step.Ports[0])
                        && stepIndex > 0)
                    {
                        ConnectionStep previous = connection.Steps[stepIndex - 1];
                        port.Index = previous.Ports.Count == 1 ? previous.Ports[0].Index : previous.Ports[1].Index;
                    }
                    else if (port.Index == null)
                    {
                        int? index = AllocNextEmpty(step.Node, int.Parse(port.Number));
                        if (index == null)
                        {
                            throw new Exception("Dupliacte Allocation");
                        }
                        port.Index = index.Value.ToString();
                    }
                }
                string stepId = connection.Id + step.Node + step.Ports.GetHashCode() as string;

                LrmReq request = new LrmReq
                {
                    ConnectionId = connection.Id,
                    Id = stepId,
                    Ports = step.Ports,
                    ReqType = type.ToString()
                };

                string allocRequest = JsonConvert.SerializeObject(request);

                AllocationRegister.RegisterStep(connection.Id, stepId);
                allocRequest = WrapWithHeader(LrmCommunicationType.DATA, LrmHeader.NONE, allocRequest);
                LrmRegister.ConnectedNodes[step.Node].Async.Send(allocRequest);

                stepIndex++;
            }
        }

        private int? AllocNextEmpty(string node, int port)
        {
            VirtualNode vNode = LrmRegister.ConnectedNodes[node];
            object[] resources = vNode.Resources[port];
            int index = -1;
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i] == null)
                {
                    index = i;
                    break;
                }
            }

            resources[index] = new object();
            return index < 0 ? (int?)null : index;
        }

        private void LocalTopology(List<int> changedPorts)
        {

        }
        private void LocalTopology()
        {
            List<TopologyData> nodes = new List<TopologyData>();
            foreach (VirtualNode node in LrmRegister.ConnectedNodes.Values)
            {
                List<TopologyConnections> connections = new List<TopologyConnections>();

                foreach (int portNumber in node.Destinations.Keys)
                {
                    TopologyConnections connection = new TopologyConnections
                    {
                        Destination = node.Destinations[portNumber].Item1,
                        Port = portNumber.ToString(),
                        Status = node.Destinations[portNumber].Item2 ? "FREE" : "OCCUPIED"
                    };
                    connections.Add(connection);
                }

                TopologyData nodeData = new TopologyData
                {
                    Domains = node.DomiansHierarchy,
                    Node = node.Name,
                    Data = connections
                };
                nodes.Add(nodeData);
            }

            TopologyReports LocalTopology = new TopologyReports
            {
                Protocol = "resources",
                Nodes = nodes
            };

            Console.WriteLine(JsonConvert.SerializeObject(LocalTopology));

        }

        private void HandleTokenData(string data, VirtualNode node)
        {
            LrmToken token = JsonConvert.DeserializeObject<LrmToken>(data);
            LrmToken invertedToken = InvertToken(token);
            AssignToken(token);
            AssignToken(invertedToken);
        }

        private LrmToken InvertToken(LrmToken token)
        {
            return new LrmToken
            {
                Tag = token.Reciver.Name,
                SenderPort = token.Reciver.Port,
                StmMaxIndex = token.StmMaxIndex,
                Reciver = new LrmDestination
                {
                    Name = token.Tag,
                    Port = token.SenderPort
                }
            };
        }
        object assigementLock = new object();
        private void AssignToken(LrmToken token)
        {
            lock (assigementLock)
            {
                VirtualNode node = LrmRegister.ConnectedNodes[token.Tag];

                int senderPort = int.Parse(token.SenderPort);
                int resourcesNumber = int.Parse(token.StmMaxIndex);

                if (!node.Resources.ContainsKey(senderPort))
                {
                    node.Resources.Add(senderPort, new object[resourcesNumber]);
                }

                if (!node.Destinations.ContainsKey(senderPort))
                {
                    node.Destinations.Add(senderPort, new Tuple<LrmDestination, bool>(token.Reciver, true));
                }
            }
        }

        private void SendLocalTopology()
        {
            Thread.Sleep(10000);
            LocalTopology();
            InitPahse = false;
        }
    }
}
