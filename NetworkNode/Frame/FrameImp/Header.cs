using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Frame
{
    public class Header : IContent
    {
        public string Checksum { get; set; }
        public string EOW { get; set; } //Engineering Orderwire Serivce - kanał dla komunikacji służbowej
        public ContentType Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Header"/> class.
        /// </summary>
        /// <param name="checksum">The checksum.</param>
        /// <param name="eow">The eow.</param>
        public Header(string checksum, string eow)
        {
            this.Checksum = checksum;
            this.EOW = eow;
            this.Type = ContentType.HEADER;
        }
        /// <summary>
        /// Determines whether the specified content is header.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static bool isHeader(IContent content)
        {
            if (content != null && content.Type == ContentType.HEADER)
                return true;
            else return false;
        }
    }
}
