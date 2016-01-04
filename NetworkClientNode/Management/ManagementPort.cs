using NetworkClientNode.Menagment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TsstSdh.SocketUtils;

namespace NetworkNode.Ports
{
    public class ManagementClientPort
    {
        private LocalSocektBuilder builder;
        private Socket menagementPort;
        private ManagementCenter center;
        private Thread managementThread;

        public ManagementClientPort(int port)
        {
            builder = LocalSocektBuilder.Instance;
            menagementPort = builder.getTcpSocket(port);
            managementThread = new Thread(new ThreadStart(startAction));
        }

        public void SetManagementCenter(ManagementCenter center)
        {
            this.center = center;
        }

        public void StartListening()
        {
            managementThread.Start();

        }

        private void startAction()
        {
            byte[] bytes = new Byte[100000];

            try
            {

                menagementPort.Listen(1);

                // Start listening for connections.
                while (true)
                {
                    Console.WriteLine("Menagement waiting for a connection...");
                    Socket handler = menagementPort.Accept();
                    string data = null;

                    // An incoming connection needs to be processed.
                    
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        
                    

                    string response = center.PerformManagementAction(data);

                    // Echo the data back to the client.
                    byte[] msg = Encoding.ASCII.GetBytes(response);

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
