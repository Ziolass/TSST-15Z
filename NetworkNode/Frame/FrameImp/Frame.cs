using System;
using System.Collections.Generic;

namespace NetworkNode.SDHFrame
{
    public class Frame : IFrame
    {
        public Header Msoh { get; set; }
        public Header Rsoh { get; set; }
        public StmLevel Level { get; set; }
        /// <summary>
        /// Gets or sets the VC4 list
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public List<IContent> Content { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Frame" /> class.
        /// This create empty Content List
        /// </summary>
        /// <param name="stmLevel">The STM level.</param>
        public Frame(StmLevel stmLevel)
        {
            Content = new List<IContent>();
            this.Level = stmLevel;
            for (int x = 0; x < ConvertSTMLevel(Level); x++)
            {
                Content.Add(null); //Add empty place for VC4
            }
            this.Msoh = new Header();
            this.Rsoh = new Header();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class. STM-1
        /// This create empty Content List.
        /// </summary>
        public Frame()
        {
            Content = new List<IContent>();
            this.Level = StmLevel.STM1;
            Content.Add(null); //Add empty place for VC4
            this.Msoh = new Header();
            this.Rsoh = new Header();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="frame">The frame.</param>
        public Frame(Frame frame)
        {
            this.Content = new List<IContent>(frame.Content);
            this.Level = frame.Level;
            this.Msoh = new Header(frame.Msoh);
            this.Rsoh = new Header(frame.Rsoh);
        }
        /// <summary>
        /// Converts the STM level.
        /// </summary>
        /// <param name="stmLevel">The STM level.</param>
        /// <returns></returns>
        public int ConvertSTMLevel(StmLevel stmLevel)
        {
            switch (stmLevel)
            {
                case StmLevel.STM1:
                    return 1;
                case StmLevel.STM4:
                    return 4;
                case StmLevel.STM16:
                    return 16;
                case StmLevel.STM64:
                    return 64;
                case StmLevel.STM256:
                    return 256;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Gets the virtual container.
        /// </summary>
        /// <param name="level">The level of Virtual Container</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public IContent GetVirtualContainer(VirtualContainerLevel level, int index)
        {
            IContent returnContent = null;
            if (level == VirtualContainerLevel.VC4)
            {
                if (this.Content.Count >= index + 1)
                {
                    returnContent = this.Content[index];
                }
            }
            else
            {
                int higherIndex = GetHigherContainerIndex(level, index);
                if (this.Content.Count >= higherIndex + 1)
                {
                    returnContent = this.Content[higherIndex];
                    if (VirtualContainer.isVirtualContainer(returnContent))
                    {
                        returnContent = ((VirtualContainer)returnContent).GetVirtualContainerAtIndex(level, index); //Get specific virtual container lower level
                    }
                }
            }
            return returnContent;
        }
        /// <summary>
        /// Gets the virtual container.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="hiIndex">Index of the hi.</param>
        /// <param name="lowIndex">Index of the low.</param>
        /// <returns></returns>
        public IContent GetVirtualContainer(VirtualContainerLevel level, int hiIndex, int? lowIndex)
        {
            IContent returnContent = null;
            if (level == VirtualContainerLevel.VC4)
            {
                if (this.Content.Count >= hiIndex + 1)
                {
                    returnContent = this.Content[hiIndex];
                }
            }
            else
            {
                if (this.Content.Count >= hiIndex + 1 && lowIndex != null)
                {
                    int lowerIndex = (int)lowIndex;
                    returnContent = this.Content[hiIndex];
                    if (VirtualContainer.isVirtualContainer(returnContent))
                    {
                        returnContent = ((VirtualContainer)returnContent).GetVirtualContainerAtIndex(level, lowerIndex); //Get specific virtual container lower level
                    }
                }
            }
            return returnContent;
        }
        /// <summary>
        /// Sets the virtual container. This overwrite the <see cref="Content" /> list member.
        /// Content must by VirtualContainer!
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="hiIndex">Index of the higher level virtual container (VC-4).</param>
        /// <param name="lowIndex">Index of the lower level virtual container.</param>
        /// <param name="content">The content. Virtual Container</param>
        /// <returns>
        /// True - success, False - fail
        /// </returns>
        public bool SetVirtualContainer(VirtualContainerLevel level, int hiIndex, int? lowIndex , IContent content)
        {
            if (VirtualContainer.isVirtualContainer(content))
            {
                VirtualContainer contentVC = (VirtualContainer)content;
                if (level == contentVC.Level && this.CalculateFreeSpace() >= Frame.ContainerSpaceConverter(level))
                {
                    if (level == VirtualContainerLevel.VC4)
                    {
                        if (this.Content.Count >= hiIndex + 1)
                        {
                            this.Content[hiIndex] = content;
                            return true;
                        }
                        else return false;
                    }
                    else
                    {
                        //int higherIndex = GetHigherContainerIndex(level, index);
                        if (this.Content.Count >= hiIndex + 1 && lowIndex != null)
                        {
                            int lowerIndex = (int)lowIndex;
                            IContent tempVirtualContainer = this.Content[hiIndex];
                            if (VirtualContainer.isVirtualContainer(tempVirtualContainer) && ((VirtualContainer)tempVirtualContainer).TryAddContainer(level, lowerIndex))
                            {
                                ((VirtualContainer)tempVirtualContainer).SetVirtualContainerAtIndex(level, lowerIndex, content);
                            }
                            else if (tempVirtualContainer == null) //Frame does not have VC4 to keep lower virtual container levels
                            {
                                this.Content[hiIndex] = new VirtualContainer(VirtualContainerLevel.VC4);
                                tempVirtualContainer = this.Content[hiIndex];
                                ((VirtualContainer)tempVirtualContainer).SetVirtualContainerAtIndex(level, lowerIndex, content);
                            }
                            return true;
                        }
                        else return false;
                    }
                }
                else return false;
            }
            return false;
        }


        /// <summary>
        /// Gets the relative index for virtual container index position.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private int GetRelativeIndex(VirtualContainerLevel level, int index)
        {
            int returnIndex = -1;
            switch (level)
            {
                case VirtualContainerLevel.VC12:
                    returnIndex = index - 21 * (GetHigherContainerIndex(level, index));
                    break;
                case VirtualContainerLevel.VC21:
                    returnIndex = index - 7 * (GetHigherContainerIndex(level, index));
                    break;
                case VirtualContainerLevel.VC32:
                    returnIndex = index - 3 * (GetHigherContainerIndex(level, index));
                    break;
                default:
                    returnIndex = index;
                    break;
            }
            return returnIndex;
        }

        /// <summary>
        /// Clears the virtual container.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="index">The number.</param>
        /// <returns></returns>
        public bool ClearVirtualContainer(VirtualContainerLevel level, int index)
        {
            if (level == VirtualContainerLevel.VC4)
            {
                this.Content[index] = null;
            }
            else
            {
                IContent tempContainer = this.Content[GetHigherContainerIndex(level, index)];
                if (VirtualContainer.isVirtualContainer(tempContainer))
                {
                    ((VirtualContainer)tempContainer).SetVirtualContainerAtIndex(level, index, null); //Get specific virtual container lower level
                }
            }
            return true;
        }
        /// <summary>
        /// Clears the virtual container.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="hiIndex">The number.</param>
        /// <param name="lowIndex">The number.</param>
        /// <returns></returns>
        public bool ClearVirtualContainer(VirtualContainerLevel level, int hiIndex, int? lowIndex)
        {
            if (level == VirtualContainerLevel.VC4)
            {
                this.Content[hiIndex] = null;
            }
            else
            {
                if (this.Content.Count >= hiIndex + 1 && lowIndex != null)
                {
                    int lowerIndex = (int)lowIndex;
                    IContent tempContainer = this.Content[hiIndex];
                    if (VirtualContainer.isVirtualContainer(tempContainer))
                    {
                        ((VirtualContainer)tempContainer).SetVirtualContainerAtIndex(level, lowerIndex, null); //Get specific virtual container lower level
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Gets the index of the higher level container.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private int GetHigherContainerIndex(VirtualContainerLevel level, int index)
        {
            int returnIndex = -1;
            switch (level)
            {
                case VirtualContainerLevel.VC12:
                    returnIndex = index / 63;
                    break;
                case VirtualContainerLevel.VC21:
                    returnIndex = index / 21;
                    break;
                case VirtualContainerLevel.VC32:
                    returnIndex = index / 3;
                    break;
                case VirtualContainerLevel.VC4:
                    returnIndex = index;
                    break;
                case VirtualContainerLevel.UNDEF:
                    break;
            }
            return returnIndex;
        }

        /// <summary>
        /// Calculates the free space in <see cref="Frame"/>
        /// </summary>
        /// <returns></returns>
        private int CalculateFreeSpace()
        {

            int freeSpace = 63 * this.ConvertSTMLevel(this.Level);
            for (int i = 0; i < Content.Count; i++)
            {
                if (VirtualContainer.isVirtualContainer(Content[i]))
                {
                    VirtualContainer VC = (VirtualContainer)Content[i];
                    freeSpace -= VC.CalculateSpace();
                }
            }
            return freeSpace;
        }
        /// <summary>
        /// Convert <see cref="ContainerLevel"/> enum to space occupied in <see cref="Frame"/>
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int ContainerSpaceConverter(VirtualContainerLevel value)
        {
            switch (value)
            {
                case VirtualContainerLevel.VC12:
                    return 1;
                case VirtualContainerLevel.VC21:
                    return 3;
                case VirtualContainerLevel.VC32:
                    return 21;
                case VirtualContainerLevel.VC4:
                    return 63;
                default:
                    return 63;
            }
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
            returnString += this.Level.ToString() + "|";
            if (this.Msoh != null)
                returnString += "MSOH|";
            else returnString += "null|";
            if (this.Rsoh != null)
                returnString += "RSOH";
            else returnString += "null";
            foreach (IContent item in this.Content)
            {
                if (item != null && VirtualContainer.isVirtualContainer(item))
                {
                    returnString += "|" + ((VirtualContainer)item).Level.ToString();
                    returnString += "+POH";
                    string VCcontent = string.Empty;
                    foreach (var innerItem in ((VirtualContainer)item).Content)
                    {
                        if (innerItem != null && VirtualContainer.isVirtualContainer(innerItem))
                        {
                            VCcontent += ((VirtualContainer)innerItem).ToString();
                        }
                    }
                    if (VCcontent != string.Empty)
                    {
                        returnString += "[" + VCcontent + "]";
                    }
                }
            }
            return returnString.Remove(returnString.LastIndexOf('|'),1);
        }
    }
}