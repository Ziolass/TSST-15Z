using NetworkNode.Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.TTF
{
    class MultiplexSectionTermination : SectionTerminator
    {
        public void evaluateHeader(IFrame sdhFrame)
        {

        }

        /// <summary>
        /// Generates the MSOH header.
        /// </summary>
        /// <param name="sdhFrame">The SDH frame.</param>
        public void generateHeader(ref IFrame sdhFrame)
        {
            Frame.Frame tempFrame = (Frame.Frame)sdhFrame;

            //byte bitFrame = Convert.ToByte(tempFrame);


            ((Frame.Frame)sdhFrame).Msoh = "";
        }
    }
}
