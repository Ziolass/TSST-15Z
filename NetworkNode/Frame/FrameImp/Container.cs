using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NetworkNode.Frame
{
    public class Container : IContent
    {
        public ContentType Type { get; private set; }
        public string Content { get; private set; }

        public Container(string content)
        {
            this.Type = ContentType.CONTAINER;
            this.Content = content;
        }
        public static bool isContainer(IContent content)
        {
            /*if (content != null && content.Type == ContentType.CONTAINER)
                return true;
            else return false;*/
            return content != null && content.Type == ContentType.CONTAINER;
        }
    }

}
