<<<<<<< HEAD
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
        public bool evaluateHeader(IFrame sdhFrame)
        {
            //Remove RSOH header from tempFrame 
            Frame tempFrame = (Frame)sdhFrame;
            tempFrame.Rsoh = null;
            //Check BIP
            if (sdhFrame.Rsoh.Checksum == BinaryInterleavedParity.generateBIP(((Frame)tempFrame), 24))
                return true;
            else return false;
        }

        public void generateHeader(IFrame sdhFrame)
        {
            SDHFrame.Frame tempFrame = (SDHFrame.Frame)sdhFrame;
            tempFrame.Rsoh = null;
            ((SDHFrame.Frame)sdhFrame).Rsoh.Checksum = BinaryInterleavedParity.generateBIP(tempFrame, 24);
        }
    }
}
=======
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
    class RegeneratorSectionTermination : SectionTerminator
    {
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
        }

    }
}
>>>>>>> origin/dev
