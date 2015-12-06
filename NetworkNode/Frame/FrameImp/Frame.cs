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
        /// Sets the virtual container. This overwrite the <see cref="Content"/> list member.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="number">The number.</param>
        /// <param name="content">The content.</param>
        public void SetVirtualContainer(ContainerLevel level, int number, IContent content)
        {
            if (this.CalculateFreeSpace() >= Frame.ContainerSpaceConverter(level))
            {
                if (Frame.ContainerLevelConvert(level) == VirtualContainerLevel.VC4)
                {
                    this.Content.Add(content);
                }
                else
                {
                    this.Content[GetContainerIndex(Frame.ContainerLevelConvert(level), number)] = content;
                }
            }
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
            int returnValue = 0;
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