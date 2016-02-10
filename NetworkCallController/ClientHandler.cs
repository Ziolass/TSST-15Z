using NetworkCallController.Adapters;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkCallController
{
    internal class ClientHandler
    {
        private TcpClient clientSocket;
        private NetworkCallController ncc;

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

            //try
            //{
            int bytesRec = clientSocket.Client.Receive(bytesFrom);
            dataFromClient = Encoding.ASCII.GetString(bytesFrom, 0, bytesRec);
            Console.WriteLine(dataFromClient);
            //TODO

            serverResponse = HandleJsonInput(dataFromClient);

            sendBytes = Encoding.ASCII.GetBytes(serverResponse);
            // TODO
            // Console.ReadKey();
            clientSocket.Client.Send(sendBytes);

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Error: " + e.Data);
            //}
            clientSocket.Client.Shutdown(SocketShutdown.Both);
            clientSocket.Client.Close();
        }

        private string HandleJsonInput(string data)
        {
            if (data.Contains(CommunicationType.CC_COMMUNICATION.ToString()))
            {
                CcResponse resp = JsonConvert.DeserializeObject<CcResponse>(data);
                data = resp.Response;
            }
            return responseHandler(data);
        }

        private string responseHandler(string query)
        {
            string[] temp = query.Split('|');

            switch (temp[0])
            {
                case "call-request":
                    return callRequest(temp[1], temp[2]);

                case "get-address":
                    return checkDictionaryForEntry(temp[1]);

                case "call-teardown":
                    return callTeardown(temp[1], temp[2]);

                case "call-malfunction":

                    return callMalfunction(temp[1], temp[2]);
                case "OK":
                    return "OK";
                default:
                    return "coś sie zj. zepsulo.";
            }
        }

        private string callMalfunction(string v1, string v2)
        {
            Tuple<int, string, int, string> tup;

            if (ncc.getConnections().TryGetValue(v1 + "|" + v2, out tup))
            {
                informTeardownSides(tup.Item1, tup.Item2, tup.Item3, tup.Item4);
                ncc.getConnections().Remove(v1 + "|" + v2);
                return "OK|";
            }
            else if (ncc.getConnections().TryGetValue(v2 + "|" + v2, out tup))
            {
                informTeardownSides(tup.Item1, tup.Item2, tup.Item3, tup.Item4);
                ncc.getConnections().Remove(v2 + "|" + v1);
                return "OK|";
            }
            return "OK|";
        }

        private bool callAccept(string callingPartyName, int calledPartyPort, string calledPartyName)
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
            int callingSignalingPort;
            int calledSignalingPort;
            string callingAddress;
            string calledAddress;

            // sprawdzenie portu osoby zadajacej
            string[] callingPartyPorts = checkDictionaryForEntry(callingPartyName).Split('|');
            if (callingPartyPorts[0].Equals("brak_wpisu"))
            {
                return "Lokalny Directory nie posiada wpisu " + callingPartyName;
            }
            if (!int.TryParse(callingPartyPorts[0], out callingSignalingPort))
            {
                return "error|Directory nie dziala";
            }
            callingAddress = callingPartyPorts[1];

            // sprawdzenie rekordu osoby zadanej
            string[] calledPartyPorts = checkDictionaryForEntry(calledPartyName).Split('|');

            // jak nie ma rekordu to chill, sprawdzamy u ziomeczka
            if (!int.TryParse(calledPartyPorts[0], out calledSignalingPort))
            {
                Console.WriteLine("Brak rekordu " + calledPartyName + ". Sprawdzam w sasiednim AS");

                calledPartyPorts = checkForeignNCCForEntry(calledPartyName).Split('|');
                if (calledPartyPorts[0].Equals("brak_wpisu"))
                {
                    return "error|Zadne Directory nie posiada takiego wpisu";
                }
                else
                {
                    Console.WriteLine("Rekord " + calledPartyName + " zidentyfikowany w sasiednim AS.");
                    calledSignalingPort = int.Parse(calledPartyPorts[0]);
                    calledAddress = calledPartyPorts[1];

                    string[] returnable = connectionTeardown(callingAddress, calledAddress, callingSignalingPort, calledSignalingPort).Split('|');
                    if (returnable[0].Equals("OK"))
                    {
                        informOtherParty(calledSignalingPort, callingPartyName);
                    }
                    return string.Join("|", returnable);
                }
            }
            else
            {
                calledAddress = calledPartyPorts[1];
            }
            string[] tmp = connectionTeardown(callingAddress, calledAddress, callingSignalingPort, calledSignalingPort).Split('|');
            if (tmp[0].Equals("OK"))
            {
                informOtherParty(calledSignalingPort, callingPartyName);
            }
            return string.Join("|", tmp);
        }

        private string callRequest(string callingPartyName, string calledPartyName)
        {
            int callingSignalingPort;
            int calledSignalingPort;
            string callingAddress;
            string calledAddress;

            // sprawdzenie portu osoby zadajacej
            string[] callingPartyPorts = checkDictionaryForEntry(callingPartyName).Split('|');

            if (callingPartyPorts[0].Equals("Brak_wpisu"))
            {
                return "Lokalny Directory nie posiada wpisu " + callingPartyName;
            }

            if (!int.TryParse(callingPartyPorts[0], out callingSignalingPort))
            {
                return "error|Directory nie dziala";
            }
            callingAddress = callingPartyPorts[1];

            // no fajnie fajnie,ale czy masz pozwolenie?
            if (!askPolicy(callingPartyName))
            {
                Console.WriteLine("Policy nie wyrazilo zgody na realizacje polaczenia");
                return "error|Policy nie wyrazilo zgody";
            }

            // sprawdzenie rekordu osoby zadanej
            string[] calledPartyPorts = checkDictionaryForEntry(calledPartyName).Split('|');

            // jak nie ma rekordu to chill, sprawdzamy u ziomeczka
            if (!int.TryParse(calledPartyPorts[0], out calledSignalingPort))
            {
 
                Console.WriteLine("Brak rekordu " + calledPartyName + ". Sprawdzam w sasiednim AS");

                calledPartyPorts = checkForeignNCCForEntry(calledPartyName).Split('|');
                if (calledPartyPorts[0].Equals("Brak_wpisu"))
                {
                    return "error|Zadne Directory nie posiada takiego wpisu";
                }
                else if (calledPartyPorts[0].Equals("Blad polaczenia z obcym Directory"))
                {
                    return "error|Blad polaczenia z obcym Directory";
                }
                else
                {
                    Console.WriteLine("Rekord " + calledPartyName + " zidentyfikowany w sasiednim AS.");
                    calledSignalingPort = int.Parse(calledPartyPorts[0]);
                    calledAddress = calledPartyPorts[1];

                    
                    //if (connectionRequst(callingAddress, calledAddress, callingSignalingPort, callingPartyName, calledSignalingPort, calledPartyName).Split('|')[0].ToLower().Equals("ok"))
                    /*if (connectionRequst(callingAddress, calledAddress, callingSignalingPort, callingPartyName, calledSignalingPort, calledPartyName).Split('|')[0].ToLower().Equals("ok"))
                    {
                        Console.Out.WriteLine("jestem w petli");
                        if (!callAccept(callingPartyName, calledSignalingPort, calledPartyName))
                        {
                            connectionTeardown(callingAddress, calledAddress, callingSignalingPort, calledSignalingPort);
                            return "error|" + calledPartyName + " nie wyrazil zgody na polaczenie";
                        }
                        return "ok|polaczenie z " + calledPartyName + " zestawione pomyslnie";
                    }
                    return "error|nie udalo sie zestawic polaczenia z " + calledPartyName;
                    */

                    // zamiana kolejności coby Komando nie zabił
                    
                    // no to w sumie by wypadalo spytac ziomeczka czy chce wgl z nami gadac zeby nie bylo przykro
                    if (!callAccept(callingPartyName, calledSignalingPort, calledPartyName))
                    {
                        return "error|" + calledPartyName + " nie wyrazil zgody na polaczenie";
                    }
                    if(connectionRequst(callingAddress, calledAddress, callingSignalingPort, callingPartyName, calledSignalingPort, calledPartyName).Split('|')[0].ToLower().Equals("ok"))
                    {
                        return "ok|polaczenie z " + calledPartyName + " zestawione pomyslnie";
                    }
                    else return "error|nie udalo sie zestawic polaczenia z " + calledPartyName;
                }
            }

            else
            {
                calledAddress = calledPartyPorts[1];
            }

            if (!callAccept(callingPartyName, calledSignalingPort, calledPartyName))
            {
                return "error|" + calledPartyName + " nie wyrazil zgody na polaczenie";
            }
            if (connectionRequst(callingAddress, calledAddress, callingSignalingPort, callingPartyName, calledSignalingPort, calledPartyName).Split('|')[0].ToLower().Equals("ok"))
            {
                return "ok|polaczenie z " + calledPartyName + " zestawione pomyslnie";
            }
            else return "error|nie udalo sie zestawic polaczenia z " + calledPartyName;
        }

        private void informOtherParty(int signallingPort, string teardownName)
        {
            string response = sendCommand("call-teardown|" + teardownName, signallingPort);
        }

        private string connectionRequst(string initAddress, string foreignAddress, int initSignalPort, string initName, int foreignSignalPort, string foreignName)
        {
            int ccPort = ncc.getCCPort();
            // string response = sendCommand("connection-request|" + initAddress + "|" + foreignAddress, ccPort);
            HigherLevelConnectionRequest toSend = prepareToSend(initAddress, foreignAddress, "connection-request");
            Console.WriteLine("dzialam dotad!");
            string response = sendCommand(JsonConvert.SerializeObject(toSend), ccPort);

            string translate = HandleJsonInput(response);
          


            if (translate.Split('|')[0].Equals("error"))
            {
                return "NCC nie moglo nawiazac polaczenia z CC";
            }
            ncc.getConnections().Add(initAddress + "|" + foreignAddress, Tuple.Create(initSignalPort, initName, foreignSignalPort, foreignName));
            Console.WriteLine("translate" + translate);
            return translate;
        }

        private HigherLevelConnectionRequest prepareToSend(string initAddress, string destinationAddress, string type)
        {
            HigherLevelConnectionRequest toSend = new HigherLevelConnectionRequest();
            toSend.Type = type;
            string[] inits = initAddress.Split(':');
            string[] destinatiosn = destinationAddress.Split(':');
            LrmSnp init = new LrmSnp { Name = inits[0], Port = inits[1] };
            LrmSnp dest = new LrmSnp { Name = destinatiosn[0], Port = destinatiosn[1] };
            toSend.Src = init;
            toSend.Dst = dest;
            return toSend;
        }

        private string connectionTeardown(string initAddress, string foreignAddress, int initSignalPort, int foreignSignalPort)
        {
            Console.WriteLine("Wszedłem w connection teardown");
            int ccPort = ncc.getCCPort();
            // string response = sendCommand("call-teardown|" + initAddress + "|" + foreignAddress, ccPort);
            HigherLevelConnectionRequest toSend = prepareToSend(initAddress, foreignAddress, "call-teardown");
            Console.WriteLine("Chce wysłać przy teardown: " + JsonConvert.SerializeObject(toSend));
            string response = sendCommand(JsonConvert.SerializeObject(toSend), ccPort);
            Console.WriteLine("teardown_odpowiedz: " + HandleJsonInput(response));
            string translate =  HandleJsonInput(response);

            if (translate.Split('|')[0].Equals("error"))
            {
                return "NCC nie moglo nawiazac polaczenia z CC";
            }
            return translate;
        }

        private void informTeardownSides(int initSignalPort, string initName, int foreignSignalPort, string foreignName)
        {
            string initResponse = sendCommand("call-malfunction|" + foreignName, initSignalPort);
            string foreignResposne = sendCommand("call-malfunction|" + initName, foreignSignalPort);
        }

        private string checkForeignNCCForEntry(string entry)
        {
            int foreignPort = ncc.getForeingPort();
            string response = sendCommand("get-address|" + entry, foreignPort);
            if (response.Split('|')[0].Equals("error"))
            {
                return "Blad polaczenia z obcym Directory";
            }

            return response;
        }

        private string checkDictionaryForEntry(string entry)
        {
            int dictPort = ncc.getDirectoryPort();
            string response = sendCommand("get-address|" + entry, dictPort);
            if (response.Split('|')[0].Equals("error"))
            {
                return "Blad polaczenia z Directory";
            }
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

        private string sendCommand(string command, int port)
        {
            Console.WriteLine("Debug:" + command);
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
                Console.WriteLine("Czekam na odpowiedz");
                int bytesRec = commandSocket.Receive(bytes);
                response += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine("Dostłem " + response);

                commandSocket.Shutdown(SocketShutdown.Both);
                commandSocket.Close();
            }
                catch (Exception e)
            {
                return "error|Brak odpowiedzi";
            }
            return response;
        }
    }
}