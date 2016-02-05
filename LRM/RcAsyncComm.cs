using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LRM
{
    public class RcAsyncComm
    {
        public Socket AsyncSocket { get; private set; }
        public bool LastOne { get; private set; }

        private ManualResetEvent PacketsReceived = new ManualResetEvent(false);
        private ManualResetEvent PacketsSend = new ManualResetEvent(false);
        private Action<RcAsyncComm> ConnectionLostCallback;
        private Action<string, RcAsyncComm> DataRedCallback;
        private Action<string, RcAsyncComm> SubscribeCallback;
        private Thread ReciveThread;
        private bool IdleState;

        public RcAsyncComm(Socket asyncSocket,
            Action<RcAsyncComm> connectionLostCallback,
            Action<string, RcAsyncComm> dataRedCallback,
            Action<string, RcAsyncComm> subscribeCallback)
        {
            AsyncSocket = asyncSocket;
            LastOne = true;
            DataRedCallback = dataRedCallback;
            SubscribeCallback = subscribeCallback;
            IdleState = true;
            ConnectionLostCallback = connectionLostCallback;
        }

        public void StartReciving()
        {
            ReciveThread = new Thread(Recive);
            ReciveThread.Start();
        }

        private void Recive()
        {
            try
            {
                while (LastOne)
                {
                    PacketsReceived.Reset();
                    StateObject state = new StateObject();
                    AsyncSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    PacketsReceived.WaitOne();
                }
            }
            catch (Exception ex)
            {
                AsyncSocket = null;
                if (ConnectionLostCallback != null)
                {
                    ConnectionLostCallback(this);
                }
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                PacketsReceived.Set();
                String content = String.Empty;
                StateObject state = (StateObject)ar.AsyncState;
                int bytesRead = AsyncSocket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    string data = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
                    state.ResponseBuilder.Append(data);

                    if (bytesRead != StateObject.BufferSize)
                    {
                        string allData = state.ResponseBuilder.ToString();

                        if (SubscribeCallback != null && IdleState && allData.Contains("INTRODUCE"))
                        {
                            SubscribeCallback(allData, this);
                            IdleState = false;
                            return;
                        }

                        DataRedCallback(allData, this);
                    }
                    else
                    {
                        AsyncSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("--------------------------------RC ReadCallback");
                Console.WriteLine(e);
            }
        }
        object sendingLock = new object();
        public void Send(String data)
        {
            Console.WriteLine("--------------------------------RC Entering send");
            lock (sendingLock)
            {

                try
                {
                    byte[] byteData = Encoding.ASCII.GetBytes(data);
                    PacketsSend.Reset();
                    StateObject state = new StateObject();
                    AsyncSocket.BeginSend(byteData, 0, byteData.Length, 0,
                        new AsyncCallback(SendCallback), null);
                    PacketsSend.WaitOne();
                }
                catch (Exception e)
                {
                    Console.WriteLine("--------------------------------RC Send:");
                    Console.WriteLine(e);
                }
            }
        }
        object finishingSending = new object();
        private void SendCallback(IAsyncResult ar)
        {
            lock (finishingSending)
            {
                try
                {
                    int bytesSent = AsyncSocket.EndSend(ar);
                    Console.WriteLine("--------------------------------RC Data Send");
                    PacketsSend.Set();
                }
                catch (Exception e)
                {
                    AsyncSocket = null;
                    Console.WriteLine(e.ToString());
                    if (ConnectionLostCallback != null)
                    {
                        ConnectionLostCallback(this);
                    }
                }
            }
        }
    }
}
