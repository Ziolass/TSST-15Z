using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Interfaces;

namespace RoutingController.Elements
{
    public class SNP : ISNP
    {
        public string Name { get; private set; }
        public string Domain { get; set; }
        public List<string> Ports { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SNP"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="ports">The ports.</param>
        public SNP(string name, string domain, List<string> ports)
        {
            this.Name = name;
            this.Domain = domain;
            this.Ports = ports;
        }
    }
}
