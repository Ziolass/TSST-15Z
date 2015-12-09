using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDHManagement2.SocketUtils;

namespace SDHManagement2.Models
{
   public class Router
    {
        public string identifier { get; set; }
        public int port { get; set; }
        public bool connected { get; set; }
        public RouterSocket socket { get; set; }
    }
}
