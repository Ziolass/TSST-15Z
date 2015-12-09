using System;
using System.Collections.Generic;

namespace NetworkNode.Frame
{
    /// <summary>
    /// Levels of Virtual Container
    /// </summary>
    public enum VirtualContainerLevel { VC12, VC2, VC3, VC4 }

    public class Frame : IFrame
    {
        public String Msoh { get; set; }
        public String Rsoh { get; set; }
        public List<IContent> Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// This create empty Content List
        /// </summary>
        public Frame()
        {
            Content = new List<IContent>();
            for (int i = 0; i < 63; i++)
            {
                Content.Add(null);
            }
        }

        /// <summary>
        /// Gets the virtual container.
        /// </summary>
        /// <param name="level">The level of Virtual Container</param>
        /// <param name="number">The number (index).</param>
        /// <returns></returns>
        public IContent GetVirtualContainer(ContainerLevel level, int number)
        {
            IContent returnContent = this.Content[GetContainerIndex(ContainerLevelConvert(level), number)];
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
        public bool SetVirtualContainer(ContainerLevel level, int number, IContent content)
        {
            VirtualContainerLevel VCLevel = Frame.ContainerLevelConvert(level);
            if (VirtualContainer.isVirtualContainer(content))
            {
                VirtualContainer contentVC = (VirtualContainer)content;
                if (VCLevel == contentVC.Level && this.CalculateFreeSpace() >= Frame.ContainerSpaceConverter(level))
                {
                    if (TestContainerSpace(VCLevel, number))
                    {
                        this.Content[GetContainerIndex(VCLevel, number)] = content;
                        return true;
                    }
                }
                else return false;
            }
            return false;
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
                case VirtualContainerLevel.VC2:
                    if (this.CheckContainerUp(level, index))
                        testUp = true;
                    if (this.CheckContainerDown(level, index))
                        testDown = true;
                    break;
                case VirtualContainerLevel.VC3:
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
                    if (CheckContainerUp(VirtualContainerLevel.VC2, parentPostion))
                    {
                        if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC12)
                            returnVal = false;
                        else returnVal = true;
                    }
                    else returnVal = false;
                    break;
                case VirtualContainerLevel.VC2:
                    parentPostion = index / 7;
                    if (CheckContainerUp(VirtualContainerLevel.VC3, parentPostion))
                    {
                        if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC2)
                            returnVal = false;
                        else returnVal = true;
                    }
                    else returnVal = false;
                    break;
                case VirtualContainerLevel.VC3:
                    if (CheckContainerUp(VirtualContainerLevel.VC4, 0))
                    {
                        if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC3)
                            returnVal = false;
                        else returnVal = true;
                    }
                    else returnVal = false;
                    break;
                case VirtualContainerLevel.VC4:
                    if (this.Content[0] != null && ((VirtualContainer)this.Content[0]).Level == VirtualContainerLevel.VC4)
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
                case VirtualContainerLevel.VC2:                 
                    childPosition = index * 3;
                    for (int i = childPosition; i < childPosition + 3; i++)
                    {
                        if (CheckContainerDown(VirtualContainerLevel.VC12, i))
                        {
                            if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC2)
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
                case VirtualContainerLevel.VC3:
                    childPosition = index * 7;
                    for (int i = childPosition; i < childPosition + 7; i++)
                    {
                        if (CheckContainerDown(VirtualContainerLevel.VC2, i))
                        {
                            if (this.Content[contentPosition] != null && ((VirtualContainer)this.Content[contentPosition]).Level == VirtualContainerLevel.VC3)
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
                        if (CheckContainerDown(VirtualContainerLevel.VC3, i))
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

                case VirtualContainerLevel.VC2:
                    for (int i = 0; i < Content.Count; i += 3)
                    {
                        if (counter == index)
                        {
                            returnValue = i;
                        }
                        counter++;
                    }
                    break;

                case VirtualContainerLevel.VC3:
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
                    returnValue = 0;
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

                        case VirtualContainerLevel.VC2:
                            VC2Count++;
                            break;

                        case VirtualContainerLevel.VC3:
                            VC3Count++;
                            break;

                        case VirtualContainerLevel.VC4:
                            VC4Count++;
                            break;
                    }
                }
            }
            int freeSpace = 63;
            freeSpace = freeSpace - (VC12Count + VC2Count * 3 + VC3Count * 21 + VC4Count * 63);
            return freeSpace;
        }

        /// <summary>
        /// Conver <see cref="ContainerLevel"/> enum to <see cref="VirtualContainerLevel"/>
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static VirtualContainerLevel ContainerLevelConvert(ContainerLevel value)
        {
            switch (value)
            {
                case ContainerLevel.TUG12:
                    return VirtualContainerLevel.VC12;

                case ContainerLevel.TUG2:
                    return VirtualContainerLevel.VC2;

                case ContainerLevel.TUG3:
                    return VirtualContainerLevel.VC3;

                case ContainerLevel.AU4:
                    return VirtualContainerLevel.VC4;

                default:
                    return VirtualContainerLevel.VC4;
            }
        }

        /// <summary>
        /// Convert <see cref="ContainerLevel"/> enum to space occupied in <see cref="Frame"/>
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int ContainerSpaceConverter(ContainerLevel value)
        {
            switch (value)
            {
                case ContainerLevel.TUG12:
                    return 1;

                case ContainerLevel.TUG2:
                    return 3;

                case ContainerLevel.TUG3:
                    return 21;

                default:
                    return 63;
            }
        }
    }
}