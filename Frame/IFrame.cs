using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Frame
{
    public enum ContainerLevel { AU4, TUG3, TUG2, TUG12, UNDEF }
    public enum ContentType { CONTAINER, DATA, UNDEF }

    public interface IFrame
    {
        public String Msoh { get; set; }
        public String Rsoh { get; set; }
        public IContent GetVirtualConteiner(ContainerLevel level, int number);
        public void SetVirtualConteiner(ContainerLevel level, int number, IContent content); 
    }
}
