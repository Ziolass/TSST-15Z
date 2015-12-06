using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Frame
{
    public interface IFrameBuilder
    {
        IFrame BuildFrame(String textFrame);
        IFrame BuildEmptyFrame();
        String BuildLiteral(IFrame textFrame);
    }
}