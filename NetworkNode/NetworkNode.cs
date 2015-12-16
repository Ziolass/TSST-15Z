using NetworkNode.HPC;
using NetworkNode.MenagmentModule;
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

        public int Id { get; private set; }

        public NetworkNode(HigherOrderPathConnection hpc, TransportTerminalFunction ttf, int id)
        {
            this.ttf = ttf;
            this.hpc = hpc;
            Id = id;
        }

        public ExecutionResult AddForwardingRecords(List<ForwardingRecord> records)
        {
            return hpc.AddForwardingRecords(records);
        }

        public bool ShudownInterface(int number)
        {
            return ttf.ShudownInterface(number);
        }

        public List<ForwardingRecord> GetForwardingRecords()
        {
            return hpc.GetForwardingRecords();
        }

        public List<List<int>> GetPorts()
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

        public bool RemoveRecord(ForwardingRecord record)
        {
            return hpc.RemoveRecord(record);
        }
    }
}
