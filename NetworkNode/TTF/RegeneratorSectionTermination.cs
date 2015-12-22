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

        /// <summary>
        /// Evaluates the header.
        /// </summary>
        /// <param name="sdhFrame">The SDH frame.</param>
        /// <returns></returns>
        public bool evaluateHeader(IFrame sdhFrame)
        {
            //Remove RSOH header from tempFrame 
            Frame tempFrame = new Frame((Frame)sdhFrame);
            tempFrame.Rsoh = null;
            //Check BIP
            if (sdhFrame.Rsoh != null && sdhFrame.Rsoh.Checksum == BinaryInterleavedParity.GenerateBIP(((Frame)tempFrame), 8))
                return true;
            else return false;
        }


        /// <summary>
        /// Generates the header.
        /// </summary>
        /// <param name="sdhFrame">The SDH frame.</param>
        public void generateHeader(IFrame sdhFrame)
        {
            Frame tempFrame = (Frame)sdhFrame;
            tempFrame.Rsoh = null;
            if (sdhFrame.Rsoh != null)
            {
                ((Frame)sdhFrame).Rsoh.Checksum = BinaryInterleavedParity.GenerateBIP(tempFrame, 8);
            }
            else
            {
                ((Frame)sdhFrame).Rsoh = new Header(BinaryInterleavedParity.GenerateBIP(tempFrame, 8), null, null);
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
