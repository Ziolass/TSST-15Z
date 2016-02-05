using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRM
{
    public class LrmRegister
    {
        public Dictionary<string,VirtualNode> ConnectedNodes { get; private set; }

        public LrmRegister()
        {
            ConnectedNodes = new Dictionary<string, VirtualNode>();
        }

        public VirtualNode FindNodeByConnection(AsyncCommunication ac)
        {
            foreach (VirtualNode node in ConnectedNodes.Values)
            {
                if (node.Async != null && node.Async.Equals(ac))
                {
                    return node;
                }
            }
            return null;
        }
    }
}
