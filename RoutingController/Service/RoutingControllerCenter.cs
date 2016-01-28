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
    public enum ActionType { LocalTopology, RouteTableQuery, Undef }

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
        /// Operations the type.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public ActionType OperationType(string request)
        {
            try
            {
                JObject metadata = JObject.Parse(request);
                if (metadata["Protocol"] != null)
                {
                    //LocalTopology
                    if (metadata["Protocol"].ToString() == "resources")
                        return ActionType.LocalTopology;
                    else if (metadata["Protocol"].ToString() == "query")
                        return ActionType.RouteTableQuery;
                    else return ActionType.Undef;
                }
                else return ActionType.Undef;
            }
            catch (Exception exp)
            {
                Console.WriteLine("Error! OperationType: " + exp.Message);
                return ActionType.Undef;
            }
        }

        /// <summary>
        /// Performs action depending on the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private string PerformAction(string request)
        {
          //  try
          // {
                ActionType actionType = OperationType(request);
                if (actionType == ActionType.LocalTopology)
                {
                    request = request.Replace("Protocol: \"resources\",", "");
                    TopologyRequest topologyRequest = JsonConvert.DeserializeObject<TopologyRequest>(request);
                    this.RoutingController.UpdateNetworkGraph(topologyRequest);
                    Console.WriteLine("Update from {0} ", topologyRequest.Node);
                    return "OK";
                }
                //RouteTableQuery
                else if (actionType == ActionType.RouteTableQuery)
                {
                    request = request.Replace("Protocol: \"query\",", "");
                    QueryRequest queryRequest = JsonConvert.DeserializeObject<QueryRequest>(request);
                    Console.WriteLine("RouteTableQuery from {0} ", queryRequest.LrmId);
                    return JsonConvert.SerializeObject(this.RoutingController.RouteTableResponse(queryRequest.Source, queryRequest.Destination));
                }
                else return "ERROR";
            /*}
            catch (Exception exp)
            {
              Console.WriteLine(exp.Message);
               return "ERROR";
            }*/
        }

        /// <summary>
        /// Determines whether the specified string input is valid json.
        /// http://stackoverflow.com/questions/14977848/how-to-make-sure-that-string-is-valid-json-using-json-net
        /// </summary>
        /// <param name="strInput">The string input.</param>
        /// <returns></returns>
        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
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

            Console.WriteLine("Start successful!");
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

                if (!String.IsNullOrEmpty(content) && IsValidJson(content))
                {
                    string response = string.Empty;
                    if (this.OperationType(content) == ActionType.LocalTopology)
                    {
                        response = this.PerformAction(content);
                        // Signal the main thread to continue.
                        allDone.Set();
                    }
                    else
                    {
                        // Signal the main thread to continue.
                        allDone.Set();
                        response = this.PerformAction(content);
                        Console.WriteLine(response);
                    }
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