using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Interfaces;

namespace RoutingController.Elements
{
    public class QueryRequest : IQuery
    {
        public string Source { get; private set; }
        public string Destination { get; private set; }
        public string LrmId { get; private set; }

        public QueryRequest(string source, string destination, string lrmId)
        {
            this.Source = source;
            this.Destination = destination;
            this.LrmId = lrmId;
        }
    }
}
