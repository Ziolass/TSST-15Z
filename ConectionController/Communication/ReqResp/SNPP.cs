using Newtonsoft.Json;
using System.Collections.Generic;

namespace RoutingController.Elements
{
    public class SNPP
    {
        public List<SNP> Nodes { get; set; }

        [JsonConstructor]
        public SNPP(List<SNP> nodes)
        {
            this.Steps = new List<SNP>(nodes);
        }

        public override string ToString()
        {
            string returnString = string.Empty;
            foreach (var item in Steps)
            {
                returnString += item.ToString();
            }
            return returnString;
        }

    }
}