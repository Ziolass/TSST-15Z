using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.HPC
{

    public class ForwardingRecord
    {
        public VirtualContainerLevel ContainerLevel { get; private set; }
        public int VcNumberIn { get; private set; }
        public int VcNumberOut { get; private set; }
        public int OutputPort { get; private set; }
        public int InputPort { get; private set; }
        public StmLevel Stm { get; private set; }

        public ForwardingRecord(int inputPort, int outputPort, StmLevel stm, VirtualContainerLevel containerLevel, int vcNumberIn, int vcNumberOut)
        {
            OutputPort = outputPort;
            InputPort = inputPort;
            ContainerLevel = containerLevel;
            VcNumberIn = vcNumberIn;
            VcNumberOut = vcNumberOut;
            InputPort = inputPort;
            Stm = stm;
        }

    }
}
