using Directory.SocketUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Directory
{
    class ConnectionHandler
    {
        private LocalSocektBuilder builder;
        private Directory directory;
        private Socket menagementPort;
        private Thread managementThread;

        public ConnectionHandler(Directory dir)
        {
            builder = LocalSocektBuilder.Instance;
            directory = dir;
            menagementPort = builder.getTcpSocket(dir.getLocalPort());
            managementThread = new Thread(new ThreadStart(startAction));
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
                    Console.WriteLine("Directory waiting for a connection...");
                    Socket handler = menagementPort.Accept();
                    string data = null;

                    // An incoming connection needs to be processed.

                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);



                    string response = directory.commandHandle(data);
                    
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
