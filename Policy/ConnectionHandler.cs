
using Policy.SocketUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Policy
{
    class ConnectionHandler
    {
        private LocalSocektBuilder builder;
        private Socket menagementPort;
        private Thread managementThread;
        private Policy policy;

        public ConnectionHandler(Policy pol)
        {
            builder = LocalSocektBuilder.Instance;
            policy  = pol;
            menagementPort = builder.getTcpSocket(pol.getLocalPort());
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
                    Console.WriteLine("Policy waiting for a connection...");
                    Socket handler = menagementPort.Accept();
                    string data = null;

                    // An incoming connection needs to be processed.
                    Console.WriteLine("Otrzymano nowe zapytanie");
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine("Uzytkownik " + data.Split('|')[1] + " ma stosowne uprawnienia. Zezwalam.");


                    string response = policy.verify(data) ? "CONFIRM" : "DECLINE";
                  //  Console.WriteLine
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
