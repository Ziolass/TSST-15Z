using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Ports
{
    public class Port
    {
        protected int PortNumber;
        protected Socket ActionSocket;
        protected IPEndPoint ActionEndPoint;

        public Port(int portNumber)
        {
            PortNumber = portNumber;
        }

        protected void SetSocket()
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            ActionEndPoint = new IPEndPoint(ipAddress, PortNumber);
            ActionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

    }
}
