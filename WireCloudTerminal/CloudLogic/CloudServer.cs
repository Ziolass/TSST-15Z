using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WireCloud.CloudLogic
{
    public delegate void HandleDataIncom(IncomeDataArgs args);

    public class IncomeDataArgs : EventArgs
    {
        public AbstractAddress Address { get; private set; }
        public string Data { get; private set; }

        public IncomeDataArgs(AbstractAddress Address, string Data)
        {
            this.Address = Address;
            this.Data = Data;
        }
    }

    public class CloudServer : Port
    {
        private int BufferSize;//SDH frame 2430
        private byte[] Bytes;
        private int MaxQueueLength;
        private Thread ServerThread;

        public event HandleDataIncom HandleDataIncom;

        public CloudServer(int port) : base(port) { }

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

        /// <summary>
        /// Stops the server thread.
        /// </summary>
        public void StopServerThread(){
            ServerThread.Abort();
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
                ActionSocket.Listen(MaxQueueLength);
                while (true)
                {
                    Socket handler = ActionSocket.Accept();
                    string data = null;
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

                    EvaluateData(data);

                    handler.Shutdown(SocketShutdown.Both); //TODO upewnic się że powininemzamykać socket
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void EvaluateData(string data)
        {
            string[] normalizedData = data.Split('|');
            string nodeId = normalizedData[0];
            int port = int.Parse(normalizedData[1]);
            AbstractAddress address = new AbstractAddress(port, nodeId);

            if (HandleDataIncom != null)
            {
                HandleDataIncom(new IncomeDataArgs(address,normalizedData[2]));
            }
        }

    }
}
