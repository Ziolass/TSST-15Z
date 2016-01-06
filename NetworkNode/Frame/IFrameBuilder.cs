using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.SDHFrame
{
    public interface IFrameBuilder
    {
        IFrame BuildFrame(String textFrame);
        String BuildLiteral(IFrame textFrame);
    }
}