using SHDClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using TsstSdh.SocketUtils;

namespace SDHClient
{
    public delegate void HandleIncomingData(object sender, EventArgs args);
    public class IOPort : IDisposable
    {
        
        private LocalSocektBuilder socketBuilder;
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private int outputPort;
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private List<byte[]> inputBuffer;
        private Thread listeningThread;
       // Socket listener;
        public bool Active { get; set; }

        public event HandleIncomingData HandleIncomingData;
        public int InputPort;

       // public bool Active { get; set; }
        public IOPort(int port_to,int listening_port)
        {
            socketBuilder = LocalSocektBuilder.Instance;
          
            outputPort = port_to;
            inputBuffer = new List<byte[]>();
            InputPort = listening_port;
            listeningThread = new Thread(new ThreadStart(startListening));
            listeningThread.Start();
        }
        private void startListening()
        {
            try
            {
                Console.WriteLine("Start Listening port {0}", listeningPort);
                 Socket listener = socketBuilder.getTcpSocket(listeningPort);
                while (true)
                {
                    try
                    {
                       
                        listener.Listen(1);
                        allDone.Reset();
                        listener.BeginAccept(new AsyncCallback(AcceptIncomingConnection), listener);
                        allDone.WaitOne();
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
        public int PortTo
        {
            get
            {
                return outputPort;
            }
        }
        public int listeningPort
        {
            get
            {
                return InputPort;
            }
        }
        private void AcceptIncomingConnection(IAsyncResult ar)
        {
            allDone.Set();
            Socket handler = ((Socket)ar.AsyncState).EndAccept(ar);
            StateObject state = new StateObject();

            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(StartReadingData), state);
        }

        public void StartReadingData(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            Socket sender = state.sender;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                addDataToBuffer(state.buffer);

                if (HandleIncomingData != null)
                {
                    HandleIncomingData(this, EventArgs.Empty);
                }
            }

        }

        public byte[] GetDataFromBuffer()
        {
            if (inputBuffer.Count == 0)
            {
                return null;
            }

            byte[] result = inputBuffer[0];
            inputBuffer.RemoveAt(0);
            return result;
        }

        private void addDataToBuffer(byte[] data)
        {
            if (inputBuffer.Count < 3)
            {
                inputBuffer.Add(data);
            }
        }













        public void sendData(byte[] dataToSend)
        {
            
            Socket sender = socketBuilder.getTcpSocket();
            sender.DontFragment = true;
            IPEndPoint endpoint = socketBuilder.getLocalEndpoint(outputPort);
            try
            {
                sender.BeginConnect(endpoint, new AsyncCallback(ConnectToNextNode), sender);
                connectDone.WaitOne();

                sendData(sender, dataToSend);
                sendDone.WaitOne();
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception ex)
            {

            }
        }

        private void ConnectToNextNode(IAsyncResult ar)
        {
            try
            {
                Socket sender = (Socket)ar.AsyncState;
                sender.EndConnect(ar);
                connectDone.Set();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
        }

        private void sendData(Socket sender, byte[] dataToSend)
        {
            sender.BeginSend(dataToSend, 0, dataToSend.Length, 0,
                new AsyncCallback(SendData), sender);
        }

        private void SendData(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
        }

    }
}
