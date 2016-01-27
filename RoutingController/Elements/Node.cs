using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Elements
{
    public class Node : IComparer
    {
        public string Name { get; set; }
        public int Port { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="ports">The ports.</param>
        public Node(string name, int ports)
        {
            this.Name = name;
            this.Port = ports;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="nodeId">The node identifier. (node id + port = node1:1)</param>
        public Node(string nodeId)
        {
            this.Name = nodeId.Substring(0, nodeId.IndexOf(":"));
            int port;
            int.TryParse(nodeId.Substring(nodeId.IndexOf(':') + 1), out port);
            this.Port = port;

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        public Node(Node node)
        {
            this.Name = node.Name;
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
            if (x is Node && y is Node)
            {
                if (((Node)x).Name == ((Node)y).Name && ((Node)x).Port.Equals(((Node)y).Port))
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
            return Name + ", " + Port;
        }

        /// <summary>
        /// Gets the node identifier.
        /// </summary>
        /// <returns></returns>
        public string GetNodeId()
        {
            return Name + ":" + Port;
        }

        public bool Equals(object y)
        {
            if (this is Node && y is Node)
            {
                if (this.Name == ((Node)y).Name && this.Port.Equals(((Node)y).Port))
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
