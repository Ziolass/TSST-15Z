
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CallingPartyCallController
{
    class ConnectionHandler
    {
        private TcpListener serverSocket;
        private TcpClient clientSocket;
        private CallingPartyCallController cpcc;
        public CallingPartyCallController getCPCC()
        {
            return cpcc;
        }
        public ConnectionHandler(int port, CallingPartyCallController cppc)
        {
            this.cpcc = cppc;
            serverSetUp(port);
            Thread th = new Thread(serverHandling);
            th.Start();

        }
        private void serverHandling()
        {
            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Request receied");
                ClientHandler clientHandler = new ClientHandler(this);
                clientHandler.startClient(clientSocket);
            }
        }
        private void serverSetUp(int port)
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            serverSocket = new TcpListener(ipAddress, port);
            clientSocket = default(TcpClient);
            serverSocket.Start();
            Console.WriteLine("CPCC waiting for requests...");
        }
        public string sendCommand(string command, int port)
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            Socket commandSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            byte[] bytes = new byte[100000];
            string response = null;


            try
            {
                byte[] msg = Encoding.ASCII.GetBytes(command);
                commandSocket.Connect(endPoint);
                commandSocket.Send(msg);



                bytes = new byte[1024];
                int bytesRec = commandSocket.Receive(bytes);
                response += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                commandSocket.Close();

            }
            catch (Exception e)
            {
            }
            return response;
        }

    }
}
