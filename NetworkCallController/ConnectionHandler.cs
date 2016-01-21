using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCallController
{
    class ConnectionHandler
    {
        private TcpListener serverSocket;
        private TcpClient clientSocket;
        public ConnectionHandler(int port)
        {
            serverSetUp(port);
            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Request receied");

            }

        }
        private void serverSetUp(int port)
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            serverSocket = new TcpListener(ipAddress, port);
            clientSocket = default(TcpClient);
            serverSocket.Start();
            Console.WriteLine("NCC waiting for requests...");
        }
    }
}
