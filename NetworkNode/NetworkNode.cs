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
        private SynchronousPhysicalInterface spi;

        public int Id { get; private set; }

        public NetworkNode(HigherOrderPathConnection hpc, SynchronousPhysicalInterface spi, int id)
        {
            this.spi = spi;
            this.hpc = hpc;
            Id = id;
        }

        public ExecutionResult AddForwardingRecords(List<ForwardingRecord> records)
        {
            return hpc.AddForwardingRecords(records);
        }

        public bool ShudownInterface(int number)
        {
            return spi.ShudownInterface(number);
        }

        public List<ForwardingRecord> GetForwardingRecords()
        {
            return hpc.GetForwardingRecords();
        }

        public List<List<int>> GetPorts()
        {
            return spi.GetPorts();
        }

        internal bool DisableNode()
        {
            return true;
        }

        public void AddRsohContent(string rsoh)
        {

        }
        public void AddMsohContent(string msoh)
        {

        }
    }
}
