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
        private List<string> nextData;

        public RegeneratorSectionTermination()
        {
            nextData = new List<string>();
        }

        public void evaluateHeader(IFrame sdhFrame)
        {

            if (sdhFrame.Rsoh != null && sdhFrame.Rsoh.Checksum == BinaryInterleavedParity.generateBIP(((SDHFrame.Frame)sdhFrame).Content, 24))
            {
            }
            else { }
        }

        public void generateHeader(ref IFrame sdhFrame)
        {
            if (sdhFrame.Rsoh == null)
            {
                return;
            }

            SDHFrame.Frame tempFrame = (SDHFrame.Frame)sdhFrame;
            ((SDHFrame.Frame)sdhFrame).Rsoh.Checksum = BinaryInterleavedParity.generateBIP(tempFrame.Content, 24);
            if (nextData.Count > 0)
            {
                ((SDHFrame.Frame)sdhFrame).Rsoh.DCC = nextData[0];
                nextData.RemoveAt(0);
            }

            
        }

        public void SetNextData(string data)
        {
            nextData.Add(data);
        } 

    }
}
