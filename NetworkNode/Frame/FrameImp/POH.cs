using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.SDHFrame
{
    public enum SignalLabelType {UNDEF,ATM, E2, E3, E4, RESERVED, HDLC_PPP, TU_ELEMENTS }
    public class POH : IContent
    {
        public ContentType Type { get; private set; }
        public SignalLabelType SignalLabel { get; private set; }
        
        public POH()
        {
            this.Type = ContentType.POH;
            this.SignalLabel = SignalLabelType.UNDEF;
        }
        public POH(SignalLabelType signalLabel)
        {
            this.SignalLabel = signalLabel;
            this.Type = ContentType.POH;
        }
        /// <summary>
        /// Determines whether the specified content is poh.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static bool isPOH(IContent content)
        {
            if (content != null && content.Type == ContentType.POH)
                return true;
            else return false;
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString()
        {
            return this.SignalLabel.ToString();
        }
    }
}
