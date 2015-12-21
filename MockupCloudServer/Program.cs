using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MockupCloudServer
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint ActionEndPoint = new IPEndPoint(ipAddress, 4032);
                Socket ActionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ActionSocket.Bind(ActionEndPoint);
                ActionSocket.Listen(100);
                while (true)
                {
                    Socket handler = ActionSocket.Accept();
                    string data = null;
                    while (true)
                    {
                        byte[] Bytes = new byte[10000];
                        int bytesRec = handler.Receive(Bytes);
                        data += Encoding.ASCII.GetString(Bytes, 0, bytesRec);
                        if (10000 > bytesRec)
                        {
                            break;
                        }
                    }

                    Console.WriteLine(data);

                    handler.Shutdown(SocketShutdown.Both); //TODO upewnic się że powininemzamykać socket
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
