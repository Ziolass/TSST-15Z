using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.SDHFrame
{

    public enum StmLevel { STM1, STM4, STM16, STM64, STM256, UNDEF }
    public enum VirtualContainerLevel { VC12, VC21, VC32, VC4, UNDEF }
    public enum ContentType { VICONTAINER, TRIBUTARYUNIT, CONTAINER, HEADER, POH }

    public class VirtualContainerLevelExt 
    {
        public static VirtualContainerLevel GetContainer(string literalContainer)
        {
            switch (literalContainer)
            {
                case "VC12":
                    {
                        return VirtualContainerLevel.VC12;
                    }
                case "VC21":
                    {
                        return VirtualContainerLevel.VC21;
                    }
                case "VC32":
                    {
                        return VirtualContainerLevel.VC32;
                    }
                case "VC4":
                    {
                        return VirtualContainerLevel.VC4;
                    }

                default:
                    {
                        return VirtualContainerLevel.UNDEF;
                    }
            }
        }
    }

    public class StmLevelExt
    {
        public static StmLevel GetContainer(string literalContainer)
        {
            switch (literalContainer)
            {
                case "STM1":
                    {
                        return StmLevel.STM1;
                    }
                case "STM4":
                    {
                        return StmLevel.STM4;
                    }
                case "STM16":
                    {
                        return StmLevel.STM16;
                    }
                case "STM64":
                    {
                        return StmLevel.STM64;
                    }
                case "STM256":
                    {
                        return StmLevel.STM256;
                    }
                default:
                    {
                        return StmLevel.UNDEF;
                    }
            }
        }
    }

    public interface IFrame
    {
         Header Msoh { get; set; }
         Header Rsoh { get; set; }
         IContent GetVirtualContainer(VirtualContainerLevel level, int hiIndex, int? lowIndex);
         bool SetVirtualContainer(VirtualContainerLevel level, int hiIndex, int? lowIndex, IContent content);
    }
}