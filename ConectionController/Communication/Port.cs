using System.Net;
using System.Net.Sockets;

namespace Cc.Communication
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