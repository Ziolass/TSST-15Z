using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TsstSdh.SocketUtils;

namespace RoutingController.Service
{
    public class RoutingControllerPort
    {
        private LocalSocektBuilder SocketBuilder;
        private Socket RoutingControllerSocket;
        private RoutingControllerCenter RoutingControllerCenter;
        private Thread RoutingControllerThread;

        public RoutingControllerPort(int port)
        {
            SocketBuilder = LocalSocektBuilder.Instance;
            RoutingControllerSocket = SocketBuilder.getTcpSocket(port);
            RoutingControllerThread = new Thread(new ThreadStart(StartAction));
        }

        /// <summary>
        /// Sets the management center.
        /// </summary>
        /// <param name="center">The center.</param>
        public void SetManagementCenter(RoutingControllerCenter center)
        {
            this.RoutingControllerCenter = center;
        }

        public void StartListening()
        {
            RoutingControllerThread.Start();
        }

        private void StartAction()
        {
            byte[] bytes = new Byte[100000];

            try
            {
                RoutingControllerSocket.Listen(1);

                // Start listening for connections.
                while (true)
                {
                    Console.WriteLine("RoutingController waiting for a connections...");
                    Socket handler = RoutingControllerSocket.Accept();
                    string data = null;

                    // An incoming connection needs to be processed.
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);


                    // Echo the data back to the client.
                    byte[] msg = Encoding.ASCII.GetBytes("");

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
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