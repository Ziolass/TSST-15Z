using System;
using System.Collections.Generic;

namespace NetworkNode.SDHFrame
{
    public class Frame : IFrame
    {
        public Header Msoh { get; set; }
        public Header Rsoh { get; set; }
        public StmLevel Level { get; set; }
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
                for (int i = 0; i < 63; i++)
                {
                    Content.Add(null);
                }
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
            for (int i = 0; i < 63; i++)
            {
                Content.Add(null);
            }
            this.Msoh = new Header();
            this.Rsoh = new Header();
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
        /// <param name="number">The number (index).</param>
        /// <returns></returns>
        public IContent GetVirtualContainer(VirtualContainerLevel level, int number)
        {
            IContent returnContent = this.Content[GetContainerIndex(level, number)];
            return returnContent;
        }

        /// <summary>
        /// Sets the virtual container. This overwrite the <see cref="Content" /> list member.
        /// Content must by VirtualContainer!
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="number">The number.</param>
        /// <param name="content">The content. Virtual Container</param>
        /// <returns>True - success, False - fail</returns>
        public bool SetVirtualContainer(VirtualContainerLevel level, int number, IContent content)
        {
            if (VirtualContainer.isVirtualContainer(content))
            {
                VirtualContainer contentVC = (VirtualContainer)content;
                if (level == contentVC.Level && this.CalculateFreeSpace() >= Frame.ContainerSpaceConverter(level))
                {
                    if (TestContainerSpace(level, number)) //Test if i can put in this location
                    {
                        this.Content[GetContainerIndex(level, number)] = content;
                        return true;
                    }
                }
                else return false;
            }
            return false;
        }

        public bool ClearVirtualContainer(VirtualContainerLevel level, int number)
        {
            this.Content[GetContainerIndex(level, number)] = null;
            return true; 
        }

