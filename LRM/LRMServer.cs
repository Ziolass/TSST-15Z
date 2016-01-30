using LRM.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LRM
{
    public delegate void NodeConnected(string NodeName);
    public delegate void NodeDisconnected(string NodeName);
    class LRMServer : LocalPort
    {
        public event NodeConnected NodeConnected;
        public event NodeDisconnected NodeDisconnected;
        private Dictionary<string, AsyncCommunication> LrmConnections;
        private Action<string> DataRedCallback; 
        public LRMServer(int port, Action<string> dataRedCallback) : base(port)
        {
            DataRedCallback = dataRedCallback;
            LrmConnections = new Dictionary<string, AsyncCommunication>();
        }

        

        
        
            ManualResetEvent ConnectNew = new ManualResetEvent(false);
            
            

            
            public void Start()
            {
                try
                {
                    ActionSocket.Bind(Endpoint);
                    ActionSocket.Listen(100);

                    while (true)
                    {
                        ConnectNew.Reset();

                        Console.WriteLine("Waiting for a connection...");
                        ActionSocket.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            ActionSocket);

                        ConnectNew.WaitOne();
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Console.WriteLine("\nPress ENTER to continue...");
                Console.Read();

            }

            public void AcceptCallback(IAsyncResult ar)
            {
                ConnectNew.Set();

                Socket listener = (Socket)ar.AsyncState;
             
                AsyncCommunication async = new AsyncCommunication(listener.EndAccept(ar), 
                    ConnectionLostCallback,
                    DataRedCallback,
                    SubscribeCallback);
                
                async.StartReciving();
            }

            private void SubscribeCallback(string data, AsyncCommunication ac)
            {
                if (LrmConnections.ContainsKey(data))
                {
                    throw new DeviceAllreadyConnected();
                }

                LrmConnections.Add(data, ac);

                if (NodeConnected != null)
                {
                    NodeConnected(data);
                }
            }

            private void ConnectionLostCallback(AsyncCommunication ac)
            {
                if (!LrmConnections.ContainsValue(ac))
                {
                    return;
                }
                string name = null;
                foreach (KeyValuePair<string, AsyncCommunication> lrmConnection in LrmConnections)
                {
                    if (lrmConnection.Value == ac)
                    {
                        name = lrmConnection.Key;
                        break;
                    }
                }

                LrmConnections.Remove(name);
                Console.WriteLine("Node : {0} DISCONECTED", name);
                if (NodeConnected != null)
                {
                    NodeConnected(name);
                }
            }

        
    }
}
