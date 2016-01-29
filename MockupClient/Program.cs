using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockupClient
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public class SynchronousSocketClient
    {

        public static void StartClient() {
        byte[] bytes = new byte[5000];

        //string data = File.ReadAllText(json);
        //string data = "TEST";
        Console.WriteLine("Pressto send JSON");
        try {
            while (true)
            {
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 18000);

                Socket sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    sender.Connect(remoteEP);

                    string data = Console.ReadLine();

                    byte[] msg = Encoding.ASCII.GetBytes(data);
                    int bytesSent = sender.Send(msg);

                    //int bytesRec = sender.Receive(bytes);
                    //Console.WriteLine("Echoed test = {0}",
                    //    Encoding.ASCII.GetString(bytes,0,bytesRec));
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

        } catch (Exception e) {
            Console.WriteLine( e.ToString());
        }
    }

        public static void Main(String[] args)
        {
            //StartClient("../../../Configs/RoutingController/test" + args[0] + ".json");
            /*StartClient("../../../Configs/RoutingController/test" + args[1] + ".json");
            StartClient("../../../Configs/RoutingController/test" + args[2] + ".json");
            StartClient("../../../Configs/RoutingController/test" + args[3] + ".json");
            StartClient("../../../Configs/RoutingController/test" + args[4] + ".json");
            //StartClient("../../../Configs/RoutingController/test" + args[5] + ".json");
            //StartClient("../../../Configs/RoutingController/testRequest0.json");
            //StartClient("../../../Configs/RoutingController/testRequest1.json");
            StartClient("../../../Configs/RoutingController/testRequest2.json");*/
            StartClient();


            //Console.ReadLine();
        }
    }
}
