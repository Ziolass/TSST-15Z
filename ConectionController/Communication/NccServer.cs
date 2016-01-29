using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cc.Communication
{
    public class NccServer : Port
    {
        private int BufferSize;//SDH frame 2430
        private byte[] Bytes;
        private int MaxQueueLength;
        private Thread ServerThread;
        public ConnectionController ConnectionCtlr { get; set; }
        private Socket HandlingSocket;

        public NccServer(int tcpPort)
            : base(tcpPort) { }

        public void SetUpServer(int bufferSize, int maxQueueLength)
        {
            this.BufferSize = bufferSize;
            this.Bytes = new Byte[bufferSize];
            this.MaxQueueLength = maxQueueLength;
            try
            {
                ConnectToLocalPoint();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot connect to server socket");
            }
        }

        public void ConnectToLocalPoint()
        {
            SetSocket();
            ActionSocket.Bind(ActionEndPoint);
        }

        public void StartListening()
        {
            if(ServerThread != null) 
            {
                return;
            }

            ServerThread = new Thread(new ThreadStart(StartAction));
            ServerThread.Start();
        }

        private void StartAction()
        {
            try
            {
                Console.WriteLine(this.PortNumber);
                ActionSocket.Listen(MaxQueueLength);
                while (true)
                {
                    try
                    {
                        Socket handler = ActionSocket.Accept();
                        List<byte> recivedData = new List<byte>();

                        string data = "";

                        while (true)
                        {

                            Bytes = new byte[BufferSize];
                            int bytesRec = handler.Receive(Bytes);
                            data += Encoding.ASCII.GetString(Bytes, 0, bytesRec);

                            if (BufferSize > bytesRec)
                            {
                                break;
                            }
                        }

                        data = ConnectionCtlr.HandleNccData(data);

                        handler.Send(Encoding.ASCII.GetBytes(data));

                        handler.Shutdown(SocketShutdown.Both); //TODO upewnic się że powininemzamykać socket
                        handler.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
