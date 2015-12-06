using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Frame
{

    public class TribiutaryUnit : IContent
    {
        public ContentType Type { get; private set; }
        public ContainerLevel Level { get; private set; }
        public string Pointer { get; set; }
        public VirtualContainer Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TribiutaryUnit"/> class.
        /// Create VC for this TU.
        /// </summary>
        /// <param name="level">The level.</param>
        public TribiutaryUnit(ContainerLevel level)
        {
            this.Level = level;
            this.Type = ContentType.TRIBUTARYUNIT;;
        }
        public VirtualContainer GetContent()
        {
            return this.Content;
        }
        public void SetContent(VirtualContainer value)
        {
            this.Content = value;
        }
    }
}

