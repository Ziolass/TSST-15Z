using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Frame
{
    public interface IContent
    {
        ContentType Type { get; }

    }
}
