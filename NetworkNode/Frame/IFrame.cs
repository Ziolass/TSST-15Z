using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.SDHFrame
{
    public enum ContentType { VICONTAINER, TRIBUTARYUNIT, CONTAINER, HEADER }

    public enum VirtualContainerLevel { VC12, VC21, VC32, VC4 }
    public enum STMLevel { STM1, STM4, STM16, STM64, STM256 }

    public interface IFrame
    {
        Header Msoh { get; set; }
        Header Rsoh { get; set; }
        IContent GetVirtualContainer(VirtualContainerLevel level, int number);
        bool SetVirtualContainer(VirtualContainerLevel level, int number, IContent content);
        //int ConvertSTMLevel(SynchronousTransportModuleLevel stmLevel);
    }
}