using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Frame
{
    public enum VirtualContainerLevel { VC12, VC2, VC3, VC4 }

    public class Frame : IFrame
    {
        public String Msoh { get; set; }

        public String Rsoh { get; set; }

        public List<IContent> Content { get; set; }

        public Frame()
        {
            Content = new List<IContent>();
            for (int i = 0; i < 63; i++)
            {
                Content.Add(null);
            }
        }

        public IContent GetVirtualContainer(ContainerLevel level, int number)
        {
            IContent returnContent = this.Content[GetContainerIndex(ContainerLevelConvert(level), number)];            
            return returnContent;
        }

        public void SetVirtualContainer(ContainerLevel level, int number, IContent content)
        {
            if (this.CalculateFreeSpace() >= Frame.ContainerSpaceConverter(level))
            {
                VirtualContainer newVC = new VirtualContainer(Frame.ContainerLevelConvert(level));
                if (newVC.Level == VirtualContainerLevel.VC4)
                {
                    this.Content.Add(newVC);
                }
                else
                {
                    this.Content[GetContainerIndex(newVC.Level, number)] = newVC;
                }
            }
        }
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
                    returnValue =  0;
                    break;

            }
            return returnValue;

        }
        private List<IContent> GetList(ContainerLevel level, bool countNull)
        {
            List<IContent> VCList = new List<IContent>();
            if (VirtualContainerLevel.VC4 != ContainerLevelConvert(level))
            {
                for (int i = 0; i < Content.Count; i++)
                {
                    if (VirtualContainer.isVirtualContainer(Content[i]))
                    {
                        VirtualContainer VC = (VirtualContainer)Content[i];
                        if (VC.Level == Frame.ContainerLevelConvert(level))
                        {
                            VCList.Add(VC);
                        }
                    }
                    else if (countNull && Content[i] == null)
                    {
                        VCList.Add(null);
                    }
                }
            }
            else
            {
                if (VirtualContainer.isVirtualContainer(this.Content[0]))
                {
                    VirtualContainer VC = (VirtualContainer)Content[0];
                    if (VC.Level == Frame.ContainerLevelConvert(level))
                    {
                        VCList.Add(VC);
                    }
                }
            }
            return VCList;
        }
        private int CalculateFreeSpace()
        {
            List<IContent> VC4List = this.GetList(ContainerLevel.AU4, false);
            List<IContent> VC3List = this.GetList(ContainerLevel.TUG3, false);
            List<IContent> VC2List = this.GetList(ContainerLevel.TUG2, false);
            List<IContent> VC12List = this.GetList(ContainerLevel.TUG12, false);
            int freeSpace = 63;
            freeSpace = freeSpace - (VC12List.Count + VC2List.Count * 3 + VC3List.Count * 21 + VC4List.Count * 63);
            return freeSpace;
        }
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
                    return VirtualContainerLevel.VC12;
            }
        }
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
