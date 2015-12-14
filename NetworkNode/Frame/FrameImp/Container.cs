namespace NetworkNode.SDHFrame
{
    /// <summary>
    /// Container is the like data in SDH Frame
    /// </summary>
    public class Container : IContent
    {
        public ContentType Type { get; private set; }
        public string Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        public Container(string content)
        {
            this.Type = ContentType.CONTAINER;
            this.Content = content;
        }

        /// <summary>
        /// Determines whether the specified content is container.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static bool isContainer(IContent content)
        {
            if (content != null && content.Type == ContentType.CONTAINER)
                return true;
            else return false;
        }
        public string ToString()
        {
            return this.Content;
        }
    }
}