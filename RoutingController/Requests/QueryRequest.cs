using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingController.Elements;
using Newtonsoft.Json;

namespace RoutingController.Requests
{
    public class QueryRequest
    {
        public string Domain { get; private set; }
        public string Id { get; private set; }
        public List<NodeElement> Ends { get; private set; }

        [JsonConstructor]
        public QueryRequest(string domain, string id, List<NodeElement> ends)
        {
            this.Domain = domain;
            this.Id = id;
            this.Ends = new List<NodeElement>(ends);
        }
    }
}
