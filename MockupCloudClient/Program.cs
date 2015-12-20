using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MockupCloudClient
{
    class Program
    {
        static void Main(string[] args)
        {
            
            while (true)
            {
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint ActionEndPoint = new IPEndPoint(ipAddress, 6000);
                Socket ActionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Console.WriteLine("wpisz dane");
                string text = Console.ReadLine();
                ActionSocket.Connect(ActionEndPoint);
                
                byte[] msg = Encoding.ASCII.GetBytes("mockup1|1|" + text);

                ActionSocket.Send(msg);
                ActionSocket.Shutdown(SocketShutdown.Both);
                ActionSocket.Close();
            }
        }
    }
}
