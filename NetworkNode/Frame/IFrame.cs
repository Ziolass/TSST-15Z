using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Frame
{
    public enum StmLevel { STM1, STM4, STM16, STM64, STM256 }
    public enum ContainerLevel { TUG12, TUG2, TUG3, AU4, UNDEF }
    public enum ContentType { VICONTAINER, TRIBUTARYUNIT, CONTAINER, HEADER }

    public interface IFrame
    {
         Header Msoh { get; set; }
         Header Rsoh { get; set; }
         IContent GetVirtualContainer(ContainerLevel level, int number);
         bool SetVirtualContainer(ContainerLevel level, int number, IContent content);
    }
}