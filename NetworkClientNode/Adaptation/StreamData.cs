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
        public int HigherPathOut { get; set; }
        public int? LowerPathOut { get; set; }
        public int Port { get; set; }
        public StmLevel Stm { get; set; }
        public VirtualContainerLevel VcLevel { get; set; }

        public StreamData(int port, StmLevel stm, VirtualContainerLevel vcLevel, int hpo, int? lpo)
        {
            Port = port;
            Stm = stm;
            VcLevel = vcLevel;
            HigherPathOut = hpo;
            LowerPathOut = lpo;
        }

        public bool Equals(StreamData other)
        {
            if (other == null)
            {
                return false;
            } 

            bool areNullableEqual = NullablePartEqality(other);
            
            return areNullableEqual &&
                HigherPathOut == other.HigherPathOut &&
                Port == other.Port &&
                Stm == other.Stm &&
                VcLevel == VcLevel;
        }

        private bool NullablePartEqality(StreamData other)
        {
            if (LowerPathOut == null && other.LowerPathOut != null || LowerPathOut != null && other.LowerPathOut == null)
            {
                return false;
            }
            if ((LowerPathOut != null && other.LowerPathOut != null) && (LowerPathOut != other.LowerPathOut))
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
            return string.Format("{0}-{1}-{2}-{3}-{4}", HigherPathOut, LowerPathOut == null ? "" : "" + LowerPathOut, Port, Stm, VcLevel).GetHashCode();
        }
    }
}
