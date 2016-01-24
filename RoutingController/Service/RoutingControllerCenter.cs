using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoutingController.Elements;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RoutingController.Service
{
    // State object for reading client data asynchronously
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

        public RoutingController RoutingController = new RoutingController();
    }

    public class RoutingControllerCenter
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false); // Thread signal.
        public int Port { get; private set; }
        private RoutingController RoutingController { get; set; }

        public RoutingControllerCenter(int port)
        {
            this.Port = port;
            this.RoutingController = new RoutingController();
        }

        /// <summary>
        /// Performs action depending on the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private string PerformAction(string request)
        {
            //parse JSON
            JObject metadata = JObject.Parse(request);

            //LocalTopology
            if (metadata["Protocol"] != null)
            {
                if (metadata["Protocol"].ToString() == "resources")
                {
                    request = request.Replace("Protocol: \"resources\",", "");
                    TopologyRequest serializedRequest = JsonConvert.DeserializeObject<TopologyRequest>(request);

                    return "Success";
                }
                else if (metadata["Protocol"].ToString() == "query")
                {
                    return "Success";
                }
                else return "ERROR";
            }
            else return "ERROR";
        }

        #region AsyncServer

        /// <summary>
        /// Starts listening.
        /// https://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx
        /// </summary>
        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, this.Port);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Waiting for a connection...");

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();
                    // Start an asynchronous socket to listen for connections.
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        /// <summary>
        /// Accepts the callback.
        /// https://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx
        /// </summary>
        /// <param name="ar">The ar.</param>
        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

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
                    string response = this.PerformAction(content);
                    Send(handler, response);
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
                Console.WriteLine("Message send to client.");

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion AsyncServer
    }
}