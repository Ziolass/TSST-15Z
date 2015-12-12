using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Ports
{
    public interface IDisposable
    {
        bool Active { get; set; }
    }
}
