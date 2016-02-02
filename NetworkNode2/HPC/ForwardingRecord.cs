using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.HPC
{

    public class ForwardingRecord : IEquatable<ForwardingRecord>
    {
        public VirtualContainerLevel ContainerLevel { get; private set; }
        public int VcNumberIn { get; private set; }
        public int HigherPathIn { get; private set; }
        public int VcNumberOut { get; private set; }
        public int OutputPort { get; private set; }
        public int HigherPathOut { get; private set; }
        public int InputPort { get; private set; }

        public ForwardingRecord(int inputPort, int outputPort, VirtualContainerLevel containerLevel, int vcNumberIn, int vcNumberOut, int hPathIn, int hPathOut)
        {
            OutputPort = outputPort;
            InputPort = inputPort;
            ContainerLevel = containerLevel;
            VcNumberIn = vcNumberIn;
            VcNumberOut = vcNumberOut;
            InputPort = inputPort;
            HigherPathIn = hPathIn;
            HigherPathOut = hPathOut;
        }

        public override bool Equals(object obj)
        {
            ForwardingRecord rec = obj as ForwardingRecord;
            return rec == null ? false : Equals(rec);
        }
        public override int GetHashCode()
        {
            return ("" + ContainerLevel
                + "" + VcNumberIn
                + "" + HigherPathIn
                + "" + VcNumberOut
                + "" + HigherPathOut
                + "" + OutputPort
                + "" + InputPort).GetHashCode();
        }
        public bool Equals(ForwardingRecord other)
        {
            if (this == other)
            {
                return true;
            }

            return this.ContainerLevel == other.ContainerLevel
                && this.HigherPathIn == other.HigherPathIn
                && this.HigherPathOut == other.HigherPathOut
                && this.InputPort == other.InputPort
                && this.OutputPort == other.OutputPort
                && this.VcNumberIn == other.VcNumberIn
                && this.VcNumberOut == other.VcNumberOut;
        }

    }
}
