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
            if (sdhFrame.Rsoh.Checksum == BinaryInterleavedParity.generateBIP(((Frame.SDHFrame)sdhFrame).Content, 24))
            {
            }
            else { }
        }

        public void generateHeader(ref IFrame sdhFrame)
        {
            Frame.SDHFrame tempFrame = (Frame.SDHFrame)sdhFrame;
            ((Frame.SDHFrame)sdhFrame).Rsoh.Checksum = BinaryInterleavedParity.generateBIP(tempFrame.Content, 24);
        }

    }
}
