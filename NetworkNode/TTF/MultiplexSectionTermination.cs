﻿using NetworkNode.SDHFrame;
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
        public void evaluateHeader(IFrame sdhFrame)
        {
            if (sdhFrame.Msoh !=null && sdhFrame.Msoh.Checksum == BinaryInterleavedParity.generateBIP(sdhFrame, 8))
            {
            }
            else { }
        }

        /// <summary>
        /// Generates the MSOH header.
        /// </summary>
        /// <param name="sdhFrame">The SDH frame.</param>
        public void generateHeader(ref IFrame sdhFrame)
        {
            if (sdhFrame.Msoh == null)
            {
                return;
            }
            SDHFrame.Frame tempFrame = (SDHFrame.Frame)sdhFrame;          
            ((SDHFrame.Frame)sdhFrame).Msoh.Checksum = BinaryInterleavedParity.generateBIP(tempFrame, 8);
        }
    }
}
