using NetworkNode.Frame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.TTF
{
    class RegeneratorSectionTermination : SectionTerminator
    {
        public void evaluateHeader(IFrame sdhFrame)
        {
            if (sdhFrame.Rsoh == BinaryInterleavedParity.generateBIP(sdhFrame, 24))
            {
            }
            else { }
        }

        public void generateHeader(ref IFrame sdhFrame)
        {
            Frame.Frame tempFrame = (Frame.Frame)sdhFrame;
            ((Frame.Frame)sdhFrame).Rsoh = BinaryInterleavedParity.generateBIP(tempFrame, 24);
        }

    }
}
