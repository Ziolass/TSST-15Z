using NetworkNode.SDHFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.TTF
{
    interface SectionTerminator
    {
        bool evaluateHeader(IFrame sdhFrame);
        void generateHeader(IFrame sdhFrame);
    }
}
