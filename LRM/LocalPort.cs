using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LRM
{
    public class StateObject
    {
        public const int BufferSize = 10000;
        public byte[] Buffer = new byte[BufferSize];
        public StringBuilder ResponseBuilder = new StringBuilder();
    }

    public abstract class LocalPort
    {
        protected Socket ActionSocket;
        protected IPEndPoint Endpoint;
        public LocalPort(int port)
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            Endpoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            ActionSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
