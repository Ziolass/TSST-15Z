using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphX.PCL.Common.Models;

namespace SDHManagement2
{
    public class DataEdge: EdgeBase<DataVertex>
    {
        public DataEdge(DataVertex source, DataVertex target, double weight =1)
            : base(source,target,weight)
        { }

        public DataEdge()
            : base(null, null, 1)
        { }

        public string Text { get; set; }
        public override string ToString()
        {
            return Text;
        }

    }
}
