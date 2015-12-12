using NetworkNode.HPC;
using NetworkNode.MenagmentModule;
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
        public NetworkNode(HigherOrderPathConnection hpc)
        {
            this.hpc = hpc;
        }

        public void AddForwardingRecord(ForwardingRecord record)
        {
            hpc.AddForwardingRecord(record);
        }

        public bool DisableNode()
        {
            return true;
        }

        public bool DisableInterface(int number)
        {
            return true;
        }

        public List<ForwardingRecord> GetForwardingRecords()
        {
            return hpc.GetForwardingRecords();
        }
    }
}
