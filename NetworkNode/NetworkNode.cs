using NetworkNode.HPC;
using NetworkNode.MenagmentModule;
using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    public class NetworkNode
    {
        private HigherOrderPathConnection hpc;
        private TransportTerminalFunction ttf;

        public string Id { get; private set; }

        public NetworkNode(HigherOrderPathConnection hpc, TransportTerminalFunction ttf, string id)
        {
            this.ttf = ttf;
            this.hpc = hpc;
            Id = id;
        }

        public ExecutionResult AddForwardingRecords(List<List<ForwardingRecord>> records)
        {
            return hpc.AddForwardingRecords(records);
        }

        public bool ShudownInterface(int number)
        {
            return ttf.ShudownInterface(number);
        }

        public List<ForwardingRecord> GetConnections()
        {
            return hpc.GetConnections();
        }

        public Dictionary<int, StmLevel> GetPorts()
        {
            return ttf.GetPorts();
        }

        internal bool DisableNode()
        {
            return true;
        }

        public void AddRsohContent(string dccContent)
        {
            ttf.AddRsohContent(dccContent);
        }

        public void AddMsohContent(string dccContent)
        {
            ttf.AddMsohContent(dccContent);
        }

        public bool RemoveTwWayRecord(List<ForwardingRecord> record)
        {
            return hpc.RemoveTwWayRecord(record);
        }
    }
}
