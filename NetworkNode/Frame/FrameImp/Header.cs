using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.SDHFrame
{
    public class Header : IContent
    {
        public string Checksum { get; set; }
        public string DCC { get; set; } //Data communication chanel
        public string EOW { get; set; } //Engineering Orderwire Serivce - kanał dla komunikacji służbowej
        public ContentType Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Header"/> class.
        /// </summary>
        /// <param name="checksum">The checksum.</param>
        /// <param name="eow">The eow.</param>
        public Header(string checksum, string eow, string dcc)
        {
            this.Checksum = checksum;
            this.EOW = eow;
            this.DCC = dcc;
            this.Type = ContentType.HEADER;
        }
        /// <summary>
        /// Initializes a new empty instance of the <see cref="Header"/> class.
        /// </summary>
        public Header()
        {
            this.Checksum = null;
            this.EOW = null;
            this.DCC = null;
            this.Type = ContentType.HEADER;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Header"/> class.
        /// </summary>
        /// <param name="header">The header.</param>
        public Header(Header header)
        {
            this.Checksum = header.Checksum;
            this.EOW = header.EOW;
            this.DCC = header.DCC;
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string returnString = string.Empty;
            returnString += "Parity: " + this.Checksum + "\n";
            returnString += "EOW: " + this.EOW + "\n";
            returnString += "DCC: " + this.DCC;            
            return returnString;
        }
    }
}
