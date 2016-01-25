using RoutingController.Interfaces;

namespace RoutingController.Elements
{
    public class SNPP : ISNPP
    {
        public string NodeName { get; set; }
        public int Port { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SNPP" /> class.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="port">The port.</param>
        public SNPP(string nodeName, int port)
        {
            this.NodeName = nodeName;
            this.Port = port;
        }
        public SNPP(string nodeName)
        {
            this.NodeName = nodeName;
            this.Port = 0;
        }

        public override string ToString()
        {
            return NodeName + ": " + Port.ToString();
        }

    }
}