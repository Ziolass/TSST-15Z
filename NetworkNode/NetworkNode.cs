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

        public string Id { get; private set; }

        public NetworkNode(HigherOrderPathConnection hpc, TransportTerminalFunction ttf, string id)
        {
            Ttf = ttf;
            Ttf.HandleLrmData += new HandleLrmData(ReportLrmToken);
            Hpc = hpc;
            Id = id;
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

        public bool RemoveTwWayRecord(List<ForwardingRecord> record)
        {
            return Hpc.RemoveTwWayRecord(record);
        }

        public void SendLrmToken(string lrmToken)
        {

            foreach (KeyValuePair<int, StmLevel> port in Ttf.GetPorts())
            {
                LrmToken token = new LrmToken
                {
                    Tag = lrmToken,
                    SenderPort = port.Key.ToString()
                };

                Ttf.SendLrmData(port.Key, JsonConvert.SerializeObject(token));
            }
        }

        private void ReportLrmToken(object sender, InputLrmArgs args)
        {
            LrmToken token = JsonConvert.DeserializeObject<LrmToken>(args.Data);
            token.Reciver = new LrmDestination();
            token.Reciver.Name = Id;
            token.Reciver.Port = args.PortNumber.ToString();
            LrmClient.ReportToken(token);
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
                        Alloc(request.Ports);
                        break;
                    }
                case ReqType.DELLOC:
                    {
                        Delloc(request.Ports);
                        break;
                    }
            }

        }

        private void Alloc(List<LrmPort> ports)
        {
            Hpc.Allocate(ports);
        }

        private void Delloc(List<LrmPort> ports)
        {

        }
    }
}
