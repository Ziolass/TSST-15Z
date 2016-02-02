using LRM.Communication;
using NetworkNode.LRM;
using NetworkNode.LRM.Communication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRM
{

    public class LinkResourceManager
    {
        private LrmServer LrmServer;
        private LrmRegister LrmRegister;
        private RcClinet RcClinet;
        private string DomianScope;


        public LinkResourceManager(int serverPort, int rcPort, int ccPort, string domianScope)
        {
            DomianScope = domianScope;
            LrmRegister = new LrmRegister();
            LrmServer = new LrmServer(serverPort, HandleNodeData, LrmRegister);
            LrmServer.NodeConnected += new NodeConnected(HandleNodeConnection);
            RcClinet = new RcClinet(rcPort, HandleRcData);
        }

        public void HandleRcData(string data, AsyncCommunication async)
        {
            Console.WriteLine("RC: " + data);
        }

        public void HandleNodeData(string data, VirtualNode node)
        {
            Console.WriteLine(data);
            if(data.Contains(ReqType.ALLOC.ToString())
                || data.Contains(ReqType.DELLOC.ToString())) 
            {
                HandleResourceLocationData(data, node);
            }

            HandleTokenData(data,node);
        }

        

        public void HandleNodeConnection(string NodeName)
        {
            AsyncCommunication async = LrmRegister.ConnectedNodes[NodeName].Async;
            LrmToken token = new LrmToken
            {
                Tag = NodeName
            };
            string data = JsonConvert.SerializeObject(token);
            data = WrapWithHeader(LrmCommunicationType.SIGNALLING,LrmHeader.BROADCAST, data);

            async.Send(data);
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
            builder.Append(header);
            builder.Append("#");
            builder.Append(data);
            return builder.ToString();
        }

        private void HandleResourceLocationData(string data, VirtualNode node)
        {
            ResourceLocation resource = JsonConvert.DeserializeObject<ResourceLocation>(data);
            List<int> changedPorts = new List<int>();
            foreach(LrmPort port in resource.AllocatedPorts) 
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
                                    = new Tuple<LrmDestination,bool>(node.Destinations[portNumber].Item1, false);
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
                LocalTopology(changedPorts);
            }
        }

        private void LocalTopology(List<int> changedPorts)
        {

        }
        private void LocalTopology()
        {
            List<TopologyData> nodes = new List<TopologyData>();
            foreach(VirtualNode node in LrmRegister.ConnectedNodes.Values) 
            {
                List<TopologyConnections> connections = new List<TopologyConnections>();
                
                foreach(int portNumber in node.Destinations.Keys) {
                    TopologyConnections connection = new TopologyConnections {
                        Destination = node.Destinations[portNumber].Item1,
                        Port = portNumber.ToString(),
                        Status = node.Destinations[portNumber].Item2 ? "FREE": "OCCUPIED"
                    };
                }

                TopologyData nodeData = new TopologyData{
                    Domains = node.DomiansHierarchy,
                    Node = node.Name,
                    Data = connections
                };
                nodes.Add(nodeData);
            }

            TopologyReports LocalTopology = new TopologyReports {
                Protocol = "resources",
                Nodes = nodes
            };
            

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

        private void AssignToken(LrmToken token)
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
                node.Destinations.Add(senderPort, new Tuple<LrmDestination, bool>(token.Reciver,true));
            }
        }
    }
}
