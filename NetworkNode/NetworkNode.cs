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

        public NetworkNode(HigherOrderPathConnection hpc, SynchronousPhysicalInterface spi)
        {
            this.spi = spi;
            this.hpc = hpc;
        }

        public void AddForwardingRecord(ForwardingRecord record)
        {
            hpc.AddForwardingRecord(record);
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
    }
}
