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

        public bool evaluateHeader(IFrame sdhFrame)
        {
            Frame tempFrame = (Frame)sdhFrame;
            tempFrame.Msoh = null;
            if (sdhFrame.Msoh != null && (sdhFrame.Msoh.Checksum == BinaryInterleavedParity.generateBIP(((VirtualContainer)sdhFrame).Content, 21)))
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
                sdhFrame.Msoh.Checksum = BinaryInterleavedParity.generateBIP(((Frame)sdhFrame).Content, 21);
            }
            else
            {
                sdhFrame.Msoh = new Header(BinaryInterleavedParity.generateBIP(((Frame)sdhFrame).Content, 21), null, null);
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