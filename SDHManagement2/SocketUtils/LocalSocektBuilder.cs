using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SDHManagement2.SocketUtils
{
    class LocalSocektBuilder
    {
        private static LocalSocektBuilder instance = null;
        private IPAddress ipAddress;

        private LocalSocektBuilder()
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
        }

        public static LocalSocektBuilder Instance
        {
            get
            {
                if (instance == null)
                {

                    instance = new LocalSocektBuilder();
                    // NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler();
                }

                return instance;
            }
        }




        public Socket getTcpSocket(int port)
        {
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            socket.Connect(remoteEP);
            return socket;
        }
        public Socket getTcpSocket()
        {
           
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            return socket;
        }
        public IPEndPoint getLocalEndpoint(int port)
        {
            return new IPEndPoint(ipAddress, port);
        }

    }
}