using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace RoutingController.Elements
{
    public class NodeElement : IComparer
    {
        public string Node { get; set; }
        public string Port { get; private set; }
        public string Domain { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node" /> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="ports">The ports.</param>
        [JsonConstructor]
        public NodeElement(string node, string port)
        {
            this.Node = node;
            this.Port = port;
            this.Domain = null;
        }
        public NodeElement(string node, string port, string domain)
        {
            this.Node = node;
            this.Port = port;
            this.Domain = domain;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="nodeId">The node identifier. (node id + port = node1:1)</param>
        public NodeElement(string nodeId)
        {
            this.Node = nodeId.Substring(0, nodeId.IndexOf(":"));
            this.Port = nodeId.Substring(nodeId.IndexOf(':') + 1);

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        public NodeElement(NodeElement node)
        {
            this.Node = node.Node;
            this.Port = node.Port;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero <paramref name="x" /> is less than <paramref name="y" />. Zero <paramref name="x" /> equals <paramref name="y" />. Greater than zero <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public int Compare(object x, object y)
        {
            if (x is NodeElement && y is NodeElement)
            {
                if (((NodeElement)x).Node == ((NodeElement)y).Node && ((NodeElement)x).Port.Equals(((NodeElement)y).Port))
                    return 0;
                else return 1;
            }
            else return 1;
        }


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Node + ", " + Port;
        }

        /// <summary>
        /// Gets the node identifier.
        /// </summary>
        /// <returns></returns>
        public string GetNodeId()
        {
            return Node + ":" + Port;
        }

        public bool Equals(object y)
        {
            if (this is NodeElement && y is NodeElement)
            {
                if (this.Node == ((NodeElement)y).Node && this.Port.Equals(((NodeElement)y).Port))
                    return true;
                else return false;
            }
            else return false;
        }

        public int GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
