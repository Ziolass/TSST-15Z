

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
    class ClientHandler
    {
        TcpClient clientSocket;
        CallingPartyCallController ncc;
        public ClientHandler(ConnectionHandler chandler)
        {
            this.ncc = chandler.getCPCC();
        }
        public void startClient(TcpClient inClientSocket)
        {
            clientSocket = inClientSocket;
            Thread ctThread = new Thread(Chat);
            ctThread.Start();
        }
        private void Chat()
        {
            byte[] bytesFrom = new byte[1024];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            
                try
                {

                    int bytesRec = clientSocket.Client.Receive(bytesFrom);
                    dataFromClient = Encoding.ASCII.GetString(bytesFrom, 0, bytesRec);
                    Console.WriteLine(dataFromClient);
                    //TODO
                    serverResponse = responseHandler(dataFromClient);

                   
                    sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    // TODO
                    // Console.ReadKey();
                    clientSocket.Client.Send(sendBytes);
                   

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Data);
                }
                clientSocket.Client.Shutdown(SocketShutdown.Both);
                clientSocket.Client.Close();
            
        }
        private string responseHandler(string query)
        {
            string response = "";

            string[] temp = query.Split('|');

            switch (temp[0])
            {
                case "call-teardown":
                    break;
                case "call-accept":
                    return acceptCall(temp[1]);
                default:
                    break;
            }
            return response;
        }
        private string acceptCall(string foreignName)
        {
            // TODO
            Console.WriteLine("Zaakceptowano zestawienie polaczenia z " + foreignName);
            return "call-accepted|";
        }
        
        private void connectionRequst(int localPort, int foreignPort)
        {

        }
       
        private string sendCommand(string command, int port)
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            Socket commandSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //LocalSocektBuilder local = LocalSocektBuilder.Instance;
            //Socket commandSocket = local.getTcpSocket(port);
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
