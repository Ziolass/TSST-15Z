using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.LRM.Communication
{
    public class LrmIntroduce
    {
        public string Type { get; set; }
        public string Node { get; set; }
        public List<string> Domians { get; set; }
        public LrmIntroduce()
        {
            Type = "INTRODUCE";
        }
    }
}
