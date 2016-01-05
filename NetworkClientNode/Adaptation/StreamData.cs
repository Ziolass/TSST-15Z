using NetworkNode.SDHFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClientNode.Adaptation
{
    public class StreamData : IEquatable<StreamData>
    {
        public int HigherPath { get; set; }
        public int? LowerPath { get; set; }
        public int Port { get; set; }
        public StmLevel Stm { get; set; }
        public VirtualContainerLevel VcLevel { get; set; }

        public StreamData(int port, StmLevel stm, VirtualContainerLevel vcLevel, int hpo, int? lpo)
        {
            Port = port;
            Stm = stm;
            VcLevel = vcLevel;
            HigherPath = hpo;
            LowerPath = lpo;
        }

        public bool Equals(StreamData other)
        {
            if (other == null)
            {
                return false;
            } 

            bool areNullableEqual = NullablePartEqality(other);
            
            return areNullableEqual &&
                HigherPath == other.HigherPath &&
                Port == other.Port &&
                Stm == other.Stm &&
                VcLevel == VcLevel;
        }

        private bool NullablePartEqality(StreamData other)
        {
            if (LowerPath == null && other.LowerPath != null || LowerPath != null && other.LowerPath == null)
            {
                return false;
            }
            if ((LowerPath != null && other.LowerPath != null) && (LowerPath != other.LowerPath))
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StreamData);
        }
        public override int GetHashCode()
        {
            return string.Format("{0}-{1}-{2}-{3}-{4}", HigherPath, LowerPath == null ? "" : "" + LowerPath, Port, Stm, VcLevel).GetHashCode();
        }
    }
}
