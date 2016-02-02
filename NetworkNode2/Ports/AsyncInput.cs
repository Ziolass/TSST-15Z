using NetworkNode.SocketUtils;
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
    //public delegate void HandleIncomingData(object sender, EventArgs args);
    public class AsyncInput : IDisposable
    {
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private List<byte[]> inputBuffer;
        private Thread listeningThread;

        public bool Active { get; set; }

        public event HandleIncomingData HandleIncomingData;
        public int InputPort { get; private set; }
        

        public AsyncInput(int port)
        {
            inputBuffer = new List<byte[]>();
            InputPort = port;
        }

        public void TurnOn()
        {
            listeningThread = new Thread(new ThreadStart(startListening));
            listeningThread.Start();
        }
       
        private void startListening()
        {
            try
            {
                Console.WriteLine("Start Listening port {0}", InputPort);
                Socket listener = LocalSocektBuilder.Instance.getTcpSocket(InputPort);
                while (true)
                {
                    try
                    {
                        listener.Listen(1);
                        allDone.Reset();
                        listener.BeginAccept(new AsyncCallback(AcceptIncomingConnection), listener);
                        allDone.WaitOne();
                    } 
                    catch (Exception ex) 
                    {

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
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


    }
}
