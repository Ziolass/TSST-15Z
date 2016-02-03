using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRM.Communication
{
    public enum PresenceType
    {
        CONNECTED,
        DISCONNECTED
    }
    public class PresenceRaport
    {
        public string Header { get; set; }
        public string Node { get; set; }
    }
}
