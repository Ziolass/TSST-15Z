using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoutingController.Elements;
using RoutingController.Requests;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RoutingController.Service
{
    public enum ActionType { LocalTopology, RouteTableQuery, NetworkTopology, Undef }

    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket WorkSocket = null;

        // Size of receive Buffer.
        public const int BufferSize = 1024;

        // Receive Buffer.
        public byte[] Buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder StringBuilder = new StringBuilder();

        public RoutingController RoutingController = new RoutingController();
    }

    public class RoutingControllerCenter
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false); // Thread signal.
        public static ManualResetEvent connectDone = new ManualResetEvent(false);
        public static ManualResetEvent receiveDone = new ManualResetEvent(false);
        public int Port { get; private set; }
        private RoutingController RoutingController { get; set; }
        public List<NeighbourRoutingController> NeighbourList { get; private set; }
        private Socket ServerSocket { get; set; }
        public string NetworkName { get; private set; }

        public RoutingControllerCenter(int port, List<NeighbourRoutingController> neighbourList, string networkName)
        {
            this.Port = port;
            this.NeighbourList = new List<NeighbourRoutingController>(neighbourList);
            this.RoutingController = new RoutingController(networkName);
            this.NetworkName = networkName;
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
                    else if (metadata["Protocol"].ToString() == "route")
                        return ActionType.RouteTableQuery;
                    else if (metadata["Protocol"].ToString() == "network")
                        return ActionType.NetworkTopology;
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
            //try
            //{
                ActionType actionType = OperationType(request);
                if (actionType == ActionType.LocalTopology)
                {
                    request = request.Replace("Protocol: \"resources\",", "");
                    LocalTopologyRequest topologyRequest = JsonConvert.DeserializeObject<LocalTopologyRequest>(request);
                    this.RoutingController.UpdateNetworkGraph(topologyRequest);
                    Console.WriteLine("Update from local topology");
                    return "OK";
                }
                //RouteTableQuery
                else if (actionType == ActionType.RouteTableQuery)
                {
                    request = request.Replace("Protocol: \"query\",", "");
                    QueryRequest queryRequest = JsonConvert.DeserializeObject<QueryRequest>(request);
                    Console.WriteLine("RouteTableQuery request");
                    return JsonConvert.SerializeObject(this.RoutingController.RouteTableResponse(queryRequest));
                }
                else if (actionType == ActionType.NetworkTopology)
                {
                    NetworkRequest queryRequest = JsonConvert.DeserializeObject<NetworkRequest>(request);
                    this.RoutingController.UpdateNetworkTopology(queryRequest);

                    Console.WriteLine("Network topology updated from {0}", queryRequest.NetworkName);
                    if (this.NeighbourList.Count > 1)
                    {
                        foreach (var item in this.NeighbourList)
                        {
                            if (item.NetworkName != queryRequest.NetworkName)
                            {
                                SendNetworkTopology(item);
                            }
                        }
                    }
                    return "OK";
                }
                else return "ERROR";
            //}
            //catch (Exception exp)
            //{
            //    Console.WriteLine(exp.Message);
            //    return "ERROR";
            //}
        }

        /// <summary>
        /// Sends the network topology.
        /// </summary>
        /// <returns></returns>
        private void SendNetworkTopology(NeighbourRoutingController neighbourRC)
        {
            NetworkRequest networkRequest = this.RoutingController.NetworkRequestResponse();
            String message = JsonConvert.SerializeObject(networkRequest);
            if (networkRequest.Clients.Count >= 1)
            {
                List<string> otherDomains = new List<string>();
                foreach (NeighbourRoutingController otherNeighbour in this.NeighbourList)
                {
                    if (otherNeighbour != neighbourRC)
                    {
                        otherDomains.Add(otherNeighbour.NetworkName);
                    }
                }
                networkRequest.OtherDomains = new List<string>(otherDomains);

                byte[] bytes = new byte[5000];
                try
                {
                    IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                    IPAddress ipAddress = ipHostInfo.AddressList[0];
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, neighbourRC.Port);
                    Socket sender = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);
                    sender.Connect(remoteEP);
                    byte[] msg = Encoding.ASCII.GetBytes(message);
                    int bytesSent = sender.Send(msg);
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    Console.WriteLine("NetworkTopology sent!");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        private void SendNetworkTopology()
        {
            foreach (NeighbourRoutingController neighbourRC in this.NeighbourList)
            {
                SendNetworkTopology(neighbourRC);
            }

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
            // Data Buffer for incoming data.
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

            Console.WriteLine("Start successful");
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
                        SendNetworkTopology(); //My topology is new  send it
                    }
                    else if (this.OperationType(content) == ActionType.RouteTableQuery)
                    {
                        // Signal the main thread to continue.
                        allDone.Set();
                        response = this.PerformAction(content);
                        Console.WriteLine("Route table query sent");
                    }
                    else if (this.OperationType(content) == ActionType.NetworkTopology)
                    {
                        this.PerformAction(content);
                        allDone.Set();
                    }
                    else
                    {
                        allDone.Set();
                        Console.WriteLine("Wrong request!");
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

        #region Commands

        /// <summary>
        /// Shows the routes.
        /// </summary>
        /// <returns></returns>
        public string ShowRoutes()
        {
            return this.RoutingController.ShowRoutes();
        }
        public string ShowExternalClients()
        {
            return this.RoutingController.ShowExternalClients();
        }

        /// <summary>
        /// Clears the routes.
        /// </summary>
        /// <returns></returns>
        public string ClearRoutes()
        {
            if (this.RoutingController.ClearNetworkGraph())
            {

                return "Done!";
            }
            else return "Error!";
        }
        #endregion
    }
}