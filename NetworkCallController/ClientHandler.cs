using NetworkCallController.SocketUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkCallController
{
    class ClientHandler
    {
        TcpClient clientSocket;
        NetworkCallController ncc;
        public ClientHandler(ConnectionHandler chandler)
        {
            this.ncc = chandler.getNcc();
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
            while (true)
            {
                try {
                   
                    int bytesRec = clientSocket.Client.Receive(bytesFrom);
                    dataFromClient = Encoding.ASCII.GetString(bytesFrom,0,bytesRec);
                    Console.WriteLine(dataFromClient);
                    //TODO
                    serverResponse = responseHandler(dataFromClient);

                    sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    Console.ReadKey();



                }
                catch(Exception e)
                {

                }
            }
        }
        private string responseHandler(string query)
        {
            string response = "";

            string[] temp = query.Split('|');

            switch (temp[0])
            {
                case "call-request":
                    int callingPort;
                    int calledPort;
                    // sprawdzenie portu osoby zadajacej
                    string callingPartyPort = checkDictionaryForEntry(temp[1]);
                    if(!int.TryParse(callingPartyPort,out callingPort))
                    {
                        return "error|Dictionary nie dziala";
                    }
                    // sprawdzenie rekordu osoby zadanej
                    string calledPartyPort = checkDictionaryForEntry(temp[2]);
                    // jak nie ma rekordu to chill, sprawdzamy u ziomeczka
                    if(!int.TryParse(calledPartyPort,out calledPort))
                    {
                        Console.WriteLine("Brak rekordu "+temp[2]+". Sprawdzam w sasiednim AS");
                        string fResponse = checkForeignNCCForEntry(temp[2]);
                        return "error|Zadne Directory nie posiada takiego wpisu";
                    }
                    // no fajnie fajnie, adresy sa ale czy masz pozwolenie?
                    if (!askPolicy(temp[1]))
                    {
                        Console.WriteLine("Policy nie wyrazilo zgody na realizacje polaczenia");
                        return "error|Policy nie wyrazilo zgody";
                    }
                    


                    break;
                case "get-address":
                    string foreignResponse = checkDictionaryForEntry(temp[1]);
                    return foreignResponse;
                default:
                    break;
            }
            return response;
        }

        private string checkForeignNCCForEntry(string entry)
        {
            int foreignPort = ncc.getForeingPort();
            string response = sendCommand("get-address|" + entry, foreignPort);
            return response;
        }

        private string checkDictionaryForEntry(string entry)
        {
            int dictPort = ncc.getDirectoryPort();
            string response = sendCommand("get-address|" + entry,dictPort);
            return response;
        }
        private bool askPolicy(string entry)
        {
            int policyPort = ncc.getPolicyPort();
            string response = sendCommand("get-policy|" + entry, policyPort);
            if (response.Equals("CONFIRM"))
            {
                return true;
            }
            return false;

        }
        private string sendCommand(string command,int port)
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
