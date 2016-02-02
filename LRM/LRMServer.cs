using LRM.Exceptions;
using NetworkNode.LRM.Communication;
using Newtonsoft.Json;
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
    class LrmServer : LocalPort
    {
        public event NodeConnected NodeConnected;
        public event NodeDisconnected NodeDisconnected;
        private LrmRegister LrmRegister;
        private Action<string, AsyncCommunication> DataRedCallback;
        private ManualResetEvent ConnectNew = new ManualResetEvent(false);
        public LrmServer(int port, Action<string, AsyncCommunication> dataRedCallback, LrmRegister lrmRegister)
            : base(port)
        {
            LrmRegister = lrmRegister;
            DataRedCallback = dataRedCallback; /* (string data, AsyncCommunication async) =>
            {
                dataRedCallback(data, LrmRegister.FindNodeByConnection(async));
            };*/
        }

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
                        null);

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

            AsyncCommunication async = new AsyncCommunication(ActionSocket.EndAccept(ar),
                ConnectionLostCallback,
                DataRedCallback,
                SubscribeCallback);

            async.StartReciving();
        }

        private void SubscribeCallback(string data, AsyncCommunication ac)
        {
            LrmIntroduce node = JsonConvert.DeserializeObject<LrmIntroduce>(data);

            if (LrmRegister.ConnectedNodes.ContainsKey(node.Node))
            {
                if (LrmRegister.ConnectedNodes[node.Node].Async != null)
                {
                    throw new DeviceAllreadyConnected();
                }
                LrmRegister.ConnectedNodes[node.Node].Async = ac;
                LrmRegister.ConnectedNodes[node.Node].DomiansHierarchy = node.Domians;
            }
            else
            {
                lock (LrmRegister)
                {
                    LrmRegister.ConnectedNodes.Add(node.Node, new VirtualNode
                    {
                        Name = node.Node,
                        Async = ac,
                        DomiansHierarchy = node.Domians
                    });
                }
            }

            if (NodeConnected != null)
            {
                NodeConnected(node.Node);
            }
        }

        private void ConnectionLostCallback(AsyncCommunication ac)
        {
            VirtualNode disconnected = LrmRegister.FindNodeByConnection(ac);

            if (disconnected == null)
            {
                return;
            }

            LrmRegister.ConnectedNodes.Remove(disconnected.Name);
            Console.WriteLine("Node : {0} DISCONECTED", disconnected.Name);
            if (NodeDisconnected != null)
            {
                NodeDisconnected(disconnected.Name);
            }
        }


    }
}
