using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireCloud.CloudLogic
{
    public class AbstractAddress : IEquatable<AbstractAddress>
    {
        public int Port { get; private set; }
        public string NodeId { get; private set; }

        public AbstractAddress(int Port, string NodeId)
        {
            this.Port = Port;
            this.NodeId = NodeId;
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as AbstractAddress);
        }

        public bool Equals(AbstractAddress other)
        {
            return Port == other.Port && NodeId.Equals(other.NodeId);  
        }
        
        public override int GetHashCode()
        {
            return NodeId == null ? 0 : NodeId.GetHashCode();
        }
    }
}
