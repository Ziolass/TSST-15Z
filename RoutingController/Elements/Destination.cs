
using RoutingController.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingController.Elements
{
    public class Destination : IDestination
    {
        public string Scope { get; private set; }

        public string Node { get; private set; }

        public int Port { get; private set; }

        public Destination(string scope, string node, int port)
        {
            this.Scope = scope;
            this.Node = node;
            this.Port = port;
        }

        /// <summary>
        /// Return the identifier of destination node.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string NodeId()
        {
            return Node + ":" + Port.ToString();
        }
    }
}
