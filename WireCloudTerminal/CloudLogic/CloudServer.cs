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

    public class StateObject
    {
        // Client  socket.
        public Socket WorkSocket = null;

        // Size of receive buffer.
        public const int BufferSize = 1024;

        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder StringBuilder = new StringBuilder();
    }


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
        //private Thread ServerThread;
        public static ManualResetEvent allDone = new ManualResetEvent(false); // Thread signal.

        public event HandleDataIncom HandleDataIncom;

        public CloudServer(int port) : base(port) { }

        public void SetUpServer(int bufferSize, int maxQueueLength)
        {
            this.BufferSize = bufferSize;
            this.Bytes = new Byte[bufferSize];
            this.MaxQueueLength = maxQueueLength;
            try
            {
                SetSocket();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot connect to server socket");
            }
        }

        /// <summary>
        /// Stops the server thread.
        /// </summary>
        public void StopServerThread()
        {
            //ServerThread.Abort();
        }

        public void StartListening()
        {
            try
            {
                ActionSocket.Bind(ActionEndPoint);
                ActionSocket.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();
                    // Start an asynchronous socket to listen for connections.
                    ActionSocket.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        ActionSocket);
                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.WorkSocket = handler;
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        /// <summary>
        /// Reads the callback.
        /// https://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx
        /// </summary>
        /// <param name="ar">The ar.</param>
        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.StringBuilder.Append(Encoding.ASCII.GetString(
                    state.Buffer, 0, bytesRead));

                //Read message
                content = state.StringBuilder.ToString();

                if (!String.IsNullOrEmpty(content))
                {
                    EvaluateData(content);
                    allDone.Set();
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }


        /// <summary>
        /// Sends the specified handler.
        /// https://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="data">The data.</param>
        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        /// <summary>
        /// Sends the callback.
        /// https://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx
        /// </summary>
        /// <param name="ar">The ar.</param>
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
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
                HandleDataIncom(new IncomeDataArgs(address, normalizedData[2]));
            }
        }

    }
}
