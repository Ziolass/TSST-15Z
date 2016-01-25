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
        public List<Link> Port { get; private set; }

        public Node(string name, List<Link> ports)
        {
            this.Name = name;
            this.Port = ports;
        }
        public Node(string name)
        {
            this.Name = name;
        }
        public Node()
        {
            this.Name = string.Empty;
            this.Port = new List<Link>();
        }

        public Node(Node node)
        {
            this.Name = node.Name;
            this.Port = new List<Link>();
            foreach (Link port in node.Port)
            {
                Port.Add(port);
            }
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

        public override string ToString()
        {
            string ports = string.Empty;
            Port.ForEach(x => ports += x.ToString());
            return Name + ", " + ports;
        }
    }
}
