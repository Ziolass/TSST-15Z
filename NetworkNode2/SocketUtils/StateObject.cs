using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.SocketUtils
{
    class StateObject
    {
        public Socket workSocket = null;
        public Socket sender = null;

        public const int BufferSize = 10024;//TODO zmienić buffer na odpowiednio duży
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }
}