        /// <summary>
        /// Tests adding the container to frame space.
        /// Check if Virtual Container have TU-space for itself
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private bool TestContainerSpace(VirtualContainerLevel level, int index)
        {
            bool testUp = false;
            bool testDown = false;
            switch (level)
            {
                case VirtualContainerLevel.VC12:
                    if (this.CheckContainerUp(level, index))
                        testUp = true;
                    testDown = true;
                    break;
                case VirtualContainerLevel.VC21:
                    if (this.CheckContainerUp(level, index))
                        testUp = true;
                    if (this.CheckContainerDown(level, index))
                        testDown = true;
                    break;
                case VirtualContainerLevel.VC32:
                    if (this.CheckContainerUp(level, index))
                        testUp = true;
                    if (this.CheckContainerDown(level, index))
                        testDown = true;
                    break;
                case VirtualContainerLevel.VC4:
                    if (this.CheckContainerDown(level, index))
                        testDown = true;
                    testUp = true;
                    break;
            }
            if (testUp && testDown)
                return true;
            else return false;
        }
        /// <summary>
        /// Checks if upper virtual container exists and has higher level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private bool CheckContainerUp(VirtualContainerLevel level, int index)
        {
            bool returnVal = false;
            int contentPosition = contentPosition = GetContainerIndex(level, index);
            int parentPostion;
            if (contentPosition == -1)
            {
                return false;
            }
            switch (level)
            {
                case VirtualContainerLevel.VC12:
                    parentPostion = index / 3;
                    if (CheckContainerUp(VirtualContainerLevel.VC21, parentPostion))
                    {
                        if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC12)
                            returnVal = false;
                        else returnVal = true;
                    }
                    else returnVal = false;
                    break;
                case VirtualContainerLevel.VC21:
                    parentPostion = index / 7;
                    if (CheckContainerUp(VirtualContainerLevel.VC32, parentPostion))
                    {
                        if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC21)
                            returnVal = false;
                        else returnVal = true;
                    }
                    else returnVal = false;
                    break;
                case VirtualContainerLevel.VC32:
                    parentPostion = index / 3;
                    if (CheckContainerUp(VirtualContainerLevel.VC4, parentPostion))
                    {
                        if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC32)
                            returnVal = false;
                        else returnVal = true;
                    }
                    else returnVal = false;
                    break;
                case VirtualContainerLevel.VC4:
                    if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC4)
                        returnVal = false;
                    else returnVal = true;
                    break;
            }
            return returnVal;
        }
        /// <summary>
        /// Checks the lower virtual container exists and has lower virtual containers.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private bool CheckContainerDown(VirtualContainerLevel level, int index)
        {
            bool returnVal = false;
            int contentPosition = GetContainerIndex(level, index);
            int childPosition;
            if (contentPosition == -1)
            {
                return false;
            }
            switch (level)
            {
                case VirtualContainerLevel.VC12:
                    if (this.Content[index] != null && ((VirtualContainer)this.Content[index]).Level == VirtualContainerLevel.VC12)
                        returnVal = false;
                    else returnVal = true;
                    break;
                case VirtualContainerLevel.VC21:
                    childPosition = index * 3;
                    for (int i = childPosition; i < childPosition + 3; i++)
                    {
                        if (CheckContainerDown(VirtualContainerLevel.VC12, i))
                        {
                            if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC21)
                                returnVal = false;
                            else returnVal = true;
                        }
                        else
                        {
                            returnVal = false;
                            break;
                        }
                    }
                    break;
                case VirtualContainerLevel.VC32:
                    childPosition = index * 7;
                    for (int i = childPosition; i < childPosition + 7; i++)
                    {
                        if (CheckContainerDown(VirtualContainerLevel.VC21, i))
                        {
                            if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC32)
                                returnVal = false;
                            else returnVal = true;
                        }
                        else
                        {
                            returnVal = false;
                            break;
                        }
                    }
                    break;
                case VirtualContainerLevel.VC4:
                    childPosition = index * 3;
                    for (int i = childPosition; i < childPosition + 3; i++)
                    {
                        if (CheckContainerDown(VirtualContainerLevel.VC32, i))
                        {
                            if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC4)
                                returnVal = false;
                            else returnVal = true;
                        }
                        else
                        {
                            returnVal = false;
                            break;
                        }
                    }
                    break;
            }
            return returnVal;
        }

        /// <summary>
        /// Gets the index of the container. This convert user index for Frame index.
        /// VC12 has user index multiplied by 1
        /// VC2 has user index multiplied by 3
        /// VC3 has user index multiplied by 21
        /// VC4 has user index multiplied by 63
        /// </summary>
        /// <param name="level">The level of Virtual Container</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private int GetContainerIndex(VirtualContainerLevel level, int index)
        {
            int counter = 0;
            int returnValue = -1;
            switch (level)
            {
                case VirtualContainerLevel.VC12:
                    for (int i = 0; i < Content.Count; i++)
                    {
                        if (counter == index)
                        {
                            returnValue = i;
                        }
                        counter++;
                    }
                    break;

                case VirtualContainerLevel.VC21:
                    for (int i = 0; i < Content.Count; i += 3)
                    {
                        if (counter == index)
                        {
                            returnValue = i;
                        }
                        counter++;
                    }
                    break;

                case VirtualContainerLevel.VC32:
                    for (int i = 0; i < Content.Count; i += 21)
                    {
                        if (counter == index)
                        {
                            returnValue = i;
                        }
                        counter++;
                    }
                    break;

                case VirtualContainerLevel.VC4:
                    for (int i = 0; i < Content.Count; i += 63)
                    {
                        if (counter == index)
                        {
                            returnValue = i;
                        }
                        counter++;
                    }
                    break;
            }
            return returnValue;
        }

        /// <summary>
        /// Calculates the free space in <see cref="Frame"/>
        /// </summary>
        /// <returns></returns>
        private int CalculateFreeSpace()
        {
            int VC12Count = 0;
            int VC2Count = 0;
            int VC3Count = 0;
            int VC4Count = 0;

            for (int i = 0; i < Content.Count; i++)
            {
                if (VirtualContainer.isVirtualContainer(Content[i]))
                {
                    VirtualContainer VC = (VirtualContainer)Content[i];
                    switch (VC.Level)
                    {
                        case VirtualContainerLevel.VC12:
                            VC12Count++;
                            break;
                        case VirtualContainerLevel.VC21:
                            VC2Count++;
                            break;
                        case VirtualContainerLevel.VC32:
                            VC3Count++;
                            break;
                        case VirtualContainerLevel.VC4:
                            VC4Count++;
                            break;
                    }
                }
            }
            int freeSpace = 63 * this.ConvertSTMLevel(this.Level);
            freeSpace = freeSpace - (VC12Count + VC2Count * 3 + VC3Count * 21 + VC4Count * 63);
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
        public string ToString()
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
                }
            }
            returnString += "|";
            return returnString;
        }
    }
}