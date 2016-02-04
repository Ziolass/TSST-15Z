
using Newtonsoft.Json;
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

        public string Name { get; private set; }

        public string Port { get; private set; }

        public DestinationType Type { get; private set;}

        public Destination()
        {
            this.Scope = null;
            this.Name = null;
            this.Port = null;
        }

        [JsonConstructor]
        public Destination(string scope, string name, string port)
        {
            this.Scope = scope;
            this.Name = name;
            this.Port = port;
            this.Type = DestinationType.LOCAL;
        }
        public Destination(string scope, string name, string port, DestinationType type)
        {
            this.Scope = scope;
            this.Name = name;
            this.Port = port;
            this.Type = type;
        }

        /// <summary>
        /// Return the identifier of destination node.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string NodeId()
        {
            return Name + ":" + Port.ToString();
        }
    }
}
