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
    class MultiplexSectionTermination : SectionTerminator
    {
        private List<string> nextData;

        public MultiplexSectionTermination()
        {
            nextData = new List<string>();
        }

        /// <summary>
        /// Evaluates the header.
        /// </summary>
        /// <param name="sdhFrame">The SDH frame.</param>
        /// <returns></returns>
        public bool evaluateHeader(IFrame sdhFrame)
        {
            Frame tempFrame = new Frame((Frame)sdhFrame);
            tempFrame.Msoh = null;
            if (sdhFrame.Msoh != null && (sdhFrame.Msoh.Checksum == BinaryInterleavedParity.GenerateBIP(((Frame)sdhFrame).Content, 21, ((Frame)sdhFrame).Level)))
                return true;
            else return false;
        }

        /// <summary>
        /// Generates the MSOH header.
        /// </summary>
        /// <param name="sdhFrame">The SDH frame.</param>
        public void generateHeader(IFrame sdhFrame)
        {
            Frame tempFrame = (Frame)sdhFrame;
            if (sdhFrame.Msoh != null)
            {
                sdhFrame.Msoh.Checksum = BinaryInterleavedParity.GenerateBIP(((Frame)sdhFrame).Content, 21, ((Frame)sdhFrame).Level);
            }
            else
            {
                sdhFrame.Msoh = new Header(BinaryInterleavedParity.GenerateBIP(((Frame)sdhFrame).Content, 21, ((Frame)sdhFrame).Level), null, null);
            }
            
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