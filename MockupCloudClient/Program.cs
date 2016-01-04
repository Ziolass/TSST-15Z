using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetworkNode;
using NetworkNode.SDHFrame;

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
                FrameBuilder fmb = new FrameBuilder();
                Frame newframe = new Frame();
                newframe.SetVirtualContainer(VirtualContainerLevel.VC32, 0, null, new VirtualContainer(VirtualContainerLevel.VC32, new Container("test")));
                string text = fmb.BuildLiteral(newframe);
                Console.ReadLine();
                ActionSocket.Connect(ActionEndPoint);
                
                byte[] msg = Encoding.ASCII.GetBytes("client|1|" + text);

                ActionSocket.Send(msg);
                ActionSocket.Shutdown(SocketShutdown.Both);
                ActionSocket.Close();
            }
        }
    }
}
