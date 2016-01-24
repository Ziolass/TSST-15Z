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
            
                try {
                   
                    int bytesRec = clientSocket.Client.Receive(bytesFrom);
                    dataFromClient = Encoding.ASCII.GetString(bytesFrom,0,bytesRec);
                    Console.WriteLine(dataFromClient);
                    //TODO
                    serverResponse = responseHandler(dataFromClient);

                    sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    // TODO
                    // Console.ReadKey();
                    clientSocket.Client.Send(sendBytes);


                }
                catch(Exception e)
                {  
                    Console.WriteLine("Error: "+e.Data);
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
                case "call-request":
                    return callRequest(temp[1], temp[2]);
                    
                case "get-address":
                    return checkDictionaryForEntry(temp[1]);

                case "call-teardown":

                    break;
                case "call-accept":
                    //return callAccept()
                default:
                    return "coś sie zj. zepsulo.";
            }
            return response;
        }
        private bool callAccept(string callingPartyName,int calledPartyPort,string calledPartyName)
        {
            string response = sendCommand("call-accept|" + callingPartyName, calledPartyPort);
            string[] temp = response.Split('|');

            if (temp[0].Equals("call-accepted"))
            {
                Console.WriteLine(calledPartyName + " zaakceptowal nawiazanie polaczenia.");
                return true;
            }
            return false;
        }
        private string callTeardown(string callingPartyName, string calledPartyName)
        {
            int ccPort = ncc.getCCPort();
            string response = sendCommand("call-teardown|", ccPort);
            ///////
            return "";
        }
        private string callRequest(string callingPartyName, string calledPartyName)
        {
            int callingPort;
            int calledPort;
            // sprawdzenie portu osoby zadajacej
            string callingPartyPort = checkDictionaryForEntry(callingPartyName);
            if (!int.TryParse(callingPartyPort, out callingPort))
            {
                return "error|Dictionary nie dziala";
            }
            // sprawdzenie rekordu osoby zadanej
            string calledPartyPort = checkDictionaryForEntry(calledPartyName);
            // jak nie ma rekordu to chill, sprawdzamy u ziomeczka
            if (!int.TryParse(calledPartyPort, out calledPort))
            {
                Console.WriteLine("Brak rekordu " + calledPartyName + ". Sprawdzam w sasiednim AS");

                string fResponse = checkForeignNCCForEntry(calledPartyName);
                if (fResponse.Equals("no-such-entry"))
                {
                    return "error|Zadne Directory nie posiada takiego wpisu";
                }
                Console.WriteLine("Rekord " + calledPartyName + " zidentyfikowany w sasiednim AS.");

            }
            // no fajnie fajnie, adresy sa ale czy masz pozwolenie?
            if (!askPolicy(callingPartyName))
            {
                Console.WriteLine("Policy nie wyrazilo zgody na realizacje polaczenia");
                return "error|Policy nie wyrazilo zgody";
            }
            // no to w sumie by wypadalo spytac ziomeczka czy chce wgl z nami gadac zeby nie bylo przykro
            if (!callAccept(callingPartyName,calledPort,calledPartyName))
            {
                return "error|" + calledPartyName + " nie wyrazil zgody na polaczenie";
            }
            return connectionRequst(callingPort, calledPort);
        }

        private string coordinateCall(int localPort,int ForeignPort)
        {
            return "";

        }
        private string connectionRequst(int localPort,int foreignPort)
        {
            //int ccPort = ncc.getCCPort();
            //string response = sendCommand("connection-request|" + localPort + "|" + foreignPort,ccPort);
            return "connection-established";//response;
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
            string[] temp = response.Split('|');
            if (temp[0].Equals("CONFIRM"))
            {
                return true;
            }
            Console.WriteLine(temp[1]);
            return false;

        }
        private string sendCommand(string command,int port)
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

                commandSocket.Shutdown(SocketShutdown.Both);
                commandSocket.Close();

            }
            catch (Exception e)
            {
                return "error|Cel nie odpowiada";
            }
            return response;
        }
    }
}
