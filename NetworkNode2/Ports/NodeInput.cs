using NetworkNode.SDHFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkNode.Ports
{
    public delegate void HandleIncomingData(object sender, EventArgs args);

    public class NodeInput : Port
    {
        private int BufferSize;//SDH frame 2430
        private byte[] Bytes;
        private int MaxQueueLength;
        private Thread ServerThread;
        private List<List<byte>> InputBuffer;

        public event HandleIncomingData HandleIncomingData;
        public int InputPort { get; set; } 
        public bool Active { get; set; }
        public StmLevel Level { get; private set; }

        public NodeInput(int tcpPort, int abstractPort, StmLevel level)
            : base(tcpPort) 
        {
            InputPort = abstractPort;
            InputBuffer = new List<List<byte>>();
            Active = true;
            Level = level;
        }

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

        public byte[] GetDataFromBuffer()
        {
            if (InputBuffer.Count == 0)
            {
                return null;
            }

            byte[] result = InputBuffer[0].ToArray();
            InputBuffer.RemoveAt(0);
            return result;
        }

        private void StartAction()
        {
            try
            {
                Console.WriteLine(this.PortNumber);
                ActionSocket.Listen(MaxQueueLength);
                while (true)
                {
                    Socket handler = ActionSocket.Accept();
                    List<byte> recivedData = new List<byte>();
                    while (true)
                    {
                        Bytes = new byte[BufferSize];
                        int bytesRec = handler.Receive(Bytes);
                        //data += Encoding.ASCII.GetString(Bytes, 0, bytesRec);
                        byte[] nonNull = new byte[bytesRec];
                        Array.Copy(Bytes, 0, nonNull, 0, bytesRec);
                        recivedData.AddRange(nonNull);
                        if (BufferSize > bytesRec)
                        {
                            break;
                        }
                    }
                    
                    if (!Active)
                    {
                        continue;
                    }

                    InputBuffer.Add(recivedData);
                    
                    if (HandleIncomingData != null) 
                    {
                        HandleIncomingData(this, EventArgs.Empty);
                    }

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
