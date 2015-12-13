namespace NetworkNode.SDHFrame
{
    /// <summary>
    /// THis class represent Virtual Container (VC) of SDH frame
    /// </summary>
    public class VirtualContainer : IContent
    {
        public ContentType Type { get; private set; }
        public VirtualContainerLevel Level { get; private set; }
        public string Pointer { get; set; }
        public string POH { get; set; }
        public Container Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualContainer" /> class.
        /// </summary>
        /// <param name="level">The level.</param>
        public VirtualContainer(VirtualContainerLevel level)
        {
            this.Level = level;
            this.Type = ContentType.VICONTAINER;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualContainer"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="content">The content.</param>
        public VirtualContainer(VirtualContainerLevel level, Container content)
        {
            this.Level = level;
            this.Type = ContentType.VICONTAINER;
            this.Content = content;
        }
        /// <summary>
        /// Determines whether the specified content is virtual container.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static bool isVirtualContainer(IContent content)
        {
            if (content != null && content.Type == ContentType.VICONTAINER)
                return true;
            else return false;
        }
    }
}