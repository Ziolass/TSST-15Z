using NetworkNode.HPC;
using NetworkNode.LRM;
using NetworkNode.LRM.Communication;
using NetworkNode.MenagmentModule;
using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    public class NetworkNode
    {
        private HigherOrderPathConnection Hpc;
        private TransportTerminalFunction Ttf;
        private LrmClient LrmClient;
        private LrmIntroduce LrmIntroduce;
        public string Id { get; private set; }

        public NetworkNode(HigherOrderPathConnection hpc,
            TransportTerminalFunction ttf,
            LrmIntroduce lrmIntroduce,
            int lrmPort)
        {
            Ttf = ttf;
            LrmIntroduce = lrmIntroduce;
            LrmClient = new LrmClient(lrmPort, SendLrmToken, HandleLrmResourceManagement);
            Ttf.HandleLrmData += new HandleLrmData(ReportLrmToken);
            Hpc = hpc;
            Hpc.LinkResourceAlloc += new LinkResourceAlloc(SendLrmResourceAlloc);
            Hpc.LinkResourceDelloc += new LinkResourceDelloc(SendLrmResourceDelloc);
            Id = lrmIntroduce.Node;
        }

        public ExecutionResult AddForwardingRecords(List<List<ForwardingRecord>> records)
        {
            return Hpc.AddForwardingRecords(records, true);
        }

        public bool ShudownInterface(int number)
        {
            return Ttf.ShudownInterface(number);
        }

        public List<ForwardingRecord> GetConnections()
        {
            return Hpc.GetConnections();
        }

        public Dictionary<int, StmLevel> GetPorts()
        {
            return Ttf.GetPorts();
        }

        internal bool DisableNode()
        {
            return true;
        }

        public void AddRsohContent(string dccContent)
        {
            Ttf.AddRsohContent(dccContent);
        }

        public void AddMsohContent(string dccContent)
        {
            Ttf.AddMsohContent(dccContent);
        }

        public ExecutionResult RemoveTwWayRecord(List<ForwardingRecord> record)
        {
            return Hpc.RemoveTwWayRecord(record, false);
        }

        public void SendLrmToken(string lrmToken)
        {

            foreach (KeyValuePair<int, StmLevel> port in Ttf.GetPorts())
            {
                LrmToken token = JsonConvert.DeserializeObject<LrmToken>(lrmToken);
                int hPathNo = StmLevelExt.GetHigherPathsNumber(port.Value);
                int containers = VirtualContainerLevelExt.GetContainersNumber(Hpc.NetworkDefaultLevel);

                token.SenderPort = port.Key.ToString();
                token.StmMaxIndex = ((hPathNo * containers) - 1).ToString();


                Ttf.SendLrmData(port.Key, JsonConvert.SerializeObject(token));
            }
        }

        private void SendLrmResourceAlloc(object sender, List<LrmPort> ports)
        {
            LrmClient.SendLrmMessage(new ResourceLocation
            {
                Type = ReqType.ALLOC.ToString(),
                AllocatedPorts = ports
            });

        }

        private void SendLrmResourceDelloc(object sender, List<LrmPort> ports)
        {
            LrmClient.SendLrmMessage(new ResourceLocation
            {
                Type = ReqType.DELLOC.ToString(),
                AllocatedPorts = ports
            });
        }

        private void ReportLrmToken(object sender, InputLrmArgs args)
        {
            LrmToken token = JsonConvert.DeserializeObject<LrmToken>(args.Data);
            token.Reciver = new LrmDestination();
            token.Reciver.Name = Id;
            token.Reciver.Port = args.PortNumber.ToString();
            LrmClient.SendLrmMessage(token);
        }

        private void HandleLrmResourceManagement(string data)
        {
            LrmReq request = JsonConvert.DeserializeObject<LrmReq>(data);

            string textualRequest = request.ReqType;
            ReqType reqType = (ReqType)Enum.Parse(typeof(ReqType), textualRequest);

            switch (reqType)
            {
                case ReqType.ALLOC:
                    {
                        Alloc(request);
                        break;
                    }
                case ReqType.DELLOC:
                    {
                        Delloc(request);
                        break;
                    }
            }

        }

        private void Alloc(LrmReq request)
        {
            ExecutionResult allocationResult = Hpc.Allocate(request.Ports);
            LrmResp resp = new LrmResp
            {
                Type = ReqType.ALLOC.ToString(),
                Status = allocationResult.Result ?
                    LrmRespStatus.ACK.ToString()
                    : LrmRespStatus.ERROR.ToString(),
                Id = request.Id
            };
            LrmClient.SendLrmMessage(resp);
        }

        private void Delloc(LrmReq request)
        {
            ExecutionResult delocationResult = Hpc.FreeResources(request.Ports);
            LrmResp resp = new LrmResp
            {
                Type = ReqType.DELLOC.ToString(),
                Status = delocationResult.Result ?
                    LrmRespStatus.ACK.ToString()
                    : LrmRespStatus.ERROR.ToString(),
                Id = request.Id
            };
            LrmClient.SendLrmMessage(resp);
        }

        public void IntroduceToLrm()
        {
            LrmClient.SendLrmMessage(LrmIntroduce);
        }

        public void StartLrmClient()
        {
            LrmClient.Start();
        }
    }
}
