using NetworkNode.SDHFrame;
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
            if (sdhFrame.Rsoh.Checksum == BinaryInterleavedParity.generateBIP(((SDHFrame.Frame)sdhFrame).Content, 24))
            {
            }
            else { }
        }

        public void generateHeader(ref IFrame sdhFrame)
        {
            SDHFrame.Frame tempFrame = (SDHFrame.Frame)sdhFrame;
            ((SDHFrame.Frame)sdhFrame).Rsoh.Checksum = BinaryInterleavedParity.generateBIP(tempFrame.Content, 24);
        }

    }
}
