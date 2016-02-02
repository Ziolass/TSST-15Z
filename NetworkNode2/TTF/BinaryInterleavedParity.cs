using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkNode.SDHFrame;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetworkNode.TTF
{
    public class BinaryInterleavedParity
    {
        /// <summary>
        /// Generates the BIP checksum
        /// </summary>
        /// <param name="sdhFrame">The SDH frame.</param>
        /// <param name="block">The block.</param>
        /// <returns></returns>
        public static string GenerateBIP(IFrame sdhFrame, int blockCount)
        {
            FrameBuilder fmb = new FrameBuilder();
            String checksum = String.Empty;
            byte[] bitFrame = BinaryInterleavedParity.ObjectToByteArray(fmb.BuildLiteral(sdhFrame));
            int hopSize = bitFrame.Length / blockCount;
            for (int z = 0; z < blockCount; z++)
            {
                byte block = 0;
                for (int x = z; x < z * hopSize && x < bitFrame.Length; x++)
                {
                    block = (byte)(block + bitFrame[x]);
                }
                block = (byte)(block % 2);
                checksum += block.ToString();
            }
            return checksum;
        }
        /// <summary>
        /// Generates the BIP checksum from content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="blockCount">The block count.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public static string GenerateBIP(List<IContent> content, int blockCount, StmLevel level)
        {
            Frame bipFrame = new Frame(level);
            bipFrame.Content = new List<IContent>(content);
            return BinaryInterleavedParity.GenerateBIP(bipFrame, blockCount);
        }
        /// <summary>
        /// Objects to byte array.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        /// <summary>
        /// Bytes the array to object.
        /// </summary>
        /// <param name="arrBytes">The arr bytes.</param>
        /// <returns></returns>
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}
