using System;
using System.Collections.Generic;

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
        public POH POH { get; set; }
        public List<IContent> Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualContainer" /> class.
        /// </summary>
        /// <param name="level">The level.</param>
        public VirtualContainer(VirtualContainerLevel level)
        {
            this.Level = level;
            this.Type = ContentType.VICONTAINER;
            this.POH = new POH();
            this.Pointer = string.Empty;
            this.Content = this.GenerateContentList();
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
            this.Content = this.GenerateContentList();
            this.Content[0] = content;
            this.POH = new POH();
            this.Pointer = string.Empty;
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string returnString = string.Empty;
            foreach (var item in this.Content)
            {
                if (item != null)
                {
                    returnString += this.Level.ToString() + "+POH|";
                }
            }
            return returnString;
        }

        /// <summary>
        /// Generates the list content depending on the current VirtualContainer level
        /// </summary>
        /// <returns></returns>
        private List<IContent> GenerateContentList()
        {
            List<IContent> returnValue = new List<IContent>();
            switch (Level)
            {
                case VirtualContainerLevel.VC4: //VC-4 have 63 space to be occupied
                    for (int i = 0; i < 63; i++)
                    {
                        returnValue.Add(null);
                    }
                    break;

                default:
                    returnValue.Add(null); //All others have one space for Container only
                    break;
            }
            return returnValue;
        }

        /// <summary>
        /// Calculates the space used in the container.
        /// If VC have only content it takes all free space
        /// </summary>
        /// <returns></returns>
        public int CalculateSpace()
        {
            int usedSpace = 0;
            switch (this.Level)
            {
                case VirtualContainerLevel.VC12:
                    usedSpace = 1;
                    break;

                case VirtualContainerLevel.VC21:
                    usedSpace = 3;
                    break;

                case VirtualContainerLevel.VC32:
                    usedSpace = 21;
                    break;

                case VirtualContainerLevel.VC4:
                    foreach (var item in this.Content)
                    {
                        if (VirtualContainer.isVirtualContainer(item)) //VC4 have other VC
                        {
                            usedSpace += ((VirtualContainer)item).CalculateSpace();
                        }
                        else if (Container.isContainer(item)) //VC4 is full of data
                        {
                            usedSpace = 63;
                            break;
                        }
                    }
                    break;

                case VirtualContainerLevel.UNDEF:
                    break;

                default:
                    break;
            }
            return usedSpace;
        }

        /// <summary>
        /// Gets the IContent from VirtualContainer at the index.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public IContent GetVirtualContainerAtIndex(VirtualContainerLevel level, int index)
        {
            IContent returnValue = null;
            int realIndex = GetContainerIndex(level, index);
            if (realIndex != -1)
            {
                returnValue = this.Content[GetContainerIndex(level, index)];
            }
            return returnValue;
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

                case VirtualContainerLevel.UNDEF:
                    break;
            }
            return returnValue;
        }

        /// <summary>
        /// Sets the content of the content list at index.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="index">The index.</param>
        /// <param name="content">The content.</param>
        public bool SetVirtualContainerAtIndex(VirtualContainerLevel level, int index, IContent content)
        {
            int realIndex = GetContainerIndex(level, index);
            if (realIndex != -1)
            {
                this.Content[GetContainerIndex(level, index)] = content;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Sets the content.
        /// </summary>
        /// <param name="container">The container.</param>
        public void SetContent(Container container)
        {
            this.Content[0] = container;
        }

        /// <summary>
        /// Sets the content.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetContent(String value)
        {
            this.Content[0] = new Container(value);
        }

        /// <summary>
        /// Tests adding the container to frame space.
        /// Check if Virtual Container have TU-space for itself
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public bool TryAddContainer(VirtualContainerLevel level, int index)
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
    }
}