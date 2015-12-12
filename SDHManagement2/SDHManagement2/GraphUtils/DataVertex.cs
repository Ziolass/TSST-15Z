using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphX.PCL.Common.Models;

namespace SDHManagement2
{
   public class DataVertex : VertexBase
    {
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }

       public DataVertex(string text = "")
       {
           Text = string.IsNullOrEmpty(text) ? "New vertex" : text;
       }
    }
}
