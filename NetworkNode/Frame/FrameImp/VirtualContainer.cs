using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Frame
{

    public class VirtualContainer : IContent
    {
        public ContentType Type { get; private set; }
        public VirtualContainerLevel Level { get; private set; }
        public string Pointer { get; set; }
        public string POH { get; set; }
        public Container Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualContainer"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        public VirtualContainer(VirtualContainerLevel level)
        {
            this.Level = level;
            this.Type = ContentType.VICONTAINER;
        }        
        public static bool isVirtualContainer(IContent content)
        {
            if (content != null && content.Type == ContentType.VICONTAINER)
                return true;
            else return false;
        }
    }
}
